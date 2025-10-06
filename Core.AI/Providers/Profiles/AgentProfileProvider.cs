using Core.AI.Models.Agent;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Providers.Profiles;

public class AgentProfileProvider
{
    private readonly List<AgentProfile> _profiles;

    public AgentProfileProvider(IConfiguration configuration)
    {
        _profiles = configuration
            .GetSection("AgentProfiles")
            .Get<List<AgentProfile>>() ?? new();

        foreach (var p in _profiles)
        {
            if (string.IsNullOrWhiteSpace(p.Id) && !string.IsNullOrWhiteSpace(p.Name))
                p.Id = ToSlug(p.Name);
        }
    }

    public AgentProfile GetProfile(string key)
    {
        if (!_profiles.Any())
            throw new InvalidOperationException("No agent profiles configured.");

        if (string.IsNullOrWhiteSpace(key))
            return _profiles.First();

        var p = _profiles.FirstOrDefault(x =>
            x.Id.Equals(key, StringComparison.OrdinalIgnoreCase) ||
            x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

        return p ?? _profiles.First();
    }

    public IEnumerable<AgentProfile> GetAllProfiles() => _profiles;

    private static string ToSlug(string s)
    {
        var slug = new string(s.ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray());
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
