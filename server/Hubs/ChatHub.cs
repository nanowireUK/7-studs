using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SevenStuds.Models;
using System.Collections.Generic;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // This is the server-side code that is called by connection.invoke("UserClickedActionButton") in chat.js on the client

        // In each case the game will process the action and the updated game state will be returned to the client

        public async Task UserClickedActionButton(ActionEnum actionType, string gameId, string user, string parameters)
        {
            Action a = ActionFactory.NewAction(Context.ConnectionId, actionType, gameId, user, parameters);
            Game g = a.ProcessActionAndReturnGameReference();
            // For each participant, send a personalised copy of the game state (hiding e.g. other people's cards)

            // New connections may have been linked to players, so link those connections to the relevant game and player groups in SignalR
            foreach ( Participant p in g.Participants ) {
                List<string> conns = p.GetConnectionIds();
                for ( int i = 0; i < conns.Count; i++ ) {
                    // If the game has not yet added this connection, then link it
                    string conn = conns[i];
                    if ( g.GetParticipantFromConnection(conn) == null ) {
                        g.LinkConnectionToParticipant(conn, p); 
                        await Groups.AddToGroupAsync(conn, g.GameLevelSignalRGroupName);
                        await Groups.AddToGroupAsync(conn, p.ParticipantLevelSignalRGroupName);
                    }
                }
            }

            // Now send the personalised game state to each player using their individual code

            //System.Diagnostics.Debug.WriteLine("Sending response to all users (across all games)");
            //await Clients.All.SendAsync("ReceiveUpdatedGameState", "BROADCAST: "+ g.AsJson());
            //System.Diagnostics.Debug.WriteLine("Sending response to whole game group");
            //await Clients.Group(g.GameLevelSignalRGroupName).SendAsync("ReceiveUpdatedGameState", "GAMEONLY: "+ g.AsJson());
            for ( int i = 0; i < g.Participants.Count; i++ ) {
                Participant p = g.Participants[i];
                //await Clients.Group(p.ParticipantLevelSignalRGroupName).SendAsync("ReceiveUpdatedGameState", "PLAYERSPECIFIC: "+ g.AsJson());
                string j = new PlayerCentricGameView(g, i).AsJson();
                await Clients.Group(p.ParticipantLevelSignalRGroupName).SendAsync("ReceiveUpdatedGameState", j);
            }
            //await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
            //await Clients.All.SendAsync("ReceiveUpdatedGameState", g.ProcessActionAndReturnUpdatedGameStateAsJson());
        }
    }
}
