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
            // Record the fact that this player has revealed their hand
            G.Participants[this.PlayerIndex].IsSharingHandDetails = true;
            G.LastEvent = this.UserName + " revealed their hand details";
            if ( G.GameMode == GameModeEnum.HandsBeingRevealed ){
                G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);    
            }
        }
    }     
}  
