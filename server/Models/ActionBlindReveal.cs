using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionBlindReveal' Class  
    /// </summary>  
    public class ActionBlindReveal : Action
    {  
        public ActionBlindReveal(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers) 
            : base(connectionId, actionType, ourGame, user, leavers)
        {
        }
        public override async Task ProcessAction()
        {
            // The player was playing blind and has now decided to look at their face-down cards and so no longer be playing blind.
            // Note that there is no change to the internal (i.e. game state) view of the blind player's face-up and face-down cards
            // (the hiding of the blind player's face-down cards from himself is managed purely in his player-centric view of the game)
            
            G.RecordLastEvent(this.UserName + " is no longer playing blind");
            G.Participants[this.PlayerIndex].IsPlayingBlindInCurrentHand = false;
            G.Participants[this.PlayerIndex].RebuildHandSummaries(G); // To enable ex-blind player to see an evaluation of his complete hand

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
