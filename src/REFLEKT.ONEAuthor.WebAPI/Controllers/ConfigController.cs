using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using REFLEKT.ONEAuthor.Application.Settings;
using REFLEKT.ONEAuthor.WebAPI.Controllers.Base;
using System;
using System.IO;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class ConfigController : ReflektBaseController
    {
        private readonly IAuthenticationManager _authenticationManager;
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(IAuthenticationManager authenticationManager, ISettingsManager settingsManager, ILogger<ConfigController> logger)
        {
            _authenticationManager = authenticationManager;
            _settingsManager = settingsManager;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [Route("api/config/cortona")]
        public CortonaPathModel GetPath()
        {
            try
            {
                var result = new CortonaPathModel { path = _settingsManager.GetRapidManualPath() };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while retrieving Cortona path");

                return CustomInternalServerResult<CortonaPathModel>(ex.Message);
            }
        }

        [HttpPatch]
        [Route("api/config/cortona")]
        public CortonaPathModel SetPath([FromBody] ApiFormInput formData)
        {
            var model = new CortonaPathModel();

            if (formData == null)
            {
                model.errors = ApiHelper.JsonError(400, new[] { "empty body" });

                return model;
            }
            
            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                model.errors = ApiHelper.JsonError(400, new[] { "wrong token" });

                return model;
            }

            var targetCortonaFile = new FileInfo(formData.path);
            if (targetCortonaFile.Name != "RapidManual.exe")
            {
                model.errors = ApiHelper.JsonError(400, new[] { "Cortona path should be pointing to RapidManual.exe file" });

                return model;
            }

            string toolsPath = PathHelper.GetUserProcessingFolder();

            CortonaConfigModel config = JsonConvert.DeserializeObject<CortonaConfigModel>(IOFile.ReadAllText(Path.Combine(toolsPath, "cortona.config")));
            config.Path = formData.path;
            model.path = formData.path;
            IOFile.WriteAllText(Path.Combine(toolsPath, "cortona.config"), JsonConvert.SerializeObject(config));

            return model;
        }

        [Authorize]
        [HttpGet]
        [Route("api/config/xsp")]
        public XSPConfigModel GetXSP()
        {
            XSPConfigModel model = new XSPConfigModel();

            string toolsPath = PathHelper.GetUserProcessingFolder();

            var configFilePath = Path.Combine(toolsPath, "xsp.config");
            if (!IOFile.Exists(configFilePath))
            {
                IOFile.WriteAllText(configFilePath, JsonConvert.SerializeObject(new XSPConfigModel { port = 0 }));
            }

            model = JsonConvert.DeserializeObject<XSPConfigModel>(IOFile.ReadAllText(configFilePath));

            return model;
        }

        [HttpPatch]
        [Route("api/config/xsp/{port}")]
        public XSPConfigModel GetXSP(int port)
        {
            string toolsPath = PathHelper.GetUserProcessingFolder();

            var model = JsonConvert.DeserializeObject<XSPConfigModel>(IOFile.ReadAllText(Path.Combine(toolsPath, "xsp.config")));
            model.port = port;

            IOFile.WriteAllText(Path.Combine(toolsPath, "xsp.config"), JsonConvert.SerializeObject(model));

            return model;
        }

        [Authorize]
        [HttpGet]
        [Route("api/config/version")]
        public VersionModel GetVersion(string ticket = "")
        {
            var model = new VersionModel { version = UsersHelper.GetVersion() };

            return model;
        }
    }
}