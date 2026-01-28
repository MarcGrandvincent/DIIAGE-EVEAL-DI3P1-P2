namespace Diiage.QuestService.Api.Models;

public static class ApiRoutes
{
    private const string BaseRoute = "/api/v1";

    public static class Quests
    {
        public const string Base = BaseRoute + "/quests";
        public const string ById = Base + "/{id:int}";
    }
    
    public static class PlayerQuests
    {
        public const string Base = BaseRoute + "/players/{playerId:guid}/quests";
    }
    
    public static class GameService
    {
        public const string PublishEvent = BaseRoute + "/gameservice/publish-event";
    }
}