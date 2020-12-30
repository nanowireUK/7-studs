using System;
using System.Collections.Generic;
using System.Text;

namespace SevenStuds.Models
{
    /// <summary>
    /// GameModeEnum: Defines the mode that the game is currently in
    /// </summary>
    public enum UserStatusEnum
    {
        WaitingInLobby = 0,
        ActiveInGame = 1,
        LeavingGame = 2,
        LeftGame = 3,
        Spectator = 4
    }    
    
    /// <summary>
    /// GameModeEnum: Defines the mode that the game is currently in
    /// </summary>
    public enum GameModeEnum
    {
        LobbyOpen = 0,
        HandInProgress = 1,
        HandsBeingRevealed = 2,
        HandCompleted = 3
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
        Continue = 5,
        Reveal = 6, // only allowed between hands (i.e. game is started and a hand has just completed)
        Spectate = 7,
        // Hand-level actions, available only to one player at any one time
        Check = 10,
        Call = 11,
        Raise = 12,
        Cover = 13,
        Fold = 14,
        // Admin or test functions not intended for general use
        GetState = 20,
        GetLog = 21,
        Replay = 22,
        GetMyState = 23,
        AdHocQuery = 24,
        // Used to indicate that no action is applicable (this is for use in 'LastActionInThisHand' when no action has been taken by the player yet)
        Undefined = 25
    }

    /// <summary>
    /// ActionResponseTypeEnum: defines what data should be pass back to the server
    /// </summary>
    public enum ActionResponseTypeEnum : int
    {
        PlayerCentricGameState = 0,
        OverallGameState = 1,
        GameLog = 2,
        ConfirmToPlayerLeavingAndUpdateRemainingPlayers = 3, // this is a bit of a messy mix of ResponseType and Audience
        AdHocServerQuery = 4
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
        AdministratorOnly = 4,
        AnyUnrevealedRegisteredPlayer = 5
    }

    /// <summary>
    /// PotResultReasonEnum: shows why a player won or lost a given pot
    /// </summary>
    public enum PotResultReasonEnum : int
    {
        PlayerFolded = 0,
        ViaHandComparisons = 1,
        NoOneElseLeft = 2,
        PlayerWasNotInThisPot = 3
    }

    public enum ReplayModeEnum : int
    {
        NewGameLog = 0,
        AdvanceOneStep = 1,
        AdvanceToNamedStep = 2
    }

    public enum PlayerStatusEnum : int
    {
        // Note that higher numbers sort first
        Spectator = 0,
        QueuingForNextGame = 1,
        PartOfMostRecentGame = 2
    }

    public enum DatabaseConnectionStatusEnum : int
    {
        ConnectionNotAttempted = 0,
        ConnectionFailed = 1,
        ConnectionEstablised = 2
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
