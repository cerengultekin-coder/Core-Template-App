using CoreApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp.Application.Common.Interfaces.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    (string rawRefresh, RefreshToken entity) GenerateRefreshToken(User user, string? sessionId, string? ip, string? ua);
}
