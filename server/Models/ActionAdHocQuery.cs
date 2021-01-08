using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionAdHocQuery' Class  
    /// </summary>  
    public class ActionAdHocQuery : Action
    {  
        public ActionAdHocQuery(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers, string queryType) 
            : base( connectionId, actionType, ourGame, user, leavers, queryType)
        {
        }
        //public string QueryType;
        public override async Task ProcessAction()
        {
            R.AdHocQueryType = this.Parameters; // Note this so that QueryResultAsJson can pick it up
            this.ResponseType = ActionResponseTypeEnum.AdHocServerQuery; 
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;   

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
