using System;

using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace SocialPokerClub.Models
{
    public class SpcMetricsManager
    {
        // An object that maintains up-to-date server statistics, with automatic updates every minute
        private readonly object metricsLock = new object();
        public DateTimeOffset sessionStart;
        private List<SpcMetricsSnapshot> minutelyObservations = new List<SpcMetricsSnapshot>();
        public double ServerTotalConsumedRUs;
        private TelemetryClient telemetry;
        public SpcMetricsManager()
        {
            TelemetryConfiguration config = null;
            QuickPulseTelemetryProcessor quickPulseProcessor = null;
            sessionStart = DateTimeOffset.UtcNow;
            ServerTotalConsumedRUs = 0;
            if ( ServerState.TelemetryActive() ) {
                Console.WriteLine("Activating Azure Monitor Telemetry Client");
                config = TelemetryConfiguration.CreateDefault();
                config.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                    .Use((next) =>
                    {
                        quickPulseProcessor = new QuickPulseTelemetryProcessor(next);
                        return quickPulseProcessor;
                    })
                    .Build();

                var quickPulseModule = new QuickPulseTelemetryModule();

                // Secure the control channel.
                string apiKey = ServerState.TelemetryApiKey();
                if ( apiKey != "" ) {
                    // This is optional, but recommended.
                    quickPulseModule.AuthenticationApiKey = apiKey;
                    quickPulseModule.Initialize(config);
                    quickPulseModule.RegisterTelemetryProcessor(quickPulseProcessor);   
                } 
                
                telemetry = new TelemetryClient(config);
            }
        }
        public SpcMetricsSummary GetMetricsSummary() {
            lock ( metricsLock ) {
                return ServerState.MetricsSummary; // ensures that any consumers will wait while statistics are being update
            }
        }
        public void GatherAndEmitStatistics(DateTimeOffset eventTimeUtc)
        {
            long ticksOfRoundedUpMinute = ( ( eventTimeUtc.Ticks / TimeSpan.TicksPerMinute ) + 1 ) * TimeSpan.TicksPerMinute;
            DateTimeOffset startOfNextMinute = new DateTime(ticksOfRoundedUpMinute, DateTimeKind.Utc).ToUniversalTime();
            lock ( metricsLock )
            {
                if ( minutelyObservations.Count == 0 ) {
                    minutelyObservations.Add(new SpcMetricsSnapshot(0)); // Set up a dummy starting entry to establish a zero baseline
                }
                minutelyObservations.Add(new SpcMetricsSnapshot()); // Add a real snapshot
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
                UpdateStatistics(eventTimeUtc, startOfNextMinute);
            }
            EmitStatistics(eventTimeUtc, startOfNextMinute);
        }
        private void UpdateStatistics(DateTimeOffset eventTimeUtc, DateTimeOffset ourExactMinute)  {
            // Update the latest statistics on the server state object (also makes it available for the player's view)
            // Note cumulative changes since we last gathered statistics
            //Console.WriteLine("Updating statistics");
            ServerState.MetricsSummary.ReadingTimestamp = ourExactMinute; // This is mainly for Azure Monitor
            SpcMetricsSnapshot obs_n = minutelyObservations[minutelyObservations.Count-1]; // The most recent measurement in the list (could be 0, i.e. same as first)
            SpcMetricsSnapshot obs_n_minus_1 = minutelyObservations[minutelyObservations.Count-2]; // penultimate entry in the list
            SpcMetricsSnapshot obs_0 = minutelyObservations[0]; // oldest measurement in the list (noting that anything older than an hour has already been removed)
            ServerState.MetricsSummary.RoomsActiveInLastHr = obs_n.RoomsWithActivityInLastHour;
            ServerState.MetricsSummary.RUsOverall = (long) ServerState.OurDB.ServerTotalConsumedRUs;
            ServerState.MetricsSummary.MovesOverall = obs_n.TotalActionsProcessed;
            ServerState.MetricsSummary.RUsInLastHr = (long) (obs_n.ServerTotalConsumedRUs - obs_0.ServerTotalConsumedRUs);
            ServerState.MetricsSummary.MovesInLastHr = obs_n.TotalActionsProcessed - obs_0.TotalActionsProcessed;
            ServerState.MetricsSummary.RUsInLastMin   = (long) (obs_n.ServerTotalConsumedRUs - obs_n_minus_1.ServerTotalConsumedRUs);
            ServerState.MetricsSummary.MovesInLastMin = obs_n.TotalActionsProcessed - obs_n_minus_1.TotalActionsProcessed;
        }
        private void EmitStatistics(DateTimeOffset eventTimeUtc, DateTimeOffset ourExactMinute)  {
            //Console.WriteLine("Emitting statistics");

            // (1) Print them as one line to the debug log
            Console.WriteLine("{0:HH:mm:ss.fff}: {1}", eventTimeUtc, ServerState.MetricsSummary.AsJson());

            // (2) Emit them to the Azure Monitor service
            if ( ServerState.TelemetryActive() ) {
                TrackSingleMetric("RoomsActiveInLastHr", ServerState.MetricsSummary.RoomsActiveInLastHr);
                TrackSingleMetric("RUsInLastMin", ServerState.MetricsSummary.RUsInLastMin);
                TrackSingleMetric("RUsInLastHr", ServerState.MetricsSummary.RUsInLastHr);
                TrackSingleMetric("RUsOverall", ServerState.MetricsSummary.RUsOverall);
                TrackSingleMetric("MovesInLastMin", ServerState.MetricsSummary.MovesInLastMin);
                TrackSingleMetric("MovesInLastHr", ServerState.MetricsSummary.MovesInLastHr);
                TrackSingleMetric("MovesOverall", ServerState.MetricsSummary.MovesOverall);
            }
        }
        private void TrackSingleMetric(string CustomMetricName, double CustomMetricValue)  {
            telemetry.Track(new MetricTelemetry(
                CustomMetricName, // name
                1, // count
                CustomMetricValue, // sum
                CustomMetricValue, // min
                CustomMetricValue, // max
                0 //standardDeviation
            ));
        }        
    }
}