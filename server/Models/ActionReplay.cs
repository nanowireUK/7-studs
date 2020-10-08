using System.Text.Json;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionReplay' Class  
    /// </summary>  
    public class ActionReplay : Action
    {  
        public ActionReplay(string connectionId, ActionEnum actionType, string gameId, string user, string leavers, string logAsJson) 
            : base(connectionId, actionType, gameId, user, leavers, logAsJson)
        {
        }
        public override void ProcessAction()
        {
            ResponseType = ActionResponseTypeEnum.OverallGameState; // On completion of replay, the tester will have the overall game state returned to them
            ResponseAudience =  ActionResponseAudienceEnum.Caller; // Tester will then have to rejoin each player using their respective rejoin codes
            GameLog historicalGameLog = JsonSerializer.Deserialize<GameLog>(this.Parameters);
            FixCardOrderInDesererialisedDecks(historicalGameLog); // (it loads them in array order, which gives a reversed deck)

            // Now rebuild/replay the current game using the previously-recorded moves
            System.Diagnostics.Debug.WriteLine("Replaying game from supplied game log");
            G.InitialiseGame(historicalGameLog); // Clear the current game and set the test context (this affects various program behaviours)
            foreach ( GameLogAction gla in historicalGameLog.actions ) {
                ActionEnum actionType = gla.ActionType;
                Action a = ActionFactory.NewAction(
                    "", // TODO think about how to handle connection ids in test mode ... 
                        // generate a new dummy one for each action? Users will have to rejoin anyway.
                    gla.ActionType, 
                    G.GameId, 
                    gla.UserName,
                    G.CountOfLeavers.ToString(), 
                    gla.Parameters
                );
                System.Diagnostics.Debug.WriteLine(
                    "Replaying action "  + gla.ActionNumber + ": "
                    + gla.ActionType.ToString().ToLower() + " by " + gla.UserName);

                a.ProcessActionAndReturnGameReference(); 

                System.Diagnostics.Debug.WriteLine("  StatusMessage: " + G.StatusMessage);

                System.Diagnostics.Debug.WriteLine("  Commentary from replay:");
                foreach ( string c in G.HandCommentary ) {
                    System.Diagnostics.Debug.WriteLine("    " + c);                    
                }
                 if ( G.StatusMessage != gla.StatusMessage ){
                    System.Diagnostics.Debug.WriteLine("  Status Message is not consistent:");
                    System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.StatusMessage);
                    System.Diagnostics.Debug.WriteLine("    REPLAY   : " + G.StatusMessage);
                }
                // if ( G.LastEvent != gla.LastEvent ){
                //     System.Diagnostics.Debug.WriteLine("  Last Event is not consistent:");
                //     System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.LastEvent);
                //     System.Diagnostics.Debug.WriteLine("    REPLAY   : " + G.LastEvent);
                // }
                // if ( G.NextAction != gla.NextAction ){
                //     System.Diagnostics.Debug.WriteLine("  Next Action is not consistent:");
                //     System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.NextAction);
                //     System.Diagnostics.Debug.WriteLine("    REPLAY   : " + G.NextAction);
                // }                
            }
            foreach ( Participant p in G.Participants ) {
                // Mark the player as locked ... this will be unlocked once someone joins as that player using a unique new connection
                p.IsLockedOutFollowingReplay = true;
            }
            System.Diagnostics.Debug.WriteLine("Replay complete, game will continue under normal conditions from here.");
            System.Diagnostics.Debug.WriteLine("Use the game state to find each player and rejoin each of them from a separate browser using their respective rejoin codes.");
            G.SetTestContext(null); // Clear the test context, game will continue under normal conditions from here            
        }

        private void FixCardOrderInDesererialisedDecks(GameLog historicalGameLog) {
            for ( int i = 0; i < historicalGameLog.decks.Count; i++){ 
                Deck d = historicalGameLog.decks[i];
                Deck temp = new Deck();
                while (d.Count != 0) {
                    temp.Push(d.Pop());
                }
                historicalGameLog.decks[i] = temp;
            }
        }
    }     
}  
