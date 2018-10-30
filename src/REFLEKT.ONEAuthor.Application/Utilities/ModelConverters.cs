using REFLEKT.ONEAuthor.Application.Models;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    /// <summary>
    /// Module responsible for all models conversions within the system
    /// </summary>
    public static class ModelConverters
    {
        public static TopicObject ConvertToDto(this TopicModel source)
        {
            return new TopicObject
            {
                id = source.index.ToString(),
                name = source.name,
                type = source.type,
                tracking_config_url = source.pathToZip ?? source.pathToZipDEFAULT,
                unique_id = source.unique_id
            };
        }
    }
}