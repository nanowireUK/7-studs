using System;
using System.ComponentModel.DataAnnotations;

namespace SocialPokerClub.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public Participant Participant { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
    }
}