using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class LobbyData
    {
        // Used to present the current status of each player who has been involved in a room in the last game or as someone waiting in the lobby
        public List<LobbyDataCurrentGame> CurrentGameStandings { get; set; } // Will show players and their results in an appropriate order
        public LobbyData(Game g) {
            CurrentGameStandings = SortedListOfParticipants(g);
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

            // First add all players who are still connected to the game, without worrying about the order as the list will be sorted later
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

            // Now add the details of the leavers
            if ( g.LeaversLogForGame != null ) {
                foreach ( LeavingRecord leaver in g.LeaversLogForGame ) {
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