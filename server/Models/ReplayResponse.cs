using System.Collections.Generic;
using System.Text.Json;

namespace SevenStuds.Models
{
    public class ReplayResponse
    {
        public string Instructions { get; set; }
        public string RoomId { get; set; }
        public List<string> RejoinCodes { get; set; }
        public ReplayResponse (Game g) {
            Instructions = "Now join the game using the room name and one or more of the player names and rejoin codes shown below";
            RoomId = g.ParentRoom().RoomId;
            RejoinCodes = new List<string>();
            foreach ( Participant p in g.Participants ) {
                RejoinCodes.Add( p.Name + ( p.IsGameAdministrator ? " (admin)" : "" ) + ": " + p.RejoinCode);
            }
        }
        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }  
    }
}