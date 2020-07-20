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
            g.ClearCommentary();
            int amountAsInt;
            int playerIndex = g.PlayerIndexFromName(user);
            if ( playerIndex == -1 ) {
                g.LastEvent = user + " tried to raise but is not a participant in this game";
                g.AddCommentary(g.LastEvent);
            }
            else if ( playerIndex != g.IndexOfParticipantToTakeNextAction ) {
                g.LastEvent = user + " tried to raise but it is not their turn";
                g.AddCommentary(g.LastEvent);                
            }
            else if ( int.TryParse(amount, out amountAsInt) == false ) {
                g.LastEvent = user + " entered a non-numeric amount to raise by";
                g.AddCommentary(g.LastEvent);                
            }
            else if ( amountAsInt < 1 ) {
                g.LastEvent = user + " did not enter a raise amount of 1 or more";
                g.AddCommentary(g.LastEvent);                
            }           
            else {
                // Handle the raise
                Participant p = g.Participants[playerIndex];
                int catchupAmount = g.MaxChipsInThePotForAnyPlayer() - g.ChipsInAllPotsForSpecifiedPlayer(playerIndex);
                if ( p.UncommittedChips < catchupAmount ) {
                    g.LastEvent = user + " tried to raise but does not have enough to stay in (consider covering pot)";
                    g.AddCommentary(g.LastEvent);                    
                }
                else if ( p.UncommittedChips == catchupAmount ) {
                    g.LastEvent = user + " tried to raise but only has enough to call";
                    g.AddCommentary(g.LastEvent);                                        
                }               
                else if ( p.UncommittedChips < ( catchupAmount + amountAsInt ) ) {
                    g.LastEvent = user + " tried to raise by " +  amount + " but maximum raise would be " + ( p.UncommittedChips - catchupAmount );
                    g.AddCommentary(g.LastEvent);                                        
                }               
                else {
                    // Implement the raise
                    p.UncommittedChips -= (catchupAmount + amountAsInt);
                    string msg = user;
                    // Add this amount to current pot for this player
                    g.AddAmountToCurrentPotForSpecifiedPlayer(playerIndex, catchupAmount + amountAsInt);
                    if ( catchupAmount > 0 ) {
                        msg += " paid " + catchupAmount + " to stay in and";
                    }
                    msg += " raised by " +  amount;
                    g.LastEvent = msg;
                    g.AddCommentary(g.LastEvent);                                        
                    // Identifier player to play next and reset action message to reflect this (in all other cases it stays unchanged)
                    g._IndexOfLastPlayerToRaise = playerIndex; // Note the raise from this player
                    g._CheckIsAvailable = false;
                    // Find and set next player (shouldn't be able to not find one but might be worth checking)
                    g.IndexOfParticipantToTakeNextAction = g.GetIndexOfPlayerToBetNext(playerIndex);
                    g.NextAction = g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                }
            }
            // Return updated status to all the clients 
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }  

       public async Task UserClickedCall(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required (shouldn't be possible)
            g.ClearCommentary();
            int playerIndex = g.PlayerIndexFromName(user);
            if ( playerIndex == -1 ) {
                g.LastEvent = user + " tried to call but is not a participant in this game";
                g.AddCommentary(g.LastEvent);                                    
            }
            else if ( playerIndex != g.IndexOfParticipantToTakeNextAction ) {
                g.LastEvent = user + " tried to call but it is not their turn";
                g.AddCommentary(g.LastEvent);                                                    
            }
            else {
                // Handle the call
                Participant p = g.Participants[playerIndex];
                int catchupAmount = g.MaxChipsInThePotForAnyPlayer() - g.ChipsInAllPotsForSpecifiedPlayer(playerIndex);
                if ( p.UncommittedChips < catchupAmount ) {
                    g.LastEvent = user + " tried to call but does not have enough to stay in (consider covering pot)";
                    g.AddCommentary(g.LastEvent);                                                        
                }
                else {
                    // Implement the call
                    p.UncommittedChips -= catchupAmount;
                    // Add this amount to current pot for this player
                    g.AddAmountToCurrentPotForSpecifiedPlayer(playerIndex, catchupAmount);
                    g.LastEvent = user + " paid " + catchupAmount + " to call";
                    g.AddCommentary(g.LastEvent);                                                        
                    g._CheckIsAvailable = false;
                    // Find and set next player (could be no one if all players have now called)
                    g.IndexOfParticipantToTakeNextAction = g.GetIndexOfPlayerToBetNext(playerIndex);
                    if ( g.IndexOfParticipantToTakeNextAction > -1 ) {
                        g.NextAction = g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    }
                    else if ( g._CardsDealtIncludingCurrent < 7 ) { 
                        g.DealNextRound();
                        g.NextAction = "Started next round, " + g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                        g.AddCommentary("End of round. Next card dealt. " + g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet");   
                    }
                    else  {
                        // This is the end of the hand
                        g.NextAction = g.ProcessEndGame(user + " called, hand ended");
                    }
                }
            }
            // Return updated status to all the clients 
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }   

        public async Task UserClickedCheck(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required (shouldn't be )
            g.ClearCommentary();
            int playerIndex = g.PlayerIndexFromName(user);
            if ( playerIndex == -1 ) {
                g.LastEvent = user + " tried to check but is not a participant in this game";
                g.AddCommentary(g.LastEvent);
            }
            else if ( g._CheckIsAvailable == false ) {
                g.LastEvent = user + " tried to check but this option is no longer available";
                g.AddCommentary(g.LastEvent);
            }
            else {
                if ( g._IndexOfLastPlayerToStartChecking == -1 ){
                    // This player is the first to check (and they can only do this at the start of a betting round)
                    g._IndexOfLastPlayerToStartChecking = playerIndex;
                }
                // Implement the check
                Participant p = g.Participants[playerIndex];
                g.LastEvent = user + " checked";
                g.AddCommentary(g.LastEvent);
                // Find and set next player (could be no one if all players have now checked)
                g.IndexOfParticipantToTakeNextAction = g.GetIndexOfPlayerToBetNext(playerIndex);
                if ( g.IndexOfParticipantToTakeNextAction > -1 ){
                    g.NextAction = g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    g.AddCommentary(g.NextAction);
                }
                else if ( g._CardsDealtIncludingCurrent < 7 ) {
                    g.DealNextRound();
                    g.NextAction = "Everyone checked; started next round, " + g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    g.AddCommentary(g.NextAction);
                }
                else  {
                    // This is the end of the hand
                    g.NextAction = g.ProcessEndGame(user + " checked, hand ended");
                }
            }
            // Return updated status to all the clients 
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        } 

        public async Task UserClickedFold(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required (shouldn't be )
            g.ClearCommentary();
            int playerIndex = g.PlayerIndexFromName(user);
            if ( playerIndex == -1 ) {
                g.LastEvent = user + " tried to fold but is not a participant in this game";
                g.AddCommentary(g.LastEvent);
            }
            else if ( playerIndex != g.IndexOfParticipantToTakeNextAction ) {
                g.LastEvent = user + " tried to fold but it is not their turn";
                g.AddCommentary(g.LastEvent);                
            }
            else {
                // Handle the fold
                Participant p = g.Participants[playerIndex];
                p.HasFolded = true;
                g.LastEvent = user + " folded";
                g.AddCommentary(g.LastEvent);                
                // Check for scenario where only one active player is left
                if ( g.CountOfPlayersLeftIn() == 1 ) {
                    // Everyone has folded except one player
                    g.NextAction = g.ProcessEndGame(user + " folded, only one player left in, hand ended");
                }
                else {
                    // Find and set next player (could be no one if all players have now called or folded)
                    g.IndexOfParticipantToTakeNextAction = g.GetIndexOfPlayerToBetNext(playerIndex);
                    if ( g.IndexOfParticipantToTakeNextAction > -1 ){
                        g.NextAction = g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    }
                    else if ( g._CardsDealtIncludingCurrent < 7 ) {
                        g.DealNextRound();
                        g.NextAction = "Started next round, " + g.Participants[g.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                    }
                    else  {
                        // All 7 cards have now been bet on, so this is the end of the hand
                        g.NextAction = g.ProcessEndGame(user + " folded, hand ended");
                    }
                }
            }
            // Return updated status to all the clients 
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }   

    }
}