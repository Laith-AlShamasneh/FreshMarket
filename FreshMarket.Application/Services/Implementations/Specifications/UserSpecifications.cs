using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories;

namespace FreshMarket.Application.Services.Implementations.Specifications;

public static class UserSpecifications
{
    public static ISpecification<User> GetByUsernameOrEmail(string usernameOrEmail)
    {
        return new UserByUsernameOrEmailSpec(usernameOrEmail);
    }

    public static ISpecification<User> GetById(int userId)
    {
        return new UserByIdSpec(userId);
    }

    public static ISpecification<User> GetByRoleId(int roleId)
    {
        return new UserByRoleIdSpec(roleId);
    }

    public static ISpecification<User> GetByUsername(string username)
    {
        return new UserByUsernameSpec(username);
    }

    public static ISpecification<User> GetByEmail(string email)
    {
        return new UserByEmailSpec(email);
    }

    // ==================== Private Specification Classes ====================

    private sealed class UserByUsernameOrEmailSpec : BaseSpecification<User>
    {
        public UserByUsernameOrEmailSpec(string usernameOrEmail)
            : base(u => u.Username == usernameOrEmail || u.Person.Email == usernameOrEmail)
        {
            AddInclude(u => u.Person);
            AddInclude(u => u.UserRoles);
        }
    }

    private sealed class UserByIdSpec : BaseSpecification<User>
    {
        public UserByIdSpec(int userId)
            : base(u => u.UserId == userId)
        {
            AddInclude(u => u.Person);
            AddInclude(u => u.UserRoles);
        }
    }

    private sealed class UserByUsernameSpec : BaseSpecification<User>
    {
        public UserByUsernameSpec(string username)
            : base(u => u.Username == username)
        {
            AddInclude(u => u.Person);
        }
    }

    private sealed class UserByEmailSpec : BaseSpecification<User>
    {
        public UserByEmailSpec(string email)
            : base(u => u.Person.Email == email)
        {
            AddInclude(u => u.Person);
        }
    }

    private sealed class UserByRoleIdSpec : BaseSpecification<User>
    {
        public UserByRoleIdSpec(int userId)
            : base(u => u.UserId == userId)
        {
            AddInclude(u => u.Person);
            AddInclude(u => u.UserRoles);
        }
    }
}
