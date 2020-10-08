using Microsoft.AspNetCore.SignalR;
using System;

namespace SevenStuds.Models
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

        protected Action ( string connectionId, ActionEnum actionType, string gameId, string user, string leavers )
        {
            this.Initialise(connectionId, actionType, gameId, user, leavers, null);
        }
        protected Action ( string connectionId, ActionEnum actionType, string gameId, string user, string leavers, string parameters )
        {
            this.Initialise(connectionId, actionType, gameId, user, leavers, parameters);
        }
        protected void Initialise ( string connectionId, ActionEnum actionType, string gameId, string user, string leavers, string parameters )
        {
            // Do any initialisation that is common to all user actions.

            // If there is any failure that only needs to be reported back to the player who attempted the action, notify
            // them by throwing a HubException with an appropriate message. The client treats a HubException as a deliberate
            // means of feeding back details of a mistake that does not affect the game state and the user can rectify.

            // The ProcessAction() method can use the same technique as long as the game state has not changed.

            G = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            ActionType = actionType;
            UserName = user;
            Parameters = parameters;
            PlayerIndex = -1;
            ConnectionId = connectionId;
            ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // Default response type for actions
            ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // Default audience for action response   
            SignalRGroupNameForAdditionalNotifications = null; // Will stay that way in most cases 

            if ( G.IsRunningInTestMode() ) {
                ConnectionId = UserName; // Simulate a unique connection id as there won't be a separate connection for each player
            }

            // Ensure name is not blank
            if ( this.UserName == "" ) {
                throw new HubException("You tried to "+ActionType.ToString().ToLower()+" but your user name was blank"); // client catches this as part of action method, i.e. no call to separate client method required
            }

            // Check that this connection is not being used by someone with a different user name
            Participant p = G.GetParticipantFromConnection(ConnectionId);
            if ( p != null ) {
                if ( p.Name != this.UserName ) {
                    // This connection is already being used by someone else
                    throw new HubException("You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") from a connection that is already in use by "+p.Name); // client catches this as part of action method, i.e. no call to separate client method required
                }
            }

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
                // (note the game will no longer be in test mode at this stage, so IsRunningInTestMode() will show false even though this is as a result of testing)
                G.Participants[PlayerIndex].NoteConnectionId(this.ConnectionId);
                G.Participants[PlayerIndex].IsLockedOutFollowingReplay = false;
                // Can continue processing the command now 
            }
        }
        protected Game G { get; set; }  
        public ActionEnum ActionType { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
        public int PlayerIndex { get; set; }
        protected string ConnectionId { get; set; }
        public ActionResponseTypeEnum ResponseType { get; set; }
        public ActionResponseAudienceEnum ResponseAudience { get; set; }
        public string SignalRGroupNameForAdditionalNotifications { get; set; } // a bit of a botch to allow for a player who is leaving
        public virtual Game ProcessActionAndReturnGameReference()
        {
            this.ProcessAction(); // Use the subclass to implement the specifics of the action

            // Set a status message that combines the last event with the next action
            // (noting that NextAction may not have changed as a result of the current action)
            G.StatusMessage = G.LastEvent + ". " + G.NextAction; 

            if ( this.ActionType != ActionEnum.Replay 
                & this.ActionType != ActionEnum.Rejoin
                & this.ActionType != ActionEnum.GetLog
                & this.ActionType != ActionEnum.GetState
                & this.ActionType != ActionEnum.AdHocQuery ) 
            {
                G.LogActionWithResults(this); // Note: only log real game actions (not GetState, GetLog, Replay, Rejoin or AdHocQuery)
            }
            // After dealing with the requested action, reset the permissions for each action to reflect the updated game state
            G.SetActionAvailabilityBasedOnCurrentPlayer();
            return G;
        }        
        public abstract void ProcessAction();
    }     
}  