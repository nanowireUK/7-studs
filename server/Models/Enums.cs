using System;
using System.Collections.Generic;
using System.Text;

namespace SevenStuds.Models
{
    /// <summary>
    /// GameModeEnum: Defines the mode that the game is currently in
    /// </summary>
    public enum GameModeEnum
    {
        LobbyOpen = 0,
        HandInProgress = 1,
        BetweenHands = 2
    }    
    /// <summary>
    /// ActionEnum: Enumeration values are used to communicate actions between client and server
    /// </summary>
    public enum ActionEnum : int
    {
        // Game-level actions, generally available for all players depending on game state
        Open = 0, // Creates a game lobby which allows players to join and the game parameters to be set (ante, start amount etc.)
        Join = 1,
        Rejoin = 2,
        Leave = 3,
        Start = 4, // Starts the first or subsequent hand for an open or started game
        Finish = 5,
        Reveal = 6, // only allowed between hands (i.e. game is started and a hand has just completed)
        // Hand-level actions, available only to one player at any one time
        Check = 10,
        Call = 11,
        Raise = 12,
        Cover = 13,
        Fold = 14,
        // Admin or test functions not intended for general use
        GetState = 20,
        GetLog = 21,
        Replay = 22
    }
   

    /// <summary>
    /// ActionResponseTypeEnum: defines what data should be pass back to the server
    /// </summary>
    public enum ActionResponseTypeEnum : int
    {
        PlayerCentricGameState = 0,
        OverallGameState = 1,
        GameLog = 2,
        ErrorMessage = 3
    }

    /// <summary>
    /// ActionResponseAudienceEnum: 
    /// </summary>
    public enum ActionResponseAudienceEnum : int
    {
        Caller = 0, // The specific client that invoked this command
        CurrentPlayer = 1, // All of the clients that the current player has used to join the game (e.g. laptop AND phone)
        AllPlayers = 2, // All of the clients for all of the players who have joined the game
        Admin = 3 // The administrator (currently thinking this will be the first person to have joined the game)
    }    


    /// <summary>
    /// AvailabilityEnum: Enumeration values are used to communicate availability of actions between client and server
    /// </summary>
    public enum AvailabilityEnum : int
    {
        NotAvailable = 0,
        ActivePlayerOnly = 1,
        AnyRegisteredPlayer = 2,
        AnyUnregisteredPlayer = 3,
        AdministratorOnly = 4
    }
   
    /// <summary>
    /// Enumeration values are used to calculate hand rank keys
    /// </summary>
    public enum CardEnum : int
    {
        Dummy = 1, // Multiplying by 1 won't change anything
        Two = 2,
        Three = 3,
        Four = 5,
        Five = 7,
        Six = 11,
        Seven = 13,
        Eight = 17,
        Nine = 19,
        Ten = 23,
        Jack = 29,
        Queen = 31,
        King = 37,
        Ace = 41
    }
    public enum SuitEnum : int
    {
        Spades,
        Hearts,
        Clubs,
        Diamonds
    }
    public enum CardToStringFormatEnum
    {
        ShortCardName,
        LongCardName
    }

    public enum HandToStringFormatEnum
    {
        ShortCardsHeld,
        LongCardsHeld,
        HandDescription
    }

}
