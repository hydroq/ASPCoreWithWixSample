using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using REFLEKT.ONEAuthor.Application.Authorization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace REFLEKT.ONEAuthor.WebAPI.Authentication.Schemes.Basic
{
    public class BasicAuthHandler : AuthenticationHandler<BasicOptions>
    {
        private readonly IAuthenticationManager _authenticationManager;

        public BasicAuthHandler(IOptionsMonitor<BasicOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAuthenticationManager authenticationManager)
            : base(options, logger, encoder, clock)
        {
            _authenticationManager = authenticationManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorizationToken = Request.Query["ticket"];

            if (string.IsNullOrWhiteSpace(authorizationToken))
            {
                return AuthenticateResult.Fail("Authentication token is missing");
            }

            if (!_authenticationManager.CheckAccessToken(authorizationToken, out var userName))
            {
                return AuthenticateResult.Fail("Invalid authentication token provided");
            }

            var claims = new[] { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}