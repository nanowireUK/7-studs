using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class LobbyData
    {
        // Used to present the current status of a room
        public LobbyDataGameStats GameStatistics { get; set; } 
        public List<LobbyDataCurrentGame> CurrentGameStandings { get; set; } // Will show players and their results in an appopriate order
        // public List<string> ConnectedPlayers { get; set; } 
        // public List<string> ConnectedSpectators { get; set; } 
        public List<string> PreviousGameResults { get; set; } 
        public LobbyData(Game g) {
            this.AddGameStatistics(g);
            this.AddGameResult(g);
            // this.AddConnectedPlayers(g);
            // this.AddSpectators(g);
            this.AddPreviousGameResults(g);
        }
        private void AddGameResult(Game g) {
            CurrentGameStandings = SortedListOfParticipants(g);
        }
        private void AddGameStatistics(Game g) {
            GameStatistics = new LobbyDataGameStats(g);
        }          
        private void AddPreviousGameResults(Game g) {
            // PreviousGameResults = null;
            // if ( ServerState.RoomHistory.Contains[g.GameId] ) {
            //     PreviousGameResults = (List<string>) ServerState.RoomHistory[g.GameId];
            // }
        }  
        public List<LobbyDataCurrentGame> SortedListOfParticipants(Game g) {
            // List all players who joined and/or left during the lifetime of the current game
            // (noting that details of previous leavers are cleared when a new game starts)

            // PlayerStatusEnum:
            //    PartOfMostRecentGame (in which case date is bankruptcy date and a more recent date ranks higher in tie-breaking)
            //    QueuingForNextGame (in which case date is joining date and an older date ranks higher in tie-breaking)
            //    Spectator (in which case date is joining date and an older date ranks higher in tie-breaking))
        
            DateTimeOffset now = DateTimeOffset.Now;
            List<LobbyDataCurrentGame> result = new List<LobbyDataCurrentGame>();
            // First add all players who are still registered in the game, without worrying about the order as the list will be sorted later
            foreach ( Participant p in g.Participants ) {
                if ( p.HasDisconnected == false ) {
                    result.Add(new LobbyDataCurrentGame(
                        p.Name, 
                        ( p.HasBeenActiveInCurrentGame ? PlayerStatusEnum.PartOfMostRecentGame : PlayerStatusEnum.QueuingForNextGame ),
                        p.UncommittedChips, 
                        false, // has not left
                        ( p.TimeOfBankruptcy == null ? now : p.TimeOfBankruptcy )
                        ));
                }
            }
            // Now add the details of the leavers (but not if they have already been added because not yet completely removed from Participant list)
            if ( g.LeaversLogForGame != null ) {
                foreach ( LeavingRecord leaver in g.LeaversLogForGame ) {
                    Boolean leaverIsNoLongerInParticipantList = true;
                    // foreach ( Participant p in g.Participants ) {
                    //     if ( p.ParticipantLevelSignalRGroupName == leaver.LeavingParticipantLevelSignalRGroupName ) {
                    //         leaverIsNoLongerInParticipantList = false;
                    //         break;
                    //     }
                    // }
                    if ( leaverIsNoLongerInParticipantList ) {
                        result.Add(new LobbyDataCurrentGame(
                            leaver.LeavingParticipantName, 
                            (
                                leaver.WasSpectator 
                                ? PlayerStatusEnum.Spectator 
                                : ( leaver.HasBeenPartOfGame ? PlayerStatusEnum.PartOfMostRecentGame : PlayerStatusEnum.QueuingForNextGame )
                            ),
                            leaver.ChipsAtEndOfGame, 
                            true, // has left
                            leaver.LeftAt_UTC));
                    }
                }
            }
            // Now add active spectators
            foreach ( Spectator p in g.Spectators ) {
                result.Add(new LobbyDataCurrentGame(
                    p.Name, 
                    PlayerStatusEnum.Spectator,
                    0, 
                    false, // has not left (not that spectators who have left are part of the leavers log)
                    now));
            }
            // Now sort the array as follows
            //    Show all 'PartOfMostRecentGame' players first (with 'left' indicator) (in descending order of funds, then bankruptcy date (latest first), then name) (includes any who have left in the meantime)
            //    Show 'New For Next Game' players next (earliest joining date at top) (not including any who joined and then left without ever being part of a game)
            //    Show spectators next (earliest joining date at top) (with 'spectator' indicator) (not including any who joined and then left)
            //    Finally show any players or spectators who have joined then left without ever being part of a game (earliest joining date at top)
            result.Sort(
                delegate(LobbyDataCurrentGame x, LobbyDataCurrentGame y) 
                {
                    // See https://www.codeproject.com/Tips/761275/How-to-Sort-a-List
                    // Sort in various ways depending on the the players' part in the game

                    int a = 0;

                    // First sort on the priority order as defined by the LobbyDataCurrentGame entry
                    if (a == 0)
                        a = y.PriorityOrderForLobbyData.CompareTo(x.PriorityOrderForLobbyData);

                    // Next sort on remaining funds (will be zero in many cases though)
                    if (a == 0)
                        a = y.RemainingFunds.CompareTo(x.RemainingFunds);

                    // Next sort on tie-breaker date (possibly reversing the sort order)
                    if (a == 0)
                        a = y.UTCTimeAsTieBreaker.CompareTo(x.UTCTimeAsTieBreaker) * y.DateComparisonModifier;

                    // If both players are inseparable even on date, use their names as a random time breaker
                    if (a == 0)
                        a = x.PlayerName.CompareTo(y.PlayerName);  

                    return a;
                }
            );
            return result;
        }    
    }
}