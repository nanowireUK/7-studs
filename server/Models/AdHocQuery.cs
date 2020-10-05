using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections;

namespace SevenStuds.Models
{
    public class AdHocQuery
    {
        // Various queries against the server
        public List<string> queryResults { get; set; } 
        public AdHocQuery(int queryNum) {
            queryResults = new List<string>();
            switch (queryNum)  
            { 
                case 1:  
                    queryResults.Add("Query " + queryNum + " listing env vars");
                    foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) {
                        queryResults.Add("Key: " + de.Key + " Value: " + de.Value);
                    }
                    break;                                                                         
                default:  
                    queryResults.Add("Unsupported query number" + queryNum);
                    break;
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