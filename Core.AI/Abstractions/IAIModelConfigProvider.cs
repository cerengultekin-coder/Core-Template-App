using Core.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Abstractions;

public interface IAIModelConfigProvider
{
    Task<AIModelConfig> GetConfigAsync(string modelNameOrAlias);
}
