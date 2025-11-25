using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories;

namespace FreshMarket.Application.Services.Implementations.Specifications;

public static class UserRoleSpecification
{
    public static ISpecification<UserRole> GetByUserId(long userId)
    {
        return new RolesByUserIdSpec(userId);
    }


    // ==================== Private Specification Classes ====================

    private sealed class RolesByUserIdSpec : BaseSpecification<UserRole>
    {
        public RolesByUserIdSpec(long userId)
            : base(ur => ur.UserId == userId)
        {
            AddInclude(u => u.Role);
        }
    }
}
