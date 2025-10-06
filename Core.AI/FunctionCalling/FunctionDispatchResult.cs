using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.FunctionCalling;

public sealed class FunctionDispatchResult
{
    public bool Handled { get; init; }
    public string? Result { get; init; }
}
