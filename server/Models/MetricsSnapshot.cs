using System;
using System.Collections.Generic;

namespace SocialPokerClub.Models
{
    public class MetricsSnapshot
    {
        // An object that represents the state of the server at the end of a given minute
        public double ServerTotalConsumedRUs;
        public int ActiveGames;
        public int TotalActionsProcessed;
        public MetricsSnapshot()
        {
            ServerTotalConsumedRUs = ServerState.OurDB.ServerTotalConsumedRUs;
            ActiveGames = ServerState.ActiveGames();
            TotalActionsProcessed = ServerState.TotalActionsProcessed;
        }
        public MetricsSnapshot(int initAmt)
        {
            ServerTotalConsumedRUs = initAmt;
            ActiveGames = initAmt;
            TotalActionsProcessed = initAmt;
        }
    }
}