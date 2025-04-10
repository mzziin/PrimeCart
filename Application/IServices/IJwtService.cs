using Domain.Enums;

namespace Application.IServices;

public interface IJwtService
{ 
    string GenerateToken(Guid userId, UserRole role);
}