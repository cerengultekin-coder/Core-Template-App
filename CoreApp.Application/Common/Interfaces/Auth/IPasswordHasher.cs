namespace CoreApp.Application.Common.Interfaces.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
