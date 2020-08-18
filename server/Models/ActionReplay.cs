using System.Text.Json;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionReplay' Class  
    /// </summary>  
    public class ActionReplay : Action
    {  
        public ActionReplay(string connectionId, ActionEnum actionType, string gameId, string user, string logAsJson) 
            : base(connectionId, actionType, gameId, user, logAsJson)
        {
        }
        public override void ProcessAction()
        {
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
                    gla.Parameters
                );
                System.Diagnostics.Debug.WriteLine("Replaying " + gla.ActionType.ToString().ToLower() + " by " + gla.UserName);

                string jsonResult = a.ProcessActionAndReturnUpdatedGameStateAsJson();  

                System.Diagnostics.Debug.WriteLine("  Commentary from replay:");

                foreach ( string c in G.HandCommentary ) {
                    System.Diagnostics.Debug.WriteLine("    " + c);                    
                }
 
                if ( G.LastEvent != gla.LastEvent ){
                    System.Diagnostics.Debug.WriteLine("  Last Event is not consistent:");
                    System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.LastEvent);
                    System.Diagnostics.Debug.WriteLine("    REPLAY   : " + G.LastEvent);
                }
                if ( G.NextAction != gla.NextAction ){
                    System.Diagnostics.Debug.WriteLine("  Next Action is not consistent:");
                    System.Diagnostics.Debug.WriteLine("    ORIGINAL : " + gla.NextAction);
                    System.Diagnostics.Debug.WriteLine("    REPLAY   : " + G.NextAction);
                }                
            }
            System.Diagnostics.Debug.WriteLine("Replay complete, game will continue under normal conditions from here");
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
