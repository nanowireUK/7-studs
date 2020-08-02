namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'Action' Abstract Class  
    /// </summary>  
    public class Action  
    {  
        protected Action ( ActionEnum actionType, string gameId, string user, string connectionId )
        {
            G = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            ActionType = actionType;
            UserName = user;
            ConnectionId = connectionId;
            PlayerIndex = -1;
            G.LastEvent = ""; // Clear this before running standard verifications. 

            // This constructor does some basic validation and sets G.LastEvent if any errors are found.
            // If this variable has a value, the Action subclass's ProcessActionAndReturnUpdatedGameStateAsJson implementation
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
        protected Game G { get; }  
        protected ActionEnum ActionType { get; set; }
        protected string UserName { get; set; }
        protected int PlayerIndex { get; set; }
        protected string ConnectionId { get; set; }
        public virtual string ProcessActionAndReturnUpdatedGameStateAsJson()
        {
            // This method should be overridden. It should contain the following code as a minimum:

            if ( G.LastEvent != "" ) {
                return G.AsJson(); // Base class set an error message so return without checking anything else
            }
            /*
                Actual implementation of the action goes here
            */
            return G.AsJson(); // subclass should override this method 
        }
    }     
}  