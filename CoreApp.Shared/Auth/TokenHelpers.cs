using System.Security.Cryptography;
using System.Text;

namespace CoreApp.Shared.Auth;

public static class TokenHelpers
{
    public static string CleanBearer(string? bearerOrToken)
    {
        if (string.IsNullOrWhiteSpace(bearerOrToken)) return string.Empty;
        var t = bearerOrToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? bearerOrToken["Bearer ".Length..]
            : bearerOrToken;
        return t.Trim().Trim('"').Replace("\r", "").Replace("\n", "").Replace("\t", "");
    }

    public static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Sha256Base64Url(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Base64Url(hash);
    }
}
