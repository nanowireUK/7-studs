using System;
using System.Text.Json;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionReplay' Class  
    /// </summary>  
    public class ActionReplay : Action
    {  
        public ActionReplay(ActionEnum actionType, string gameId, string user, string logAsJson, string connectionId) 
            : base(actionType, gameId, user, connectionId, logAsJson)
        {
        }
        public override void ProcessAction()
        {
            GameLog historicalGameLog = JsonSerializer.Deserialize<GameLog>(this.Parameters);
            // Now rebuild/replay the current game using the previously-recorded moves
            System.Diagnostics.Debug.WriteLine("Replaying game from supplied game log");
            G.InitialiseGame(historicalGameLog); // Clear the current game and set the test context (this affects various program behaviours)
            foreach ( GameLogAction gla in historicalGameLog.actions ) {
                ActionEnum actionType = gla.ActionType;
                Action a = ActionFactory.NewAction(
                    gla.ActionType, 
                    G.GameId, 
                    gla.UserName, 
                    gla.Parameters, 
                    this.ConnectionId
                );
                System.Diagnostics.Debug.WriteLine("Replaying " + gla.ActionType.ToString().ToLower() + " by " + gla.UserName);

                string jsonResult = a.ProcessActionAndReturnUpdatedGameStateAsJson();  

                string commentaryFromGame = "";
                foreach ( string c in G.HandCommentary ) {
                    commentaryFromGame += c + Environment.NewLine;
                }
                System.Diagnostics.Debug.WriteLine("Result of replay: " + commentaryFromGame);
 
                if ( commentaryFromGame != gla.HandCommentary ){
                    System.Diagnostics.Debug.WriteLine("Differs from original results: " + gla.HandCommentary);
                }
            }
            System.Diagnostics.Debug.WriteLine("Replay complete, game will continue under normal conditions from here");
            G.SetTestContext(null); // Clear the test context, game will continue under normal conditions from here
        }
    }     
}  
