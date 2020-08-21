namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionStart' Class - used to start a new hand (note that only the administrator can do this)
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
            }
            else {
                G.StartNextHand(); 
                G.LastEvent = this.UserName + " started next hand";
            }            
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.GameMode = GameModeEnum.HandInProgress;
        }
    }     
}  