namespace REFLEKT.ONEAuthor.Application.Utilities
{
    public interface IJsonSerializerUtility
    {
        T DeserializeFromFile<T>(string filePath);

        void SerializeToFile(string filePath, object content);

        bool TryDeserializeFromFile<T>(string filePath, out T result);
    }
}