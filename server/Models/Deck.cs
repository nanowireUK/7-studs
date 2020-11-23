using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Deck
    {
        /// <summary>
        /// A deck of 52 playing cards.
        /// </summary>
        /// <remarks>This class represents a standard deck of 52 playing cards.</remarks>

        /// <summary>
        /// Initializes the Deck.
        /// </summary>
        /// <param name="Shuffled">Optional. If True, Deck will be shuffled after it is initialized.</param>
        /// <remarks>Creates a new Deck with 52 standard playing cards.</remarks>

        public string DeckId { get; set; }
        public Stack<Card> Cards { get; set; }
        public Deck() // parameterless constructor is required for the JSON deserialiser
        {
            Cards = new Stack<Card>();
            Cards.Clear(); 
        }
        public Deck(string deckId, bool shuffle = true)
        {
            Cards = new Stack<Card>();
            Cards.Clear(); 
            DeckId = deckId;
            InitDeck();
            if (shuffle)
            {
                Shuffle();
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
            Cards.Clear();

            Cards.Push(new Card(CardEnum.Two, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Three, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Four, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Five, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Six, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Seven, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Eight, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Nine, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Ten, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Jack, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Queen, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.King, SuitEnum.Spades));
            Cards.Push(new Card(CardEnum.Ace, SuitEnum.Spades));

            Cards.Push(new Card(CardEnum.Two, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Three, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Four, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Five, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Six, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Seven, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Eight, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Nine, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Ten, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Jack, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Queen, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.King, SuitEnum.Hearts));
            Cards.Push(new Card(CardEnum.Ace, SuitEnum.Hearts));

            Cards.Push(new Card(CardEnum.Two, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Three, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Four, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Five, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Six, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Seven, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Eight, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Nine, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Ten, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Jack, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Queen, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.King, SuitEnum.Clubs));
            Cards.Push(new Card(CardEnum.Ace, SuitEnum.Clubs));

            Cards.Push(new Card(CardEnum.Two, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Three, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Four, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Five, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Six, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Seven, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Eight, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Nine, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Ten, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Jack, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Queen, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.King, SuitEnum.Diamonds));
            Cards.Push(new Card(CardEnum.Ace, SuitEnum.Diamonds));

        }
        /// <summary>
        /// Shuffles the cards in the Deck.
        /// </summary>
        /// <remarks>If the Deck is not full (Count=52) then the Deck will be reinitialized with 52 Cards and shuffled.</remarks>
        private void Shuffle()
        {
            List<Card> lst = new List<Card>();
            Random r = ServerState.ServerLevelRandomNumberGenerator;
            Card c;
            int j;
            for (int i = 0; i < 52; i++)
            {
                c = Cards.Pop();
                lst.Add(c);
            }

            for (int i = 0; i < 52; i++)
            {
                j = r.Next(0, 52 - i);
                c = lst[j];
                lst.RemoveAt(j);
                Cards.Push(c);
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
            return Cards.Pop();
        }

        public Deck Clone() 
        {
            // Clone the current deck, keeping the same deck id
            return this.Clone(this.DeckId);
        }
        public Deck Clone(string deckId)
        {
            // Clone the current deck, but using the supplied deck id
            Deck myClone = new Deck(deckId, false);
            // Remove cards until all gone (thought Clear() would do this but not convinced)
            while ( myClone.Cards.Count > 0 ) {
                myClone.Cards.Pop(); 
            }
            // Copy the cards across one by one            
            Card[] cardsAsArray = this.Cards.ToArray();
            for ( int i = 0; i < cardsAsArray.Length; i++ ) {
                myClone.Cards.Push(cardsAsArray[i]);
            }
            return myClone;
        }
    }
}