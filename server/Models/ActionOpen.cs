namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionOpen' Class: to initially open a game lobby, or to return to the lobby at the end of a hand  
    /// </summary>  
    public class ActionOpen : Action
    {  
        public ActionOpen(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }

        public override void ProcessAction()
        {
            // Note that this command can be used to open a new game lobby or for the administrator to reopen the lobby at the end of a hand
            if ( G.Participants.Count == 0 ) {
                // This is someone opening the game lobby for the first time
                Participant p = new Participant(this.UserName);
                G.Participants.Add(p);
                G.LastEvent = this.UserName + " created the game and opened the lobby";
                G.NextAction = "Await new player or start the game";
                p.IsGameAdministrator = true; // First player to join becomes the administrator (may need to find ways of changing this later)
                G.SetActionAvailability(ActionEnum.Join, AvailabilityEnum.AnyUnregisteredPlayer); // Open up JOIN to anyone who has not yet joined
                G.SetActionAvailability(ActionEnum.Open, AvailabilityEnum.NotAvailable); // OPEN is no longer possible as lobby is already open
                p.NoteConnectionId(this.ConnectionId);
            }
            else {
                G.GameMode = GameModeEnum.LobbyOpen;
                G.LastEvent = this.UserName + " reopened the game lobby to allow joining/leaving";
                G.NextAction = "Await players leaving/joining, or restart the game";
            }
        }
    }     
}  