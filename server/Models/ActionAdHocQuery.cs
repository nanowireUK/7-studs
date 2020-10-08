using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionAdHocQuery' Class  
    /// </summary>  
    public class ActionAdHocQuery : Action
    {  
        public ActionAdHocQuery(string connectionId, ActionEnum actionType, string gameId, string user, string leavers, string queryType) 
            : base( connectionId, actionType, gameId, user, leavers, queryType)
        {
        }
        public string QueryType;
        public override void ProcessAction()
        {
            G.AdHocQueryType = this.Parameters; // Note this so that QueryResultAsJson can pick it up
            this.ResponseType = ActionResponseTypeEnum.AdHocServerQuery; 
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;                  
        }
    }     
}  
