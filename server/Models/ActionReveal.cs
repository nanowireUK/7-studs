namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionReveal' Class  
    /// </summary>  
    public class ActionReveal : Action
    {  
        public ActionReveal(string connectionId, ActionEnum actionType, string gameId, string user) 
            : base(connectionId, actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {
            // Note that reveal is used in two circumstances:
            // (1) The hand is still being played but all players have completed betting and are now either showing their cards or folding
            // (2) The hand has completed and players are revealing their full hand for other players' information
            G.Participants[this.PlayerIndex].IsSharingHandDetails = true;
            G.LastEvent = this.UserName + " revealed their hand details";
        }
    }     
}  
