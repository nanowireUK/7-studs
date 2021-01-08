using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionLeave' Class  
    /// </summary>  
    public class ActionLeave : Action
    {  
        public ActionLeave(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }

        public override async Task ProcessAction()
        {
            // A spectator can leave at any time as they do not influence the game in any way.
            int spectatorIndex = G.SpectatorIndexFromName(UserName);
            if ( spectatorIndex != -1 )
            {
                // Remove the spectator from the game
                // Set the response type that will trigger the player's client session to disconnect and everyone else's game state to be updated
                SignalRGroupNameForAdditionalNotifications = G.Spectators[spectatorIndex].SpectatorSignalRGroupName;
                ResponseType = ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers;   
                G.LeaversLogForGame.Add(new LeavingRecord(UserName, DateTimeOffset.Now, SignalRGroupNameForAdditionalNotifications, 0, false, true));
                G.Spectators.RemoveAt(spectatorIndex);
                if ( G.GameMode == GameModeEnum.LobbyOpen ) {
                    G.LobbyData = new LobbyData(G); // Update the lobby data
                }
                return; // All done
            }

            //
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

            // If this was the last player still connected to the game, remove the game from the system 
            // and notify the player via an exception that the game is now gone
            if ( G.Participants.Count == 1 ) {
                //ServerState.EraseGame(G.GameId);
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
                        p.IsGameAdministrator = false;
                        p1.IsGameAdministrator = true;
                        changeOfAdminMessage = ". " + p1.Name + " is new game administrator";
                        break;
                    }
                }
            }

            if ( G.IndexOfParticipantDealingThisHand == PlayerIndex ) {
                // Leaving player is currently the dealer.
                // Don't think we have to do anything here because although they are now out of the game,
                // they are still notionally the dealer (i.e. hand evaluation starts with the player to their left).
                // Leaving this code in place though as a reminder to think about this when stepping through the code.
            }

            if ( p.HasBeenActiveInCurrentGame == true && p.StartedHandWithNoFunds == false && p.HasFolded == false ) {
                // Player was still in the game in some form, so we have to treat this as if they folded
                p.HasFolded = true;
                G.Participants[PlayerIndex].LastActionInThisHand = ActionEnum.Fold;
                G.Participants[PlayerIndex].LastActionAmount = 0;   
                G.Participants[PlayerIndex].RoundNumberOfLastAction = G._CardsDealtIncludingCurrent;

                // Player is now bankrupt (their contributions to the pots will remain but any uncommitted funds will be discarded at the end of the hand)
                G.LeaversLogForGame.Add(new LeavingRecord(p.Name, DateTimeOffset.Now, p.ParticipantSignalRGroupName, 0, true, false));
                if ( G.IndexOfParticipantToTakeNextAction == PlayerIndex ) {
                    // It was player's turn to move anyway, so implement the fold in the same way as if they had just folded in turn
                    G.RecordLastEvent(UserName + " has left the game and effectively folded"+ changeOfAdminMessage);
                    await G.SetNextPlayerToActOrHandleEndOfHand(G.IndexOfParticipantToTakeNextAction, G.LastEvent);   
                }
                else {
                    // They are leaving out of turn, so it will remain the turn of the current player unless the player leaving now means there is only one person left in
                    G.RecordLastEvent(UserName + " has left the game and effectively folded out of turn"+ changeOfAdminMessage);
                    if ( G.CountOfPlayersLeftInHand() == 1 ) {
                        // Everyone has folded except one player
                        G.NextAction = await G.ProcessEndOfHand(G.LastEvent + ", only one player left in, hand ended"); // will also update commentary with hand results
                        G.AddCommentary(G.NextAction);
                    }
                }
            }
            else {
                // Player was already out of the hand so no consequence to game flow (we might even be back in the lobby)
                G.LeaversLogForGame.Add(new LeavingRecord(
                    p.Name, 
                    ( 
                        ( p.HasBeenActiveInCurrentGame && p.UncommittedChips == 0 && p.AllInDateTime != DateTimeOffset.MinValue )
                        ? p.AllInDateTime
                        : (
                            p.TimeOfBankruptcy != DateTimeOffset.MinValue 
                            ? p.TimeOfBankruptcy 
                            : DateTimeOffset.Now // catch-all, don't think this is actually possible  
                        )
                    ),                     
                    p.ParticipantSignalRGroupName, 
                    p.UncommittedChips, 
                    p.HasBeenActiveInCurrentGame, 
                    false));
                G.RecordLastEvent(p.Name + " has left the game"+changeOfAdminMessage);
            }

            if ( G.GameMode == GameModeEnum.LobbyOpen ) {
                G.RemoveDisconnectedPlayersFromGameState(); // clear out disconnected players
                G.LobbyData = new LobbyData(G); // Update the lobby data
            }

            // Set the response type that will trigger the player's client session to disconnect and everyone else's game state to be updated
            SignalRGroupNameForAdditionalNotifications = p.ParticipantSignalRGroupName;
            ResponseType = ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers;   
        }
    }     
}  