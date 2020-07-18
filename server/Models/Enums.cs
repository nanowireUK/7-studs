using System;
using System.Collections.Generic;
using System.Text;

namespace SevenStuds.Models
{
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
