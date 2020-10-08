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
        public AdHocQuery(string queryType) {
            queryResults = new List<string>();
            try
            {
                switch (queryType.ToLower())  
                { 
                    case "list-vars": // List environment variables 
                        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                            queryResults.Add("Key: " + de.Key + " Value: " + de.Value);
                        }
                        break;   
                    case "list-games": // List open games  
                        foreach (DictionaryEntry pair in ServerState.GameList )
                        {
                            Game g = (Game) pair.Value;
                            queryResults.Add(
                                "Game:" + pair.Key 
                                + ", Participants:" + g.Participants.Count
                                + ", Hands:" + g.HandsPlayedIncludingCurrent
                                + ", Last Action:" + g.LastSuccessfulAction.ToString("yyyy-MM-dd HH:mm")
                                + " (" + g.MinutesSinceLastAction()+" Minutes Ago)"
                                );
                        }
                        break;  
                    case "list-env": // Check if running on public server
                        if ( ServerState.IsRunningOnPublicServer() ) {
                            queryResults.Add("Game is running on the public server");
                        }
                        else {
                            queryResults.Add("Game is running on something other than the public server");

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
        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            //options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }  
    }
}