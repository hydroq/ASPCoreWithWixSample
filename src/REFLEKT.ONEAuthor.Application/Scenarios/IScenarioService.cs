namespace REFLEKT.ONEAuthor.Application.Scenarios
{
    public interface IScenarioService
    {
        string GetScenarioFolder(string scenarioId);

        bool CheckIfScenarioIsBusy(string scenarioId);
    }
}