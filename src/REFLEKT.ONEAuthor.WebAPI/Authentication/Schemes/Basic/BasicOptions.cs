using Microsoft.AspNetCore.Authentication;

namespace REFLEKT.ONEAuthor.WebAPI.Authentication.Schemes.Basic
{
    /// <summary>
    /// Contains the options used by the <see cref="BasicMiddleware"/>.
    /// </summary>
    public class BasicOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The default realm used by basic authentication.
        /// </summary>
        public string Realm { get; set; } = BasicDefaults.Realm;
    }
}