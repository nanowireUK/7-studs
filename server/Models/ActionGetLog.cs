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
        public override void ProcessAction()
        {
            // Doesn't actually do anything beyond letting the hub know what to return
            this.ResponseType = ActionResponseTypeEnum.GameLog;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;
        }
    }     
}  
