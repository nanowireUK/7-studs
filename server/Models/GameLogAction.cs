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
        public string Result { get; set; } // a copy of NextAction at the point the command completes, for checking on replaying
        public string HandCommentary { get; set; } // a copy of NextAction at the point the command completes, for checking on replaying
        public GameLogAction() {
            // Constructor without parameters required by the JSON deserialiser (along with setters for the public variables)
        }        
        public GameLogAction(Action argAction, string argResult, List<string> argCommentary) {
            this.ActionType = argAction.ActionType;
            this.UserName = argAction.UserName;
            this.Parameters = argAction.Parameters;
            this.Result = argResult;
            this.HandCommentary = "";
            foreach ( string c in argCommentary ) {
                this.HandCommentary += c + Environment.NewLine;
            }
        }
    }
}