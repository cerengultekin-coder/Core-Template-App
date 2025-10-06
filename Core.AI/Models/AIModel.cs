using Core.AI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models;

public class AIModel
{
    public string Provider { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsDefault { get; set; } = false;
}
