using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Application.Features.Profile.DTOs;
using MediatR;

namespace CoreApp.Application.Features.Profile.Queries;

public class GetProfileQuery : IRequest<ProfileDto>, IAuthorizedRequest
{

}
