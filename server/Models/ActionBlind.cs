using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionBlind' Class  
    /// Allow a player to toggle their intent to play blind in the NEXT hand (does not directly affect any ongoing hand)
    /// </summary>  
    public class ActionBlind : Action
    {  
        public ActionBlind(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Record the fact that this player either intends or not longer intends to play blind in the next hand.
            // The response will only be returned to the caller as it doesn't affect the current game state for anyone.

            this.ResponseType = ActionResponseTypeEnum.PlayerCentricGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;
            if ( G.Participants[this.PlayerIndex].IntendsToPlayBlindInNextHand == false ) {
                G.Participants[this.PlayerIndex].IntendsToPlayBlindInNextHand = true;
            }
            else {
                G.Participants[this.PlayerIndex].IntendsToPlayBlindInNextHand = false;
            }
            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
