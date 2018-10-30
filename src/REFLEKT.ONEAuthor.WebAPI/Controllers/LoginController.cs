using Microsoft.AspNetCore.Mvc;
using REFLEKT.ONEAuthor.Application;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using TicketModel = REFLEKT.ONEAuthor.WebAPI.Models.DTO.TicketModel;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationManager _authenticationManager;

        public LoginController(IAuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        [HttpPost]
        [Route("api/login")]
        public TicketModel Post([FromBody] ApiFormInput formData)
        {
            if (formData == null)
            {
                return new TicketModel
                {
                    Errors = ApiHelper.JsonError(400, new[] { "User credentials are incomplete" })
                };
            }

            if (string.IsNullOrWhiteSpace(formData.username) && string.IsNullOrWhiteSpace(formData.password))
            {
                return new TicketModel
                {
                    Errors = ApiHelper.JsonError(400, new[] { "Invalid login or password" })
                };
            }

            if (!_authenticationManager.Authenticate(formData.username, formData.password, out var authToken, out var refreshToken))
            {
                return new TicketModel
                {
                    Errors = ApiHelper.JsonError(400, new[] { "Invalid login or password" })
                };
            }

            var resulTicketModel = new TicketModel { Token = authToken, RefreshToken = refreshToken };

            var server = string.IsNullOrEmpty(formData.server) ? "localhost" : formData.server;
            SVNManager.Authorizate(server, formData.username, formData.password, _ => { });

            return resulTicketModel;
        }

        [HttpPost]
        [Route("login")]
        [Route("viewer/login")]
        public ActionResult Get()
        {
            string user = Request.Query["user"];
            string password = Request.Query["password"];
            if (_authenticationManager.Authenticate(user, password, out var token, out var _))
            {
                return Content(token);
            }

            return BadRequest();
        }
    }
}