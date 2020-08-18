namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionRejoin' Class  
    /// </summary>  
    public class ActionRejoin : Action
    {  
        public ActionRejoin(string connectionId, ActionEnum actionType, string gameId, string user, string rejoinCode) 
            : base(connectionId, actionType, gameId, user, rejoinCode)
        {
        }

        public override void ProcessAction()
        {
            // Add a new connection for the rejoining player (note that the base class has already checked the player's basic eligibility for this action)

            Participant p = G.Participants[this.PlayerIndex];

            // Check that player has supplied the correct rejoin code
            if ( this.Parameters != p.RejoinCode ) {
                // This connection is already being used by someone else
                G.LastEvent = this.UserName + " attempted to rejoin using an invalid rejoin code";
                return;
            }

            G.LastEvent = this.UserName + " rejoined game";
            // G.NextAction not changing as a result of rejoin ... is this enough, or do we have to set it again?

            p.NoteConnectionId(this.ConnectionId);
            //G.LinkConnectionToParticipant(this.ConnectionId, p); /// NOTE this is now done in ChatHub, not here
        }
    }     
}  