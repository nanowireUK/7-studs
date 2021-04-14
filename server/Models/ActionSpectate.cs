using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPokerClub.Models
{
    /// <summary>
    /// The 'ActionSpectate' Class
    /// </summary>
    public class ActionSpectate : Action
    {
        public ActionSpectate(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers)
            : base(connectionId, actionType, ourGame, user, "-1")
        {
        }
        public override async Task ProcessAction()
        {
            // Add player as a spectator, i.e. they will not have a place at the table and will only see cards that are currently face up
            // (note that the base class has already checked the player's basic eligibility for this action)

            if ( G.AcceptNewSpectators == false ) {
                throw new HubException(SpcExceptionCodes.RoomNotAcceptingSpectators.ToString());
            }
            Spectator p = new Spectator(this.UserName);
            G.Spectators.Add(p);
            p.NoteConnectionId(this.ConnectionId);
            G.LobbyData = new LobbyData(G); // Update the lobby data
            // Note: the following are the default response types, but showing here for clarity
            // (there is no change to the game state but everyone will see that a spectator has joined and the spectator gets the current game state)
            ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // Default response type for actions
            ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // Default audience for action response
            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }
}