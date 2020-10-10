namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionStart' Class - used to start a new hand (note that only the administrator can do this)
    /// </summary>  
    public class ActionStart : Action
    {  
        public ActionStart(string connectionId, ActionEnum actionType, string gameId, string user, string leavers) 
            : base(connectionId, actionType, gameId, user, leavers)
        {
        }

        public override void ProcessAction()
        {
            // Start a new game (note that the base class has already checked the player's eligibility for this action)

            if ( G.HandsPlayedIncludingCurrent > 0 ) {
                // Archive the results of the last game before setting up the new one 
                ServerState.AddCompletedGameToRoomHistory(G); 
            }
            G.RecordLastEvent(this.UserName + " started the game (player order now randomised)");
            G.StartGame(); // Initialise the game
            G.StartNextHand(); 
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            G.GameMode = GameModeEnum.HandInProgress;
        }
    }     
}  