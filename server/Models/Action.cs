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
            Type = actionType;
            UserName = user;
            ConnectionId = connectionId;
        }
        protected Game G { get; }  
        protected ActionEnum Type { get; set; }
        protected string UserName { get; set; }
        protected string ConnectionId { get; set; }
        public virtual string ProcessActionAndReturnUpdatedGameStateAsJson()
        {
            return G.AsJson(); // subclass should override this method 
        }
    }     
}  