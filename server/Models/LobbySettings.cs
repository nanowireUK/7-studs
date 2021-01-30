namespace SevenStuds.Models
{
    public class LobbySettings
    {
        // Used to present the current status of a game 
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        public bool AcceptNewPlayers { get; set; }
        public bool AcceptNewSpectators { get; set; }
        public LobbySettings ()
        {
            // This constructor is used only by the JSON deserialiser
        }
        public LobbySettings (Game g)
        {
            Ante = g.Ante;
            InitialChipQuantity = g.InitialChipQuantity;
            AcceptNewPlayers = g.AcceptNewPlayers;
            AcceptNewSpectators = g.AcceptNewSpectators;
        }
    }
}