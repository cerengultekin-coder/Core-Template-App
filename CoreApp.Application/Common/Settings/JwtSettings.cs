using System.ComponentModel.DataAnnotations;

namespace CoreApp.Application.Common.Settings;

public class JwtSettings
{
    [Required] public string Secret { get; set; } = "";
    [Required] public string Issuer { get; set; } = "";
    [Required] public string Audience { get; set; } = "";
    [Range(5, 120)] public int AccessTokenMinutes { get; set; } = 15;
    [Range(1, 30)] public int RefreshTokenDays { get; set; } = 7;
}