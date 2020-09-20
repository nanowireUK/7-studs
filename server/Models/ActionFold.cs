namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionFold' Class  
    /// </summary>  
    public class ActionFold : Action
    {  
        public ActionFold(string connectionId, ActionEnum actionType, string gameId, string user) : base(connectionId, actionType, gameId, user)
        {
        }
        public override void ProcessAction()
        {
            // Handle the Fold (note that the base class has already checked the player's eligibility for this action)
            Participant p = G.Participants[PlayerIndex];

            G.RecordLastEvent(UserName + " folded");

            // Implement the Fold
            p.HasFolded = true;

            // Find and set next player (could be no one if all players have now folded)
            G.SetNextPlayerToActOrHandleEndOfHand(PlayerIndex, G.LastEvent);           
        }
    }     
}  
