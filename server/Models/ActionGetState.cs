namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionGetState' Class  
    /// </summary>  
    public class ActionGetState : Action
    {  
        public ActionGetState(ActionEnum actionType, string gameId, string user, string connectionId) 
            : base(actionType, gameId, user, connectionId)
        {
        }
        public override void ProcessAction()
        {
            // Dummy action ... the base class already does what we need
        }
    }     
}  
