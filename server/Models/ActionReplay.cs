using System;
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
        public ActionReplay(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers, string logAsJson) 
            : base(connectionId, actionType, ourGame, user, leavers, logAsJson)
        {
        }
        public override async Task ProcessAction()
        {
            // ActionReplay has been completely reworked to fit in with the new stateless mode.
            // A game log can now simply be entered into the parameters area of the server screen, 
            // and, on clicking 'Replay Game From Log', Chathub.UserClickedReplaySetup() 
            // will control the whole replay (this used to be done here, in ActionReplay).
            //
            // ActionReplay now only deals with stepping through a replayed game that was paused because of a PauseAfter value in the replayed log
            // (you should use the server screen to rejoin the replayed game according to the instructions that UserClickedReplaySetup,
            // then use the 'Step' and 'Advance To Action <n>' options to step through the rest of the game

            ReplayModeEnum replayMode;
            int advanceToActionNumber = 0;
            if ( this.Parameters == null || this.Parameters == "" ) {
                // Parameter is blank, so assume user is stepping through an existing replay one step at a time
                replayMode = ReplayModeEnum.AdvanceOneStep;
                if ( G.IsRunningInReplayMode() == false ) {
                    throw new HubException("Stepwise replay is not possible as this game is not paused");
                }
            }
            else {
                // Only other option is a numeric parameter, which would be a request to advance an existing replay to a given action number (inclusive)
                replayMode = ReplayModeEnum.AdvanceToNamedStep;
                if ( int.TryParse(Parameters, out advanceToActionNumber) == false ) {
                    throw new HubException("A numeric action number is required by the Replay(AdvanceTo) action");
                }
                if ( G.IsRunningInReplayMode() == false ) {
                    throw new HubException("Advancing to a given step is not possible as this game is not paused");
                }
            }

            bool actionSucceeded;
            int inconsistenciesFound = 0;

            GameLog replayContext = G.GetReplayContext();

            if ( replayMode == ReplayModeEnum.AdvanceToNamedStep ) {
                // Continue the replay until we have completed a step whose action number (not index) is not less than the requested action number
                ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // All connected players will receive their own view of the updated game state
                ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // All connected players will receive an update
                for ( int actionIndex = replayContext.indexOfLastReplayedAction + 1; actionIndex < replayContext.actions.Count; actionIndex++ ) {
                    replayContext.indexOfLastReplayedAction = actionIndex;
                    GameLogAction gla = replayContext.actions[actionIndex];
                    actionSucceeded = await ReplayAction(G.ParentRoom(), gla);
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
                actionSucceeded = await ActionReplay.ReplayAction(G.ParentRoom(), gla);
                inconsistenciesFound += ( actionSucceeded ? 0 : 1);
            }            

            if ( inconsistenciesFound > 0 ) {
                 System.Diagnostics.Debug.WriteLine("WARNING: " + inconsistenciesFound + " inconsistencies in results were identified ... please review the replay log");
            }
           
            if ( replayContext.indexOfLastReplayedAction >= ( replayContext.actions.Count - 1 ) ) {
                System.Diagnostics.Debug.WriteLine("Replayed game is no longer in replay mode and will continue under normal conditions from here");
                G.SetReplayContext(null); 
            }
        }
        public static async Task<bool> ReplayAction(Room replayRoom, GameLogAction gla) {
            bool resultsAreConsistent = true;
            ActionEnum actionType = gla.ActionType;
            Action a = await ActionFactory.NewAction(
                "", // Note that the lack of a connection id is also an indicator to the ActionFactory that the command is running in Replay mode
                gla.ActionType, 
                replayRoom.RoomId, 
                gla.UserName,
                replayRoom.SavedCountOfLeavers.ToString(), 
                gla.Parameters
            );
            System.Diagnostics.Debug.WriteLine(
                "Replaying action "  + gla.ActionNumber + ": "
                + gla.ActionType.ToString().ToLower() + " by " + gla.UserName);

            Game replayedGame = await a.ProcessActionAndReturnGameReference();
            replayRoom.SavedCountOfLeavers = replayedGame.CountOfLeavers; // preserve this in the same way that the client would

            System.Diagnostics.Debug.WriteLine("  StatusMessage: " + replayedGame.StatusMessage);

            System.Diagnostics.Debug.WriteLine("  Commentary from replay:");
            foreach ( string c in replayedGame.HandCommentary ) {
                System.Diagnostics.Debug.WriteLine("    " + c);                    
            }
            if ( replayedGame.StatusMessage != gla.StatusMessage ) {
                resultsAreConsistent = false;
                System.Diagnostics.Debug.WriteLine("  Status Message is not consistent:");
                System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.StatusMessage);
                System.Diagnostics.Debug.WriteLine("    REPLAY   : " + replayedGame.StatusMessage);
            }
            if ( gla.PlayerSummaries != null && gla.PlayerSummaries != "" ) {
                string new_ps = replayedGame.PlayerSummaries();
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
                if ( d.CardList == "" ) {
                    // If the Deck has been deserialised from a source that does not yet have the CardList value, we have to do two things:
                    // (1) Reverse the order of the Cards in the Deck (because deserialisation restores them in reverse order)
                    // (2) Build the CardList element from repaired set of Cards
                    Deck temp = new Deck();
                    while (d.Cards.Count != 0) {
                        temp.Cards.Push(d.Cards.Pop());
                    }
                    temp.EnsureCardListHasBeenInitialisedFollowingDeserialisation();
                    replayContext.decks[i] = temp;
                }
            }
        }
    }     
}  
