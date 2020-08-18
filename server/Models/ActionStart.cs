namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionStart' Class  
    /// </summary>  
    public class ActionStart : Action
    {  
        public ActionStart(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }

        public override void ProcessAction()
        {
            // Start game (note that the base class has already checked the player's eligibility for this action)
            G.StartGame();
            G.LastEvent = this.UserName + " started game (player order now randomised)";
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.SetActionAvailability(ActionEnum.Join, AvailabilityEnum.NotAvailable); // JOIN no longer available
            G.SetActionAvailability(ActionEnum.Start, AvailabilityEnum.NotAvailable); // START no longer available
        }
    }     
}  