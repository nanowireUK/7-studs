using System;
using System.ComponentModel.DataAnnotations;

namespace SignalRChat.Models
{
    public class Card
    {
        public Guid Id { get; set; }
        public string Suit { get; set; } // C, D, H, S: Could make an object out of this but can't see any benefit yet
        public string Rank { get; set; } // 2 - 10, J, Q ,K, A: Could make an object out of this but can't see any benefit yet
        public string Description { get; set; } // e.g 2D, KS, etc., or perhaps 'Two of Diamonds', etc.

        // Might want some way of linking each card to its UI representation
    }
}