namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionGetMyState' Class  
    /// </summary>  
    public class ActionGetMyState : Action
    {  
        public ActionGetMyState(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override void ProcessAction()
        {
            // This is not part of the game but enables the server web page to re-get the player's state for convenience after calling other admin functions
            this.ResponseType = ActionResponseTypeEnum.PlayerCentricGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;
        }
    }     
}  
