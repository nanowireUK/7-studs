using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SignalRChat.Models
{
    public class Pot
    {
        public Guid Id { get; set; }
        [Required]

        public List<PotContribution> PotContributions { get; set; }

        // No need to record pot number (it comes from the order of the pots held against the gane)
        // No need to record whether the pot is covered (it comes from the fact that the contributing player has no chips left)
    }
}