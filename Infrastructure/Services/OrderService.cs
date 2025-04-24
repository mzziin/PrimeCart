using Application.Common;
using Application.DTOs.Order;
using Application.IServices;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<OrderResponse>>> GetOrdersByCustomerId(Guid customerId)
    {
        var orders = await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderResponse
            {
                OrderId = o.Id,
                CustomerId = o.CustomerId,
                TotalAmount = o.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                OrderDate = o.OrderDate,
                OrderItems = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    UnitPrice = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.Price * oi.Quantity,
                    Status = oi.Status,
                    DeliveredAt = oi.DeliveredAt
                }).ToList()
            }).ToListAsync();

        return Result<List<OrderResponse>>.Success(orders);
    }

    public async Task<Result<OrderResponse>> GetOrderById(Guid orderId)
    {
        var order = await _context.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new OrderResponse
            {
                OrderId = o.Id,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                TotalAmount = o.OrderItems.Sum(oi => oi.Quantity * oi.Price),
                OrderItems = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    UnitPrice = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.Price * oi.Quantity,
                    Status = oi.Status,
                    DeliveredAt = oi.DeliveredAt
                }).ToList()
            }).FirstOrDefaultAsync();

        if (order is null)
            return Result<OrderResponse>.Failure("Order not found", 404);

        return Result<OrderResponse>.Success(order);
    }

    public async Task<Result<OrderResponse>> PlaceOrder(Guid customerId, PlaceOrderRequest placeOrderRequest)
    {
        if (placeOrderRequest.OrderItems.Count == 0)
            return Result<OrderResponse>.Failure("Order items cannot be empty", 400);

        // Validate all quantities are positive
        if (placeOrderRequest.OrderItems.Any(oi => oi.Quantity <= 0))
            return Result<OrderResponse>.Failure("All item quantities must be positive", 400);

        // Validate customer exists
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            return Result<OrderResponse>.Failure("Customer not found", 404);

        var requestedProductIds = placeOrderRequest.OrderItems.Select(oi => oi.ProductId).ToList();

        // Check if all products exist in the database
        var existingProducts = await _context.Products
            .Where(p => requestedProductIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p);

        // Check if any product IDs are not found
        var notFoundProductIds = requestedProductIds
            .Where(id => !existingProducts.ContainsKey(id))
            .ToList();

        if (notFoundProductIds.Count != 0)
            return Result<OrderResponse>.Failure($"Products not found: {string.Join(", ", notFoundProductIds)}", 400);

        // Check product inventory (assuming Products have a Stock property)
        var insufficientStockItems = placeOrderRequest.OrderItems
            .Where(oi => existingProducts[oi.ProductId].Stock < oi.Quantity)
            .Select(oi => new
            {
                ProductId = oi.ProductId,
                ProductName = existingProducts[oi.ProductId].Name,
                RequestedQuantity = oi.Quantity,
                AvailableStock = existingProducts[oi.ProductId].Stock
            })
            .ToList();

        if (insufficientStockItems.Count != 0)
        {
            var stockDetails = string.Join(", ", insufficientStockItems.Select(item =>
                $"{item.ProductName} (ID: {item.ProductId}, Requested: {item.RequestedQuantity}, Available: {item.AvailableStock})"));
            return Result<OrderResponse>.Failure($"Insufficient stock for: {stockDetails}", 400);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                OrderItems = placeOrderRequest.OrderItems.Select(oi => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = oi.ProductId,
                    Status = OrderItemStatus.Pending,
                    Quantity = oi.Quantity,
                    Price = existingProducts[oi.ProductId].Price
                }).ToList()
            };

            // Update product inventory
            foreach (var item in placeOrderRequest.OrderItems)
            {
                var product = existingProducts[item.ProductId];
                product.Stock -= item.Quantity;
                _context.Products.Update(product);
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var orderResponse = new OrderResponse
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                TotalAmount = order.OrderItems.Sum(oi => oi.Price * oi.Quantity),
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = existingProducts[oi.ProductId].Name,
                    UnitPrice = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.Price * oi.Quantity,
                    Status = oi.Status,
                    DeliveredAt = oi.DeliveredAt
                }).ToList()
            };

            return Result<OrderResponse>.Success(orderResponse);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<OrderResponse>.Failure($"Failed to place order: {ex.Message}", 500);
        }
    }

    public async Task<Result<OrderItemResponse>> UpdateOrderStatus(Guid orderItemId, OrderItemStatus orderItemStatus)
    {
        var orderItem = await _context.OrderItems
            .Include(orderItem => orderItem.Product)
            .FirstOrDefaultAsync(o => o.Id == orderItemId);

        if (orderItem is null)
            return Result<OrderItemResponse>.Failure("Order Item not found", 404);

        if (orderItem.Status == OrderItemStatus.Cancelled && orderItemStatus != OrderItemStatus.Cancelled)
            return Result<OrderItemResponse>.Failure("Cannot change status of a cancelled order item", 400);

        if (orderItem.Status == OrderItemStatus.Delivered && orderItemStatus != OrderItemStatus.Delivered)
            return Result<OrderItemResponse>.Failure("Cannot change status of a delivered order item", 400);
        
        if (orderItem.Status == orderItemStatus)
            return Result<OrderItemResponse>.Success(new OrderItemResponse()
            {
                Id = orderItem.Id,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product.Name,
                UnitPrice = orderItem.Price,
                Quantity = orderItem.Quantity,
                TotalPrice = orderItem.Price * orderItem.Quantity,
                Status = orderItem.Status,
                DeliveredAt = orderItem.DeliveredAt
            });

        if (orderItemStatus == OrderItemStatus.Cancelled)
        {
            var cancelRequest = await CancelOrder(orderItemId);
            if (!cancelRequest.IsSuccess)
                return Result<OrderItemResponse>.Failure(cancelRequest.Error!, cancelRequest.StatusCode);

            var cancelledItem = await _context.OrderItems
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderItemId);

            if (cancelledItem is null) // This should never happen as we just cancelled it
                return Result<OrderItemResponse>.Failure("Order item not found after cancellation", 500);

            return Result<OrderItemResponse>.Success(new OrderItemResponse()
            {
                Id = cancelledItem.Id,
                ProductId = cancelledItem.ProductId,
                ProductName = cancelledItem.Product.Name,
                UnitPrice = cancelledItem.Price,
                Quantity = cancelledItem.Quantity,
                TotalPrice = cancelledItem.Price * cancelledItem.Quantity,
                Status = cancelledItem.Status,
                DeliveredAt = cancelledItem.DeliveredAt
            });
        }

        orderItem.Status = orderItemStatus;

        if (orderItemStatus == OrderItemStatus.Delivered && orderItem.DeliveredAt == null)
        {
            orderItem.DeliveredAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        var status = await _context.SaveChangesAsync();
        
        if (status == 0)
            return Result<OrderItemResponse>.Failure("Failed to update order item status", 500);
        
        var response = new OrderItemResponse
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.Product.Name,
            UnitPrice = orderItem.Price,
            Quantity = orderItem.Quantity,
            TotalPrice = orderItem.Price * orderItem.Quantity,
            Status = orderItem.Status,
            DeliveredAt = orderItem.DeliveredAt
        };
        return Result<OrderItemResponse>.Success(response);
    }

    public async Task<Result<bool>> CancelOrder(Guid orderItemId)
    {
        var orderItem = await _context.OrderItems
            .Include(orderItem => orderItem.Product)
            .FirstOrDefaultAsync(o => o.Id == orderItemId);

        if (orderItem is null)
            return Result<bool>.Failure("Order not found", 404);

        if (orderItem.Status == OrderItemStatus.Cancelled)
            return Result<bool>.Failure("Order item is already cancelled", 400);

        if (orderItem.Status == OrderItemStatus.Delivered)
            return Result<bool>.Failure("Cannot cancel a delivered order item", 400);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            orderItem.Product.Stock += orderItem.Quantity;
            _context.Products.Update(orderItem.Product);

            orderItem.Status = OrderItemStatus.Cancelled;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<bool>.Failure($"Failed to cancel order: {ex.Message}", 500);
        }
    }
}