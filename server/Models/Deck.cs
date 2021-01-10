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
        /// <remarks>
        /// Objects of this class represent a standard deck of 52 playing cards.
        /// The deck be be shuffled on creation, and it will be depleted as cards are dealt,.
        /// </remarks>

        /// <summary>
        /// Initializes the Deck.
        /// </summary>
        /// <param name="Shuffled">Optional. If True, Deck will be shuffled after it is initialized.</param>
        /// <remarks>Creates a new Deck with 52 standard playing cards.</remarks>

        public string CardList { get; set; }
        public int DeckNumber { get; set; }
        public Stack<Card> Cards { get; set; }
        public Deck() // parameterless constructor is required for the JSON deserialiser
        {
            CardList = "";
            Cards = new Stack<Card>();
        }
        public Deck(int deckNo, bool shuffle = true)
        {
            CardList = "2c3c4c5c6c7c8c9cTcJcQcKcAc2d3d4d5d6d7d8d9dTdJdQdKdAd2h3h4h5h6h7h8h9hThJhQhKhAh2s3s4s5s6s7s8s9sTsJsQsKsAs";
            DeckNumber = deckNo;
            if (shuffle)
            {
                Shuffle();
            }
            Cards = NewCardStackFromCardList(); // This deck was created from a string of cards ... create an equivalent stack of cards
        }
        public Deck(int deckNo, string sourceDeck)
        {
            DeckNumber = deckNo;
            CardList = sourceDeck; 
            Cards = NewCardStackFromCardList(); // This deck was created from a string of cards ... create an equivalent stack of cards
        }
        private void Shuffle()
        {
            Random r = ServerState.ServerLevelRandomNumberGenerator;
            int startCard;
            int cardsToMove;
            for (int i = 0; i < 100; i++) {
                // Take random block of cards from anywhere in the pack and move it to the front (simulate manual shuffle action)
                startCard = r.Next(2, 53); // returns value from 2 to 52 (not 53)
                cardsToMove = r.Next(1, 52-startCard+1); // decide how many cards to move
                string blockToMove = CardList.Substring((startCard-1)*2, cardsToMove*2);
                CardList = CardList.Remove((startCard-1)*2, cardsToMove*2);
                CardList = CardList.Insert(0,blockToMove);
            }
        }
        private Stack<Card> NewCardStackFromCardList()
        {
            Stack<Card> s = new Stack<Card>();
            for (int cardNum = (CardList.Length / 2); cardNum > 0; cardNum-- ) {
                string cardCode = CardList.Substring((cardNum-1)*2, 2);
                s.Push(new Card(cardCode));
            }
            return s;
        }
        public Card NextCard()
        {
            EnsureCardListHasBeenInitialisedFollowingDeserialisation();
            string dealtCard = CardList.Substring(0,2); // First card in card list
            Card nextCard = new Card(dealtCard);
            CardList = CardList.Remove(0,2);
            Card popCard = Cards.Pop();
            if ( popCard.ToString(CardToStringFormatEnum.ShortCardName) != dealtCard) {
                System.Diagnostics.Debug.WriteLine("Error: card dealt from stack '" + popCard.ToString(CardToStringFormatEnum.ShortCardName) 
                    + "' does not match card dealt from string'"+dealtCard+"'");
            }
            return nextCard;
        }
        public void EnsureCardListHasBeenInitialisedFollowingDeserialisation() {
            // This is to allow for the possibility of the Deck having been created by deserialisation of a JSON file that does not yet have the Cardlist field in it
            // (i.e. only the 'Stack<Card> Cards' element was deserialised so we now need to populate CardList from that)
            if ( this.CardList != "" ) {
                return;
            }
            Card[] cardsAsArray = this.Cards.ToArray();
            for ( int i = 0; i < cardsAsArray.Length; i++ ) {
                CardList += cardsAsArray[i].ToString(CardToStringFormatEnum.ShortCardName);
            }
        }
        public Deck Clone() 
        {
            // Clone the current deck, keeping the same deck id
            return this.Clone(this.DeckNumber);
        }
        public Deck Clone(int deckNo)
        {
            EnsureCardListHasBeenInitialisedFollowingDeserialisation();
            // Clone the current deck, but using the supplied deck id
            Deck myClone = new Deck(deckNo, this.CardList);
            return myClone;
        }

        public override string ToString() {
            EnsureCardListHasBeenInitialisedFollowingDeserialisation();
            return this.CardList;

        }
    }
}