using System;
using System.Collections.Generic;

namespace SocialPokerClub.Models
{
    public class GameLogAction
    {
        // Used to record an action and the results of that action
        public int ActionNumber { get; set; }
        public ActionEnum ActionType { get; set; }
        public string UserName { get; set; }
        public string Parameters { get; set; }
        public string StatusMessage { get; set; }
        private string LastEvent { get; set; }
        private string NextAction { get; set; }
        public string PlayerSummaries { get; set; }
        public List<string> HandCommentary { get; set; }
        public GameLogAction() {
            // Constructor without parameters required by the JSON deserialiser (along with setters for the public variables)
        }
        public GameLogAction(
                Action argAction,
                int argActionNumber,
                string argStatusMessage,
                string argPlayerSummaries,
                List<string> argCommentary
                ) {
            this.ActionType = argAction.ActionType;
            this.ActionNumber = argActionNumber;
            this.UserName = argAction.UserName;
            this.Parameters = argAction.Parameters;
            this.StatusMessage = argStatusMessage;
            this.HandCommentary = new List<String>();
            foreach ( string s in argCommentary ) {
                // Take a snapshot of the original commentary
                this.HandCommentary.Add(s);
            }
            this.PlayerSummaries = argPlayerSummaries;
        }
    }
}