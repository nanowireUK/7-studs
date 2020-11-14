using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionSpectate' Class  
    /// </summary>  
    public class ActionSpectate : Action
    {  
        public ActionSpectate(string connectionId, ActionEnum actionType, string gameId, string user, string leavers) 
            : base(connectionId, actionType, gameId, user, "-1")
        {
        }

        public override void ProcessAction()
        {
            // Add player as a spectator, i.e. they will not have a place at the table and will only see cards that are currently face up
            // (note that the base class has already checked the player's basic eligibility for this action)
            Spectator p = new Spectator(this.UserName);
            G.Spectators.Add(p);
            p.NoteConnectionId(this.ConnectionId);
            G.LobbyData = new LobbyData(G); // Update the lobby data
            // Note: the following are the default response types, but showing here for clarity
            // (there is no change to the game state but everyone will see that a spectator has joined and the spectator gets the current game state) 
            ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // Default response type for actions
            ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // Default audience for action response   
        }
    }     
}  