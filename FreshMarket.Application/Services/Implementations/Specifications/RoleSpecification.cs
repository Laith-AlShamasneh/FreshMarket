using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories;

namespace FreshMarket.Application.Services.Implementations.Specifications;

internal class RoleSpecification
{
    public static ISpecification<Role> GetDefaultRole()
    {
        return new DefaultRoleSpec();
    }


    // ==================== Private Specification Classes ====================

    private sealed class DefaultRoleSpec : BaseSpecification<Role>
    {
        public DefaultRoleSpec()
            : base(r => r.IsDefault && r.IsActive)
        {
        }
    }
}
