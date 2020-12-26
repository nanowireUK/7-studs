using System.Threading.Tasks;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionContinue' Class - used to start a new hand in the current game (note that only the administrator can do this)
    /// </summary>  
    public class ActionContinue : Action
    {  
        public ActionContinue(string connectionId, ActionEnum actionType, string roomId, string user, string leavers) 
            : base(connectionId, actionType, roomId, user, leavers)
        {
        }

        public override async Task ProcessAction()
        {
            // Start the next hand in an existing game (note that the base class has already checked the player's eligibility for this action)
            G.RecordLastEvent(this.UserName + " started next hand");
            G.StartNextHand(); 
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.GameMode = GameModeEnum.HandInProgress;

            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  