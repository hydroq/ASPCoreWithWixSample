using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using REFLEKT.ONEAuthor.Application;
using REFLEKT.ONEAuthor.Application.Helpers;
using REFLEKT.ONEAuthor.Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using IOFile = System.IO.File;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class HistoryController : Controller
    {
        [HttpGet]
        [Authorize]
        [Route("api/history/commit")]
        public CommitModel Commit()
        {
            SVNManager.ResolveConflicts((conRes) =>
            {
                SVNManager.RemoveMissingFiles((r) =>
                {
                    SVNManager.Changelist("**", (res) =>
                    {
                        SVNManager.Commit("update", (ress) =>
                        {
                            SVNManager.UpdateLocalRepo((resss) =>
                            {
                                if (IOFile.Exists(Path.Combine(UsersHelper.GetUserFolder(), "missing.list")))
                                {
                                    IOFile.Delete(Path.Combine(UsersHelper.GetUserFolder(), "missing.list"));
                                }
                            });
                        });
                    });
                });
            });

            return new CommitModel();
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/commit/status")]
        public CommitStatusModel CommitStatus(string ticket = "")
        {
            CommitStatusModel model = new CommitStatusModel();
            
            model.status = IOFile.Exists(Path.Combine(UsersHelper.GetUserFolder(), "missing.list")) ? "commiting" : "commited";

            return model;
        }

        [HttpGet]
        [Route("api/history/")]
        public HistoryVersionsModel Test()
        {
            HistoryVersionsModel model = new HistoryVersionsModel();
            SVNManager.Changelist("**", (res) =>
            {
                SVNManager.Commit("update", (ress) =>
                {
                    SVNManager.UpdateLocalRepo((resss) =>
                    {
                        string s = resss;
                    });
                });
            });
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/{scenario_id}/versions")]
        public HistoryVersionsModel GetVersions(string scenario_id, string ticket = "", string topic_id = "")
        {
            HistoryVersionsModel model = new HistoryVersionsModel();
            List<int> revisions = new List<int>();

            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            string infoPath = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");

            if (!string.IsNullOrEmpty(topic_id))
            {
                List<TopicModel> topics = ApiHelper.GetTopicsOrigin(scenario_id);
                foreach (TopicModel t in topics)
                {
                    if (t.index.ToString() == topic_id)
                    {
                        infoPath = t.vmpPath;
                        break;
                    }
                }
            }

            model.versions = revisions;

            SVNManager.GetCurrentRevisionFile(infoPath, (res) =>
            {
                res = res.Replace("Revision:", "");
                res = res.Replace("\r\n", "");
                try
                {
                    model.current_version = int.Parse(res);
                }
                catch (Exception e)
                {
                }
            });

            SVNManager.GetAllRevisionsForFile(infoPath, (res) =>
            {
                res = res.Replace("------------------------------------------------------------------------", "-_-_-");
                string[] splits = res.Split(new string[] { "-_-_-" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in splits)
                {
                    string[] revisionParts = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (revisionParts.Length > 3)
                    {
                        try
                        {
                            int revisionNumber = int.Parse(revisionParts[0].Replace("r", ""));
                            string[] timeAndDate = revisionParts[2].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                            string revisionText = $"{revisionNumber} - {timeAndDate[0]} {timeAndDate[1]}  {revisionParts[1]}";
                            revisions.Add(revisionNumber);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                if (revisions != null && revisions.Count > 0)
                {
                    model.current_version = revisions[0];
                }
                model.versions = revisions;
            });

            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/{scenario_id}/description/version/{version_number}")]
        public HistoryVersionModel GetScenarioVersion(string scenario_id, string version_number, string ticket = "", string topic_id = "")
        {
            HistoryVersionModel model = new HistoryVersionModel();

            if (string.IsNullOrEmpty(ticket))
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "error" });
                return model;
            }
            
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            string infoPath = Path.Combine(folder, new DirectoryInfo(folder).Name + ".info");

            if (!string.IsNullOrEmpty(topic_id))
            {
                List<TopicModel> topics = ApiHelper.GetTopicsOrigin(scenario_id);
                foreach (TopicModel t in topics)
                {
                    if (t.index.ToString() == topic_id)
                    {
                        infoPath = t.infoPath;
                        break;
                    }
                }
            }

            Diff diff = GetRevisionDiff(infoPath, int.Parse(version_number));
            model.current = diff.current;//.Replace("\",","\",\r\n").Replace("{", "{\r\n").Replace("}", "\r\n}").Replace("[", "\r\n[\r\n").Replace("]", "\r\n]");
            model.requested = diff.requested;//.Replace("\",", "\",\r\n").Replace("{", "{\r\n").Replace("}", "\r\n}").Replace("[", "\r\n[\r\n").Replace("]", "\r\n]");
            model.scenario_id = scenario_id;

            //return "{current:\"" + current + "\",requested:\"" + requested + "\",scenario_id:\"" + scenario_id + "\"}";
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/{scenario_id}/topics/{topic_id}/version/{version_number}")]
        public HistoryTopicVersion GetTopicVersion(string scenario_id, string topic_id, string version_number, string ticket = "")
        {
            HistoryTopicVersion model = new HistoryTopicVersion();
            
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            string topicsFolder = Path.Combine(folder, "topics");
            foreach (string t in Directory.GetFiles(topicsFolder))
            {
                TopicModel topic = JsonConvert.DeserializeObject<TopicModel>(IOFile.ReadAllText(t));
                if (topic.index.ToString() == topic_id)
                {
                    Diff diff = GetRevisionDiff(t, int.Parse(version_number));
                    model.current = diff.current;
                    model.requested = diff.requested;
                    model.topic_id = topic_id;
                    model.topic_type = topic.type;
                    model.topic_name = topic.name;

                    //return "{topic_id:\"" + topic_id + "\",topic_type:\"" + topic.type + "\",topic_name:\"" + topic.name + "\",current:\"" + current + "\",requested:\"" + requested + "\"}";
                    return model;
                }
            }

            //return JsonConvert.SerializeObject(ApiHelper.JsonError(400, new string[] { "not found topic" }));
            model.errors = ApiHelper.JsonError(400, new string[] { "not found topic" });
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/{scenario_id}/version/{version_number}")]
        public HistoryScenarioVersion SetScenarioVersion(string scenario_id, string version_number, string ticket = "", string topic_id = "")
        {
            HistoryScenarioVersion model = new HistoryScenarioVersion();
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            }

            string topicInfoFile = string.Empty;

            if (!string.IsNullOrEmpty(topic_id))
            {
                List<TopicModel> topics = ApiHelper.GetTopicsOrigin(scenario_id);
                foreach (TopicModel t in topics)
                {
                    if (t.index.ToString() == topic_id)
                    {
                        folder = t.vmpPath;
                        topicInfoFile = t.infoPath;
                        break;
                    }
                }
            }

            int version = int.Parse(version_number);
            SVNManager.RevertToRevision(version, folder, (res) =>
            {
                try
                {
                    List<TopicObject> topics = ApiHelper.GetTopics(scenario_id);
                    topics = topics.OrderBy(t => t.id).ToList();

                    string ret = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());
                    string folderr = new DirectoryInfo(ret).Name;
                    DescModel modell = JsonConvert.DeserializeObject<DescModel>(IOFile.ReadAllText(Path.Combine(ret, folderr + ".info")));
                    model.topics = topics;
                    model.scenario = modell;
                }
                catch
                {
                }

                if (string.IsNullOrEmpty(topicInfoFile))
                {
                    SVNManager.RevertToRevision(version, topicInfoFile, (r) => { });
                }

                model.current_version = version;
                model.message = res;
            });
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/{scenario_id}/last")]
        public HistorySwitchVersion SetLastversion(string scenario_id, string ticket = "", string topic_id = "")
        {
            HistorySwitchVersion model = new HistorySwitchVersion();
            
            string folder = ApiHelper.FindFolderById(scenario_id, PathHelper.GetRepoPath());

            if (folder == null)
            {
                model.errors = ApiHelper.JsonError(400, new string[] { "scenario not exist" });
                return model;
            };

            string topicInfoFile = string.Empty;

            if (!string.IsNullOrEmpty(topic_id))
            {
                List<TopicModel> topics = ApiHelper.GetTopicsOrigin(scenario_id);
                foreach (TopicModel t in topics)
                {
                    if (t.index.ToString() == topic_id)
                    {
                        folder = t.vmpPath;
                        topicInfoFile = t.infoPath;
                        break;
                    }
                }
            }

            SVNManager.RevertToLastVersion("./", (res) =>
            {
                model.message = res;
            });
            return model;
        }

        [HttpGet]
        [Authorize]
        [Route("api/history/version/{version_number}")]
        public HistorySwitchVersion SetVersionRepo(string version_number, string ticket = "")
        {
            HistorySwitchVersion model = new HistorySwitchVersion();
            int version = int.Parse(version_number);
            SVNManager.RevertToRevision(version, "./", (res) =>
            {
                model.current_version = version;
                model.message = res;
            });
            return model;
        }

        private Diff GetRevisionDiff(string pathForComparer, int revision)
        {
            Diff diff = new Diff();

            SVNManager.GetInfo((res) =>
            {
                int currentRevision = 0;
                string[] splits = res.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in splits)
                {
                    if (s.StartsWith("Last Changed Rev: ", StringComparison.Ordinal))
                    {
                        currentRevision = int.Parse(s.Replace("Last Changed Rev: ", ""));
                    }
                    if (s.StartsWith("Revision:", StringComparison.Ordinal))
                    {
                        currentRevision = int.Parse(s.Replace("Revision: ", ""));
                    }
                }

                SVNManager.GetFileInRevision(revision, pathForComparer.Replace("\\", "/").Replace(PathHelper.GetRepoPath().Replace("\\", "/") + "/", ""), (cont) =>
                {
                    diff.current = IOFile.ReadAllText(pathForComparer);
                    diff.requested = cont;
                });
            });

            return diff;
        }

        /*public class RevisionsContainer
        {
            public List<int> versions;
        }*/

        public class Diff
        {
            public string current;
            public string requested;
        }
    }
}