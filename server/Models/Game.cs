using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Game
    {

        public Guid id { get; }
        public Game() {
            Guid guid = System.Guid.NewGuid();
            id = guid;
            SevenStuds.Models.ServerState.GameList.Add(id, this); // Maps the game guid to the game itself (possibly better than just iterating through a list?)
            Participants = new List<Participant>();
        }

        [Required]
        // Fixed game properties
        public int InitialChipQuantity { get; set; }
        public int Ante { get; set; }

        // Game state
        public List<Participant> Participants { get; set; } // ordered list of participants (order represents order around the table)
        public List<Event> Events { get; set; } // ordered list of events associated with the game
        public List<Card> UndealtCards { get; set; } // cards not yet assigned to specific players in this hand
        public List<Pot> Pots { get; set; } // pots built up in the current hand (over multiple rounds of betting)
        public int IndexOfParticipantDealingThisHand { get; set; }
        public int IndexOfParticipantToTakeNextAction { get; set; }
        public int CardsDealtToEachPlayerSoFar { get; set; }
        public int IndexOfLastEvent { get; set; }
        public string NextAction { get; set; } // Just text ... the clients could potentially work this out from the game state

    }
}