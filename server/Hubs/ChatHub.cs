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

        public async Task UserClickedOpen(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Open, roomId,  user, leavers,  ""); }
        public async Task UserClickedJoin(string roomId, string user) { await UserClickedActionButton(ActionEnum.Join, roomId,  user, "-1",  ""); }
        public async Task UserClickedSpectate(string roomId, string user) { await UserClickedActionButton(ActionEnum.Spectate, roomId,  user, "-1",  ""); }
        public async Task UserClickedRejoin(string roomId, string user, string rejoinCode) { await UserClickedActionButton(ActionEnum.Rejoin, roomId,  user, "-1",  rejoinCode); }
        public async Task UserClickedLeave(string roomId, string user) { await UserClickedActionButton(ActionEnum.Leave, roomId,  user, "-1",  ""); }
        public async Task UserClickedStart(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Start, roomId,  user, leavers,  ""); }
        public async Task UserClickedReveal(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Reveal, roomId,  user, leavers,  ""); }
        public async Task UserClickedContinue(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Continue, roomId,  user, leavers,  ""); }
        public async Task UserClickedCheck(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Check, roomId,  user, leavers,  ""); }
        public async Task UserClickedCall(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Call, roomId,  user, leavers,  ""); }
        public async Task UserClickedRaise(string roomId, string user, string leavers, string raiseAmount) { await UserClickedActionButton(ActionEnum.Raise, roomId,  user, leavers,  raiseAmount); }
        public async Task UserClickedCover(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Cover, roomId,  user, leavers,  ""); }
        public async Task UserClickedFold(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.Fold, roomId,  user, leavers,  ""); }
        public async Task UserClickedGetState(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.GetState, roomId,  user, leavers,  ""); }
        public async Task UserClickedGetLog(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.GetLog, roomId,  user, leavers,  ""); }
        public async Task UserClickedReplay(string roomId, string user, string leavers, string gameLog) { await UserClickedActionButton(ActionEnum.Replay, roomId,  user, leavers,  gameLog); }
        public async Task UserClickedGetMyState(string roomId, string user, string leavers) { await UserClickedActionButton(ActionEnum.GetMyState, roomId,  user, leavers,  ""); }
        public async Task UserClickedAdHocQuery(string roomId, string user, string leavers, string queryNum) { await UserClickedActionButton(ActionEnum.AdHocQuery, roomId,  user, leavers,  queryNum); }

        // --------------------------------------------------------------------------------------------------
        // Internal methods

        private async Task UserClickedActionButton(ActionEnum actionType, string roomId, string user, string leavers, string parameters)
        {
            Action a = ActionFactory.NewAction(Context.ConnectionId, actionType, roomId, user, leavers, parameters);
            Room r = await a.ProcessActionAndReturnRoomReference();
            Game g = r.ActiveGame;

            // New connections may have been linked to players, so link those connections to the relevant game and player groups in SignalR
            foreach ( Participant p in g.Participants ) {
                List<string> conns = p.GetConnectionIds();
                for ( int i = 0; i < conns.Count; i++ ) {
                    // If the game has not yet added this connection, then link it
                    string conn = conns[i];
                    if ( g.GetParticipantFromConnection(conn) == null ) {
                        g.LinkConnectionToParticipant(conn, p);
                        await Groups.AddToGroupAsync(conn, p.ParticipantLevelSignalRGroupName);
                    }
                }
            }

            // New connections may have been linked to spectators, so link those connections to the relevant game and player groups in SignalR
            foreach ( Spectator p in g.Spectators ) {
                List<string> conns = p.GetConnectionIds();
                for ( int i = 0; i < conns.Count; i++ ) {
                    // If the game has not yet added this connection, then link it
                    string conn = conns[i];
                    if ( g.GetSpectatorFromConnection(conn) == null ) {
                        g.LinkConnectionToSpectator(conn, p);
                        await Groups.AddToGroupAsync(conn, p.SpectatorLevelSignalRGroupName);
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
                case ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers:
                    // Result will be generated separately for each remaining player with the deleted player being notified separately
                    break;
                case ActionResponseTypeEnum.GameLog:
                    resultAsJson = g.GameLogAsJson();
                    targetMethod = "ReceiveGameLog";
                    break;
                case ActionResponseTypeEnum.OverallGameState:
                    AddRejoinCodes(g);
                    resultAsJson = g.AsJson();
                    targetMethod = "ReceiveOverallGameState";
                    break;
                case ActionResponseTypeEnum.AdHocServerQuery:
                    resultAsJson = string.Join("", r.AdHocQueryResult());
                    targetMethod = "ReceiveAdHocServerData";
                    break;
                default:
                    throw new System.Exception("7Studs User Exception: Unsupported response type");
            }

            // Now send the appropriate response to the players indicated by the action's ResponseAudience setting
            switch ( a.ResponseAudience )
            {
                case ActionResponseAudienceEnum.Caller:
                    if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                        resultAsJson = new PlayerCentricGameView(g, a.PlayerIndex, -1).AsJson();
                    }
                    await Clients.Caller.SendAsync(targetMethod, resultAsJson);
                    break;
                case ActionResponseAudienceEnum.CurrentPlayer:
                    if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                        resultAsJson = new PlayerCentricGameView(g, a.PlayerIndex, -1).AsJson();
                    }
                    await Clients.Group(g.Participants[a.PlayerIndex].ParticipantLevelSignalRGroupName).SendAsync(targetMethod, resultAsJson);
                    break;
                case ActionResponseAudienceEnum.AllPlayers:
                    // Send personalised views to all players sat around the table
                    for ( int i = 0; i < g.Participants.Count; i++ ) {
                        if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState
                            || a.ResponseType == ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers ) {
                            // Send each player (including any leaving player) a view of the game from their own perspective
                            resultAsJson = new PlayerCentricGameView(g, i, -1).AsJson();
                        }
                        await Clients.Group(g.Participants[i].ParticipantLevelSignalRGroupName).SendAsync(targetMethod, resultAsJson);
                    }
                    // For any spectators, send a view from the dealer's perspective but with all face-down cards obscured
                    // Note that we have to build a view for each spectator as each spectator has their own rejoin code (this is a bit messy)
                    if ( g.Spectators.Count > 0) {
                        for ( int i = 0; i < g.Spectators.Count; i++ ) {
                            if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState
                                || a.ResponseType == ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers ) {
                                // Send each player (including any leaving player) a view of the game from their own perspective
                                resultAsJson = new PlayerCentricGameView(g, -1, i).AsJson();
                            }
                            await Clients.Group(g.Spectators[i].SpectatorLevelSignalRGroupName).SendAsync(targetMethod, resultAsJson);
                        }
                    } 
                    if ( a.ResponseType == ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers ) {
                        // Extra bit to notify all of the leaving player's connections that they have left
                        string leavingConfirmationAsJson = "{ \"ok\" }";
                        await Clients.Group(a.SignalRGroupNameForAdditionalNotifications).SendAsync("ReceiveLeavingConfirmation", leavingConfirmationAsJson);
                        }
                    break;
                 default:
                    throw new System.Exception("7Studs User Exception: Unsupported response audience");  // e.g. Admin
            }
        }
        private void AddRejoinCodes(Game g) {
            g.rejoinCodes = new List<string>();
            foreach ( Participant p in g.Participants ) {
                g.rejoinCodes.Add( ( p.IsGameAdministrator ? "*" : "" ) + p.Name + ": " + p.RejoinCode );
            }
        }
    }
}
