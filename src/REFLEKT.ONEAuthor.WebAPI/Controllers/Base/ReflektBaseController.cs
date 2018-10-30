using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REFLEKT.ONEAuthor.Application.Models.Base;
using System.Collections.Generic;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers.Base
{
    public class ReflektBaseController : Controller
    {
        protected List<string> BadRequestJson(string errorMessage)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;

            return new List<string> { errorMessage };
        }

        /// <summary>
        /// Sets response status code to 400 and returns new instance
        /// of specific T class, that implements <see cref="IErrorResult"/>
        /// </summary>
        /// <typeparam name="T">Type to be returned</typeparam>
        /// <param name="errorMessage">Message to be displayed to the client</param>
        /// <returns>New instance of type T with populated Error property and 400 status code</returns>
        protected T CustomBadRequestResult<T>(string errorMessage)
            where T : class, IErrorResult, new()
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;

            return new T { errors = new[] { errorMessage } };
        }

        /// <summary>
        /// Sets response status code to 500 and returns new instance
        /// of specific T class, that implements <see cref="IErrorResult"/>
        /// </summary>
        /// <typeparam name="T">Type to be returned</typeparam>
        /// <param name="errorMessage">Message to be displayed to the client</param>
        /// <returns>New instance of type T with populated Error property and 500 status code</returns>
        protected T CustomInternalServerResult<T>(string errorMessage)
            where T : class, IErrorResult, new()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;

            return new T { errors = new[] { errorMessage } };
        }
    }
}