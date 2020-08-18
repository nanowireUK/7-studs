namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionGetLog' Class  
    /// </summary>  
    public class ActionGetLog : Action
    {  
        public ActionGetLog(string connectionId, ActionEnum actionType, string gameId, string user) 
            : base(connectionId, actionType, gameId, user)
        {
        }

        public override string ProcessActionAndReturnUpdatedGameStateAsJson()
        {
            if ( G.LastEvent != "" ) {
                return G.AsJson(); // If the base class (i.e. this class) set an error message in the constructor then return without checking anything else
            }

            // This is ugly as it's returning the game log instead of the game state but as long as the client knows this it should be fine
            return G.GameLogAsJson(); 
        }
        public override void ProcessAction()
        {
            // Won't actually be called because the above method overrides the base class method that calls this, but C# needs it to be implemented
        }
    }     
}  
