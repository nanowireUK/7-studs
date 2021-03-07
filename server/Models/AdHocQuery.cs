using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Globalization;
using System.Collections;

namespace SocialPokerClub.Models
{
    public class AdHocQuery
    {
        // Various queries against the server
        public List<string> queryResults { get; set; }
        public AdHocQuery(Room thisRoom, string queryType) {
            queryResults = new List<string>();
            try
            {
                switch (queryType.ToLower())
                {
                    // case "list-vars":
                    //     // List environment variables
                    //     List<string> envVars = new List<string>();
                    //     foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                    //         envVars.Add("Key: " + de.Key + " Value: " + de.Value);
                    //     }
                    //     envVars.Sort();
                    //     queryResults.Add(ServerState.StringArrayAsJson(envVars));
                    //     break;
                    // case "list-games":
                    //     // List rooms with active games
                    //     List<string> games = new List<string>();
                    //     foreach (DictionaryEntry pair in ServerState.RoomList )
                    //     {
                    //         Room r = (Room) pair.Value;
                    //         Game g = r.ActiveGame;
                    //         games.Add(
                    //             "Room:" + pair.Key
                    //             + ", Participants:" + g.Participants.Count
                    //             + ", Hands:" + g.HandsPlayedIncludingCurrent
                    //             + ", Last Action:" + g.LastSuccessfulAction.ToString("yyyy-MM-dd HH:mm")
                    //             + " (" + g.MinutesSinceLastAction()+" Minutes Ago)"
                    //             );
                    //     }
                    //     queryResults.Add(ServerState.StringArrayAsJson(games));
                    //     break;
                    case "list-env":
                        // Check if running on public server
                        if ( ServerState.AllowTestFunctions() ) {
                            queryResults.Add("Test functions are allowed");
                        }
                        else {
                            queryResults.Add("Test functions are disallowed");
                        }
                        break;
                    case "test-hand-lookup":
                        // Check if running on public server
                        TestHandLookupPerformance(queryResults);
                        break;
                    // case "list-logs":
                    //     // Return the game logs from all games completed since the server last restarted
                    //     foreach (DictionaryEntry pair in ServerState.RoomList )
                    //     {
                    //         Room r = (Room) pair.Value;
                    //         foreach ( GameLog gl in r.GameLogs) {
                    //             queryResults.Add(gl.AsJson());
                    //         }
                    //     }
                    //     break;
                    case "log-conns":
                        // Lists all the currently mapped connection ids
                        System.Diagnostics.Debug.WriteLine("Listing all connections for all games");
                        foreach ( string roomId in ServerState.StatefulData.RoomLevelMapOfGroupToConnections.Keys ) {
                            Dictionary<string, Dictionary<string, bool>> groupData = ServerState.StatefulData.RoomLevelMapOfGroupToConnections[roomId];
                            foreach ( string groupId in groupData.Keys ) {
                                Dictionary<string, bool> connData = groupData[groupId];
                                foreach ( string connId in connData.Keys ) {
                                    bool isLinked = connData[connId];
                                    System.Diagnostics.Debug.WriteLine("Room '{0}' : Group '{1}' : Connection '{2}' : Registered '{3}'", roomId, groupId, connId, isLinked);
                                }
                            }  
                        }
                        break;                    
                    default:
                        throw new SystemException("Query type " + queryType + " not implemented");
                }
            }
            catch (System.Exception e)
            {
                queryResults.Add("Query type " + queryType + " failed:");
                queryResults.Add(e.Message);
            }
        }

