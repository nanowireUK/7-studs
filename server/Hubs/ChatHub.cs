using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SevenStuds.Models;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // --------------------------------------------------------------------------------------------------
        // This is the server-side code that is called directly by the client

        public async Task UserClickedCreateAndJoinRoom(string roomId, string user) { await UserClickedGameRelatedActionButton(ActionEnum.Join, roomId,  user, "-1", "RoomCannotExist"); }
        public async Task UserClickedJoinExistingRoom(string roomId, string user) { await UserClickedGameRelatedActionButton(ActionEnum.Join, roomId,  user, "-1", "RoomMustExist"); }
        public async Task UserClickedJoin(string roomId, string user) { await UserClickedGameRelatedActionButton(ActionEnum.Join, roomId,  user, "-1",  "RoomCanExist"); }
        public async Task UserClickedOpen(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Open, roomId,  user, leavers,  ""); }
        public async Task UserClickedSpectate(string roomId, string user) { await UserClickedGameRelatedActionButton(ActionEnum.Spectate, roomId,  user, "-1",  ""); }
        public async Task UserClickedRejoin(string roomId, string user, string rejoinCode) { await UserClickedGameRelatedActionButton(ActionEnum.Rejoin, roomId,  user, "-1",  rejoinCode); }
        public async Task UserClickedLeave(string roomId, string user) { await UserClickedGameRelatedActionButton(ActionEnum.Leave, roomId,  user, "-1",  ""); }
        public async Task UserClickedStart(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Start, roomId,  user, leavers,  ""); }
        public async Task UserClickedReveal(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Reveal, roomId,  user, leavers,  ""); }
        public async Task UserClickedContinue(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Continue, roomId,  user, leavers,  ""); }
        public async Task UserClickedCheck(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Check, roomId,  user, leavers,  ""); }
        public async Task UserClickedCall(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Call, roomId,  user, leavers,  ""); }
        public async Task UserClickedRaise(string roomId, string user, string leavers, string raiseAmount) { await UserClickedGameRelatedActionButton(ActionEnum.Raise, roomId,  user, leavers,  raiseAmount); }
        public async Task UserClickedCover(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Cover, roomId,  user, leavers,  ""); }
        public async Task UserClickedFold(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.Fold, roomId,  user, leavers,  ""); }
        public async Task UserClickedBlindIntent(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.BlindIntent, roomId,  user, leavers,  ""); }
        public async Task UserClickedBlindReveal(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.BlindReveal, roomId,  user, leavers,  ""); }
        public async Task UserClickedGetState(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.GetState, roomId,  user, leavers,  ""); }
        public async Task UserClickedGetLog(string gameId) { await UserClickedGetLogButton(gameId); } // NOTE: not a standard action
        public async Task UserClickedReplaySetup(string gameLog) { await UserClickedReplaySetupButton(gameLog); } // NOTE: not a standard action
        public async Task UserClickedReplay(string roomId, string user, string leavers, string replayOption) { await UserClickedGameRelatedActionButton(ActionEnum.Replay, roomId,  user, leavers, replayOption ); }
        public async Task UserClickedGetMyState(string roomId, string user, string leavers) { await UserClickedGameRelatedActionButton(ActionEnum.GetMyState, roomId,  user, leavers,  ""); }
        public async Task UserClickedAdHocQuery(string roomId, string user, string leavers, string queryNum) { await UserClickedGameRelatedActionButton(ActionEnum.AdHocQuery, roomId,  user, leavers,  queryNum); }
        public async Task UserClickedUpdateLobbySettings(string roomId, string user, string settingsAsJson) { await UserClickedGameRelatedActionButton(ActionEnum.UpdateLobbySettings, roomId,  user, "-1", settingsAsJson); }

        // --------------------------------------------------------------------------------------------------
        // Internal methods

        private async Task UserClickedGameRelatedActionButton(ActionEnum actionType, string roomId, string user, string leavers, string parameters)
        {
            SevenStuds.Models.Action a = await ActionFactory.NewAction(Context.ConnectionId, actionType, roomId, user, leavers, parameters);
            Game g = await a.ProcessActionAndReturnGameReference();
         
            // New connections may have been linked to players, so link those connections to the relevant player groups in SignalR
            foreach ( Participant p in g.Participants ) {
                List<string> conns = p.GetConnectionIds();
                for ( int i = 0; i < conns.Count; i++ ) {
                    // If the game has not yet added this connection, then link it
                    string conn = conns[i];
                    if ( ServerState.GetParticipantFromConnection(g, conn) == null ) {
                        ServerState.LinkConnectionToParticipant(g, conn, p);
                        await Groups.AddToGroupAsync(conn, p.ParticipantSignalRGroupName);
                    }
                }
            }

            // New connections may have been linked to spectators, so link those connections to the relevant player groups in SignalR
            foreach ( Spectator p in g.Spectators ) {
                List<string> conns = p.GetConnectionIds();
                for ( int i = 0; i < conns.Count; i++ ) {
                    // If the game has not yet added this connection, then link it
                    string conn = conns[i];
                    if ( ServerState.GetSpectatorFromConnection(g, conn) == null ) {
                        ServerState.LinkConnectionToSpectator(g, conn, p);
                        await Groups.AddToGroupAsync(conn, p.SpectatorSignalRGroupName);
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
                // case ActionResponseTypeEnum.GameLog:
                //     resultAsJson = g.GameLogAsJson();
                //     targetMethod = "ReceiveGameLog";
                //     break;
                case ActionResponseTypeEnum.OverallGameState:
                    AddRejoinCodes(g);
                    resultAsJson = g.AsJson();
                    targetMethod = "ReceiveOverallGameState";
                    break;
                case ActionResponseTypeEnum.AdHocServerQuery:
                    resultAsJson = string.Join("", g.ParentRoom().AdHocQueryResult());
                    targetMethod = "ReceiveAdHocServerData";
                    break;
                default:
                    throw new System.Exception("7Studs User Exception: Unsupported response type");
            }

            // Now send the appropriate response to the players indicated by the action's ResponseAudience setting
            var notificationTasks = new List<Task>();
            switch ( a.ResponseAudience )
            {
                case ActionResponseAudienceEnum.Caller:
                    if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                        resultAsJson = new PlayerCentricGameView(g, a.PlayerIndex, -1).AsJson();
                    }
                    notificationTasks.Add(Clients.Caller.SendAsync(targetMethod, resultAsJson));
                    break;
                case ActionResponseAudienceEnum.CurrentPlayer:
                    if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState ) {
                        resultAsJson = new PlayerCentricGameView(g, a.PlayerIndex, -1).AsJson();
                    }
                    notificationTasks.Add(Clients.Group(g.Participants[a.PlayerIndex].ParticipantSignalRGroupName).SendAsync(targetMethod, resultAsJson));
                    break;
                case ActionResponseAudienceEnum.AllPlayers:
                    // Send personalised views to all players sat around the table
                    for ( int i = 0; i < g.Participants.Count; i++ ) {
                        if ( a.ResponseType == ActionResponseTypeEnum.PlayerCentricGameState
                            || a.ResponseType == ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers ) {
                            // Send each player (including any leaving player) a view of the game from their own perspective
                            resultAsJson = new PlayerCentricGameView(g, i, -1).AsJson();
                        }
                        notificationTasks.Add(Clients.Group(g.Participants[i].ParticipantSignalRGroupName).SendAsync(targetMethod, resultAsJson));
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
                            notificationTasks.Add(Clients.Group(g.Spectators[i].SpectatorSignalRGroupName).SendAsync(targetMethod, resultAsJson));
                        }
                    } 
                    if ( a.ResponseType == ActionResponseTypeEnum.ConfirmToPlayerLeavingAndUpdateRemainingPlayers ) {
                        // Extra bit to notify all of the leaving player's connections that they have left
                        string leavingConfirmationAsJson = "{ \"ok\" }";
                        notificationTasks.Add(Clients.Group(a.SignalRGroupNameForAdditionalNotifications).SendAsync("ReceiveLeavingConfirmation", leavingConfirmationAsJson));
                        }
                    break;
                 default:
                    throw new System.Exception("7Studs User Exception: Unsupported response audience");  // e.g. Admin
            }
            await Task.WhenAll(notificationTasks); // Wait until all of the notification tasks completed
        }
        private async Task UserClickedReplaySetupButton(string gameLogToReplay)
        {
            // This is a non-standard action that does not require a game or a player as it automatically builds a room
            // and a game that includes all the players from the replayed game. 
            // The response to the caller will just be a JSON doc containing a room id and a set of rejoin codes.
            // Note that you can pause the game at a specific action by including a 'pauseAfter' attribute in the game log
            // (the replay will be paused after any action with a number qual to or higher than the pauseAfter number)
            Room replayRoom = null;
            GameLog replayContext = JsonSerializer.Deserialize<GameLog>(gameLogToReplay);
            replayContext.ListDecks(); // Lists decks for info in debug console

            // Now create a new Game which we will apply the previously-recorded actions to one step at a time
            System.Diagnostics.Debug.WriteLine("Creating a new Room and Game in which to replay a game from supplied game log");
            replayRoom = await ServerState.FindOrCreateRoom("Replay-"+DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss"));
            Game replayGame = await ServerState.LoadOrRecoverOrCreateGame(replayRoom); // Should end up just creating it (no game exists in the room)
            replayGame.InitialiseGame(replayContext); // Initialise the replayed game and also stores the game log statefully on the replay Room
            // Add all the players (note that the replay log should not contain join actions for the players who joined at the start of the game)
            foreach ( string playerName in replayContext.playersInOrderAtStartOfGame ) {
                Participant newPlayer = new Participant(playerName);
                newPlayer.IsGameAdministrator = ( playerName == replayContext.administrator ); 
                newPlayer.IntendsToPlayBlindInNextHand = replayContext.playersStartingBlind.Contains(playerName);
                replayGame.Participants.Add(newPlayer);
            }
            replayGame.SetActionAvailabilityBasedOnCurrentPlayer(); // Ensures the initial selection of available actions is set
            Console.WriteLine("Replay is saving initial game state for replayed game id '{0}'\n", replayGame.GameId);
            double dbCost = await ServerState.OurDB.UpsertGameState(replayGame); // Store the initial game state for the game that we are recreating (or note that no DB is being used)
            replayGame.AddToAccumulatedDbCost("Initial save of replayed game", dbCost);

            // Replay each game action in the recorded order (including joins and starts)
            bool actionSucceeded;
            int inconsistenciesFound = 0;
            for ( int actionIndex = 0; actionIndex < replayContext.actions.Count; actionIndex++ ) {
                replayContext.indexOfLastReplayedAction += 1;
                GameLogAction gla = replayContext.actions[actionIndex];
                actionSucceeded = await ActionReplay.ReplayAction(replayRoom, gla); // use the same single-action replay method as the in-game replay methods use
                inconsistenciesFound += ( actionSucceeded == true ? 0 : 1 );
                replayContext.indexOfLastReplayedAction = actionIndex;
                if ( replayContext.pauseAfter > 0 && gla.ActionNumber >= replayContext.pauseAfter ) {
                    // The user asked us to pause after processing a given action (and to leave the replay in a paused state that can be stepped through)
                    System.Diagnostics.Debug.WriteLine("Replay paused due to pauseAfter ("+replayContext.pauseAfter
                        +") request ... single step via Replay with no parameters or advance multiple steps by specifying target action number");
                    break;
                }
            }
            // Mark each player as locked ... this will be unlocked once someone rejoins the newly created game
            foreach ( Participant p in replayGame.Participants ) {
                p.IsLockedOutFollowingReplay = true;
            }
            System.Diagnostics.Debug.WriteLine("Use the game state to find each player and rejoin each of them from a separate browser using their respective rejoin codes.\n");
            System.Diagnostics.Debug.WriteLine("Replay room is '" + replayRoom.RoomId + "'\n");
            // Send the results of the action according to the ResponseType and ResponseAudience on the action object
            ReplayResponse response = new ReplayResponse(replayGame);
            string resultAsJson = response.AsJson();
            await Clients.Caller.SendAsync("ReceiveMyGameState", resultAsJson);
        }
       private async Task UserClickedGetLogButton(string gameId)
        {
            // This is a non-standard action that does not require a game or a player as it just loads a historical game log from the database.
            // The response to the caller will just be a JSON doc containing the game log.
            System.Diagnostics.Debug.WriteLine("Attempting to load historical game log for game with id {0}\n", gameId);
            GameLog gl = await ServerState.OurDB.LoadGameLog(gameId);
            if ( gl == null ) { return; }
            // Check that game is at least 15 minutes old (i.e. 15 mins from last move)
            int minAge = 15;
            double lastMoveMinutesAgo = ( DateTimeOffset.UtcNow - gl.endTimeUtc ).TotalMinutes;
            if ( lastMoveMinutesAgo < minAge && ServerState.AllowTestFunctions() == false ) {
                string msg = "Last move in game occurred "+lastMoveMinutesAgo+" minutes ago. Log is not available until "+minAge+" minutes after last move";
                System.Diagnostics.Debug.WriteLine(msg+"\n");
                await Clients.Caller.SendAsync("ReceiveMyGameState", "{"+msg+"}");
            }
            else {
                string resultAsJson = gl.AsJson();
                await Clients.Caller.SendAsync("ReceiveMyGameState", resultAsJson);
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
