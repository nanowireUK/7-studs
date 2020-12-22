using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionReplay' Class  
    /// </summary>  
    public class ActionReplay : Action
    {  
        public ActionReplay(string connectionId, ActionEnum actionType, string roomId, string user, string leavers, string logAsJson) 
            : base(connectionId, actionType, roomId, user, leavers, logAsJson)
        {
        }
        public override async Task ProcessAction()
        {
            // ActionReplay works in three related modes:
            // - If a game log is supplied as a parameter, a new game will be created and replayed from that log
            //   - If a 'pauseAfter' attribute is included in that game log, the replay will be paused after any action with a number >= the pauseAfter number
            // - If no parameter is passed, the replay will process the next unprocessed action
            // - If a numeric parameter is passed, it will effectively reset the pauseAfter value (i.e. it is like a continue to step 'n')
            // Note that if any player (who has rejoined the game in the meantime) make a move in the UI, this will exit the 'replay' mode and play will continue as normal

            GameLog replayContext = G.GetReplayContext(); // Can be null at this point
            ReplayModeEnum replayMode;
            int advanceToActionNumber = 0;
            if ( this.Parameters != null && this.Parameters.Length >= 5 ) {
                // Parameter is neither blank, nor short enough to be a number, so assume we are handling a new game log
                replayMode = ReplayModeEnum.NewGameLog; 
            }
            else if ( this.Parameters == null || this.Parameters == "" ) {
                // Parameter is blank, so assume user is stepping through an existing replay one step at a time
                replayMode = ReplayModeEnum.AdvanceOneStep;
                if ( G.IsRunningInReplayMode() == false ) {
                    throw new HubException("Stepwise Replay (i.e. without supplying a GameLog) is not possible at this stage as there is no current replay context");
                }
            }
            else {
                // Only other option is a numeric parameter, which would be a request to advance an existing replay to a given action number (inclusive)
                replayMode = ReplayModeEnum.AdvanceToNamedStep;
                if ( int.TryParse(Parameters, out advanceToActionNumber) == false ) {
                    throw new HubException("A numeric action number is required by the Replay(AdvanceTo) action");
                }
                if ( G.IsRunningInReplayMode() == false ) {
                    throw new HubException("Advancing to a given step is not possible at this stage as there is no current replay context");
                }
                // if ( advanceToActionNumber <= replayContext.indexOfLastReplayedAction ) {
                //     throw new HubException("Requested target step (" + advanceToActionNumber + ") is not after the currently reached step (" + replayContext.indexOfLastReplayedAction + ")");
                // }  
            }

            bool actionSucceeded;
            int inconsistenciesFound = 0;

            if ( replayMode == ReplayModeEnum.NewGameLog ) {
                // Handle the most complicated scenario of taking a game log and playing it through to the end or to the action number named in the log in the pauseAfter attribute 
                ResponseType = ActionResponseTypeEnum.OverallGameState; // On completion of replay, the tester will have the overall game state returned to them
                ResponseAudience =  ActionResponseAudienceEnum.Caller; // Tester will then have to rejoin each player using their respective rejoin codes
                G.ClearConnectionMappings();
                replayContext = JsonSerializer.Deserialize<GameLog>(this.Parameters);
                FixCardOrderInDesererialisedDecks(replayContext); // (it loads them in array order, which gives a reversed deck)

                // Now rebuild/replay the current game using the previously-recorded moves
                System.Diagnostics.Debug.WriteLine("Replaying game from supplied game log");
                G.InitialiseGame(replayContext); // Clear the current game and set the replay context (this affects various game behaviours)

                // Add all the players (note that the replay log should not contain join actions for the players who joined at the start of the game)
                foreach ( string playerName in replayContext.playersInOrderAtStartOfGame ) {
                    Participant newPlayer = new Participant(playerName);
                    newPlayer.IsGameAdministrator = ( playerName == replayContext.administrator ); 
                    G.Participants.Add(newPlayer);
                }
                G.SetActionAvailabilityBasedOnCurrentPlayer(); // Ensures the initial section of available actions is set
 
                // Replay each game action in the recorded order (including joins and starts)
                for ( int actionIndex = 0; actionIndex < replayContext.actions.Count; actionIndex++ ) {
                    replayContext.indexOfLastReplayedAction += 1;
                    GameLogAction gla = replayContext.actions[actionIndex];
                    actionSucceeded = await ReplayAction(gla);
                    inconsistenciesFound += ( actionSucceeded == true ? 0 : 1 );
                    replayContext.indexOfLastReplayedAction = actionIndex;
                    if ( replayContext.pauseAfter > 0 && gla.ActionNumber >= replayContext.pauseAfter ) {
                        // The user asked us to pause after processing a given action (and to leave the replay in a paused state that can be stepped through)
                        System.Diagnostics.Debug.WriteLine("Replay paused due to pauseAfter ("+replayContext.pauseAfter
                            +") request ... single step via Replay with no parameters or advance multiple steps by specifying target action number");
                        break;
                    }
                }
                // Mark each player as locked ... this will be unlocked once someone rejoins as that player using a unique new connection                
                foreach ( Participant p in G.Participants ) {
                    p.IsLockedOutFollowingReplay = true;
                }
                System.Diagnostics.Debug.WriteLine("Use the game state to find each player and rejoin each of them from a separate browser using their respective rejoin codes.");
            }
            else if ( replayMode == ReplayModeEnum.AdvanceToNamedStep ) {
                // Continue the replay until we have completed a step whose action number (not index) is not less than the requested action number
                ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // All connected players will receive their own view of the updated game state
                ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // All connected players will receive an update
                for ( int actionIndex = replayContext.indexOfLastReplayedAction + 1; actionIndex < replayContext.actions.Count; actionIndex++ ) {
                    replayContext.indexOfLastReplayedAction = actionIndex;
                    GameLogAction gla = replayContext.actions[actionIndex];
                    actionSucceeded = await ReplayAction(gla);
                    inconsistenciesFound += ( actionSucceeded == true ? 0 : 1 );
                    if ( gla.ActionNumber >= advanceToActionNumber ) {
                        // We have reached (and processed) the requested target action
                        System.Diagnostics.Debug.WriteLine("Replay has advanced to requested point and will pause again (if log not exhausted)");
                        break;
                    }
                }
            }            
            else if ( replayMode == ReplayModeEnum.AdvanceOneStep ) {
                // Move a paused replay forward one step
                ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // All connected players will receive their own view of the updated game state
                ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // All connected players will receive an update
                replayContext.indexOfLastReplayedAction++; // Move to next action
                GameLogAction gla = replayContext.actions[replayContext.indexOfLastReplayedAction];
                actionSucceeded = await ReplayAction(gla);
                inconsistenciesFound += ( actionSucceeded ? 0 : 1);
            }            

            if ( inconsistenciesFound > 0 ) {
                 System.Diagnostics.Debug.WriteLine("WARNING: " + inconsistenciesFound + " inconsistencies in results were identified ... please review the replay log");
            }
           
            if ( replayContext.indexOfLastReplayedAction >= ( replayContext.actions.Count - 1 ) ) {
                System.Diagnostics.Debug.WriteLine("Game is no longer in replay mode and will continue under normal conditions from here");
                G.SetReplayContext(null);
            }
        }
        private async Task<bool> ReplayAction(GameLogAction gla) {
            bool resultsAreConsistent = true;
            ActionEnum actionType = gla.ActionType;
            Action a = ActionFactory.NewAction(
                "", // Note that the lack of a connection id is also an indicator to the ActionFactory that the command is running in Replay mode
                gla.ActionType, 
                R.RoomId, 
                gla.UserName,
                G.CountOfLeavers.ToString(), 
                gla.Parameters
            );
            System.Diagnostics.Debug.WriteLine(
                "Replaying action "  + gla.ActionNumber + ": "
                + gla.ActionType.ToString().ToLower() + " by " + gla.UserName);

            await a.ProcessActionAndReturnRoomReference(); 

            System.Diagnostics.Debug.WriteLine("  StatusMessage: " + G.StatusMessage);

            System.Diagnostics.Debug.WriteLine("  Commentary from replay:");
            foreach ( string c in G.HandCommentary ) {
                System.Diagnostics.Debug.WriteLine("    " + c);                    
            }
            if ( G.StatusMessage != gla.StatusMessage ) {
                resultsAreConsistent = false;
                System.Diagnostics.Debug.WriteLine("  Status Message is not consistent:");
                System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.StatusMessage);
                System.Diagnostics.Debug.WriteLine("    REPLAY   : " + G.StatusMessage);
            }
            if ( gla.PlayerSummaries != null && gla.PlayerSummaries != "" ) {
                string new_ps = G.PlayerSummaries();
                if ( new_ps != gla.PlayerSummaries ) {
                    resultsAreConsistent = false;
                    System.Diagnostics.Debug.WriteLine("  Player Summaries are not consistent:");
                    System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.PlayerSummaries);
                    System.Diagnostics.Debug.WriteLine("    REPLAY   : " + new_ps);
                }
            }
            return resultsAreConsistent;
        }
        private void FixCardOrderInDesererialisedDecks(GameLog replayContext) {
            // This is really ugly and needs to go somewhere else, probably in Deck itself
            for ( int i = 0; i < replayContext.decks.Count; i++){ 
                Deck d = replayContext.decks[i];
                Deck temp = new Deck();
                while (d.Cards.Count != 0) {
                    temp.Cards.Push(d.Cards.Pop());
                }
                replayContext.decks[i] = temp;
            }
        }
    }     
}  
