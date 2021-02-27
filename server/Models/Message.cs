using System;
using System.ComponentModel.DataAnnotations;

namespace SocialPokerClub.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Game RelatedGame { get; set; }
        [Required]
        public string Contents { get; set; }
        [Required]
        public string UserName { get; set; }
        public DateTimeOffset PostedAt { get; set; }
    }
}