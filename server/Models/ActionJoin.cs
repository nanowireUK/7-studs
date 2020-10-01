namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionJoin' Class  
    /// </summary>  
    public class ActionJoin : Action
    {  
        public ActionJoin(string connectionId, ActionEnum actionType, string gameId, string user, string leavers) 
            : base(connectionId, actionType, gameId, user, leavers)
        {
        }

        public override void ProcessAction()
        {
            // Add player (note that the base class has already checked the player's basic eligibility for this action)
            Participant p = new Participant(this.UserName);
            G.Participants.Add(p);

            if ( G.Participants.Count == 1 )
            {
                p.IsGameAdministrator = true; // First player to join becomes the administrator (may find ways of changing this later)
            }
            p.NoteConnectionId(this.ConnectionId);

            G.RecordLastEvent(this.UserName + " joined game" + ( p.IsGameAdministrator ? " as administrator" : ""));
            G.NextAction = "Await new player or start the game";            
        }
    }     
}  