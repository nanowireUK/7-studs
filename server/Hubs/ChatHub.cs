using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SevenStuds.Models;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // This is the server-side code that is called by connection.invoke("UserClickedActionButton") in chat.js on the client

        // In each case the game will process the action and the updated game state will be returned to the client

        public async Task UserClickedActionButton(ActionEnum actionType, string gameId, string user, string amount)
        {
            Action a = ActionFactory.NewAction(actionType, gameId, user, amount, Context.ConnectionId);
            await Clients.All.SendAsync("ReceiveUpdatedGameState", a.ProcessActionAndReturnUpdatedGameStateAsJson());
        }
    }
}