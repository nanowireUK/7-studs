using System;

namespace SocialPokerClub.Models
{
    public class LobbySettings
    {
        // Used to present the current status of a game

        // Note: when adding new settings:
        // (1) Ensure that 'Game' has the same fields and sets defaults for them
        // (2) Ensure that 'LobbySettings' (this class) handles the new settings using the same pattern as existing ones (four different places below)

        public int? InitialChipQuantity { get; set; }
        public int? Ante { get; set; }
        public bool? AcceptNewPlayers { get; set; }
        public bool? AcceptNewSpectators { get; set; }
        public bool? LowestCardPlacesFirstBet { get; set; }
        public bool? HideFoldedCards { get; set; }
        public bool? IsLimitGame { get; set; }
        public int? LimitGameBringInAmount { get; set; }
        public int? LimitGameSmallBet { get; set; }
        public int? LimitGameBigBet { get; set; }
        public int? LimitGameMaxRaises { get; set; }
        public LobbySettings ()
        {
            // This constructor is used only by the JSON deserialiser
            // We set default values that will only be overridden if present in the LobbySettings JSON being deserialised
            Ante = null;
            InitialChipQuantity = null;
            AcceptNewPlayers = null;
            AcceptNewSpectators = null;
            LowestCardPlacesFirstBet = null;
            HideFoldedCards = null;
            IsLimitGame = null;
            LimitGameBringInAmount = null;
            LimitGameSmallBet = null;
            LimitGameBigBet = null;
            LimitGameMaxRaises = null;
        }
        public LobbySettings (Game g)
        {
            Ante = g.Ante;
            InitialChipQuantity = g.InitialChipQuantity;
            AcceptNewPlayers = g.AcceptNewPlayers;
            AcceptNewSpectators = g.AcceptNewSpectators;
            LowestCardPlacesFirstBet = g.LowestCardPlacesFirstBet;
            HideFoldedCards = g.HideFoldedCards;
            IsLimitGame = g.IsLimitGame;
            LimitGameBringInAmount = g.LimitGameBringInAmount;
            LimitGameSmallBet = g.LimitGameSmallBet;
            LimitGameBigBet = g.LimitGameBigBet;
            LimitGameMaxRaises = g.LimitGameMaxRaises;
        }

        public bool UpdateGameSettings(Game g) {
            // Update the relevant settings on the game.
            // Some changes may result in further changes.
            bool anythingChanged = false;
            if ( this.Ante != null && this.Ante != g.Ante ) {
                Console.WriteLine("Changing Ante from "+g.Ante+" to "+this.Ante);
                g.Ante = (int) this.Ante;
                anythingChanged = true;
            }
            if ( this.InitialChipQuantity != null && this.InitialChipQuantity != g.InitialChipQuantity ) {
                Console.WriteLine("Changing InitialChipQuantity from "+g.InitialChipQuantity+" to "+this.InitialChipQuantity);
                g.InitialChipQuantity = (int) this.InitialChipQuantity;
                if ( g._ContinueIsAvailable == true ) {
                    Console.WriteLine("Disabling the Continue action"); /// Really only necessary if players have joined in the meantime
                    g._ContinueIsAvailable = false;
                }
                anythingChanged = true;
            }
            if ( this.AcceptNewPlayers != null && this.AcceptNewPlayers != g.AcceptNewPlayers ) {
                Console.WriteLine("Changing AcceptNewPlayers from "+g.AcceptNewPlayers+" to "+this.AcceptNewPlayers);
                g.AcceptNewPlayers = (bool) this.AcceptNewPlayers;
                anythingChanged = true;
            }
            if ( this.AcceptNewSpectators != null && this.AcceptNewSpectators != g.AcceptNewSpectators ) {
                Console.WriteLine("Changing AcceptNewSpectators from "+g.AcceptNewSpectators+" to "+this.AcceptNewSpectators);
                g.AcceptNewSpectators = (bool) this.AcceptNewSpectators;
                anythingChanged = true;
            }
            if ( this.LowestCardPlacesFirstBet != null && this.LowestCardPlacesFirstBet != g.LowestCardPlacesFirstBet ) {
                Console.WriteLine("Changing LowestCardPlacesFirstBet from "+g.LowestCardPlacesFirstBet+" to "+this.LowestCardPlacesFirstBet);
                g.LowestCardPlacesFirstBet = (bool) this.LowestCardPlacesFirstBet;
                anythingChanged = true;
            }
            if ( this.IsLimitGame != null && this.IsLimitGame != g.IsLimitGame ) {
                Console.WriteLine("Changing IsLimitGame from "+g.IsLimitGame+" to "+this.IsLimitGame);
                g.IsLimitGame = (bool) this.IsLimitGame;
                anythingChanged = true;
            }
            if ( this.HideFoldedCards != null && this.HideFoldedCards != g.HideFoldedCards ) {
                Console.WriteLine("Changing HideFoldedCards from "+g.HideFoldedCards+" to "+this.HideFoldedCards);
                g.HideFoldedCards = (bool) this.HideFoldedCards;
                anythingChanged = true;
            }
            if ( this.LimitGameBringInAmount != null && this.LimitGameBringInAmount != g.LimitGameBringInAmount ) {
                Console.WriteLine("Changing LimitGameBringInAmount from "+g.LimitGameBringInAmount+" to "+this.LimitGameBringInAmount);
                g.LimitGameBringInAmount = (int) this.LimitGameBringInAmount;
                anythingChanged = true;
            }
            if ( this.LimitGameSmallBet != null && this.LimitGameSmallBet != g.LimitGameSmallBet ) {
                Console.WriteLine("Changing LimitGameSmallBet from "+g.LimitGameSmallBet+" to "+this.LimitGameSmallBet);
                g.LimitGameSmallBet = (int) this.LimitGameSmallBet;
                anythingChanged = true;
            }
            if ( this.LimitGameBigBet != null && this.LimitGameBigBet != g.LimitGameBigBet ) {
                Console.WriteLine("Changing LimitGameBigBet from "+g.LimitGameBigBet+" to "+this.LimitGameBigBet);
                g.LimitGameBigBet = (int) this.LimitGameBigBet;
                anythingChanged = true;
            }
            if ( this.LimitGameMaxRaises != null && this.LimitGameMaxRaises != g.LimitGameMaxRaises ) {
                Console.WriteLine("Changing LimitGameMaxRaises from "+g.LimitGameMaxRaises+" to "+this.LimitGameMaxRaises);
                g.LimitGameMaxRaises = (int) this.LimitGameMaxRaises;
                anythingChanged = true;
            }
            return anythingChanged;
        }
    }
}