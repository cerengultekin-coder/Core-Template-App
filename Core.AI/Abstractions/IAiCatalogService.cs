namespace Core.AI.Abstractions;

public interface IAiCatalogService
{
    string GetDefaultProvider();
    string GetDefaultModel();
    IEnumerable<string> GetProviders();
    IEnumerable<string> GetModels(string provider);
    bool IsValidModel(string provider, string model);
}
