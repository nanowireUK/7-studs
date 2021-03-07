using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialPokerClub.Models
{
    /// <summary>
    /// The 'Action' Abstract Class
    /// </summary>
    public abstract class Action
    {
        protected Action ()
        {
            // This parameterless constructor is not used in the main game logic, but is required to enable
            // the JSON deserialiser to create an empty data structure that it can then populate field-by-field
            // from the individual fields in the JSON structure
        }

        protected Action ( string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers )
        {
            this.Initialise(connectionId, actionType, ourGame, user, leavers, null);
        }
        protected Action ( string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers, string parameters )
        {
            this.Initialise(connectionId, actionType, ourGame, user, leavers, parameters);
        }
        protected void Initialise ( string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers, string parameters )
        {
            // Do any initialisation that is common to all user actions.

            // If there is any failure that only needs to be reported back to the player who attempted the action, notify
            // them by throwing a HubException with an appropriate message. The client treats a HubException as a deliberate
            // means of feeding back details of a mistake that does not affect the game state and the user can rectify.

            // The ProcessAction() method can use the same technique as long as the game state has not changed.

            G = ourGame;
            R = G.ParentRoom();
            ActionType = actionType;
            UserName = user;
            Parameters = parameters;
            PlayerIndex = -1;
            ConnectionId = connectionId;
            ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // Default response type for actions
            ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // Default audience for action response
            SignalRGroupNameForAdditionalNotifications = null; // Will stay that way in most cases

            bool triggeredByRealUser = ( connectionId != "" );
            if ( triggeredByRealUser && G.IsRunningInReplayMode() && ( this.ActionType != ActionEnum.Replay && this.ActionType != ActionEnum.Rejoin ) )
            {
                Console.WriteLine("Game was in replay mode but a user triggered an action other than Replay or Rejoin");
                Console.WriteLine("Game is being taken out of replay mode and will continue under normal conditions from here");
                G.SetReplayContext(null);
            }

            if ( G.IsRunningInReplayMode() && triggeredByRealUser == false ) {
                ConnectionId = UserName; // Simulate a unique connection id as there won't be a separate connection for players created via the replay process
            }

            // Ensure name is not blank
            if ( this.UserName == "" ) {
                throw new HubException("You tried to "+ActionType.ToString().ToLower()+" but your user name was blank"); // client catches this as part of action method, i.e. no call to separate client method required
            }

            // JOIN is a bit of a special case we do some checks here ahead of the more generic permissions testing
            if ( actionType == ActionEnum.Join ) {
                if ( Parameters == "RoomCannotExist" && G.Participants.Count > 0 ) {
                    throw new HubException(SpcExceptionCodes.RoomAlreadyExists.ToString());
                }
                if ( Parameters == "RoomMustExist" && G.Participants.Count == 0 ) {
                    throw new HubException(SpcExceptionCodes.RoomDoesNotExist.ToString());
                }
                if ( G.GameMode != GameModeEnum.LobbyOpen ) {
                    throw new HubException(SpcExceptionCodes.CannotJoinGameInProgress.ToString());
                }
            }

            // Check that this connection is not being used by someone with a different user name
            Participant p = ServerState.StatefulData.GetParticipantFromConnection(G, ConnectionId);
            if ( p != null ) {
                if ( p.Name != this.UserName ) {
                    // This connection is already being used by someone else
                    throw new HubException("You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") from a connection that is already in use by "+p.Name); // client catches this as part of action method, i.e. no call to separate client method required
                }
            }
            Spectator s = ServerState.StatefulData.GetSpectatorFromConnection(G, ConnectionId);
            if ( s != null ) {
                if ( s.Name != this.UserName ) {
                    // This connection is already being used by someone else
                    throw new HubException("You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") from a connection that is already in use by "+s.Name); // client catches this as part of action method, i.e. no call to separate client method required
                }
            }

            // Check whether player is a spectator
            int spectatorIndex = G.SpectatorIndexFromName(user);

            if ( spectatorIndex != -1 ) {
                // Check permissions for spectators
                if ( ! G.ActionIsAvailableToSpectator(ActionType, spectatorIndex) ) {
                    throw new HubException("You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") but this option is not available to you at this point"); // client catches this as part of action method, i.e. no call to separate client method required
                }
            }
            else {
                // Check permissions for registered players

                // Check player has permission to trigger this action
                PlayerIndex = G.PlayerIndexFromName(user);
                if ( ! G.ActionIsAvailableToPlayer(ActionType, PlayerIndex) ) {
                    throw new HubException("You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") but this option is not available to you at this point"); // client catches this as part of action method, i.e. no call to separate client method required
                }

                // Check that number of players who have left the game is as expected (i.e. that someone has not left in between)

                int leaversAsInt;
                bool leaversIsNumeric = int.TryParse(leavers, out leaversAsInt);
                if ( !leaversIsNumeric) {
                    leaversAsInt = -1; // -1 means don't check
                }
                if ( leaversAsInt != -1 && leaversAsInt != G.CountOfLeavers ) {
                    throw new HubException("A player has just left the game."
                        + " This has updated the game state and might have affected whose turn it is and what they can do."
                        + " Your requested move has been ignored as a result. If it is still your turn, please try again.");
                    // Note: client catches this as part of action method, i.e. no call to separate client method required
                }

                if ( p == null /* from above */ && PlayerIndex != -1 && G.Participants[PlayerIndex].IsLockedOutFollowingReplay == true ) {
                    // This is a new connection AND the player is currently locked out following a 'replay' action,
                    // so do an implicit Rejoin by linking the new connection to the current player
                    // (note the game will no longer be in test mode at this stage, so IsRunningInReplayMode() will show false even though this is as a result of testing)
                    //G.Participants[PlayerIndex].NoteConnectionId(this.ConnectionId);
                    ServerState.StatefulData.LinkConnectionToGroup(this.G, this.ConnectionId, G.Participants[PlayerIndex]);                    
                    G.Participants[PlayerIndex].IsLockedOutFollowingReplay = false;
                    // Can continue processing the command now
                }
                if ( this.ActionType != ActionEnum.Replay && this.ActionType != ActionEnum.Rejoin ) {
                    G.RoundNumberIfCardsJustDealt = -1; // The new action will clear any requirement for the client to animate the dealing of the cards
                    foreach ( Participant p1 in G.Participants ) {
                        // Clear this as someone else is now doing something (this helps ensure the reveal is animated only once)
                        p1.HasJustSharedHandDetails = false;
                    }
                }
            }
        }
        protected Room R { get; set; }
        protected Game G { get; set; }
        public ActionEnum ActionType { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
        public int PlayerIndex { get; set; }
        protected string ConnectionId { get; set; }
        public ActionResponseTypeEnum ResponseType { get; set; }
        public ActionResponseAudienceEnum ResponseAudience { get; set; }
        public string SignalRGroupNameForAdditionalNotifications { get; set; } // a bit of a botch to allow for a player who is leaving
        public virtual async Task<Game> ProcessActionAndReturnGameReference()
        {
            await this.ProcessAction(); // Use the subclass to implement the specifics of the action
            ServerState.TotalActionsProcessed++;

            // Set a status message that combines the last event with the next action
            // (noting that NextAction may not have changed as a result of the current action)
            G.StatusMessage = G.LastEvent + ". " + G.NextAction;
            // After dealing with the requested action, reset the permissions for each action to reflect the updated game state
            G.SetActionAvailabilityBasedOnCurrentPlayer();
            G.GameStatistics.UpdateStatistics(G); // Record game times etc.

            var dbTasks = new List<Task<double>>();

            if ( this.ActionType != ActionEnum.Replay
                & this.ActionType != ActionEnum.Rejoin
                & this.ActionType != ActionEnum.GetLog
                & this.ActionType != ActionEnum.GetState
                & this.ActionType != ActionEnum.AdHocQuery )
            {
                // Note: only log real game actions (not GetState, GetLog, Replay, Rejoin or AdHocQuery)
                dbTasks.Add(G.LogActionWithResults(this));
            }

            if ( this.ActionType != ActionEnum.Replay
                & this.ActionType != ActionEnum.GetLog
                & this.ActionType != ActionEnum.GetState
                & this.ActionType != ActionEnum.AdHocQuery )
            {
                // Note: Only update game state following actions that would normally change the game state
                dbTasks.Add(ServerState.OurDB.UpsertGameState(G));
            }

            await Task.WhenAll(dbTasks); // Wait until all of the DB tasks completed
            foreach (Task<double> t in dbTasks) {
                G.AddToAccumulatedDbCost(this.ActionType.ToString(), t.Result);
            }
            return G;
        }
        public abstract Task ProcessAction();
    }
}