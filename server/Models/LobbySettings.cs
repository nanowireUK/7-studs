namespace SevenStuds.Models
{
    public class LobbySettings
    {
        // Used to present the current status of a game 
        public int? InitialChipQuantity { get; set; }
        public int? Ante { get; set; }
        public bool? AcceptNewPlayers { get; set; }
        public bool? AcceptNewSpectators { get; set; }
        public bool? LowestCardPlacesFirstBet { get; set; }
        public LobbySettings ()
        {
            // This constructor is used only by the JSON deserialiser
            // We set default values that will only be overridden if present in the LobbySettings JSON being deserialised
            Ante = null;
            InitialChipQuantity = null;
            AcceptNewPlayers = null;
            AcceptNewSpectators = null;
            LowestCardPlacesFirstBet = null;
        }
        public LobbySettings (Game g)
        {
            Ante = g.Ante;
            InitialChipQuantity = g.InitialChipQuantity;
            AcceptNewPlayers = g.AcceptNewPlayers;
            AcceptNewSpectators = g.AcceptNewSpectators;
            LowestCardPlacesFirstBet = g.LowestCardPlacesFirstBet;
        }

        public bool UpdateGameSettings(Game g) {
            // Update the relevant settings on the game.
            // Some changes may result in further changes.
            bool anythingChanged = false;
            if ( this.Ante != null && this.Ante != g.Ante ) {
                System.Diagnostics.Debug.WriteLine("Changing Ante from "+g.Ante+" to "+this.Ante);
                g.Ante = (int) this.Ante; 
                anythingChanged = true;
            }
            if ( this.InitialChipQuantity != null && this.InitialChipQuantity != g.InitialChipQuantity ) {
                System.Diagnostics.Debug.WriteLine("Changing InitialChipQuantity from "+g.InitialChipQuantity+" to "+this.InitialChipQuantity);
                g.InitialChipQuantity = (int) this.InitialChipQuantity;
                if ( g._ContinueIsAvailable == true ) {
                    System.Diagnostics.Debug.WriteLine("Disabling the Continue action"); /// Really only necessary if players have joined in the meantime
                    g._ContinueIsAvailable = false;
                }
                anythingChanged = true;
            }
            if ( this.AcceptNewPlayers != null && this.AcceptNewPlayers != g.AcceptNewPlayers ) {
                System.Diagnostics.Debug.WriteLine("Changing AcceptNewPlayers from "+g.AcceptNewPlayers+" to "+this.AcceptNewPlayers);
                g.AcceptNewPlayers = (bool) this.AcceptNewPlayers; 
                anythingChanged = true;
            }
            if ( this.AcceptNewSpectators != null && this.AcceptNewSpectators != g.AcceptNewSpectators ) {
                System.Diagnostics.Debug.WriteLine("Changing AcceptNewSpectators from "+g.AcceptNewSpectators+" to "+this.AcceptNewSpectators);
                g.AcceptNewSpectators = (bool) this.AcceptNewSpectators; 
                anythingChanged = true;
            }
            if ( this.LowestCardPlacesFirstBet != null && this.LowestCardPlacesFirstBet != g.LowestCardPlacesFirstBet ) {
                System.Diagnostics.Debug.WriteLine("Changing LowestCardPlacesFirstBet from "+g.LowestCardPlacesFirstBet+" to "+this.LowestCardPlacesFirstBet);
                g.LowestCardPlacesFirstBet = (bool) this.LowestCardPlacesFirstBet; 
                anythingChanged = true;
            }
            return anythingChanged;
        }
    }
}