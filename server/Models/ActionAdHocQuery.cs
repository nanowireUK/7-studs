using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionAdHocQuery' Class  
    /// </summary>  
    public class ActionAdHocQuery : Action
    {  
        public ActionAdHocQuery(string connectionId, ActionEnum actionType, string gameId, string user, string leavers, string queryNumber) 
            : base( connectionId, actionType, gameId, user, leavers, queryNumber)
        {
        }
        public int QueryNumber;
        public override void ProcessAction()
        {
            int queryType;
            if ( int.TryParse(Parameters, out queryType ) == false ) {
                throw new HubException("You entered a non-numeric query number");
            }
            G.AdHocQueryNumber = queryType; // Note this so that QueryResultAsJson can pick it up
            this.ResponseType = ActionResponseTypeEnum.AdHocServerQuery; 
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;                  
        }


    }     
}  
