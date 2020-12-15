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
        /// and which does not give any details of other players' hands
        /// </summary>
        /// <remarks>A player-specific view of the game state that is suitable for passing to the client as a deserialisable JSON string</remarks>
  
        public string Name { get; set; }
        public int UncommittedChips { get; set; }
        public Boolean IsMe { get; set; }
        public Boolean IsCurrentPlayer { get; set; }
        public Boolean IsDealer { get; set; }
        public Boolean IsAdmin { get; set; }
        public Boolean IsSharingHandDetails { get; set; }
        public Boolean HasFolded { get; set; }
        public Boolean HasCovered { get; set; }
        public Boolean IsOutOfThisGame { get; set; }
        public Boolean HasDisconnected { get; set; }
        public string VisibleHandDescription { get; set; }
        public int GainOrLossInLastHand { get; set; }
        public int HandsWon { get; set; }
        public List<string> Cards { get; set; }

        public PlayerCentricParticipantView(Game g, int thisPlayersIndex, int observedPlayersIndex, bool isSpectatorView ) {
            Participant viewingPlayer = g.Participants[thisPlayersIndex];
            Participant observedPlayer = g.Participants[observedPlayersIndex];
            // Build up this viewing player's view of the observed player (which might be the player themselves)
            Name = observedPlayer.Name;
            UncommittedChips  = observedPlayer.UncommittedChips;
            IsMe = ( isSpectatorView == false && observedPlayersIndex == thisPlayersIndex );
            IsCurrentPlayer = ( observedPlayersIndex == g.IndexOfParticipantToTakeNextAction );
            IsDealer = ( observedPlayersIndex == g.IndexOfParticipantDealingThisHand ) ;
            IsAdmin  = ( observedPlayersIndex == g.GetIndexOfAdministrator() );   
            IsSharingHandDetails = observedPlayer.IsSharingHandDetails;   
            HasFolded = observedPlayer.HasFolded;
            HasCovered = observedPlayer.HasCovered;
            IsOutOfThisGame = observedPlayer.StartedHandWithNoFunds; 
            HasDisconnected = observedPlayer.HasDisconnected; 
            VisibleHandDescription = observedPlayer._VisibleHandDescription;
            GainOrLossInLastHand = observedPlayer.GainOrLossInLastHand;
            HandsWon = observedPlayer.HandsWon;
            // Add a list of this player's cards, substituting with '?' if the player receiving this data is not allowed to see this card
            Cards = new List<string>();
            for ( int i = 0; i < observedPlayer.Hand.Count; i++ ) {
                if ( ( observedPlayer.Name != viewingPlayer.Name || isSpectatorView == true ) 
                    && g.CardPositionIsVisible[i] == false 
                    && observedPlayer.IsSharingHandDetails == false
                    ) {
                    // This is a view of a different player's cards and this card is currently face down and they have not consented to reveal them (at hand end)
                    Cards.Add("?");
                }
                else{
                    Cards.Add(observedPlayer.Hand[i].ToString(CardToStringFormatEnum.ShortCardName));
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