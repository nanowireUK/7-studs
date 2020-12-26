using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionJoin' Class  
    /// </summary>  
    public class ActionJoin : Action
    {  
        public ActionJoin(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, "-1")
        {
        }

        public override async Task ProcessAction()
        {
            G.RemoveDisconnectedPlayersFromGameState(); // clear out disconnected players to possibly make way for new joiner

            // First make sure the limit of 8 players is not exceeded
            if ( G.Participants.Count == 8 ) {
                throw new HubException("This game already has the maximum of 8 registered players");
            }

            // Add player (note that the base class has already checked the player's basic eligibility for this action)
            Participant p = new Participant(this.UserName);
            G.Participants.Add(p);

            // If the player is joining a game that is already in progress, ensure they are allocated chips
            if ( G.HandsPlayedIncludingCurrent > 0 ) {
                p.UncommittedChips = G.InitialChipQuantity;
                G.Pots[0].Add(0); // Add a new empty position to pot 0 (all players should have 0 chips in the pot at this stage)
                p.Hand = new List<Card>();
            }

            if ( G.Participants.Count == 1 )
            {
                p.IsGameAdministrator = true; // First player to join becomes the administrator (may find ways of changing this later)
            }
            p.NoteConnectionId(this.ConnectionId);
            G.RecordLastEvent(this.UserName + " joined game" + ( p.IsGameAdministrator ? " as administrator" : ""));
            G.NextAction = "Await new player or start the game";

            G.LobbyData = new LobbyData(G); // Update the lobby data

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  