using Core.AI.Abstractions;
using MediatR;

namespace Core.AI.Commands;
public class PromptTextCommandHandler : IRequestHandler<PromptTextCommand, string>
{
    private readonly IAIService _aiService;

    public PromptTextCommandHandler(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<string> Handle(PromptTextCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt cannot be empty.");

        var result = await _aiService.PromptAsync(
            request.Prompt,
            request.Options,
            cancellationToken
        );

        return result;
    }
}
