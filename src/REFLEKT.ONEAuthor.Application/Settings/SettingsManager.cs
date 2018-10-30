using REFLEKT.ONEAuthor.Application.Utilities;
using System.IO;
using REFLEKT.ONEAuthor.Application.Helpers;

namespace REFLEKT.ONEAuthor.Application.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private const string CortonaConfigFileName = "cortona.config";
        private const string DefaultCortonaPath = "C:/Program Files/ParallelGraphics/RapidManual/RapidManual.exe";

        private readonly IJsonSerializerUtility _jsonSerializer;

        public SettingsManager(IJsonSerializerUtility jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public string GetRapidManualPath()
        {
            string toolsPath = PathHelper.GetUserProcessingFolder(); // TODO: move cortona.config to other path
            string cortonaConfigFullPath = Path.Combine(toolsPath, CortonaConfigFileName);

            if (File.Exists(cortonaConfigFullPath))
            {
                return _jsonSerializer.DeserializeFromFile<CortonaConfigModel>(cortonaConfigFullPath).Path;
            }

            CreateConfigFile(cortonaConfigFullPath);

            return DefaultCortonaPath;
        }

        private void CreateConfigFile(string cortonaConfigFullPath)
        {
            _jsonSerializer.SerializeToFile(cortonaConfigFullPath, new CortonaConfigModel { Path = DefaultCortonaPath });
        }
    }
}