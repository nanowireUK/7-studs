namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionCall' Class  
    /// </summary>  
    public class ActionCall : Action
    {  
        public ActionCall(ActionEnum actionType, string gameId, string user) : base(actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {         
            int playerIndex = G.PlayerIndexFromName(this.UserName);

            // Handle the Call (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[playerIndex];
            int catchupAmount = G.MaxChipsInAllPotsForAnyPlayer() - G.ChipsInAllPotsForSpecifiedPlayer(playerIndex);
            // Implement the Call
            G.ClearCommentary(); 
            G.LastEvent = UserName + " paid " + catchupAmount + " to call";
            G.AddCommentary(G.LastEvent);                                                        
            G._CheckIsAvailable = false;
            // Add this amount to the pot for this player
            G.MoveAmountToPotForSpecifiedPlayer(playerIndex, catchupAmount);

            // No one can check from this point onwards (until next card is dealt)
            G._CheckIsAvailable = false;
            G.SetActionAvailability(ActionEnum.Check, AvailabilityEnum.NotAvailable); 
            
            // Find and set next player (could be no one if all players have now called)
            G.IndexOfParticipantToTakeNextAction = G.GetIndexOfPlayerToBetNext(playerIndex);
            if ( G.IndexOfParticipantToTakeNextAction > -1 ) {
                G.NextAction = G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
            }
            else if ( G._CardsDealtIncludingCurrent < 7 ) { 
                G.DealNextRound();
                G.NextAction = "Started next round, " + G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet"; 
                G.AddCommentary("End of round. Next card dealt. " + G.Participants[G.IndexOfParticipantToTakeNextAction].Name + " to bet");   
            }
            else  {
                // This is the end of the hand
                G.NextAction = G.ProcessEndOfHand(UserName + " called, hand ended");
            }
            G.SetActionAvailabilityBasedOnCurrentPlayer();
        }
    }     
}  

/*
// original implementation
      public async Task UserClickedCall(string gameId, string user)
        {
            Game g = Game.FindOrCreateGame(gameId); // find our game or create a new one if required (shouldn't be possible)
            g.ClearCommentary();
            int playerIndex = g.PlayerIndexFromName(user);
                // Handle the call
                Participant p = g.Participants[playerIndex];
                int catchupAmount = g.MaxChipsInAllPotsForAnyPlayer() - g.ChipsInAllPotsForSpecifiedPlayer(playerIndex);
                if ( p.UncommittedChips < catchupAmount ) {
                    g.LastEvent = user + " tried to call but does not have enough to stay in (consider covering pot)";
                    g.AddCommentary(g.LastEvent);                                                        
                }
                else if ( catchupAmount == 0 ) {
                    g.LastEvent = user + " tried to call but this is not valid with no bets in current round (consider check or raise)";
                    g.AddCommentary(g.LastEvent);                                                        
                }                
                else {
                    // Implement the call
                    // Add this amount to the pot for this player
                    g.MoveAmountToPotForSpecifiedPlayer(playerIndex, catchupAmount);
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
                        g.NextAction = g.ProcessEndOfHand(user + " called, hand ended");
                    }
                }
            }
            // Return updated status to all the clients 
            await Clients.All.SendAsync("ReceiveUpdatedGameState", g.AsJson());
        }   
*/