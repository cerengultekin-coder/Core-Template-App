using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Core.AI.FunctionCalling;

public interface IFunctionRegistry
{
    IEnumerable<IAiFunction> GetAll();
    bool TryGet(string name, out IAiFunction fn);
}
