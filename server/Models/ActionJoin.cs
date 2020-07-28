namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionJoin' Class  
    /// </summary>  
    public class ActionJoin : Action
    {  
        public ActionJoin(ActionEnum actionType, string gameId, string user, string connectionId) : base(actionType, gameId, user, connectionId)
        {
        }

        public override string ProcessActionAndReturnUpdatedGameStateAsJson()
        {
            // Ensure name is not blank
            if ( this.UserName == "" ) {
                G.LastEvent = "Someone attempted to join game with a blank name";
                return G.AsJson(); // No update to game status other than this
            }
            // Test whether player name already exists
            if ( G.Participants.Count > 0 ) {
                for (int player = 0; player < G.Participants.Count; player++) {
                    Participant p = G.Participants[player]; // Get reference to player to be moved
                    if ( p.Name == this.UserName ) {
                        G.LastEvent = this.UserName + " attempted to join game but is already registered";
                        return G.AsJson(); // No update to game status other than this
                    }
                }
            }

            // Add player
            G.Participants.Add(new Participant(this.UserName, this.ConnectionId));
            G.LastEvent = this.UserName + " joined game";
            G.NextAction = "Await new player or start the game";
            if ( G.Participants.Count >= 2)
            {
                G.SetActionAvailability(ActionEnum.Start, AvailabilityEnum.AnyPlayer); // Open up START to anyone
            }
            return G.AsJson();
        }
    }     
}  