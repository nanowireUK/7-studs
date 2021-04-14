using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace SocialPokerClub.Models
{
    /// <summary>
    /// A five card poker hand.
    /// </summary>
    /// <remarks>This class represents a five card poker hand.
    /// It implements the IComparable(Of PokerHand) interface, so
    /// hands can be compared and sorted.</remarks>
    public class PokerHand : IComparable<PokerHand>
    {
        private Card mC1;
        private Card mC2;
        private Card mC3;
        private Card mC4;
        private Card mC5;
        private EvalHand mEvalHand;

        /// <summary>
        /// Initialize the hand.
        /// </summary>
        /// <param name="C1">The first of 5 cards.</param>
        /// <param name="C2">The second of 5 cards.</param>
        /// <param name="C3">The third of 5 cards.</param>
        /// <param name="C4">The fourth of 5 cards.</param>
        /// <param name="C5">The fifth of 5 cards.</param>
        /// <param name="EvalTable">An instance of the PokerHandList class.</param>
        /// <remarks>As this class initializes, it will calculate
        /// its "Key" value based on the cards held, and get its rank
        /// and description from the PokerHandList instance passed in
        /// by using the calculated Key value,
        /// which will allow the user of this class to compare
        /// this hand to other hands for the purpose of declaring
        /// winning, losing, or tied hands.</remarks>
        public PokerHand(Card C1, Card C2, Card C3, Card C4, Card C5, PokerHandRankingTable EvalTable)
        {
            int key;
            this.mC1 = C1;
            this.mC2 = C2;
            this.mC3 = C3;
            this.mC4 = C4;
            this.mC5 = C5;

            key = (int)mC1.CardValue * (int)mC2.CardValue * (int)mC3.CardValue * (int)mC4.CardValue * (int)mC5.CardValue;
            if (mC1.CardSuit == mC2.CardSuit && mC2.CardSuit == mC3.CardSuit && mC3.CardSuit == mC4.CardSuit && mC4.CardSuit == mC5.CardSuit)
            {
                //flush keys are negative to differentiate them
                //from non-flush hands of the same 5 card values
                if ( mC5.CardValue != CardEnum.Dummy)
                {
                    // All five cards are present so we have to allow for flushes too
                    key *= -1;
                }
            }
            mEvalHand = EvalTable.EvalHands[key];
        }
        public List<int> PresentationOrder() {
            return mEvalHand.PresentationOrder();
        }
        public List<int> CardValues() {
            return new List<int>{(int)mC1.CardValue, (int)mC2.CardValue, (int)mC3.CardValue , (int)mC4.CardValue, (int)mC5.CardValue};
        }

        public int Strength() {
            // All non-zero hand values are integers of the form lssnnnnnnnn, where 'l' is the level (0-8) and 'ss' is the sublevel (02 - 14)
            if ( mEvalHand.HandValue == 0 ) { return 0; }
            int level = (int) ( mEvalHand.HandValue / 10000000000 ); // Takes the most significant digit which will be 0-8
            if ( level > 8 ) { throw new Exception("Hand value of "+mEvalHand.HandValue+" gives a strength level of "+level+" which is not in the expected range of 1-8"); }
            int subLevel = (int) ( ( mEvalHand.HandValue - ( 10000000000 * level ) ) / 100000000 ); // Get the 'ss' value which will be 2 - 14
            if ( subLevel > 14 ) { throw new Exception("Hand value of "+mEvalHand.HandValue+" gives a strength sub-level of "+subLevel+" which is not in the expected range of 2-14"); }
            int result = ( ( level + 1 ) * 10 ) + ( subLevel > 5 ? subLevel - 5 : 0 ); // should return a strength value from 10 to 99
            return result;
        }

        /// <summary>
        /// The Key value of this hand.
        /// </summary>
        /// <value></value>
        /// <returns>The Key value of this hand.</returns>
        /// <remarks>The Key is calculated by multiplying
        /// the "CardValue" of each card in this five card
        /// hand. Each of the 13 cards (2-A) has a unique
        /// prime number associated with it (same for each suit).
        /// Those numbers are multiplied together to get a unique
        /// value for the hand. If the hand is a flush (all
        /// five cards are of the same suit) the Key is
        /// multiplied by -1 to make it negative, to differentiate
        /// it from a non-flush hand of the same five cards.</remarks>
        public int Key
        {
            get
            {
                return mEvalHand.Key;
            }
        }

        /// <summary>
        /// The rank of this hand.
        /// </summary>
        /// <value></value>
        /// <returns>The Rank of this hand.</returns>
        /// <remarks>The Rank is used to compare this hand
        /// with other hands to determine which is the
        /// "better" hand. The lower the rank, the better
        /// the hand.</remarks>
        public int Rank
        {
            get
            {
                return mEvalHand.Rank;
            }
        }

        /// <summary>
        /// Compares this instance to another instance.
        /// </summary>
        /// <param name="other">An instance of the PokerHand class to be compared
        /// to this instance.</param>
        /// <returns>Less than zero if this instance is less than "Other", Zero if this instance is equal to "Other", More than zero if this instance is greater than "Other"</returns>
        /// <remarks></remarks>
        int IComparable<PokerHand>.CompareTo(PokerHand other)
        {
            return mEvalHand.Rank.CompareTo(other.Rank);
        }

        #region "Operator overrides"

        /// <summary>
        /// Equality Operator.
        /// </summary>
        /// <param name="ThisHand">The PokerHand object on the left hand side of the Operator.</param>
        /// <param name="OtherHand">The PokerHand object on the right hand side of the Operator.</param>
        /// <returns>True if ThisHand equals OtherHand
        /// False if ThisHand does not equal OtherHand</returns>
        /// <remarks>Internally, the Rank property of ThisHand is compared to the Rank property of OtherHand.</remarks>
        public static bool operator ==(PokerHand lhs, PokerHand rhs)
        {
            return lhs.Rank == rhs.Rank;
        }

        /// <summary>
        /// Inequality Operator
        /// </summary>
        /// <param name="ThisHand">The PokerHand object on the left hand side of the Operator.</param>
        /// <param name="OtherHand">The PokerHand object on the right hand side of the Operator.</param>
        /// <returns>False if ThisHand equals OtherHand
        /// True if ThisHand does not equal OtherHand</returns>
        /// <remarks>Internally, the Rank property of ThisHand is compared to the Rank property of OtherHand.</remarks>
        public static bool operator !=(PokerHand lhs, PokerHand rhs)
        {
            return lhs.Rank != rhs.Rank;
        }

        /// <summary>
        /// Greater Than Operator
        /// </summary>
        /// <param name="ThisHand">The PokerHand object on the left hand side of the Operator.</param>
        /// <param name="OtherHand">The PokerHand object on the right hand side of the Operator.</param>
        /// <returns>True if ThisHand is greater than OtherHand
        /// False if ThisHand is less than or equal to OtherHand</returns>
        /// <remarks>Internally, the Rank property of ThisHand is compared to the Rank property of OtherHand.
        /// The smaller Ranks are "greater than" larger Ranks (i.e. a rank of 300 is "greater than" a rank of 500)
        /// because smaller Ranks represent better hands.</remarks>
        public static bool operator >(PokerHand lhs, PokerHand rhs)
        {
            return lhs.Rank > rhs.Rank;
        }

        /// <summary>
        /// Less Than Operator
        /// </summary>
        /// <param name="ThisHand">The PokerHand object on the left hand side of the Operator.</param>
        /// <param name="OtherHand">The PokerHand object on the right hand side of the Operator.</param>
        /// <returns>True if ThisHand is less than OtherHand
        /// False if ThisHand is greater than or equal to OtherHand</returns>
        /// <remarks>Internally, the Rank property of ThisHand is compared to the Rank property of OtherHand.
        /// The larger Ranks are "less than" smaller Ranks (i.e. a rank of 500 is "less than" a rank of 300)
        /// because smaller Ranks represent better hands.</remarks>
        public static bool operator <(PokerHand lhs, PokerHand rhs)
        {
            return lhs.Rank < rhs.Rank;
        }

        /// <summary>
        /// Greater Than Or Equal To Operator
        /// </summary>
        /// <param name="ThisHand">The PokerHand object on the left hand side of the Operator.</param>
        /// <param name="OtherHand">The PokerHand object on the right hand side of the Operator.</param>
        /// <returns>True if ThisHand is greater than or equal to OtherHand
        /// False if ThisHand is less than OtherHand</returns>
        /// <remarks>Internally, the Rank property of ThisHand is compared to the Rank property of OtherHand.
        /// The smaller Ranks are "greater than" larger Ranks (i.e. a rank of 300 is "greater than" a rank of 500)
        /// because smaller Ranks represent better hands.</remarks>
        public static bool operator >=(PokerHand lhs, PokerHand rhs)
        {
            return lhs.Rank >= rhs.Rank;
        }

        /// <summary>
        /// Less Than Or Equal To Operator
        /// </summary>
        /// <param name="ThisHand">The PokerHand object on the left hand side of the Operator.</param>
        /// <param name="OtherHand">The PokerHand object on the right hand side of the Operator.</param>
        /// <returns>True if ThisHand is less than or equal to OtherHand
        /// False if ThisHand is greater than OtherHand</returns>
        /// <remarks>Internally, the Rank property of ThisHand is compared to the Rank property of OtherHand.
        /// The larger Ranks are "less than" smaller Ranks (i.e. a rank of 500 is "less than" a rank of 300)
        /// because smaller Ranks represent better hands.</remarks>
        public static bool operator <=(PokerHand lhs, PokerHand rhs)
        {
            return lhs.Rank <= rhs.Rank;
        }

        #endregion "Operator overrides"

        public override string ToString()
        {
            return ToString(HandToStringFormatEnum.HandDescription);
        }

        /// <summary>
        /// Returns a formatted string  for the PokerHand.
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ToString(HandToStringFormatEnum Format)
        {
            switch (Format)
            {
                case HandToStringFormatEnum.ShortCardsHeld:
                    return mC1.ToString(CardToStringFormatEnum.ShortCardName) + mC2.ToString(CardToStringFormatEnum.ShortCardName) + mC3.ToString(CardToStringFormatEnum.ShortCardName) + mC4.ToString(CardToStringFormatEnum.ShortCardName) + mC5.ToString(CardToStringFormatEnum.ShortCardName);
                case HandToStringFormatEnum.LongCardsHeld:
                    return mC1.ToString() + ", " + mC2.ToString() + ", " + mC3.ToString() + ", " + mC4.ToString() + ", " + mC5.ToString();
                case HandToStringFormatEnum.HandDescription:
                    return mEvalHand.Name;
                default:
                    return mEvalHand.Name;
            }
        }

        public override bool Equals(object obj)
        {
            var hand = obj as PokerHand;
            return hand != null && Rank == hand.Rank;
        }

        public override int GetHashCode()
        {
            return 1852875615 + Rank.GetHashCode();
        }
    }
}


