using Core.AI.Models;

namespace Core.AI.Config;

public class AiCatalogOptions
{
    public List<AIModel> Models { get; set; } = new();
}
