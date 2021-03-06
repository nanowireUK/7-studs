using System;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class GameLogAction
    {
        // Used to record an action and the results of that action
        public ActionEnum ActionType { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
        public string LastEvent { get; set; }
        public string NextAction { get; set; }        
        public List<string> HandCommentary { get; set; } // a copy of NextAction at the point the command completes, for checking on replaying
        public List<string> HandSummaries { get; set; } // list each player's hand
        public GameLogAction() {
            // Constructor without parameters required by the JSON deserialiser (along with setters for the public variables)
        }        
        public GameLogAction(Action argAction, string argLastEvent, string argNextAction, List<string> argCommentary, List<string> argHandSummaries) {
            this.ActionType = argAction.ActionType;
            this.UserName = argAction.UserName;
            this.Parameters = argAction.Parameters;
            this.LastEvent = argLastEvent;
            this.NextAction = argNextAction;
            this.HandCommentary = new List<String>();
            foreach ( string s in argCommentary ) {
                // Take a snapshot of the original commentary
                this.HandCommentary.Add(s);
            }
            this.HandSummaries = new List<String>();
            foreach ( string s in argHandSummaries ) {
                // Take a snapshot of the original commentary
                this.HandSummaries.Add(s);
            }            
        }
    }
}