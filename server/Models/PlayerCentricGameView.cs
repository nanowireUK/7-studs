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
        public string GameMode { get; set; }
        public Boolean IsMyTurn { get; set; }
        public Boolean IAmDealer { get; set; }
        public Boolean IAmAdministrator { get; set; }
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        public string GameId { get; }
        public int HandsPlayedIncludingCurrent { get; set; } // 0 = game not yet started
        public int IndexOfParticipantDealingThisHand { get; set; } // Rotates from player 0
        public int IndexOfParticipantToTakeNextAction { get; set; } // Determined by cards showing (at start of round) then on player order
        public int IndexOfAdministrator { get; set; }
        public List<string> AvailableActions { get; set; } // A player-centric view of the actions available to them
        public List<List<int>> Pots { get; set; } // pot(s) built up in the current hand (over multiple rounds of betting)
        public List<PlayerCentricParticipantView> PlayerViewOfParticipants { get; set; } // ordered list of participants (order represents order around the table)


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
            GameMode = g.GameMode.ToString();
            IsMyTurn = ( playerIndex == IndexOfParticipantToTakeNextAction );
            IAmDealer = ( playerIndex == IndexOfParticipantDealingThisHand ) ;
            InitialChipQuantity = g.InitialChipQuantity;
            Ante = g.Ante;
            GameId = g.GameId;
            HandsPlayedIncludingCurrent = g.HandsPlayedIncludingCurrent;
            IndexOfParticipantDealingThisHand = g.IndexOfParticipantDealingThisHand;
            IndexOfParticipantToTakeNextAction = g.IndexOfParticipantToTakeNextAction;
            //CardPositionIsVisible = g.CardPositionIsVisible;
            Pots = g.Pots;
            // Determine the index of the administrator
            IndexOfAdministrator = -1; 
            for ( int i = 0; i < g.Participants.Count; i++ ) {
                if ( g.Participants[i].IsGameAdministrator ) {
                    IndexOfAdministrator = i;
                }
            } 
            IAmAdministrator  = ( playerIndex == IndexOfAdministrator );             
            // Add a list of participants, with data relevant to this player
            PlayerViewOfParticipants = new List<PlayerCentricParticipantView>();
            for ( int i = 0; i < g.Participants.Count; i++ ) {
                PlayerViewOfParticipants.Add(new PlayerCentricParticipantView(g, playerIndex, i, IndexOfAdministrator));
            }
            // Convert the game-level permissions into a player view of their permissions
            AvailableActions = new List<string>();
            foreach ( ActionAvailability aa in g.ActionAvailabilityList )
            {
                if ( aa.Availability == AvailabilityEnum.AnyRegisteredPlayer
                    || ( aa.Availability == AvailabilityEnum.ActivePlayerOnly & MyIndex == IndexOfParticipantToTakeNextAction )
                    || ( aa.Availability == AvailabilityEnum.AdministratorOnly & MyIndex == IndexOfAdministrator )
                ) {
                    AvailableActions.Add(aa.Action.ToString());
                }

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