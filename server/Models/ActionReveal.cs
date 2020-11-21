namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionReveal' Class  
    /// </summary>  
    public class ActionReveal : Action
    {  
        public ActionReveal(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override void ProcessAction()
        {
            // Record the fact that this player has revealed their hand
            G.Participants[this.PlayerIndex].IsSharingHandDetails = true;
            G.RecordLastEvent(this.UserName + " revealed their hand details");
            G.Participants[this.PlayerIndex]._VisibleHandDescription = G.Participants[this.PlayerIndex]._FullHandDescription;
            if ( G.GameMode == GameModeEnum.HandsBeingRevealed ){
                G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);    
            }
        }
    }     
}  
