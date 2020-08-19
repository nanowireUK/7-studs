namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionGetState' Class  
    /// </summary>  
    public class ActionGetState : Action
    {  
        public ActionGetState(string connectionId, ActionEnum actionType, string gameId, string user) 
            : base(connectionId, actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {
            // Doesn't actually do anything beyond letting the hub know what to return
            this.ResponseType = ActionResponseTypeEnum.OverallGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;
        }
    }     
}  
