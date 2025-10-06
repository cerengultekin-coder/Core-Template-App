namespace CoreApp.Application.Features.Profile.DTOs;

public class ProfileDto
{
    public Guid Id { get; set; }

    public string Username { get; set; } = default!;

    public string Email { get; set; } = default!;
}
