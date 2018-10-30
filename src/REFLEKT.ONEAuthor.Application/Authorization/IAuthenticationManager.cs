namespace REFLEKT.ONEAuthor.Application.Authorization
{
    public interface IAuthenticationManager
    {
        bool Authenticate(string userName, string password, out string accessToken, out string refreshToken);

        bool TryIssueAccessToken(string refreshToken, out string accessToken);

        bool CheckAccessToken(string token, out string userName);

        bool CheckAccessToken(string token);

        string GetActiveUserByToken(string token);
    }
}