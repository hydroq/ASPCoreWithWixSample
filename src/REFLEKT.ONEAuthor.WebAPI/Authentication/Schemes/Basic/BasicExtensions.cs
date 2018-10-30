using System;
using Microsoft.AspNetCore.Authentication;

namespace REFLEKT.ONEAuthor.WebAPI.Authentication.Schemes.Basic
{
    public static class BasicExtensions
    {
        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder)
            => builder.AddBasic(BasicDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder,
                                                     Action<BasicOptions> configureOptions)
            => builder.AddBasic(BasicDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder,
                                                     string authenticationScheme,
                                                     Action<BasicOptions> configureOptions)
            => builder.AddBasic(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder builder,
                                                     string authenticationScheme,
                                                     string displayName, 
                                                     Action<BasicOptions> configureOptions)
            => builder.AddScheme<BasicOptions, BasicAuthHandler>(authenticationScheme, displayName, configureOptions);
    }
}