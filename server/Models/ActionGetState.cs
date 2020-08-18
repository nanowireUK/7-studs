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
            // Dummy action ... the base class already does what we need
        }
    }     
}  
