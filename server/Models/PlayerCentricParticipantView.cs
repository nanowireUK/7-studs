using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace SevenStuds.Models
{
    public class PlayerCentricParticipantView
    {
        /// <summary>
        /// A view of a player that that is suitable for passing to the client as a deserialisable JSON string
        /// and which does not give
        /// </summary>
        /// <remarks>A player-specific view of the game state that is suitable for passing to the client as a deserialisable JSON string</remarks>
  
        public string Name { get; set; }
        public int UncommittedChips { get; set; }
        //public int ChipsCommittedToCurrentBettingRound { get; set; }
        public Boolean HasFolded { get; set; }
        public Boolean HasCovered { get; set; }
        public Boolean IsOutOfThisGame { get; set; } // Can work this out but possibly cleaner to record it explicitly
        public string VisibleHandDescription { get; set; }
        public List<string> Cards { get; set; }

        public PlayerCentricParticipantView(Participant p, int playerCentricIndex, int playerIndex, List<Boolean> CardPositionIsVisible) {
            // Build up this player's view of the game
            Name = p.Name;
            UncommittedChips  = p.UncommittedChips;
            HasFolded = p.HasFolded;
            HasCovered = p.HasCovered;
            IsOutOfThisGame = p.IsOutOfThisGame ; // Can work this out but possibly cleaner to record it explicitly
            VisibleHandDescription = p._VisibleHandDescription;
            // Add a list of this player's cards, substituting with '?' if the player receiving this data is not allowed to see this card
            Cards = new List<string>();
            for ( int i = 0; i < p.Hand.Count; i++ ) {
                if ( playerIndex != playerCentricIndex && CardPositionIsVisible[i] == false ) {
                    // This is a view of a different player's cards and this card is currently face down
                    Cards.Add("?");
                }
                else{
                    Cards.Add(p.Hand[i].ToString(CardToStringFormatEnum.ShortCardName));
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