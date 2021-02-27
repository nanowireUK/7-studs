using System;
using System.Collections;
using System.Collections.Generic;
// using Microsoft.ApplicationInsights;
// using Microsoft.ApplicationInsights.Extensibility;
// using Microsoft.ApplicationInsights.DataContracts;

namespace SocialPokerClub.Models
{
    public class MetricsManager
    {
        // An object that maintains up-to-date server statistics, with automatic updates every minute
        public DateTimeOffset sessionStart;
        public List<MetricsSnapshot> minutelyObservations = new List<MetricsSnapshot>();
        public double ServerTotalConsumedRUs;
        //private TelemetryClient telemetry;
        public MetricsManager()
        {
            sessionStart = DateTimeOffset.UtcNow;
            ServerTotalConsumedRUs = 0;
            // if ( ServerState.TelemetryActive() ) {
            //     telemetry = new TelemetryClient(TelemetryConfiguration.CreateDefault());
            // }
        }

        public void GatherAndEmitStatistics(DateTimeOffset eventTimeUtc)
        {
            long ticksOfRoundedUpMinute = ( ( eventTimeUtc.Ticks / TimeSpan.TicksPerMinute ) + 1 ) * TimeSpan.TicksPerMinute;
            DateTimeOffset startOfNextMinute = new DateTime(ticksOfRoundedUpMinute, DateTimeKind.Utc).ToUniversalTime();
            if ( minutelyObservations.Count == 0 ) {
                minutelyObservations.Add(new MetricsSnapshot(0)); // Set up a dummy starting entry to establish a zero baseline
            }
            minutelyObservations.Add(new MetricsSnapshot()); // Add a real snapshot
            // Remove old entries
            int maxEntries = 60;
            if ( minutelyObservations.Count > maxEntries ) {
                //Console.WriteLine("There are now {0} entries, removing the oldest", minutelyObservations.Count);
                int doomed = minutelyObservations.Count - maxEntries;
                for ( int i = 0; i < doomed; i++ ) {
                    minutelyObservations.RemoveAt(0); // Remove current oldest entry
                }
                //Console.WriteLine("There are now {0} entries", minutelyObservations.Count);
            }
            EmitStatistics(eventTimeUtc, startOfNextMinute);
        }
        private void EmitStatistics(DateTimeOffset eventTimeUtc, DateTimeOffset ourExactMinute)  {
            // (1) Update the latest statistics on the server state object (also makes it available for the player's view)
            // Note cumulative changes since we last gathered statistics
            //Console.WriteLine("Emitting statistics");
            ServerState.MetricsSummary.ReadingTimestamp = ourExactMinute; // This is mainly for Azure Monitor
            ServerState.MetricsSummary.RUsOverall = ServerState.OurDB.ServerTotalConsumedRUs;
            MetricsSnapshot obs_n = minutelyObservations[minutelyObservations.Count-1]; // The most recent measurement in the list (could be 0, i.e. same as first)
            MetricsSnapshot obs_n_minus_1 = minutelyObservations[minutelyObservations.Count-2]; // penultimate entry in the list
            MetricsSnapshot obs_0 = minutelyObservations[0]; // oldest measurement in the list (noting that anything older than an hour has already been removed)
            ServerState.MetricsSummary.ActiveRooms = obs_n.ActiveGames;
            ServerState.MetricsSummary.MovesOverall = obs_n.TotalActionsProcessed;
            ServerState.MetricsSummary.RUsInLastHour = (long) (obs_n.ServerTotalConsumedRUs - obs_0.ServerTotalConsumedRUs);
            ServerState.MetricsSummary.MovesInLastHour = obs_n.TotalActionsProcessed - obs_0.TotalActionsProcessed;
            ServerState.MetricsSummary.RUsInLastMinute   = (long) (obs_n.ServerTotalConsumedRUs - obs_n_minus_1.ServerTotalConsumedRUs);
            ServerState.MetricsSummary.MovesInLastMinute = obs_n.TotalActionsProcessed - obs_n_minus_1.TotalActionsProcessed;

            // (2) Print them as one line to the debug log
            Console.WriteLine("{0:HH:mm:ss.fff}: {1}", eventTimeUtc, ServerState.MetricsSummary.AsJson());

            // (3) Emit them to the Azure Monitor service
            // if ( ServerState.TelemetryActive() ) {
            //     Console.WriteLine("Azure Monitor is active");
            //     telemetry.TrackMetric(new MetricTelemetry(
            //         "ActiveRooms", // name
            //         1, // count
            //         ServerState.MetricsSummary.ActiveRooms, // sum
            //         ServerState.MetricsSummary.ActiveRooms, // min
            //         ServerState.MetricsSummary.ActiveRooms, // max
            //         0 //standardDeviation
            //     ));
            //     telemetry.TrackMetric(new MetricTelemetry(
            //         "RUsInLastMinute", // name
            //         1, // count
            //         ServerState.MetricsSummary.RUsInLastMinute, // sum
            //         ServerState.MetricsSummary.RUsInLastMinute, // min
            //         ServerState.MetricsSummary.RUsInLastMinute, // max
            //         0 //standardDeviation
            //     ));
            // }
            // else {
            //     //Console.WriteLine("Azure Monitor is not active");
            // }
        }
    }
}