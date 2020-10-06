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
        public AdHocQuery(int queryNum) {
            queryResults = new List<string>();
            try
            {
                switch (queryNum)  
                { 
                    case 1: // List environment variables 
                        queryResults.Add("Query " + queryNum + " listing env vars");
                        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                            queryResults.Add("Key: " + de.Key + " Value: " + de.Value);
                        }
                        break;   
                    case 2: // List open games  
                        queryResults.Add("Query " + queryNum + " listing open games");
                        foreach (DictionaryEntry pair in ServerState.GameList )
                        {
                            Game g = (Game) pair.Value;
                            queryResults.Add(
                                "Game: " + pair.Key 
                                + ", Participants: " + g.Participants.Count
                                + ", Last Action: " + g.LastSuccessfulAction.ToString("yyyy-MM-dd HH:mm")
                                + " (" + (DateTimeOffset.Now - g.LastSuccessfulAction).TotalMinutes.ToString("F0", CultureInfo.InvariantCulture)+" Minutes Ago)"
                                );
                        }
                        break;  
                    case 3: // Check if running on public server
                        queryResults.Add("Query " + queryNum + " check whether running on public server = "
                            + ServerState.IsRunningOnPublicServer().ToString());
                        break;                                                                                                                        
                    default:  
                        throw new SystemException("Query number not implemented");
                }  
            }
            catch (System.Exception e)
            {
                queryResults.Add("Query " + queryNum + " failed:");
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