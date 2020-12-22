using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionGetState' Class  
    /// </summary>  
    public class ActionGetState : Action
    {  
        public ActionGetState(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Doesn't actually do anything beyond letting the hub know what to return
            this.ResponseType = ActionResponseTypeEnum.OverallGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;
        }
    }     
}  
