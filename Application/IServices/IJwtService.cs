using Domain.Enums;

namespace Application.IServices;

public interface IJwtService
{
    public string GenerateToken(Guid userId, UserRole role);
}