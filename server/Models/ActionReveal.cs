using System.Threading.Tasks;

namespace SocialPokerClub.Models
{
    /// <summary>
    /// The 'ActionReveal' Class
    /// </summary>
    public class ActionReveal : Action
    {
        public ActionReveal(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers)
            : base(connectionId, actionType, ourGame, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Record the fact that this player has revealed their full hand

            // Note that reveal can be called under two different circumstances:
            // (1) The hand result is still being decided as players either reveal or fold their hands in turn
            // (2) The hand result has been decided and players are revealing their hand if they want to (for fun or effect)

            G.RecordLastEvent(this.UserName + " revealed their hand details");
            G.Participants[this.PlayerIndex].IsSharingHandDetails = true;
            G.Participants[this.PlayerIndex]._VisibleHandDescription = G.Participants[this.PlayerIndex]._FullHandDescription;
            G.Participants[this.PlayerIndex].IsPlayingBlindInCurrentHand = false; // If player was playing blind, they are no longer doing so
            G.Participants[this.PlayerIndex].HasJustSharedHandDetails = true; // Enables the client to animate the reveal if desired
            if ( G.GameMode == GameModeEnum.HandsBeingRevealed ){
                await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
            }
        }
    }
}
