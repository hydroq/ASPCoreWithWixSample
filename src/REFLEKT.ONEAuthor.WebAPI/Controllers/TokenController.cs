using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.WebAPI.Models.DTO;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class TokenController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;

        public TokenController(IAuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        [HttpPost]
        [Route("api/token")]
        public ActionResult UpdateToken()
        {
            string refreshToken = Request.Headers["refresh-token"];
            if (!_authenticationManager.TryIssueAccessToken(refreshToken, out var accessToken))
            {
                return Forbid();
            }

            var result = new TicketModel { Token = accessToken, RefreshToken = refreshToken };

            return Json(result);
        }
    }
}