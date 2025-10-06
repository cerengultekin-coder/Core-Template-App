using CoreApp.Application.Features.Profile.DTOs;
using CoreApp.Application.Features.Profile.Queries;
using CoreApp.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoreApp.Infrastructure.Features.Profile.Queries;

public class GetProfileQueryHandler(CoreAppDbContext context, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetProfileQuery, ProfileDto>
{
    private readonly CoreAppDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            throw new UnauthorizedAccessException();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId), cancellationToken);

        if (user is null)
            throw new Exception("User not found");

        return new ProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username
        };
    }
}
