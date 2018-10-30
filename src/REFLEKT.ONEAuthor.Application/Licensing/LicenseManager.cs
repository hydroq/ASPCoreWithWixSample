//
// LicenseManager.cs
//
// Copyright (c) 2018 RE'FLEKT GmbH. All Rights Reserved.
// Copyright Bosch Automotive Service Solutions Limited 2018 all rights reserved.
//
// Authors:
//     Axel Peter
//
// Defines:
//     [C] One.Licensing.LicenseManager 
//

//using BestHTTP;
using Newtonsoft.Json;
//using One.Core.DataTypes;
//using One.Core.DataTypes.Observable;
//using One.Core.Services;
using REFLEKT.ONEAuthor.Application.Utilities;
//using One.UI2D.ViewModels;
using System;
using System.Net;
using System.Globalization;
using System.IO;
//using UnityEngine;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using REFLEKT.ONEAuthor.Application.Helpers;

namespace REFLEKT.ONEAuthor.Application.Licensing
{
    /// <summary>
    /// Manager class for the Licensing module. Handles internal logic of the module.
    /// Is responsible for connecting with the licensing server and retrieving and verify a license.
    /// </summary>
    public class LicenseManager
    {
        private readonly ILogger _logger;

        /// <summary>
        /// The name of the license file.
        /// </summary>
        private const string FileName = "License.json";

        /// <summary>
        /// The value for the content type within the http header.
        /// </summary>
        private const string ContentType = "application/x-www-form-urlencoded";

        /// <summary>
        /// The string for requesting a license, with parameters to be replaced.
        /// </summary>
        private const string RequestString = "/user/{0}/licenses/app/{1}/hwid/{2}/key/{3}";

        /// <summary>
        /// The string for checking a license, with parameters to be replaced.
        /// </summary>
        private const string VerifyString = "/user/{0}/licenses/app/{1}/hwid/{2}";

        /// <summary>
        /// Error message for request timeout.
        /// </summary>
        private const string TimeOutMessage = "The request timed out ({0}).";

        /// <summary>
        /// Invalid response message.
        /// </summary>
        private const string InvalidResponseMessage = "The response to the request was invalid ({0}).";

        /// <summary>
        /// Rejected request message.
        /// </summary>
        private const string RejectedRequestMessage = "The request was rejected ({0}).";

        /// <summary>
        /// The url of the licensing server.
        /// </summary>
        private const string Host = "https://user.reflektone.com";

        /// <summary>
        /// Connect timeout value.
        /// </summary>
        private const float ConnectTimeout = 3.0f;

        /// <summary>
        /// Request timeout value.
        /// </summary>
        private const float RequestTimeout = 3.0f;

        /// <summary>
        /// The response code for valid requests.
        /// </summary>
        private const int ValidResponseCode = 200;

        /// <summary>
        /// The format in which the experation date is received.
        /// </summary>
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Date time string utc identifier.
        /// </summary>
        private const string UtcIdentifier = " UTC";

        /// <summary>
        /// The static public key.
        /// </summary>
        private const string PubKeyText = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAugOLQNxR/R/kgx4wsialHukQvOVCwYRGPSS2v8lzyk1TNrMZUysfz3j0KKKCDMuZC//GgSvVKbyQjF5Lr9lGpDIXQiobu1iI1i0xuAT7wUkMmpjEwXOhg0DtLDiDmgtDw/EnfDqKPzA0MOqCMxqFktsjRcIOXPMmwAnMvq3ub1K02sD1zR4PvreBIInmd4UXu9kpJLkIb3ccltmQ0fiQUqocvWg7s0vHxkEM5jKkAEGQGT4f5XM9pBbmeC5KLKmHUBGpJIbWI/2dwJ/axagTuUR85TAeELRLWfNU+9QU+mor/lFoCk7okWCQu5vB0t92fwh/M/Xaq4OqNzUTSHjh8wIDAQAB";

        /// <summary>
        /// The has algorithm used.
        /// </summary>
        private const string HashAlgorithm = "SHA256";

        /// <summary>
        /// Observable to get notified when the license manager is requesting a license and when it finished.
        /// </summary>
        //public IObservableProperty<bool> RequestingLicense { get; }

