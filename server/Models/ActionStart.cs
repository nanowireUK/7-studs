using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionStart' Class - used to start a new hand (note that only the administrator can do this)
    /// </summary>  
    public class ActionStart : Action
    {  
        public ActionStart(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }

        public override async Task ProcessAction()
        {
            // Start a new game (note that the base class has already checked the player's eligibility for this action)

            // THIS NEEDS TO WORK FROM A ROOM PERSPECTIVE
            // STARTING A GAME SHOULD ASSIGN PLAYERS TO IT (i.e. JOIN IS A ROOM LEVEL FUNCTION NOT A GAME LEVEL)
            // CHANGE PARTICIPANT TO PLAYER?

            // Check there are still enough connected players to start a game
            int stillIn = 0;
            foreach ( Participant p in G.Participants ) {
                if ( p.HasDisconnected == false ) {
                    stillIn++;
                }
            }
            if ( stillIn < 2 ) {
                throw new HubException("You need at least two players before you can start a game");
            }  

            if ( G.HandsPlayedIncludingCurrent > 0 ) {
                // Archive the results of the last game before setting up the new one 
                ///////R.AddCompletedGameToRoomHistory(G); 
            }
            G.RecordLastEvent(this.UserName + " started the game (player order now randomised)");
            await G.StartNewGame(); // Initialise the game
            await G.StartNextHand(); 
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.GameMode = GameModeEnum.HandInProgress;
        }
    }     
}  