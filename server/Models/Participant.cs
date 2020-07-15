using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Participant
    {
        public Participant(string PName) {
            this.Name = PName;
        }
        public Guid Id { get; set; }
        [Required]
        public string RejoinCode { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        [Required]
        public List<Card> Cards { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int UncommittedChips { get; set; }
        public int ChipsCommittedToCurrentBettingRound { get; set; }
        public Boolean HasFolded { get; set; }
        public Boolean IsAllIn { get; set; } // Can this be derived from UncommittedChips = 0 and CurrentRound > 0 ?
    }
}