        /// <summary>
        /// Observable to get notified if the license is valid.
        /// </summary>
        //public IObservableProperty<bool> LicenseValid { get; }
        public Action<bool, AppLicense, string> OnCheckLicense;

        /// <summary>
        /// Gets the error message from the http request, if any.
        /// </summary>
        public string errorMessage { get { return _errorMessage; } }

        /// <summary>
        /// The scene view model.
        /// </summary>
        //private ISceneViewModel _sceneViewModel;

        /// <summary>
        /// The resource manager.
        /// </summary>
        //private IResourceManager _resourceManager;

        /// <summary>
        /// The deserialized object representing the app license.
        /// </summary>
        private AppLicense _appLicense;

        /// <summary>
        /// The deserialized object representing the license in jwt.
        /// </summary>
        private AppLicenseJwt _appLicenseJwt;

        /// <summary>
        /// The os type, determined at runtime.
        /// </summary>
        private string _deviceId => NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString();

        /// <summary>
        /// The error message from the http request, if any.
        /// </summary>
        private string _errorMessage;

        /// <summary>
        /// The os type, determined at compiletime.
        /// </summary>
        private string _osType
        {
            get
            {
                /*#if UNITY_IOS
                                return "ios";
                #elif UNITY_ANDROID
                                return "android";
                #elif UNITY_STANDALONE_WIN
                                return "win";
                #elif UNITY_WSA
                                return "wsa";
                #else
                                return "osx";
                #endif*/
                return "author";
            }
        }

        /// <summary>
        /// Path to the license json.
        /// </summary>
        //private AbsoluteUri _licenseFileUri;
        private string _licenseFileUri;

