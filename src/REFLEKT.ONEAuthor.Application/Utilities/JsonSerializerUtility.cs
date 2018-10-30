using Newtonsoft.Json;
using System.IO;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    public class JsonSerializerUtility : IJsonSerializerUtility
    {
        /// <summary>
        /// Reads data from target file and deserializes to target type
        /// </summary>
        /// <typeparam name="T">Type of file content</typeparam>
        /// <param name="filePath">Target file path</param>
        /// <returns>Deserialized content of the file</returns>
        public T DeserializeFromFile<T>(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// Serializes specified object to target file.
        /// Creates file if it doesn't exist, otherwise overrides existing
        /// </summary>
        /// <param name="filePath">Target file path</param>
        /// <param name="content">Object to serialize</param>
        public void SerializeToFile(string filePath, object content)
        {
            var rawContent = JsonConvert.SerializeObject(content);

            File.WriteAllText(filePath, rawContent);
        }

        public bool TryDeserializeFromFile<T>(string filePath, out T result)
        {
            try
            {
                result = DeserializeFromFile<T>(filePath);

                return true;
            }
            catch
            {
                result = default(T);

                return false;
            }
        }
    }
}