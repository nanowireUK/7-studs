using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Deck : Stack<Card>
    {

    /// <summary>
    /// A deck of playing cards.
    /// </summary>
    /// <remarks>This class represents a standard deck of 52 playing cards.</remarks>

        /// <summary>
        /// Initializes the Deck.
        /// </summary>
        /// <param name="Shuffled">Optional. If True, Deck will be shuffled after it is initialized.</param>
        /// <remarks>Creates a new Deck with 52 standard playing cards.</remarks>
        public Deck()
        {
            this.Clear(); // used by JSON deserialiser
        }
        public Deck(bool shuffle = true)
        {
            if (shuffle)
            {
                Shuffle();
            }
            else
            {
                InitDeck();
            }
        }

        /// <summary>
        /// Adds standard 52 playing cards to the deck.
        /// </summary>
        /// <remarks>Cards will be in order, just like in a 
        /// new box of cards from the store. 
        /// Deck has 52 Cards (no Jokers).</remarks>
        private void InitDeck()
        {
            this.Clear();

            this.Push(new Card(CardEnum.Two, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Three, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Four, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Five, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Six, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Seven, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Eight, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Nine, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Ten, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Jack, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Queen, SuitEnum.Spades));
            this.Push(new Card(CardEnum.King, SuitEnum.Spades));
            this.Push(new Card(CardEnum.Ace, SuitEnum.Spades));

            this.Push(new Card(CardEnum.Two, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Three, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Four, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Five, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Six, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Seven, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Eight, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Nine, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Ten, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Jack, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Queen, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.King, SuitEnum.Hearts));
            this.Push(new Card(CardEnum.Ace, SuitEnum.Hearts));

            this.Push(new Card(CardEnum.Two, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Three, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Four, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Five, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Six, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Seven, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Eight, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Nine, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Ten, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Jack, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Queen, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.King, SuitEnum.Clubs));
            this.Push(new Card(CardEnum.Ace, SuitEnum.Clubs));

            this.Push(new Card(CardEnum.Two, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Three, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Four, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Five, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Six, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Seven, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Eight, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Nine, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Ten, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Jack, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Queen, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.King, SuitEnum.Diamonds));
            this.Push(new Card(CardEnum.Ace, SuitEnum.Diamonds));

        }
        /// <summary>
        /// Shuffles the cards in the Deck.
        /// </summary>
        /// <remarks>If the Deck is not full (Count=52) then the Deck will be reinitialized with 52 Cards and shuffled.</remarks>
        public void Shuffle()
        {
            //Collection<Card> col = new Collection<Card>();
            List<Card> lst = new List<Card>();
            Random r = new Random();
            Card c;
            int j;

            if (this.Count != 52)
            {
                //cards have been dealt (popped from stack), 
                //or the deck has not been created yet, 
                //so lets start fresh.
                //NEVER shuffle a partial deck.
                InitDeck();
            }

            for (int i = 0; i < 52; i++)
            {
                c = this.Pop();
                lst.Add(c);
            }

            for (int i = 0; i < 52; i++)
            {
                j = r.Next(0, 52 - i);
                c = lst[j];
                lst.RemoveAt(j);
                this.Push(c);
            }
        }
        /// <summary>
        /// Removes and returns the card at the top of the deck. 
        /// </summary>
        /// <returns>The top Card object from the top of the deck.</returns>
        /// <remarks>This function should be called to "Deal" the next card from the deck.
        /// This function will reduce the deck "Count" by 1.</remarks>
        public Card NextCard()
        {
            return this.Pop();
        }

        public Deck Clone()
        {
            // Create a copy of the deck (ideally before any cards are dealt from it)
            Deck myClone = new Deck(false);
            while ( myClone.Count > 0 ) {
                myClone.Pop(); // remove cards until all gone (thought Clear() would do this but not convinced)
            }            
            Card[] cards = this.ToArray(); // does this preserve the order?
            for ( int i = 0; i < cards.Length; i++ ) {
                myClone.Push(cards[cards.Length - 1 - i]);
            }
            return myClone;
        }
    }
}