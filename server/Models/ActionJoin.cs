namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionJoin' Class  
    /// </summary>  
    public class ActionJoin : Action
    {  
        public ActionJoin(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }

        public override void ProcessAction()
        {
            // Add player (note that the base class has already checked the player's basic eligibility for this action)
            Participant p = new Participant(this.UserName);
            G.Participants.Add(p);
            G.LastEvent = this.UserName + " joined game";
            G.NextAction = "Await new player or start the game";
            if ( G.Participants.Count >= 2 )
            {
                G.SetActionAvailability(ActionEnum.Start, AvailabilityEnum.AnyRegisteredPlayer); // Open up START to anyone
            }
            p.NoteConnectionId(this.ConnectionId);
        }
    }     
}  