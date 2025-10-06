using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Common.Results;

public sealed record Error(string Code, string? Message = null)
{
    public static readonly Error None = new("None");
    public static Error Validation(string message) => new("Validation", message);
    public static Error Unauthorized(string? message = null) => new("Unauthorized", message ?? "Unauthorized");
    public static Error NotFound(string entity, string? key = null) => new("NotFound", $"{entity} not found{(key is null ? "" : $" ({key})")}.");
    public static Error Conflict(string message) => new("Conflict", message);
}
