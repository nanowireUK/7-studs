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

        public string RoomId { get; }
        public int GameNumber { get; set; }
        public int HandsPlayedIncludingCurrent { get; set; } // 0 = game not yet started
        public int ActionNumber { get; set; }
        public string StatusMessage { get; set; }
        public string MyHandSummary  { get; set; }
        public string MyHandDescription { get; set; }
        public string MyRejoinCode { get; set; }
        public int MyCallAmount { get; set; }
        public int MyMaxRaise { get; set; }
        public string GameMode { get; set; }
        public Boolean IsMyTurn { get; set; }
        public Boolean IAmDealer { get; set; }
        public Boolean IAmAdministrator { get; set; }
        public Boolean IIntendToPlayBlindInNextHand { get; set; }
        public Boolean IAmPlayingBlindInCurrentHand { get; set; }
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }
        public int RoundNumberIfCardsJustDealt { get; set; } // So that client know it can animate the deal
        public int RoundNumber { get; set; }
        public int CountOfLeavers { get; set; }
        public DatabaseConnectionStatusEnum DatabaseConnectionStatus { get; set; }
        public double DatabaseRequestUnits { get; set; }
        public List<string> AvailableActions { get; set; } // A player-centric view of the actions available to them
        public List<List<int>> Pots { get; set; } // pot(s) built up in the current hand (over multiple rounds of betting)
        //public List<List<string>> LastHandResult { get; set; }
        public List<PlayerCentricParticipantView> PlayerViewOfParticipants { get; set; } // ordered list of participants (order represents order around the table)
        public string CommunityCard { get; set; }

        public GameStatistics GameStatistics { get; set; }
        public LobbyData LobbyData { get; set; }
        public List<Boolean> CardPositionIsVisible { get; set; }
        public List<List<PotResult>> MostRecentHandResult { get; set; }
        public PlayerCentricGameView(Game g, int requestedPlayerIndex, int spectatorIndex) {
            // Build up this player's view of the game
            // (note that if player index = -1 it means we're building a view for a spectator and the dealer will be the first player shown)
            int playerIndex = requestedPlayerIndex;
            bool isSpectatorView = ( playerIndex == -1 );
            if ( isSpectatorView ) {
                playerIndex = g.IndexOfParticipantDealingThisHand;
            }
            // Set the game level items that don't matter whether the player is a spectator or a player
            GameNumber = g.GameNumber;
            HandsPlayedIncludingCurrent = g.HandsPlayedIncludingCurrent;
            ActionNumber = g.ActionNumber;
            StatusMessage = g.StatusMessage;
            InitialChipQuantity = g.InitialChipQuantity;
            Ante = g.Ante;
            RoomId = g.RoomId;
            RoundNumberIfCardsJustDealt = g.RoundNumberIfCardsJustDealt;
            RoundNumber = g.RoundNumber;
            CountOfLeavers = g.CountOfLeavers;
            DatabaseConnectionStatus = ServerState.OurDB.dbStatus;
            DatabaseRequestUnits = ServerState.OurDB.consumedRUs;
            CommunityCard = g.CommunityCard == null ? "" : g.CommunityCard.ToString(CardToStringFormatEnum.ShortCardName);
            CardPositionIsVisible = g.CardPositionIsVisible;
            LobbyData = g.LobbyData;
            GameStatistics = g.GameStatistics;
            //LastHandResult = new List<List<string>>(g.LastHandResult); // This definitely needs to be a copy
            MostRecentHandResult = new List<List<PotResult>>(g.MostRecentHandResult); // This definitely needs to be a copy
            GameMode = g.GameMode.ToString();
            if ( isSpectatorView ) {
                // For spectators, always show neutral values
                MyHandSummary = "";
                MyHandDescription = "";
                MyRejoinCode = g.Spectators[spectatorIndex].RejoinCode;
                MyCallAmount = 0;
                MyMaxRaise = 0;
                IsMyTurn = false;
                IAmDealer = false;
                IAmAdministrator = false;
            }
            else {
                // Show values from active player's perspective
                Participant p = g.Participants[playerIndex];
                MyHandSummary = p._HandSummary;
                MyHandDescription = p.IsPlayingBlindInCurrentHand ? "Blind" : p._FullHandDescription;
                MyRejoinCode = p.RejoinCode;
                MyCallAmount = g.IndexOfParticipantToTakeNextAction == playerIndex ? g.CallAmountForParticipantToTakeNextAction : 0;
                MyMaxRaise = g.IndexOfParticipantToTakeNextAction == playerIndex ? g.MaxRaiseForParticipantToTakeNextAction : 0;
                IsMyTurn = ( playerIndex == g.IndexOfParticipantToTakeNextAction );
                IAmDealer = ( playerIndex == g.IndexOfParticipantDealingThisHand ) ;
                IAmAdministrator = ( g.GetIndexOfAdministrator() == playerIndex );
                IIntendToPlayBlindInNextHand = ( p.IntendsToPlayBlindInNextHand );
                IAmPlayingBlindInCurrentHand = ( p.IsPlayingBlindInCurrentHand );
            }
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
            // Add a list of participants, with data relevant to this player
            PlayerViewOfParticipants = new List<PlayerCentricParticipantView>();
            for ( int i = 0; i < g.Participants.Count; i++ ) {
                PlayerViewOfParticipants.Add(new PlayerCentricParticipantView(g, playerIndex, i, isSpectatorView));
            }
            // Rotate the list so that current player (or dealer) is first person in the list
            for ( int i = 0; i < playerIndex; i++) {
                PlayerViewOfParticipants.Add(PlayerViewOfParticipants[0]); // Copy first element to end of list
                PlayerViewOfParticipants.RemoveAt(0); // Remove the first element
            }
            // Convert the game-level permissions into a player view of their permissions
            AvailableActions = new List<string>();
            if ( isSpectatorView ) {
                // The only action available to a spectator is to leave the game, and they can do this at any time
                AvailableActions.Add(ActionEnum.Leave.ToString());
            }
            else {
                foreach ( ActionAvailability aa in g.Permissions.MapOfActionsToAvailabilities )
                {
                    if ( aa.Availability == AvailabilityEnum.AnyRegisteredPlayer
                        || ( aa.Availability == AvailabilityEnum.ActivePlayerOnly & IsMyTurn )
                        || ( aa.Availability == AvailabilityEnum.AdministratorOnly & IAmAdministrator )
                        || ( aa.Availability == AvailabilityEnum.AnyUnrevealedRegisteredPlayer & g.Participants[playerIndex].IsSharingHandDetails == false )
                    ) {
                        AvailableActions.Add(aa.Action.ToString());
                    }
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