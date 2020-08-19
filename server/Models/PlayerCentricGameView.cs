using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace SevenStuds.Models
{
    public class PlayerCentricGameView
    {
        /// <summary>
        /// A player-specific view of the game state that is suitable for passing to the client as a deserialisable JSON string
        /// </summary>
        /// <remarks>A player-specific view of the game state that is suitable for passing to the client as a deserialisable JSON string</remarks>
  
        public string LastEvent { get; set; }
        public string NextAction { get; set; }
        public string MyHandSummary  { get; set; }
        public string MyHandDescription { get; set; }
        public List<string> HandCommentary { get; set; }        
        public int MyIndex { get; set; }
        public string MyRejoinCode { get; set; }
        public int MyMaxRaise { get; set; }

        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        public int HandsPlayedIncludingCurrent { get; set; } // 0 = game not yet started
        public int IndexOfParticipantDealingThisHand { get; set; } // Rotates from player 0
        public int IndexOfParticipantToTakeNextAction { get; set; } // Determined by cards showing (at start of round) then on player order
        public List<List<int>> Pots { get; set; } // pot(s) built up in the current hand (over multiple rounds of betting)
        public List<PlayerCentricParticipantView> PlayerViewOfParticipants { get; set; } // ordered list of participants (order represents order around the table)
        public List<ActionAvailability> ActionAvailabilityList { get; set; } // This list contains references to the same objects as in the Dictionary       
        public List<Boolean> CardPositionIsVisible { get; } = new List<Boolean>{false, false, true, true, true, true, false};

        public PlayerCentricGameView(Game g, int playerIndex) {
            // Build up this player's view of the game
            LastEvent = g.LastEvent;
            NextAction = g.NextAction;
            MyHandSummary = g.Participants[playerIndex]._HandSummary;
            MyHandDescription = g.Participants[playerIndex]._FullHandDescription;                    
            HandCommentary = g.HandCommentary; // Not sure whether I need to do a deep copy of this (suspect not, as the view is temporary anyway)    
            MyIndex =  playerIndex;
            MyRejoinCode = g.Participants[playerIndex].RejoinCode;
            MyMaxRaise = g.IndexOfParticipantToTakeNextAction == playerIndex ? g.MaxRaiseForParticipantToTakeNextAction : 0;
            InitialChipQuantity = g.InitialChipQuantity;
            Ante = g.Ante;
            HandsPlayedIncludingCurrent = g.HandsPlayedIncludingCurrent;
            IndexOfParticipantDealingThisHand = g.IndexOfParticipantDealingThisHand;
            IndexOfParticipantToTakeNextAction = g.IndexOfParticipantToTakeNextAction;
            ActionAvailabilityList = g.ActionAvailabilityList;
            CardPositionIsVisible = g.CardPositionIsVisible;
            Pots = g.Pots;
            // Add a list of participants, with data relevant to this player
            PlayerViewOfParticipants = new List<PlayerCentricParticipantView>();
            for ( int i = 0; i < g.Participants.Count; i++ ) {
                PlayerViewOfParticipants.Add(new PlayerCentricParticipantView(g.Participants[i], playerIndex, i, CardPositionIsVisible));
            }
        }
        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                
            };
            options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
            string jsonString = JsonSerializer.Serialize(this, options);
            return jsonString;
        }  
    }
}