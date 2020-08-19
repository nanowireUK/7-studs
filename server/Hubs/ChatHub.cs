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

            // Send the results of the action according to the ResponseType and ResponseAudience on the action object 

            string resultAsJson = ""; // default will be to set the result on a per-player basis
            string targetMethod = "ReceiveMyGameState";
            switch ( a.ResponseType )  
            { 
                case ActionResponseTypeEnum.GameLog:  
                    resultAsJson = g.GameLogAsJson();
                    targetMethod = "ReceiveGameLog";
                    break;
                case ActionResponseTypeEnum.OverallGameState:  
                    resultAsJson = g.AsJson();
                    targetMethod = "ReceiveOverallGameState";
                    break;   
                case ActionResponseTypeEnum.PlayerCentricGameState:
                    // This will be managed below for each individual player
                    break;
                default:  
                    throw new System.Exception("7Studs User Exception: Unsupported response type");                    
            }

            // Now send the appropriate response to the players indicated by the action's ResponseAudience setting
            switch ( a.ResponseAudience )  
            { 
                case ActionResponseAudienceEnum.Caller:
                    if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                        resultAsJson = new PlayerCentricGameView(g, a.PlayerIndex).AsJson();
                    }
                    await Clients.Caller.SendAsync(targetMethod, resultAsJson);
                    break;
                case ActionResponseAudienceEnum.CurrentPlayer:
                    if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                        resultAsJson = new PlayerCentricGameView(g, a.PlayerIndex).AsJson();
                    }
                    await Clients.Group(g.Participants[a.PlayerIndex].ParticipantLevelSignalRGroupName).SendAsync(targetMethod, resultAsJson);
                    break; 
                case ActionResponseAudienceEnum.AllPlayers:                    
                    for ( int i = 0; i < g.Participants.Count; i++ ) {
                        if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                            resultAsJson = new PlayerCentricGameView(g, i).AsJson();
                        }
                        await Clients.Group(g.Participants[i].ParticipantLevelSignalRGroupName).SendAsync(targetMethod, resultAsJson);
                    }   
                    break; 
                default:  
                    throw new System.Exception("7Studs User Exception: Unsupported response audience");  // e.g. Admin                                    
            }  
        }
    }
}
