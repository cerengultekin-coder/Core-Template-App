namespace Core.AI.Models.Agent;

public interface IAgentProfileProvider
{
    Task<AgentProfile?> GetProfileByIdAsync(string id);
    Task<List<AgentProfile>> GetAllAsync();
}
