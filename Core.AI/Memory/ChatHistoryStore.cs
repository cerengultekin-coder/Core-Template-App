using System.Collections.Concurrent;

namespace Core.AI.Memory;

public class ChatHistoryStore
{
    private readonly ConcurrentDictionary<string, List<(string Role, string Content)>> _store = new();
    private const int MaxPerUser = 100;

    public IReadOnlyList<(string Role, string Content)> GetHistory(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Array.Empty<(string, string)>();
        return _store.TryGetValue(userId, out var list) ? list.ToList() : new List<(string, string)>();
    }

    public void AddMessage(string userId, string role, string content)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;
        var list = _store.GetOrAdd(userId, _ => new List<(string, string)>());
        lock (list)
        {
            list.Add((role, content));
            if (list.Count > MaxPerUser)
                list.RemoveRange(0, list.Count - MaxPerUser);
        }
    }

    public void Clear(string userId) => _store.TryRemove(userId, out _);
}
