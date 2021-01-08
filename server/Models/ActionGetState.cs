using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionGetState' Class  
    /// </summary>  
    public class ActionGetState : Action
    {  
        public ActionGetState(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers) 
            : base(connectionId, actionType, ourGame, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Doesn't actually do anything beyond letting the hub know what to return
            this.ResponseType = ActionResponseTypeEnum.OverallGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
