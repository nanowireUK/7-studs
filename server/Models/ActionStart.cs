namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionStart' Class  
    /// </summary>  
    public class ActionStart : Action
    {  
        public ActionStart(ActionEnum actionType, string gameId, string user, string connectionId) : base(actionType, gameId, user, connectionId)
        {
        }

        public override string ProcessActionAndReturnUpdatedGameStateAsJson()
        {
            if ( G.LastEvent != "" ) {
                return G.AsJson(); // Base class set an error message so return without checking anything else
            }            
            // Start game (note that the base class has already checked the player's eligibility for this action)
            G.InitialiseGame();
            G.LastEvent = this.UserName + " started game (player order now randomised)";
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.SetActionAvailability(ActionEnum.Join, AvailabilityEnum.NotAvailable); // JOIN no longer available
            G.SetActionAvailability(ActionEnum.Start, AvailabilityEnum.NotAvailable); // START no longer available
            return G.AsJson();
        }
    }     
}  