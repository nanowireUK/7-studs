using Microsoft.AspNetCore.SignalR;

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

        protected Action ( string connectionId, ActionEnum actionType, string gameId, string user )
        {
            this.Initialise(connectionId, actionType, gameId, user, null);
        }
        protected Action ( string connectionId, ActionEnum actionType, string gameId, string user, string parameters )
        {
            this.Initialise(connectionId, actionType, gameId, user, parameters);
        }
        protected void Initialise ( string connectionId, ActionEnum actionType, string gameId, string user, string parameters )
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
        public virtual Game ProcessActionAndReturnGameReference()
        {
            this.ProcessAction(); // Use the subclass to implement the specifics of the action

            if ( this.ActionType != ActionEnum.Replay 
                & this.ActionType != ActionEnum.Rejoin
                & this.ActionType != ActionEnum.GetLog
                & this.ActionType != ActionEnum.GetState ) 
            {
                G.LogActionWithResults(this); // only do this for real game actions (not GetState, GetLog, Replay, Rejoin)
            }

            // After dealing with the requested action, reset the permissions for each action to reflect the updated game state
            G.SetActionAvailabilityBasedOnCurrentPlayer();
            G.StatusMessage = G.LastEvent + ". " + G.NextAction; // Note that NextAction may not have changed as a result of the current action

            return G;
        }        
        public abstract void ProcessAction();
    }     
}  