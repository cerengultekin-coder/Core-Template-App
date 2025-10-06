using FluentValidation;

namespace Core.AI.Models.Agent;

public class AgentRequestOptionsValidator : AbstractValidator<AgentRequestOptions>
{
    public AgentRequestOptionsValidator()
    {
        RuleFor(x => x.Temperature)
            .InclusiveBetween(0, 2).When(x => x.Temperature.HasValue);
        RuleFor(x => x.Model).Must(_ => true);
    }
}