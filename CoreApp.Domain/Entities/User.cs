using CoreApp.Domain.Common;

namespace CoreApp.Domain.Entities;

public class User : BaseEntity
{
    private readonly List<Role> _roles = new();
    private readonly List<RefreshToken> _refreshTokens = new();

    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } 
    public User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }

    public void UpdatePassword(string newHash)
    {
        PasswordHash = newHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRole(Role role) => _roles.Add(role);
    public void AddRefresh(RefreshToken rt) => _refreshTokens.Add(rt);
}
