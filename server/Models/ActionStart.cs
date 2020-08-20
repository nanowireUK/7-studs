namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionStart' Class - used to start a new hand (noye that only the administrator can do this)
    /// </summary>  
    public class ActionStart : Action
    {  
        public ActionStart(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }

        public override void ProcessAction()
        {
            // Start game (note that the base class has already checked the player's eligibility for this action)
            if ( G.HandsPlayedIncludingCurrent == 0 ) {
                G.StartGame(); // Initialise the game
                G.StartNextHand(); 
                G.LastEvent = this.UserName + " started the first hand (player order now randomised)";
                G.SetActionAvailability(ActionEnum.Join, AvailabilityEnum.NotAvailable); // JOIN no longer available once game is underway
            }
            else {
                G.StartNextHand(); 
                G.LastEvent = this.UserName + " started next hand";
            }            
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.SetActionAvailability(ActionEnum.Start, AvailabilityEnum.NotAvailable); // START no longer available (until this hand ends)
        }
    }     
}  