using CoreApp.Domain.Common;

namespace CoreApp.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Expires { get; set; }

    public bool IsRevoked { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
