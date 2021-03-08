using System;
using System.Collections.Generic;

namespace SocialPokerClub.Models
{
    public class SpcMetricsSnapshot
    {
        // An object that represents the state of the server at the end of a given minute
        public double ServerTotalConsumedRUs;
        public int RoomsWithActivityInLastHour;
        public int TotalActionsProcessed;
        public SpcMetricsSnapshot()
        {
            ServerTotalConsumedRUs = ServerState.OurDB.ServerTotalConsumedRUs;
            RoomsWithActivityInLastHour = ServerState.RoomsWithActivityInLastHour();
            TotalActionsProcessed = ServerState.TotalActionsProcessed;
        }
        public SpcMetricsSnapshot(int initAmt)
        {
            ServerTotalConsumedRUs = initAmt;
            RoomsWithActivityInLastHour = initAmt;
            TotalActionsProcessed = initAmt;
        }
    }
}