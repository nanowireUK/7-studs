using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class LobbyData
    {
        // Used to present the current status of each player who has been involved in a room in the last game or as someone waiting in the lobby
        public List<LobbyDataCurrentGame> CurrentGameStandings { get; set; } // Will show players and their results in an appropriate order
        public LobbyData() {} // Parameterless constructor for use by JSON serlialiser/deserialiser
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
        
            DateTimeOffset now = DateTimeOffset.UtcNow;
            List<LobbyDataCurrentGame> result = new List<LobbyDataCurrentGame>();

            // First add all players who are still connected to the game, without worrying about the order as the list will be sorted later
            foreach ( Participant p in g.Participants ) {
                if ( p.HasDisconnected == false ) {
                    result.Add(new LobbyDataCurrentGame(
                        p.Name, 
                        ( p.HasBeenActiveInCurrentGame ? PlayerStatusEnum.PartOfMostRecentGame : PlayerStatusEnum.QueuingForNextGame ),
                        p.UncommittedChips, 
                        false, // has not left
                        // If player was already bankrupt at start of hand, use that date
                        // If they went bankrupt during the hand (by going all-in) use the date/time they went all in
                        // (i.e. on the principle that committing your last chip to the pot means that that was the time you exposed yourself to bankruptcy)
                        ( p.TimeOfBankruptcy != DateTimeOffset.MinValue ? p.TimeOfBankruptcy 
                        : ( ( p.UncommittedChips == 0 && p.AllInDateTime != DateTimeOffset.MinValue ) ? p.AllInDateTime : now )) // 'now' is catch-all for players not yet bankrupt       
                    ));
                }
            }

            // Now add the details of the leavers (who may or may not also have been part of the last game)
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
                        leaver.EndOfRelevanceToGame_UTC));
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

                    // Next sort on remaining funds (could be zero in many cases though)
                    if (a == 0)
                        a = y.RemainingFunds.CompareTo(x.RemainingFunds);

                    // Next sort on tie-breaker date (possibly reversing the sort order)
                    if (a == 0)
                        a = y.UTCTimeAsTieBreaker.CompareTo(x.UTCTimeAsTieBreaker) * y.DateComparisonModifier;

                    // If both players are inseparable even on date, use their names as a random time breaker (ties will be recognised after sorting is complete)
                    if (a == 0)
                        a = x.PlayerName.CompareTo(y.PlayerName);  

                    return a;
                }
            );
            // Now that the list is sorted, note each player's relative positions on the leader board (if they are on it)
            for ( int i = 0; i < result.Count; i++ ) {
                LobbyDataCurrentGame p = result[i];
                if ( p.PriorityOrderForLobbyData == 5 ) {
                    // Player is on the leaderboard, their position will be as is unless their situation is identical to the player above them,
                    // in which case they will inherit that position (which may itself have been inherited in the case of multiple-way ties)
                    p.LeaderBoardPosition = i+1; 
                    if ( i > 0 ) {
                        if ( p.RemainingFunds == result[i-1].RemainingFunds && p.UTCTimeAsTieBreaker == result[i-1].UTCTimeAsTieBreaker ) {
                            // Player are tied on funds (but use bankruptcy date to ensure players with zero funds are not treated as ties)
                            p.LeaderBoardPosition = result[i-1].LeaderBoardPosition;
                            p.LeaderBoardPositionIsTied = true;
                            result[i-1].LeaderBoardPositionIsTied = true;
                        }
                    }
                }
            }
            return result;
        }    
    }
}