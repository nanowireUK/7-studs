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
            // Note this players willingness to reveal their full hand (at the end of a game)
            G.Participants[this.PlayerIndex].IsSharingHandDetails = true;
            G.LastEvent = this.UserName + " revealed their hand details";
            this.ResponseType = ActionResponseTypeEnum.PlayerCentricGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.AllPlayers;
        }
    }     
}  