        private void TestHandLookupPerformance(List<string> r) {
            // Test the performance of the SortedList lookup used to lookup the ranking of a singple poker hand

            // CONCLUSIONS (Feb 2021 on Dell XPS15 laptop):
            // Total lookups = 311,875,200 (52*51*50*49*48)
            // Time taken to loop through and just increment the count = 108,162.552 milliseconds
            // Time taken to loop through and lookup each card key     = 123,540.9692 milliseconds
            // Incremental time to lookup every card value             =  15,378.4172 milliseconds (15 seconds)
            // In one second it can do 20,280,058 card lookups!!
            // (note that this is using SortedList which I think does a relatively efficient binary search)
            // THEREFORE: No need to look into improving performance by changing to Dictionary and custom HashCodes

            List<Card> cards = new List<Card>();
            foreach (CardEnum c in Enum.GetValues(typeof(CardEnum)))
            {
                foreach (SuitEnum s in Enum.GetValues(typeof(SuitEnum)))
                {
                    Card n = new Card(c, s);
                    cards.Add(n);
                    System.Diagnostics.Debug.WriteLine("Added "+n.ToString(CardToStringFormatEnum.ShortCardName));
                }
            }
            DateTimeOffset startStamp = DateTimeOffset.UtcNow;
            string x1; string x2; string x3; string x4; string x5;
            int v0 = (int) CardEnum.Dummy;
            int v1; int v2; int v3; int v4; int v5;
            int s1; int s2; int s3; int s4; int s5;
            int lookupCount = 0;
            EvalHand matchingHand;
            foreach ( Card c1 in cards ) {
                x1 = c1.ToString(CardToStringFormatEnum.ShortCardName);
                v1 = (int) c1.CardValue;
                s1 = (int) c1.CardSuit;
                if ( v1 == v0 ) { continue; }
                System.Diagnostics.Debug.WriteLine("Testing hands starting with "+c1.ToString(CardToStringFormatEnum.ShortCardName));
                foreach ( Card c2 in cards ) {
                    x2 = c2.ToString(CardToStringFormatEnum.ShortCardName);
                    v2 = (int) c2.CardValue;
                    s2 = (int) c2.CardSuit;
                    if ( v2 == v0 ) { continue; }
                    foreach ( Card c3 in cards ) {
                        x3 = c3.ToString(CardToStringFormatEnum.ShortCardName);
                        v3 = (int) c3.CardValue;
                        s3 = (int) c3.CardSuit;
                        if ( v3 == v0 ) { continue; }
                        foreach ( Card c4 in cards ) {
                            x4 = c4.ToString(CardToStringFormatEnum.ShortCardName);
                            v4 = (int) c4.CardValue;
                            s4 = (int) c4.CardSuit;
                            if ( v4 == v0 ) { continue; }
                            foreach ( Card c5 in cards ) {
                                x5 = c5.ToString(CardToStringFormatEnum.ShortCardName);
                                v5 = (int) c5.CardValue;
                                s5 = (int) c5.CardSuit;
                                if ( v5 == v0 ) { continue; }
                                if ( x1 != x2 && x1 != x3 && x1 != x4 && x1 != x5 && x2 != x3 && x2 != x4 && x2 != x5 && x3 != x4 &&x3 != x5 && x4 != x5 )
                                {
                                    // Determine the key of a poker hand containing this combination of five cards
                                    // (see PokerHand implementation for details ... the clever bit is the use of prime numbers to generate unique hand keys)
                                    int key = v1 * v2 * v3 * v4 * v5;
                                    if (s1 == s2 && s2 == s3 && s3 == s4 && s4 == s5 ) { key *= -1; } // flushes are given negative numbers
                                    lookupCount++;
                                    matchingHand = ServerState.RankingTable.EvalHands[key]; // Comment this out to determine underlying loop overhead
                                }
                            }
                        }
                    }
                }
            }
            DateTimeOffset endStamp = DateTimeOffset.UtcNow;
            double ms = ( endStamp - startStamp ).TotalMilliseconds;
            queryResults.Add(lookupCount + " lookups done, time taken was "+ ms + " milliseconds");
            System.Diagnostics.Debug.WriteLine(lookupCount + " lookups done, time taken was "+ ms + " milliseconds");
        }
    }
}