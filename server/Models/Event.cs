using System;
using System.ComponentModel.DataAnnotations;

namespace SevenStuds.Models
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