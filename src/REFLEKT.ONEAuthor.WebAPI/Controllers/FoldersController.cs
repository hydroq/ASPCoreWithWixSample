using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application;
using REFLEKT.ONEAuthor.Application.Authorization;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using REFLEKT.ONEAuthor.WebAPI.Controllers.Base;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class FoldersController : ReflektBaseController
    {
        private readonly IAuthenticationManager _authenticationManager;

        public FoldersController(IAuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        [HttpPost]
        [Route("api/folders/{folder_id}/import")]
        public ImportedScenarioModel Import(string folder_id, [FromBody] ApiFormInput formData)
        {
            ImportedScenarioModel model = new ImportedScenarioModel();

            if (!formData.local)
            {
                string fileName = formData.filename;
                string base64 = formData.file;

                formData.file = Path.Combine(PathHelper.GetViewerImagesFolder(), fileName);

                byte[] fileContent = ApiHelper.DecodeBase64(base64);
                IOFile.WriteAllBytes(formData.file, fileContent);
            }

            if (string.IsNullOrEmpty(formData.ticket))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (string.IsNullOrEmpty(formData.file))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "File is required" });
                return model;
            }

            if (!PathHelper.CheckIfStringIsPathCompliant(Path.GetFileName(formData.file)))
            {
                model.errors = ApiHelper.JsonError(400, new[] { "Invalid file name provided" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            formData.file = formData.file.Replace("file:///", "");
            formData.file = formData.file.Replace("file://", "");
            string ret = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "folder not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }
            if (!IOFile.Exists(formData.file))
            {
                //return ApiHelper.JsonError(400, new string[] { "file not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "file not exist" });
                return model;
            }

            if (!string.IsNullOrEmpty(ret))
            {
                ret = ret.Replace(PathHelper.GetRepoPath(), "").Replace("\\", "/");
                Exporter.Import(formData.file, ret, formData.overRide, (res) =>
                {
                    model = res;
                });
            }

            SVNManager.SaveChanges();
            return model;
        }

        public string Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        [HttpPost]
        [Route("api/folders")]
        public FolderModel CreateCategory([FromBody] ApiFormInput formData)
        {
            FolderModel result = new FolderModel();
            if (formData == null)
            {
                result.errors = ApiHelper.JsonError(400, new[] { "error" });
                return result;
            }

            if (string.IsNullOrWhiteSpace(formData.name))
            {
                result.errors = ApiHelper.JsonError(400, new[] { "Incorrect name" });
                return result;
            }

            if (string.IsNullOrWhiteSpace(formData.name))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return result;
            }

            if (formData.name.Length > 50)
            {
                result.errors = ApiHelper.JsonError(400, new[] { "Name is too long" }); //TODO: change all instances to ReflektBaseController.CustomBadRequestResult<T>()
                return result;
            }
            
            result = ApiHelper.CreateFolder("", formData.name);
            if (result.folder != null)
                result.folder.modified_utc = Scenarios.ConvertToUnixTimestamp(DateTime.Now);
            SVNManager.SaveChanges();
            return result;
        }

        [HttpPost]
        [Route("api/folders/{folder_id}/folders")]
        public FolderModel CreateProduct(string folder_id, [FromBody] ApiFormInput formData)
        {
            FolderModel result = new FolderModel();
            if (formData == null)
            {
                result.errors = ApiHelper.JsonError(400, new[] { "error" });
                return result;
            }

            if (string.IsNullOrWhiteSpace(formData.name))
            {
                result.errors = ApiHelper.JsonError(400, new[] { "Incorrect name" });
                return result;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                result.errors = ApiHelper.JsonError(400, new[] { "wrong token" });
                return result;
            }

            if (formData.name.Length > 50)
            {
                result.errors = ApiHelper.JsonError(400, new[] { "Name are too long" });
                return result;
            }
            
            result = ApiHelper.CreateFolder(folder_id, formData.name);

            if (result.folder != null)
                result.folder.modified_utc = Scenarios.ConvertToUnixTimestamp(DateTime.Now);

            SVNManager.SaveChanges();

            return result;
        }

        [HttpGet]
        [Authorize]
        [Route("api/folders/{folder_id}")]
        public FolderModel GetFolderInfo(string folder_id, string ticket = "")
        {
            FolderModel result = new FolderModel();
            string ret = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "folder not exist" });
                result.errors = ApiHelper.JsonError(400, new string[] { "folder not exist" });
                return result;
            }

            result = ApiHelper.GetFolderInfo(folder_id, Path.Combine(PathHelper.GetRepoPath(), ret));
            return result;
        }

        [HttpPatch]
        [Route("api/folders/{folder_id}")]
        //public string UpdateFolder(string folder_id, string folder, string ticket = "")
        public FolderModel UpdateFolder(string folder_id, [FromBody] ApiFormInput formData)
        {
            FolderModel result = new FolderModel();
            if (formData == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "empty body" });
                result.errors = ApiHelper.JsonError(400, new string[] { "empty body" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                result.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.folder.name))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "Incorrect name" });
                return result;
            }

            if (string.IsNullOrEmpty(formData.folder.name))
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "Folder name are empty" });
                return result;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return result;
            }

            if (formData.folder.name.Length > 50)
            {
                result.errors = ApiHelper.JsonError(400, new string[] { "Name are too long" });
                return result;
            }

            string ret = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "folder not exist" });
                result.errors = ApiHelper.JsonError(400, new string[] { "folder not exist" });
                return result;
            }

            try
            {
                string sc = formData.folder.name;
                if (sc != null)
                {
                    string dir = new DirectoryInfo(ret).Name;
                    CategoryModel model = JsonConvert.DeserializeObject<CategoryModel>(IOFile.ReadAllText(Path.Combine(ret, dir + ".info")));
                    model.title = sc;
                    string newModel = JsonConvert.SerializeObject(model);
                    IOFile.WriteAllText(Path.Combine(ret, dir + ".info"), newModel);
                    //PathHelper.RenameFolder(ret, dir, sc);
                    ret = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());
                    result = ApiHelper.GetFolderInfo(folder_id, Path.Combine(PathHelper.GetRepoPath(), ret));

                    string oldPath = new DirectoryInfo(ret).FullName.Replace("\\", "/");
                    //SVNManager.Rename(oldPath, oldPath.Replace(dir,sc));

                    SVNManager.SaveChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                //return ApiHelper.JsonError(400, new string[] { "wrong folder object" });
                result.errors = ApiHelper.JsonError(400, new string[] { "wrong folder object" });
                return result;
            }
            return null;
        }

        [HttpDelete]
        [Authorize]
        [Route("api/folders/{folder_id}")]
        public FolderIdModel RemoveFolder(string folder_id, string ticket = "")
        {
            FolderIdModel model = new FolderIdModel();

            if (string.IsNullOrEmpty(ticket))
            {
                //return ApiHelper.JsonError(400, new string[] { "error" });
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (folder_id.Contains("^") || folder_id.Contains("@"))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "folder not exist" });
                return model;
            }

            string ret = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());

            if (ret == null)
            {
                //return ApiHelper.JsonError(400, new string[] { "folder not exist" });
                model.errors = ApiHelper.JsonError(400, new string[] { "folder not exist" });
                return model;
            }
            else
            {
                Command.RemoveFolder(ret);
                model.folder_id = folder_id;
                SVNManager.SaveChanges();

                return model;
            }
        }

        [HttpPost]
        [Route("api/folders/{folder_id}/scenarios")]
        //public string CreateScenario(string folder_id, string name, string description, string ticket = "")
        public ScenarioModel CreateScenario(string folder_id, [FromBody] ApiFormInput formData)
        {
            if (formData == null)
            {
                ScenarioModel model = new ScenarioModel();
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }

            if (string.IsNullOrWhiteSpace(formData.name))
            {
                ScenarioModel model = new ScenarioModel();
                model.errors = ApiHelper.JsonError(400, new string[] { "Incorrect name" });
                return model;
            }

            if (!_authenticationManager.CheckAccessToken(formData.ticket))
            {
                ScenarioModel model = new ScenarioModel();
                //return ApiHelper.JsonError(401, new string[] { "wrong token" });
                model.errors = ApiHelper.JsonError(400, new string[] { "wrong token" });
                return model;
            }

            if (formData.name.Length > 50)
            {
                ScenarioModel model = new ScenarioModel();
                model.errors = ApiHelper.JsonError(400, new string[] { "Name are too long" });
                return model;
            }
            
            ScenarioModel mod = ApiHelper.CreateScenarioModel(folder_id, formData.name, formData.description);

            if (mod.scenario != null)
                mod.scenario.modified_utc = Scenarios.ConvertToUnixTimestamp(DateTime.Now);

            SVNManager.SaveChanges();
            return mod;
        }

        [HttpGet]
        [Authorize]
        [Route("api/folders/{folder_id}/export")]
        public FileLinkModel ExportFolder(string folder_id, string ticket = "")
        {
            FileLinkModel model = new FileLinkModel();

            string folder = ApiHelper.FindFolderById(folder_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "folder not exist" });
                return model;
            }
            ExportType exType = ApiHelper.GetFolderInfo(folder_id, Path.Combine(PathHelper.GetRepoPath(), folder)).folder.type == FolderType.Category.ToString() ? ExportType.Category : ExportType.Product;

            folder = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");

            string scenarioContent = IOFile.ReadAllText(folder);
            PublishedScenario scenarioModel = JsonConvert.DeserializeObject<PublishedScenario>(scenarioContent);
            scenarioModel.title = scenarioModel.title.Replace(" ", "_");
            scenarioModel.title = scenarioModel.title.Replace("[", "");
            scenarioModel.title = scenarioModel.title.Replace("]", "");

            scenarioModel.title = scenarioModel.title.Replace("ü", "u");
            scenarioModel.title = scenarioModel.title.Replace("ä ", "a");
            scenarioModel.title = scenarioModel.title.Replace("ö", "o");

            string zipName = scenarioModel.title + "_" + ApiHelper.GetCurrentDate();
            zipName = zipName.Replace(":", ".");
            zipName = zipName.Replace("+", "_");
            zipName = zipName.Replace("ä ", "a");
            zipName = zipName.Replace("ö", "o");
            zipName = zipName.Replace("ü", "u");
            string zip = Path.Combine(PathHelper.GetUploadFolder(), zipName);

            Exporter.Export(zip, folder, exType);

            model.file_url = "file:///" + zip.Replace("\\", "/") + ".zip";
            return model;
        }
    }
}