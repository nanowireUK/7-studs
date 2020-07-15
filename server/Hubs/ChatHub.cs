using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Text.Json;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // This is the server-side code that is called by connection.invoke("ProcessActionAndSendUpdatedGameState") in chat.js on the client
        public async Task ProcessActionAndSendUpdatedGameState(string user, string message)
        {
            // Process commands
            if (message == "g") {
                // Request to create a new game (probably best to do most of this in the constructor)
                SevenStuds.Models.Game g = new SevenStuds.Models.Game();
                g.Participants.Add(new Models.Participant(user));
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };
                string jsonString = JsonSerializer.Serialize(g, options);
                await Clients.All.SendAsync("ReceiveUpdatedGameState", user, jsonString);
            }
            else {
                // Standard behaviour from sample code
                await Clients.All.SendAsync("ReceiveMessage", user, message);
            }
        }
    }
}