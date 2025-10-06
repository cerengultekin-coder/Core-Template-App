using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CoreApp.Application.Common;

public static class Guard
{
    public static void NotNull<T>(T? value, string name)
    {
        if (value is null) throw new ArgumentNullException(name);
    }

    public static void NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", name);
    }
}