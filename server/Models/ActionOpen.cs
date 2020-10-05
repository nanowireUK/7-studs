namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionOpen' Class: to initially open a game lobby, or to return to the lobby at the end of a hand  
    /// </summary>  
    public class ActionOpen : Action
    {  
        public ActionOpen(string connectionId, ActionEnum actionType, string gameId, string user, string leavers) 
            : base(connectionId, actionType, gameId, user, leavers)
        {
        }

        public override void ProcessAction()
        {
            // Note that this command is used to reopen the lobby at the end of a hand
            G.RecordLastEvent(this.UserName + " reopened the game lobby");
            G.NextAction = "Await players leaving/joining, or continue the game, or start a new one";
            G.GameMode = GameModeEnum.LobbyOpen;
            G.RemoveDisconnectedPlayersFromGameState(); // clear out disconnected players
            G.LobbyData = new LobbyData(G); // Update the lobby data
        }
    }     
}  