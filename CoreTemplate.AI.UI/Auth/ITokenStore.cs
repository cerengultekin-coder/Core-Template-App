namespace CoreTemplate.AI.UI.Auth;

public interface ITokenStore
{
    Task<(string Access, string Refresh)> GetAsync();
    Task SetAsync(string access, string refresh);
    Task ClearAsync();
}