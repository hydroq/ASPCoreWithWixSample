using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Models;
using REFLEKT.ONEAuthor.Application.Utilities;
using System;
using System.Buffers.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using REFLEKT.ONEAuthor.Application.Helpers;

namespace REFLEKT.ONEAuthor.Application.Authorization
{
    /// <summary>
    /// Manager, responsible for Authentication functionality within the app
    /// </summary>
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IMemoryCache _memoryCache;

        private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

        public AuthenticationManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Tries to authenticate user into the app, based on gived credentials.
        /// Populates <see cref="accessToken"/> and <see cref="refreshToken"/> values in case of 
        /// successful authentication
        /// </summary>
        /// <param name="accessToken">Contains generated access token, used to access resources</param>
        /// <param name="refreshToken">Contains generated refresh token, used to issue new access tokens</param>
        /// <returns>Flag, indicating whether user was successfully authenticated</returns>
        public bool Authenticate(string userName, string password, out string accessToken, out string refreshToken)
        {
            accessToken = "";
            refreshToken = "";

            var userModel = RetrieveUserCredentials(userName);
            if (userName != userModel.User || ComputeHash(password) != userModel.Password)
            {
                return false;
            }

            accessToken = CreateNewToken(userName, AccessTokenLifetime);
            refreshToken = CreateNewToken(userName, RefreshTokenLifetime);

            return true;
        }

        /// <summary>
        /// Tries to generate new access token, based on given refresh token
        /// </summary>
        /// <returns>Flag, indicating whether refresh token was found in the system</returns>
        public bool TryIssueAccessToken(string refreshToken, out string accessToken)
        {
            accessToken = "";

            if (string.IsNullOrWhiteSpace(refreshToken) || 
                !_memoryCache.TryGetValue(refreshToken, out string userName))
            {
                return false;
            }

            accessToken = CreateNewToken(userName, AccessTokenLifetime);

            return true;
        }

        /// <summary>
        /// Performs check, whether gived access token is valid,
        /// populates <see cref="userName"/> parameter if so.
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <param name="userName">Contains name of the user, that this token was assigned to</param>
        /// <returns></returns>
        public bool CheckAccessToken(string token, out string userName)
        {
            return _memoryCache.TryGetValue(token, out userName) || CheckStaticToken(token, out userName);
        }

        public bool CheckAccessToken(string token)
        {
            return CheckAccessToken(token, out var _);
        }

        public string GetActiveUserByToken(string token)
        {
            return _memoryCache.Get<string>(token);
        }

        private UserModel RetrieveUserCredentials(string userName)
        {
            string configFolder = PathHelper.GetAdminsWorkingDirectory();
            string fullConfigPath = Path.Combine(configFolder, userName + ".json");

            if (File.Exists(fullConfigPath) &&
                new JsonSerializerUtility().TryDeserializeFromFile<UserModel>(fullConfigPath, out var resultUserModel))
            {
                return resultUserModel;
            }

            resultUserModel = GetDefaultUserModel();
            if (userName.Equals(resultUserModel.User, StringComparison.CurrentCultureIgnoreCase))
            {
                File.WriteAllText(fullConfigPath, JsonConvert.SerializeObject(resultUserModel));
            }
            
            return resultUserModel;
        }

        private bool CheckStaticToken(string token, out string userName)
        {
            string configFolder = PathHelper.GetAdminsWorkingDirectory();
            foreach (var userFilePath in Directory.GetFiles(configFolder))
            {
                if (!new JsonSerializerUtility().TryDeserializeFromFile<UserModel>(userFilePath, out var userModel))
                {
                    continue;
                }

                if (userModel.StaticTicket == token)
                {
                    userName = userModel.User;

                    return true;
                }
            }

            userName = string.Empty;

            return false;
        }

        private UserModel GetDefaultUserModel()
        {
            return new UserModel
            {
                User = "rfone",
                Password = ComputeHash("rfone")
            };
        }

        private string CreateNewToken(string userName, TimeSpan lifeTime)
        {
            var token = GenerateDummyToken();

            _memoryCache.Set(token, userName, new MemoryCacheEntryOptions { SlidingExpiration = lifeTime });

            return token;
        }

        private string GenerateDummyToken()
        {
            return LocaleConverter.ToUrlSafeBase64(Guid.NewGuid().ToByteArray());
        }

        private string ComputeHash(string content)
        {
            var rawContent = Encoding.Default.GetBytes(content);

            var rawHashedContent = new SHA512Managed().ComputeHash(rawContent);

            var result = Convert.ToBase64String(rawHashedContent);

            return result;
        }
    }
}