using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionLeave' Class  
    /// </summary>  
    public class ActionLeave : Action
    {  
        public ActionLeave(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }

        public override void ProcessAction()
        {
            // Remove the player from the game
            // and if they were the administrator then choose someone else
            // and if they were the last player in the game then remove the game

            // Note whether player was admin and how many chips they had
            Participant p = G.Participants[PlayerIndex];
            bool playerWasAdministrator = p.IsGameAdministrator;
            int playersFunds = p.UncommittedChips;
            string deletedPlayersName = p.Name;

            // Remove all traces of the player from the game
            G.Participants.RemoveAt(PlayerIndex);
            for (int i = 0; i < G.Pots.Count; i++) {
                G.Pots[i].RemoveAt(PlayerIndex); // Remove this player's slot from the pot array (the pot should be empty at this point anyway)
            }

            // If this was the last player, remove the game from the system and notify the player via an exception that the game is now gone
            if ( G.Participants.Count == 0 ) {
                Game.EraseGame(G.GameId);
                throw new HubException("You were the last player to leave the game so the game has been deleted");
            }    

            string changeOfAdminMessage = "";
            if ( playerWasAdministrator )
            {
                // Randomly nominate player 0 as the administrator now
                G.Participants[0].IsGameAdministrator = true;
                changeOfAdminMessage = G.Participants[0].Name + " is new game administrator. ";
            }
            G.RecordLastEvent(deletedPlayersName + " left the game. " + changeOfAdminMessage);
            G.NextAction = "Await new player or start the game";            
        }
    }     
}  