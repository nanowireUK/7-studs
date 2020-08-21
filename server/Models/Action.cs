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
            // Implemented only to enable serialisation for use in the game log
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
            G = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            ActionType = actionType;
            UserName = user;
            Parameters = parameters;
            PlayerIndex = -1;
            ConnectionId = connectionId;
            ResponseType = ActionResponseTypeEnum.PlayerCentricGameState; // Default response type for actions
            ResponseAudience =  ActionResponseAudienceEnum.AllPlayers; // Default audience for action response          

            G.LastEvent = ""; // Clear this before running standard verifications. 

            if ( G.IsRunningInTestMode() ) {
                ConnectionId = UserName; // Simulate a unique connection id as there won't be a separate connection for each player
            }

            // This constructor does some basic validation and sets G.LastEvent if any errors are found.
            // If this variable has a value, the Action subclass's ProcessAction implementation
            // should 'return G.AsJson();' immediately (see sample implementation below).
            // TODO: There's probably a cleaner OOP way of doing this but this will do for now

            // Ensure name is not blank
            if ( this.UserName == "" ) {
                G.LastEvent = "You tried to "+ActionType.ToString().ToLower()+" but your user name was blank";
                throw new HubException(G.LastEvent); // client catches this as part of action method, i.e. no call to separate client method required
            }

            // Check that this connection is not being used by someone with a different user name
            Participant p = G.GetParticipantFromConnection(ConnectionId);
            if ( p != null ) {
                if ( p.Name != this.UserName ) {
                    // This connection is already being used by someone else
                    G.LastEvent = "You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") from a connection that is already in use by "+p.Name;
                    throw new HubException(G.LastEvent); // client catches this as part of action method, i.e. no call to separate client method required
                }
            }

            // Check player has permission to trigger this action
            PlayerIndex = G.PlayerIndexFromName(user);
            if ( ! G.ActionIsAvailableToPlayer(ActionType, PlayerIndex) ) {
                G.LastEvent = "You attempted to "+ActionType.ToString().ToLower()+" (as user "+this.UserName+") but this option is not available to you at this point";
                throw new HubException(G.LastEvent); // client catches this as part of action method, i.e. no call to separate client method required
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
            if ( G.LastEvent != "" ) {
                return G; // If the base class (i.e. this class) set an error message in the constructor then return without checking anything else
            }

            this.ProcessAction(); // Use the subclass to implement the specifics of the action

            if ( this.ActionType != ActionEnum.Replay 
                & this.ActionType != ActionEnum.Rejoin
                & this.ActionType != ActionEnum.GetLog
                & this.ActionType != ActionEnum.GetState ) 
            {
                G.LogActionWithResults(this); // only do this for real game actions (not GetState, GetLog, Replay, Rejoin)
            }

            G.SetActionAvailabilityBasedOnCurrentPlayer(); 

            return G;
        }        
        public abstract void ProcessAction();
    }     
}  