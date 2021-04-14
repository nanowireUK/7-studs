using System.Collections.Generic;
using System;

namespace SocialPokerClub.Models
{
    public class User
    {
        public string UniqueUserId { get; set; } // Allocated by systeml, e.g. John[3]
        public string DisplayName;
        public UserStatusEnum Status; // e.g. WaitingInLobby; ActiveInGame; LeavingGame; LeftGame; Spectator
        public DateTimeOffset JoinedAtUtc { get; set; }
        public DateTimeOffset LeftAtUtc { get; set; }
        public User(string userName) {
            DisplayName = userName;
            JoinedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}