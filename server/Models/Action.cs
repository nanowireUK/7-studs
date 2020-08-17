namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'Action' Abstract Class  
    /// </summary>  
    public abstract class Action  
    { 
        protected Action () 
        {
            // Implemented only to enable serialisation
        }

        protected Action ( ActionEnum actionType, string gameId, string user )
        {
            this.Initialise(actionType, gameId, user, null);
        }
        protected Action ( ActionEnum actionType, string gameId, string user, string parameters )
        {
            this.Initialise(actionType, gameId, user, parameters);
        }
        protected void Initialise ( ActionEnum actionType, string gameId, string user, string parameters )
        {
            G = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            ActionType = actionType;
            UserName = user;
            Parameters = parameters;
            PlayerIndex = -1;
            G.LastEvent = ""; // Clear this before running standard verifications. 

            // This constructor does some basic validation and sets G.LastEvent if any errors are found.
            // If this variable has a value, the Action subclass's ProcessAction implementation
            // should 'return G.AsJson();' immediately (see sample implementation below).
            // TODO: There's probably a cleaner OOP way of doing this but this will do for now

            // Ensure name is not blank
            if ( this.UserName == "" ) {
                G.LastEvent = "Someone tried to "+ActionType.ToString().ToLower()+" but user name was blank";
                return;
            }
            // Check player has permission to trigger this action
            PlayerIndex = G.PlayerIndexFromName(user);
            if ( ! G.ActionIsAvailableToPlayer(ActionType, PlayerIndex) ) {
                G.LastEvent = "User " + UserName + " tried to "+ActionType.ToString().ToLower()+" but this option is not available to them at this stage";
                return;
            }
        }
        protected Game G { get; set; }  
        public ActionEnum ActionType { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
        protected int PlayerIndex { get; set; }
        protected string RejoinCode { get; set; }
        public virtual string ProcessActionAndReturnUpdatedGameStateAsJson()
        {
            if ( G.LastEvent != "" ) {
                return G.AsJson(); // If the base class (i.e. this class) set an error message in the constructor then return without checking anything else
            }

            this.ProcessAction(); // Use the subclass to implement the specifics of the actionhis

            if ( this.ActionType != ActionEnum.Replay 
                & this.ActionType != ActionEnum.GetLog
                & this.ActionType != ActionEnum.GetState ) 
            {
                G.LogActionWithResults(this); // only do this for real game actions (not GetState, GetLog, Replay)
            }

            return G.AsJson();
        }
        public abstract void ProcessAction();
    }     
}  