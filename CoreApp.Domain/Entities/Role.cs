using CoreApp.Domain.Common;

namespace CoreApp.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private Role() { }
    public Role(string name) => Name = name;
}
