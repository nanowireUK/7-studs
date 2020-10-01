using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionLeave' Class  
    /// </summary>  
    public class ActionLeave : Action
    {  
        public ActionLeave(string connectionId, ActionEnum actionType, string gameId, string user, string leavers) 
            : base(connectionId, actionType, gameId, user, leavers)
        {
        }

        public override void ProcessAction()
        {
            // A player can leave at any time with the following immediate effect:
            //    1) They will be sent a 'ReceiveLeavingConfirmation' message which the client should handle by terminating its connection
            //    2) They will no longer be sent any updates from the game (which will have closed the SignalR group that the player was associated with)
            //    3) If they were the administrator then someone else will be randomly nominated to be the administrator
            //    4) If they were the last player in the game then the game will be closed/deleted
            //    5) The game will increment the count of players who have left (see below for how this is used)
            //    6) If they were already out of the game (i.e. with zero funds) at the start of the hand, then:
            //       a) note the fact that they have left the game, and
            //       b) remove them when the hand ends
            //    7) If they are still in the current hand: 
            //       a) behave as if they had folded (regardless of whether or not it is their turn) and determine next player as usual 
            //       b) remove them from the game completely when the hand ends
            //
            // To prevent clashes between leave actions and any other actions:
            //
            //    1) Whenever an action completes and the new game status is sent to each player, it should include the count of players who have left
            //    2) Every action that is sent to the server should include the count
            //    3) Before processing an action, the server will check that the counts are the same and reject the action (with an exception) if they are not

            // If this was the last player, remove the game from the system and notify the player via an exception that the game is now gone
            if ( G.Participants.Count == 0 ) {
                Game.EraseGame(G.GameId);
                throw new HubException("You were the last player to leave the game so the game has now been deleted");
            }  

            Participant p = G.Participants[PlayerIndex];
            p.HasDisconnected = true;
            G.CountOfLeavers++; 
            
            string changeOfAdminMessage = "";
            if ( p.IsGameAdministrator ) {
                // Randomly nominate first available active player as administrator
                foreach ( Participant p1 in G.Participants ) {
                    if ( p1.HasDisconnected == false ) {
                        p1.IsGameAdministrator = true;
                        changeOfAdminMessage = ". " + p1.Name + " is new game administrator";
                        break;
                    }
                }
            }

            if ( G.IndexOfParticipantDealingThisHand == PlayerIndex ) {
                // Make the next free player the dealer
                G.IndexOfParticipantDealingThisHand = 0;
            }

            if ( p.IsOutOfThisGame == false && p.HasFolded == false ) {
                // Player was still in the game in some form, so we have to treat this as if they folded
                G.RecordLastEvent(UserName + " has left the game and effectively folded"+ changeOfAdminMessage);
                // Implement the Fold
                p.HasFolded = true;
                // Find and set next player (could be no one if all players have now folded)
                G.SetNextPlayerToActOrHandleEndOfHand(G.IndexOfParticipantToTakeNextAction, G.LastEvent);    
            }
            else {
                // Player was already out so no real consequence unless someone else has to be nominated to be the game administrator
                G.RecordLastEvent(p.Name + " has left the game"+changeOfAdminMessage);
            }

            // Note: we don't explicitly change the next action. It will either stay as it was or be updated as a result of the fold
            //G.NextAction = "..."; 

            // Set the response type that will trigger the player's client session to disconnect
            SignalRGroupNameForAdditionalNotifications = p.ParticipantLevelSignalRGroupName;
            ResponseType = ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers;           
        }
    }     
}  