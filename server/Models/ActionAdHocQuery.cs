using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionAdHocQuery' Class  
    /// </summary>  
    public class ActionAdHocQuery : Action
    {  
        public ActionAdHocQuery(string connectionId, ActionEnum actionType, string roomId, string user, string leavers, string queryType) 
            : base( connectionId, actionType, roomId, user, leavers, queryType)
        {
        }
        //public string QueryType;
        public override async Task ProcessAction()
        {
            R.AdHocQueryType = this.Parameters; // Note this so that QueryResultAsJson can pick it up
            this.ResponseType = ActionResponseTypeEnum.AdHocServerQuery; 
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;                  
        }
    }     
}  
