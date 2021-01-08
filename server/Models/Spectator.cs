using System;
using System.Collections.Generic;

namespace SevenStuds.Models
{
    public class Spectator
    {
        public Spectator(string PName) {
            this.Name = PName;
            this.RejoinCode = GenerateRejoinCode();
            this.SpectatorSignalRGroupName = PName + '.' + Guid.NewGuid().ToString(); // Unique group id for this player (who may connect)
            this._ConnectionIds = new List<string>(); // This user's connection ids will be recorded here
        }
        public string Name { get; set; }
        public string RejoinCode { get; set; } // e.g. 3 alphanumeric characters that enables a disconnected player to rejoin as the same person
        public string SpectatorSignalRGroupName { get; set; }
        private List<string> _ConnectionIds { get; set; }
        public string GenerateRejoinCode() {
            string seed = "abcdefghijkmnopqrstuvwxyz023456789"; // no '1' and no 'l' as too easy to mix up
            string code = "";
            Random r = ServerState.ServerLevelRandomNumberGenerator;
            for ( int i = 0; i < 4; i++) {
                code += seed[r.Next(0, seed.Length - i)];
            }
            return code;
        }
        public List<string> GetConnectionIds() {
            return _ConnectionIds;
        }

        public void NoteConnectionId(string connectionId) {
            if ( ! _ConnectionIds.Contains(connectionId)) {
                _ConnectionIds.Add(connectionId);
            }
        }
    }
}