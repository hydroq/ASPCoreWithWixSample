//
// AppLicense.cs
//
// Copyright (c) 2018 RE'FLEKT GmbH. All Rights Reserved.
// Copyright Bosch Automotive Service Solutions Limited 2018 all rights reserved.
//
// Authors:
//     Axel Peter
//
// Defines:
//     [C] One.Licensing.AppLicense
//

namespace REFLEKT.ONEAuthor.Application.Licensing
{
    /// <summary>
    /// Class representation of the license JSON used to verify the product key and device.
    /// <seealso cref="https://re-flekt.atlassian.net/wiki/spaces/PUBCAP/pages/247595089/RFQ+Licensing+server"/>
    /// </summary>
    public class AppLicense
    {
        /// <summary>
        /// The hardware id.
        /// </summary>
        public readonly string Hwid;

        /// <summary>
        /// The used name.
        /// </summary>
        public readonly string User;

        /// <summary>
        /// Flag indicating if the license is valid.
        /// </summary>
        public readonly bool Licensed;

        /// <summary>
        /// The timestamp until when the license is valid.
        /// </summary>
        public readonly string Until;

        /// <summary>
        /// The timestamp when the license was requested/checked.
        /// </summary>
        public readonly string TimeStamp;

        /// <summary>
        /// The details of the request results.
        /// </summary>
        public readonly string Details;

        /// <summary>
        /// Create a new ApplicationLicense with given values.
        /// </summary>
        /// <param name="user">The user id.</param>
        /// <param name="licensed">Flag indicating if the license is valid.</param>
        /// <param name="until">The timestamp until when the license is valid.</param>
        /// <param name="timeStamp">The timestamp when the license was requested/checked.</param>
        /// <param name="details">The details of the request results.</param>
        public AppLicense(string user, bool licensed, string until, string timeStamp, string details)
        {
            User = user;
            Licensed = licensed;
            Until = until;
            TimeStamp = timeStamp;
            Details = details;
        }
    }
}