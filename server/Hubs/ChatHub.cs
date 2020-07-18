using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SevenStuds.Models;

namespace SevenStuds.Hubs
{
    public class ChatHub : Hub
    {
        // This is the server-side code that is called by connection.invoke("xxx") in chat.js on the client

        // ---- JOIN GAME
        public async Task UserClickedJoin(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            g.Participants.Add(new Participant(user, Context.ConnectionId));
            g.LastEvent = user + " joined game";
            g.NextAction = "Await new player or start the game";
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }

        public async Task UserClickedStart(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required
            g.InitialiseGame();
            g.LastEvent = user + " started game (player order now randomised)";
            g.NextAction = g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            
            // Find next player: Need to allow for next player being out
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }   

        public async Task UserClickedRaise(string gameId, string user, string amount)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required (shouldn't be )
            int amountAsInt;
            int playerIndex = g.PlayerIndexFromName(user);
            if ( playerIndex == -1 ) {
                g.LastEvent = user + " tried to raise but is not a participant in this game";
            }
            else if ( playerIndex != g.IndexOfParticipantToTakeNextAction ) {
                g.LastEvent = user + " tried to raise but it is not their turn";
            }
            else if ( int.TryParse(amount, out amountAsInt) == false ) {
                g.LastEvent = user + " entered a non-numeric amount to raise by";
            }
            else if ( amountAsInt < 1 ) {
                g.LastEvent = user + " did not enter a raise amount of 1 or more";
            }           
            else {
                // Handle the raise
                Participant p = g.Participants[playerIndex];
                int catchupAmount = g.MaxChipsInThePotForAnyPlayer() - g.ChipsInThePotForSpecifiedPlayer(playerIndex);
                if ( p.UncommittedChips < catchupAmount ) {
                    g.LastEvent = user + " tried to raise but does not have enough to stay in (consider covering pot)";
                }
                else if ( p.UncommittedChips == catchupAmount ) {
                    g.LastEvent = user + " tried to raise but only has enough to call";
                }               
                else if ( p.UncommittedChips < ( catchupAmount + amountAsInt ) ) {
                    g.LastEvent = user + " tried to raise by " +  amount + " but maximum raise would be " + ( p.UncommittedChips - catchupAmount );
                }               
                else {
                    // Implement the raise
                    p.UncommittedChips -= (catchupAmount + amountAsInt);
                    string msg = user;
                    g.AddAmountToCurrentPotForSpecifiedPlayer(playerIndex, catchupAmount + amountAsInt);
                    // Add this amount to current pot for this player
                    if ( catchupAmount > 0 ) {
                        msg += " paid " + catchupAmount + " to stay in and";
                    }
                    msg += " raised by " +  amount;
                    g.LastEvent = msg;
                    // Identifier player to play next and reset action message to reflect this (in all other cases it stays unchanged)
                    g._IndexOfLastPlayerToRaiseOrStartChecking = playerIndex; // Note the raise from this player
                    // Find and set next player (shouldn't be able to not find one but might be worth checking)
                    g.IndexOfParticipantToTakeNextAction = g.GetIndexOfPlayerToBetNext(playerIndex);
                    g.NextAction = g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                }
            }
            // Return updated status to all the clients 
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }        
    }
}