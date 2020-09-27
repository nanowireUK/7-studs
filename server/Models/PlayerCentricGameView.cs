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
  
        public string StatusMessage { get; set; }
        public string LastEvent { get; set; }
        public string NextAction { get; set; }
        public string MyHandSummary  { get; set; }
        public string MyHandDescription { get; set; }
        public List<string> HandCommentary { get; set; }        
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
        public List<string> AvailableActions { get; set; } // A player-centric view of the actions available to them
        public List<List<int>> Pots { get; set; } // pot(s) built up in the current hand (over multiple rounds of betting)
        public List<PlayerCentricParticipantView> PlayerViewOfParticipants { get; set; } // ordered list of participants (order represents order around the table)
        public PlayerCentricGameView(Game g, int playerIndex) {
            // Build up this player's view of the game
            StatusMessage = g.StatusMessage;
            LastEvent = g.LastEvent;
            NextAction = g.NextAction;
            MyHandSummary = g.Participants[playerIndex]._HandSummary;
            MyHandDescription = g.Participants[playerIndex]._FullHandDescription;                    
            HandCommentary = g.HandCommentary; // Not sure whether I need to do a deep copy of this (suspect not, as the view is temporary anyway)    
            //MyIndex =  playerIndex;
            MyRejoinCode = g.Participants[playerIndex].RejoinCode;
            MyMaxRaise = g.IndexOfParticipantToTakeNextAction == playerIndex ? g.MaxRaiseForParticipantToTakeNextAction : 0;
            GameMode = g.GameMode.ToString();
            IsMyTurn = ( playerIndex == g.IndexOfParticipantToTakeNextAction );
            IAmDealer = ( playerIndex == g.IndexOfParticipantDealingThisHand ) ;
            InitialChipQuantity = g.InitialChipQuantity;
            Ante = g.Ante;
            GameId = g.GameId;
            HandsPlayedIncludingCurrent = g.HandsPlayedIncludingCurrent;
            //IndexOfParticipantDealingThisHand = g.IndexOfParticipantDealingThisHand;
            //IndexOfParticipantToTakeNextAction = g.IndexOfParticipantToTakeNextAction;
            //CardPositionIsVisible = g.CardPositionIsVisible;
            // Reproduce the pots (the pots themselves stay in the same order, but the current player's contributions becomes the first slot in the inner array)
            if ( g.Pots == null ) {
                this.Pots = null;
            }
            else {
                this.Pots = new List<List<int>>();
                for ( int pot = 0; pot < g.Pots.Count; pot++ ) {
                    // Add a pot to reflect a pot from the main game
                    this.Pots.Add(new List<int>()); 
                    // Add pot contributions, starting with this player and going clockwise
                    for ( int slot = 0; slot < g.Participants.Count; slot++ ) {
                        int sourceSlot =  ( slot + playerIndex ) % g.Participants.Count; // puts current player in slot 0 with rest following clockwise
                        this.Pots[pot].Add(g.Pots[pot][sourceSlot]); 
                    }
                }  
            }              
            // Determine the index of the administrator
            IAmAdministrator = ( g.GetIndexOfAdministrator() == playerIndex ); 
            // Add a list of participants, with data relevant to this player
            PlayerViewOfParticipants = new List<PlayerCentricParticipantView>();
            for ( int i = 0; i < g.Participants.Count; i++ ) {
                PlayerViewOfParticipants.Add(new PlayerCentricParticipantView(g, playerIndex, i));
            }
            // Rotate the list so that current player is first person in the list
            for ( int i = 0; i < playerIndex; i++) {
                PlayerViewOfParticipants.Add(PlayerViewOfParticipants[0]); // Copy first element to end of list
                PlayerViewOfParticipants.RemoveAt(0); // Remove the first element
            }
            // Convert the game-level permissions into a player view of their permissions
            AvailableActions = new List<string>();
            foreach ( ActionAvailability aa in g.ActionAvailabilityList )
            {
                if ( aa.Availability == AvailabilityEnum.AnyRegisteredPlayer
                    || ( aa.Availability == AvailabilityEnum.ActivePlayerOnly & IsMyTurn )
                    || ( aa.Availability == AvailabilityEnum.AdministratorOnly & IAmAdministrator )
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