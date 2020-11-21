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
                        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                            queryResults.Add("Key: " + de.Key + " Value: " + de.Value);
                        }
                        break;   
                    case "list-rooms": 
                        // List open games  
                        foreach (DictionaryEntry pair in ServerState.RoomList )
                        {
                            Room r = (Room) pair.Value;
                            Game g = r.ActiveGame;
                            queryResults.Add(
                                "Room:" + pair.Key 
                                + ", Participants:" + g.Participants.Count
                                + ", Hands:" + g.HandsPlayedIncludingCurrent
                                + ", Last Action:" + g.LastSuccessfulAction.ToString("yyyy-MM-dd HH:mm")
                                + " (" + g.MinutesSinceLastAction()+" Minutes Ago)"
                                );
                        }
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
                        // Return the game logs from all games completed under the current game id
                        // List<string> logsForThisGame = (List<string>) R.RoomGameLogHistory[thisRoom.GameId];
                        // foreach ( string log in logsForThisGame ) {
                        //     queryResults.Add(log);
                        // }
                        queryResults.Add("Not implemented");
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
        // public string AsJson()
        // {
        //     var options = new JsonSerializerOptions
        //     {
        //         WriteIndented = true,
        //     };
        //     //options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
        //     string jsonString = JsonSerializer.Serialize(this, options);
        //     return jsonString;
        // }  
    }
}