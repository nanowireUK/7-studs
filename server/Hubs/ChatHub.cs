using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SevenStuds.Models;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // This is the server-side code that is called by connection.invoke("xxx") in chat.js on the client

        // ---- JOIN GAME
        public async Task UserClickedJoin(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            g.Participants.Add(new Participant(user, Context.ConnectionId));
            g.LastEvent = user + " joined game";
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }

        public async Task UserClickedStart(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            g.Initialise();
            g.LastEvent = user + " started game";
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }        
    }
}