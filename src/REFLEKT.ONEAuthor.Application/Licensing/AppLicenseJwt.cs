//
// AppLicenseJwt.cs
//
// Copyright (c) 2018 RE'FLEKT GmbH. All Rights Reserved.
// Copyright Bosch Automotive Service Solutions Limited 2018 all rights reserved.
//
// Authors:
//     Axel Peter
//
// Defines:
//     [C] One.Licensing.AppLicenseJwt 
//

namespace REFLEKT.ONEAuthor.Application.Licensing
{
    /// <summary>
    /// Class representation of the license JSON in JWT. Is decoded in the license manager into
    /// AppLicense JSON representation.
    /// </summary>
    public class AppLicenseJwt
    {
        /// <summary>
        /// The jwt token.
        /// </summary>
        public readonly string Token;

        /// <summary>
        /// Create a new AppLicenseJwt with given value.
        /// </summary>
        /// <param name="token">The jwt token.</param>
        public AppLicenseJwt(string token)
        {
            Token = token;
        }
    }
}