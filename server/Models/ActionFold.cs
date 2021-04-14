using System;
using System.Threading.Tasks;

namespace SocialPokerClub.Models
{
    /// <summary>
    /// The 'ActionFold' Class
    /// </summary>
    public class ActionFold : Action
    {
        public ActionFold(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers)
            : base(connectionId, actionType, ourGame, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // Handle the fold (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            G.RecordLastEvent(UserName + " folded");

            // Implement the Fold
            p.HasFolded = true;

            if ( p.UncommittedChips == 0 ) {
                // Player is now bankrupt, use the time they went all-in as the time they went bankrupt
                p.TimeOfBankruptcy = p.AllInDateTime;
            }

            G.Participants[PlayerIndex].LastActionInThisHand = this.ActionType;
            G.Participants[PlayerIndex].LastActionAmount = 0;
            G.Participants[PlayerIndex].RoundNumberOfLastAction = G._CardsDealtIncludingCurrent;
            G.Participants[PlayerIndex].RebuildMyHandSummary(G); // Enables folded cards to be hidden if requireed

            // Find and set next player (could be no one if all players have now folded)
            await G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);
        }
    }
}
