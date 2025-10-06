using Core.AI.Models;
using MediatR;

namespace Core.AI.Commands;

public record PromptTextCommand(string Prompt, AIRequestOptions? Options) : IRequest<string>;
