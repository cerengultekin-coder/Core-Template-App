using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Core.AI.FunctionCalling;

public sealed class InMemoryFunctionRegistry : IFunctionRegistry
{
    private readonly ConcurrentDictionary<string, IAiFunction> _map = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryFunctionRegistry(IEnumerable<IAiFunction> functions)
    {
        foreach (var f in functions)
            _map[f.Name] = f;
    }

    public IEnumerable<IAiFunction> GetAll() => _map.Values;

    public bool TryGet(string name, out IAiFunction fn) => _map.TryGetValue(name, out fn!);
}
