using System.Threading.Tasks;

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
        public override async Task ProcessAction()
        {
            // Note that reveal can be called in a number of circumstances, which can happen in this order in the game:
            // (1) The player was playing blind and has now decided to reveal those cards that would normally be visible to everyone
            // (2) The hand result is still being decided as players either reveal or fold their hands in turn (this could include a player who is still blind)
            // (3) The hand result has been decided and players are revealing their hand if they want to (for fun or effect)
            if ( G.Participants[this.PlayerIndex].IsPlayingBlindInCurrentHand == true && G.GameMode == GameModeEnum.HandInProgress ) {
                // Note that there is no change to the internal view of the player's visible hand (any blind cards are only blanked out in the player view)
                G.RecordLastEvent(this.UserName + " revealed their blind cards");
                G.Participants[this.PlayerIndex].IsPlayingBlindInCurrentHand = false; // Player is no longer playing blind
                G.Participants[this.PlayerIndex].RebuildHandSummaries(G);
            }
            else {
                // Record the fact that this player has revealed their full hand
                G.RecordLastEvent(this.UserName + " revealed their hand details");
                G.Participants[this.PlayerIndex].IsSharingHandDetails = true;
                G.Participants[this.PlayerIndex]._VisibleHandDescription = G.Participants[this.PlayerIndex]._FullHandDescription;
                G.Participants[this.PlayerIndex].IsPlayingBlindInCurrentHand = false; // If player was playing blind, they are no longer doing so
                if ( G.GameMode == GameModeEnum.HandsBeingRevealed ){
                    await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);    
                }
            }
        }
    }     
}  
