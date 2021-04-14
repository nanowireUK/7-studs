using System;
using System.ComponentModel.DataAnnotations;

namespace SocialPokerClub.Models
{
    public class PotContribution
    {
        public Guid Id { get; set; }
        [Required]

        public Participant Participant { get; set; }
        public int Contribution  { get; set; }
    }
}