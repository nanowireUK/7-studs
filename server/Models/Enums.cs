using System;
using System.Collections.Generic;
using System.Text;

namespace SevenStuds.Models
{
    /// <summary>
    /// ActionEnum: Enumeration values are used to communicate actions between client and server
    /// </summary>
    public enum ActionEnum : int
    {
        // Game-level actions, generally available for all players depending on game state
        Join = 1,
        Rejoin = 2,
        Leave = 3,
        Start = 4,
        Finish = 5,
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
    /// AvailabilityEnum: Enumeration values are used to communicate availability of actions between client and server
    /// </summary>
    public enum AvailabilityEnum : int
    {
        NotAvailable = 0,
        ActivePlayerOnly = 1,
        AnyRegisteredPlayer = 2,
        AnyUnregisteredPlayer = 3
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
