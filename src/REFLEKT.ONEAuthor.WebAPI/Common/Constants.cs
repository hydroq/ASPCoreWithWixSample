namespace REFLEKT.ONEAuthor.WebAPI.Common
{
    public static class Constants
    {
        public const string ScenarioNotFound = "Requested scenario was not found in the system.";
        public const string TopicNotFound = "Requested topic was not found in the system.";
        public const string TopicIsOpened = "Cannot delete specified topic, as it's being opened in Cortona.";
        public const string InputDataIsMissing = "Input data is mising.";
        public const string CortonaNotFound = "Cortona was not found in the system, please check settings.";
        public const string ScenarioIsPublishingOrDraft = "Requested scenario is currently publishing or draft.";
        public const string NotAuthorized = "Request is not authorized, please check input ticket.";
    }
}