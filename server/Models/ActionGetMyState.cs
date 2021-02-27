using System.Threading.Tasks;

namespace SocialPokerClub.Models
{
    /// <summary>
    /// The 'ActionGetMyState' Class
    /// </summary>
    public class ActionGetMyState : Action
    {
        public ActionGetMyState(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers)
            : base(connectionId, actionType, ourGame, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // This is not part of the game but enables the server web page to re-get the player's state for convenience after calling other admin functions
            this.ResponseType = ActionResponseTypeEnum.PlayerCentricGameState;
            this.ResponseAudience = ActionResponseAudienceEnum.Caller;

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }
}
