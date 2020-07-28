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
            // Ensure name is not blank
            if ( this.UserName == "" ) {
                G.LastEvent = "Someone attempted to start game with a blank name";
                return G.AsJson(); // No update to game status other than this
            }
            if ( G.Participants.Count == 0 ) {
                G.LastEvent = this.UserName + " attempted to start the game but no one has joined as yet";
                return G.AsJson(); // No update to game status other than this
            }
            // Test that player name exists and note the player index if so
            int playerIndex = -1;            
            for (int player = 0; player < G.Participants.Count; player++) {
                Participant p = G.Participants[player]; // Get reference to player to be moved
                if ( p.Name == this.UserName ) {
                    playerIndex = player;
                }
            }
            if ( playerIndex == -1 ) {
                G.LastEvent = this.UserName + " attempted to start game but is not yet part of the game";
                return G.AsJson(); // No update to game status other than this  
            }
            // if ( ! G.ActionIsAvailableToThisPlayerAtThisPoint(this.Type, playerIndex) )
            // {
            //     return G.AsJson(); // No update to game status except that the called method will update G.LastEvent
            // }
            // Start game
            G.InitialiseGame();
            G.LastEvent = this.UserName + " started game (player order now randomised)";
            G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 

            G.SetActionAvailability(ActionEnum.Join, AvailabilityEnum.NotAvailable); // Stop anyone else from joining at this stage
            G.SetActionAvailability(ActionEnum.Start, AvailabilityEnum.NotAvailable); // Stop anyone else from attempting to restart the game
            return G.AsJson();
        }
    }     
}  