using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Globalization;
using System.Collections;

namespace SevenStuds.Models
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
                    case "list-vars": 
                        // List environment variables 
                        List<string> envVars = new List<string>();
                        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                            envVars.Add("Key: " + de.Key + " Value: " + de.Value);
                        }
                        queryResults.Add(ServerState.StringArrayAsJson(envVars));
                        break;   
                    case "list-games": 
                        // List rooms with active games
                        List<string> games = new List<string>();
                        foreach (DictionaryEntry pair in ServerState.RoomList )
                        {
                            Room r = (Room) pair.Value;
                            Game g = r.ActiveGame;
                            games.Add(
                                "Room:" + pair.Key 
                                + ", Participants:" + g.Participants.Count
                                + ", Hands:" + g.HandsPlayedIncludingCurrent
                                + ", Last Action:" + g.LastSuccessfulAction.ToString("yyyy-MM-dd HH:mm")
                                + " (" + g.MinutesSinceLastAction()+" Minutes Ago)"
                                );
                        }
                        queryResults.Add(ServerState.StringArrayAsJson(games));
                        break;  
                    case "list-env": 
                        // Check if running on public server
                        if ( ServerState.IsRunningOnPublicServer() ) {
                            queryResults.Add("Game is running on the public server");
                        }
                        else {
                            queryResults.Add("Game is running on something other than the public server");

                        }
                        break;    
                    case "list-logs":
                        // Return the game logs from all games completed since the server last restarted
                        foreach (DictionaryEntry pair in ServerState.RoomList )
                        {
                            Room r = (Room) pair.Value;
                            foreach ( GameLog gl in r.GameLogs) {
                                queryResults.Add(gl.AsJson());
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
    }
}