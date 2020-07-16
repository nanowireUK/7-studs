using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Participant
    {
        public Participant(string PName, string connectionId) {
            this.Name = PName;
            this.ConnectionId = connectionId;
        }
        [Required]
        
        public string Name { get; set; }
        public string ConnectionId { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        [Required]
        public List<string> Cards { get; set; }
        public int UncommittedChips { get; set; }
        public int ChipsCommittedToCurrentBettingRound { get; set; }
        public Boolean HasFolded { get; set; }
        public Boolean IsAllIn() {
            return UncommittedChips == 0 & ChipsCommittedToCurrentBettingRound > 0;
        }
    }
}