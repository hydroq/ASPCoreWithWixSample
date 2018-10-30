using REFLEKT.ONEAuthor.Application.Models;
using System.Collections.Generic;

namespace REFLEKT.ONEAuthor.Application.Topics
{
    public interface ITopicService
    {
        void EditTopic(string scenarioId, bool uniqueId, string topicId);

        IEnumerable<TopicModel> GetEditingTopicsFromFolder(string topicsFolderName);
    }
}