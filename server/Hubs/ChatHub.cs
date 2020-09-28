using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SevenStuds.Models;
using System.Collections.Generic;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // --------------------------------------------------------------------------------------------------
        // This is the server-side code that is called directly by the client

        public async Task UserClickedOpen(string gameId, string user) { await UserClickedActionButton(ActionEnum.Open, gameId,  user, ""); }
        public async Task UserClickedJoin(string gameId, string user) { await UserClickedActionButton(ActionEnum.Join, gameId,  user, ""); }
        public async Task UserClickedRejoin(string gameId, string user, string rejoinCode) { await UserClickedActionButton(ActionEnum.Rejoin, gameId,  user, rejoinCode); }
        public async Task UserClickedLeave(string gameId, string user) { await UserClickedActionButton(ActionEnum.Leave, gameId,  user, ""); }
        public async Task UserClickedStart(string gameId, string user) { await UserClickedActionButton(ActionEnum.Start, gameId,  user, ""); }
        public async Task UserClickedReveal(string gameId, string user) { await UserClickedActionButton(ActionEnum.Reveal, gameId,  user, ""); }
        public async Task UserClickedFinish(string gameId, string user) { await UserClickedActionButton(ActionEnum.Finish, gameId,  user, ""); }
        public async Task UserClickedCheck(string gameId, string user) { await UserClickedActionButton(ActionEnum.Check, gameId,  user, ""); }
        public async Task UserClickedCall(string gameId, string user) { await UserClickedActionButton(ActionEnum.Call, gameId,  user, ""); }
        public async Task UserClickedRaise(string gameId, string user, string raiseAmount) { await UserClickedActionButton(ActionEnum.Raise, gameId,  user, raiseAmount); }
        public async Task UserClickedCover(string gameId, string user) { await UserClickedActionButton(ActionEnum.Cover, gameId,  user, ""); }
        public async Task UserClickedFold(string gameId, string user) { await UserClickedActionButton(ActionEnum.Fold, gameId,  user, ""); }
        public async Task UserClickedGetState(string gameId, string user) { await UserClickedActionButton(ActionEnum.GetState, gameId,  user, ""); }
        public async Task UserClickedGetLog(string gameId, string user) { await UserClickedActionButton(ActionEnum.GetLog, gameId,  user, ""); }
        public async Task UserClickedReplay(string gameId, string user, string gameLog) { await UserClickedActionButton(ActionEnum.Replay, gameId,  user, gameLog); }

        // --------------------------------------------------------------------------------------------------
        // Internal methods
        private async Task UserClickedActionButton(ActionEnum actionType, string gameId, string user, string parameters)
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
                        //await Groups.AddToGroupAsync(conn, g.GameLevelSignalRGroupName);
                        await Groups.AddToGroupAsync(conn, p.ParticipantLevelSignalRGroupName);
                    }
                }
            }

            // Send the results of the action according to the ResponseType and ResponseAudience on the action object 

            string resultAsJson = ""; // default will be to set the result on a per-player basis
            string targetMethod = "ReceiveMyGameState";
            switch ( a.ResponseType )  
            { 
                case ActionResponseTypeEnum.PlayerCentricGameState:
                    // Result will be generated separately for each individual player so no point setting it here
                    break;   
                case ActionResponseTypeEnum.ConfirmPlayerLeaving:
                    // Result will be generated separately for each remaining player with the deleted player being notified separately
                    break;   
                case ActionResponseTypeEnum.GameLog:  
                    resultAsJson = g.GameLogAsJson();
                    targetMethod = "ReceiveGameLog";
                    break;
                case ActionResponseTypeEnum.OverallGameState:  
                    resultAsJson = g.AsJson();
                    targetMethod = "ReceiveOverallGameState";
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
                        if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState 
                            || a.ResponseType == ActionResponseTypeEnum.ConfirmPlayerLeaving ) {
                            resultAsJson = new PlayerCentricGameView(g, i).AsJson();
                        }
                        await Clients.Group(g.Participants[i].ParticipantLevelSignalRGroupName).SendAsync(targetMethod, resultAsJson);
                    }
                    if ( a.ResponseType == ActionResponseTypeEnum.ConfirmPlayerLeaving ) {
                        // Extra bit to notify all of the leaving player's connections that they have left 
                        string leavingConfirmationAsJson = "{ \"ok\" }";
                        await Clients.Group(a.SignalRGroupNameForAdditionalNotifications).SendAsync("ReceiveLeavingConfirmation", leavingConfirmationAsJson);
                        }
                    break; 
                 default:  
                    throw new System.Exception("7Studs User Exception: Unsupported response audience");  // e.g. Admin                                    
            }  
        }
    }
}