        /// <summary>
        /// Create new instance of licensing manager
        /// </summary>
        public LicenseManager(ILogger<LicenseManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Create new instance that creates connections between the licensing manager and the scene view model
        /// and starts the check if license is valid.
        /// ...
        /// Is obsolete in v3*
        /// </summary>
        public LicenseManager(Action<bool, AppLicense, string> onCheck = null/*ISceneViewModel sceneViewModel, IResourceManager resourceManager*/)
        {
            //RequestingLicense = new ObservableProperty<bool>(true);
            //LicenseValid = new ObservableProperty<bool>();
            OnCheckLicense = onCheck;

            //_sceneViewModel = sceneViewModel;
            //_resourceManager = resourceManager;

            /*try
            {
                //_licenseFileUri = new AbsoluteUri(Path.Combine(Application.persistentDataPath, FileName));
                _licenseFileUri = Path.Combine(UsersHelper.GetUserFolder(), UsersHelper.GetToolsFolder(), FileName);

                LoadLocalLicense();

                if (_appLicense != null)
                {
                    CheckLicenseOnline(_appLicense.User);
                }
                else
                {
                    // No license object could be loaded, show request screen
                    //_sceneViewModel.LicenseCheckVisible.Value = true;
                }
            }
            catch (Exception e)
            {
                //Debug.LogWarning("Exception while trying to check license:" + e);
                //_sceneViewModel.LicenseCheckVisible.Value = true;
            }*/
        }

        public async Task<AppLicenseDto> CheckLicense()
        {
            try
            {
                LoadLocalLicense();
                if (_appLicense == null)
                {
                    // No license object could be loaded, show request screen
                    return new AppLicenseDto("No license object could be loaded");
                }

                if (await CheckForInternetConnection())
                {
                    await CheckLicenseOnline(_appLicense.User);
                }

                var valid = CheckLocalLicense();

                return new AppLicenseDto(_appLicense, valid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception while trying to check license: " + ex);

                return new AppLicenseDto("Error on attempt to check license");
            }
        }

        public static async Task<bool> CheckForInternetConnection()
        {
            try
            {
                using (var requestResult = await new HttpClient().GetAsync("http://clients3.google.com/generate_204"))
                {
                    return requestResult.StatusCode == HttpStatusCode.NoContent;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Request a new license for given user and key.
        /// </summary>
        /// <param name="user">The user to request a license for.</param>
        /// <param name="key">The key to request a license for.</param>
        public async Task<AppLicenseDto> RequestLicenseOnline(string user, string key)
        {
            try
            {
                return await SendRequestForNewLicense(user, key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Exception while trying to obtain new license: " + ex);

                return new AppLicenseDto("Error on attempt to obtain new license");
            }
        }

        /// <summary>
        /// Checks the license online for given user name.
        /// </summary>
        /// <param name="userId">The user name to check the license for.</param>
        private async Task CheckLicenseOnline(string userName)
        {
            await SendRequestToVerifyLicense(userName);
        }

        /// <summary>
        /// Send a http request, to request a new license.
        /// </summary>
        /// <param name="user">User to check/request license for.</param>
        /// <param name="key">Key to request license for (not required for check).</param>
        private async Task<AppLicenseDto> SendRequestForNewLicense(string user, string key)
        {
            HttpClient client = new HttpClient();

            var data = string.Format(RequestString, Uri.EscapeUriString(user.ToLower()), _osType, _deviceId, key);
            //var request = new HTTPRequest(new Uri(data), HTTPMethods.Post, OnlineRequest_Finished);
            //WebRequest request = WebRequest.Create (new Uri(Host+ data));

            client.BaseAddress = new Uri(Host);// new Uri(data);
            var result = await client.PostAsync(data, null);
            string resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"Error on connecting to License server. Status: {result.StatusCode}. Details: {resultContent}");

                return new AppLicenseDto($"Cannot connect to License server. Status: {result.StatusCode}");
            }

            OnlineRequest_Finished(resultContent);

            var valid = CheckLocalLicense();

            return new AppLicenseDto(_appLicense, valid);
        }

        /// <summary>
        /// Send a http request, to check an existing license online.
        /// </summary>
        /// <param name="user">User to check/request license for.</param>
        private async Task SendRequestToVerifyLicense(string user)
        {
            var data = string.Format(VerifyString, Uri.EscapeUriString(user.ToLower()), _osType, _deviceId);
            /*//var request = new HTTPRequest(new Uri(data), HTTPMethods.Get, OnlineRequest_Finished);
            var request = WebRequest.Create(new Uri(data));
            request.Method = "GET";
            ConfigureAndSendRequest(request, (x, y) => OnlineRequest_Finished(x, y));*/
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Host);// new Uri(data);
            var result = await client.GetAsync(data);
            string resultContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"Error on connecting to License server. Status: {result.StatusCode}. Details: {resultContent}");

                return;
            }

            OnlineRequest_Finished(resultContent);
        }

        /// <summary>
        /// Adds the default configuration to given http request and sends it.
        /// </summary>
        /// <param name="request">The http request to be configured.</param>
        private void ConfigureAndSendRequest(WebRequest/*HTTPRequest*/ request, Action<WebRequest, WebResponse> onFinish)
        {
            //request.AddHeader("Content-Type", ContentType);
            //request.ConnectTimeout = TimeSpan.FromSeconds(ConnectTimeout);
            //request.Timeout = TimeSpan.FromSeconds(RequestTimeout);
            //request.Send();
            WebResponse reponse = request.GetResponse();
            onFinish(request, reponse);
        }

        /// <summary>
        /// License check finished, check response and either load locally or validate.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="response">The http response for the request.</param>
        private void OnlineRequest_Finished(WebRequest/*HTTPRequest*/ request, WebResponse/*HTTPResponse*/ response)
        {
            var res = (HttpWebResponse)response;
            try
            {
                if (res.StatusCode == HttpStatusCode.GatewayTimeout || response == null)
                //if (request.State == HTTPRequestStates.ConnectionTimedOut || response == null)
                {
                    //Debug.LogWarning("LicenseManager: No response.");
                    //_errorMessage = string.Format(TimeOutMessage, response?.StatusCode);
                    _errorMessage = string.Format(TimeOutMessage, res?.StatusCode);
                }
                else
                {
                    if (res.StatusCode != HttpStatusCode.OK)
                    //if (response.StatusCode != ValidResponseCode)
                    {
                        //Debug.LogWarning("LicenseManager: Invalid status code returned - " + response.StatusCode);
                        //_errorMessage = string.Format(InvalidResponseMessage, response?.StatusCode);
                        _errorMessage = string.Format(InvalidResponseMessage, res?.StatusCode);
                    }
                    else
                    {
                        //_appLicenseJwt = JsonConvert.DeserializeObject<AppLicenseJwt>(response.DataAsText);
                        var stream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(stream);
                        string data = reader.ReadToEnd();
                        _appLicenseJwt = JsonConvert.DeserializeObject<AppLicenseJwt>(data);
                        var payload = JwtUtilities.GetPayload(_appLicenseJwt.Token);
                        _appLicense = CreateAndSaveLicenses(payload);
                    }
                }
            }
            catch (Exception e)
            {
                //Debug.LogWarning("Exception while processing license response: " + e);
                _errorMessage = string.Format(InvalidResponseMessage, "error while processing response");
            }

            var validLicense = CheckLocalLicense();
        }

        private void OnlineRequest_Finished(string responce)
        {
            _appLicenseJwt = JsonConvert.DeserializeObject<AppLicenseJwt>(responce);
            var payload = JwtUtilities.GetPayload(_appLicenseJwt.Token);
            _appLicense = CreateAndSaveLicenses(payload);
        }
        
        /// <summary>
        /// Checks if a local license file is available and if it's still valid.
        /// Does check both the jwt signature and the actual license validity and expiration.
        /// </summary>
        /// <returns><c>true</c> if a valid license is found, otherwise <c>false</c>.</returns>
        private bool CheckLocalLicense()
        {
            if (string.IsNullOrEmpty(_appLicense?.Until) || string.IsNullOrEmpty(_appLicenseJwt?.Token))
            {
                return false;
            }

            var verifiedSignature = JwtUtilities.VerifySignature(_appLicenseJwt.Token, PubKeyText, HashAlgorithm);
            if (!verifiedSignature)
            {
                return false;
            }

            var validUntilString = _appLicense.Until;
            var isUtc = false; 

            if (validUntilString.Contains(UtcIdentifier))
            {
                validUntilString = validUntilString.Replace(UtcIdentifier, string.Empty);
                isUtc = true;
            }

            var validUntil = DateTime.ParseExact(validUntilString, DateFormat, CultureInfo.InvariantCulture);
            if (isUtc)
            {
                validUntil = validUntil.ToUniversalTime();
            }

            var currentTime = DateTime.Now;

            if (DateTime.Compare(currentTime, validUntil) > 0)
            {
                //Debug.LogWarning("License expired: " + validUntil);
                return false;
            }

            return _appLicense.Licensed;
        }

        /// <summary>
        /// Tries to deserialize the license from local storage and to convert it from jwt.
        /// </summary>
        private void LoadLocalLicense()
        {
            _licenseFileUri = Path.Combine(PathHelper.GetApplicationWorkingDirectory(), FileName);

            if (!File.Exists(_licenseFileUri/*.OriginalUri*/))
            {
                return;
            }

            using (var stream = new FileStream (_licenseFileUri, FileMode.OpenOrCreate)/*_resourceManager.GetStream(_licenseFileUri)*/)
            {
                if (stream != null)
                {
                    
                    var reader = new StreamReader(stream);
                    var localFileText = reader.ReadToEnd();
                    _appLicenseJwt = JsonConvert.DeserializeObject<AppLicenseJwt>(localFileText);
                    var payload = JwtUtilities.GetPayload(_appLicenseJwt.Token);
                    _appLicense = JsonConvert.DeserializeObject<AppLicense>(payload);
                }
            }
        }

        /// <summary>
        /// Creates a new app license object from given JWT payload and saves the jwt json to disc.
        /// </summary>
        /// <param name="body">JSON string containing the app license object informations in JWT.</param>
        /// <returns>The newly created license object.</returns>
        private AppLicense CreateAndSaveLicenses(string payload)
        {
            var licenseObject = JsonConvert.DeserializeObject<AppLicense>(payload);

            if (licenseObject.Licensed)
            {
                _licenseFileUri = Path.Combine(UsersHelper.GetUserFolder(), FileName);
                File.WriteAllText(_licenseFileUri/*.OriginalUri*/, JsonConvert.SerializeObject(_appLicenseJwt));
            }
            else
            {
                _errorMessage = string.Format(RejectedRequestMessage, licenseObject?.Details);
            }

            return licenseObject;
        }
    }
}