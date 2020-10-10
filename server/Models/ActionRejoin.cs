using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionRejoin' Class  
    /// </summary>  
    public class ActionRejoin : Action
    {  
        public ActionRejoin(string connectionId, ActionEnum actionType, string gameId, string user, string leavers, string rejoinCode) 
            : base(connectionId, actionType, gameId, user, "-1", rejoinCode)
        {
        }

        public override void ProcessAction()
        {
            // Add a new connection for the rejoining player (note that the base class has already checked the player's basic eligibility for this action)

            Participant p = G.Participants[this.PlayerIndex];

            // Check that player has supplied the correct rejoin code
            if ( this.Parameters != p.RejoinCode ) {
                throw new HubException("You attempted to rejoin using an invalid rejoin code");
            }

            // For rejoin, do not change the last event or the next action
            //G.RecordLastEvent( "(" + this.UserName + " rejoined game) " + G.LastEvent); // Prepend the rejoin to the previous last event

            p.NoteConnectionId(this.ConnectionId);
        }
    }     
}  