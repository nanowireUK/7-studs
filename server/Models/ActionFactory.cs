using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{
    /// <summary>  
    /// The 'ActionFactory' static class  
    /// </summary>  
    public static class ActionFactory
    {
        public static Action NewAction(
            string connectionId,
            ActionEnum actionType,
            string gameId,
            string user,
            string leavers,
            string parameters // only used in some cases
        )
        {
            // Only Join is allowable for games that do not exist
            if ( Game.GameExists(gameId) == false && actionType != ActionEnum.Join ) {
                throw new HubException("The only allowable action against a non-existent game is to join it");
            }
            switch (actionType)  
            { 
                case ActionEnum.Open:  
                    return new ActionOpen(connectionId, actionType, gameId, user, leavers);
                case ActionEnum.Join:  
                    return new ActionJoin(connectionId, actionType, gameId, user, leavers);                    
                case ActionEnum.Rejoin:  
                    return new ActionRejoin(connectionId, actionType, gameId, user, leavers, parameters);                
                case ActionEnum.Leave:  
                    return new ActionLeave(connectionId, actionType, gameId, user, leavers);
                case ActionEnum.Start:  
                    return new ActionStart(connectionId, actionType, gameId, user, leavers);   
                case ActionEnum.Continue:  
                    return new ActionContinue(connectionId, actionType, gameId, user, leavers);                     
                case ActionEnum.Reveal:  
                    return new ActionReveal(connectionId, actionType, gameId, user, leavers);                         
                case ActionEnum.Check:  
                    return new ActionCheck(connectionId, actionType, gameId, user, leavers);   
                case ActionEnum.Call:  
                    return new ActionCall(connectionId, actionType, gameId, user, leavers);   
                case ActionEnum.Raise:  
                    return new ActionRaise(connectionId, actionType, gameId, user, leavers, parameters);     
                case ActionEnum.Cover:  
                    return new ActionCover(connectionId, actionType, gameId, user, leavers);   
                case ActionEnum.Fold:  
                    return new ActionFold(connectionId, actionType, gameId, user, leavers);   
                case ActionEnum.GetState:  
                    return new ActionGetState(connectionId, actionType, gameId, user, leavers);   
                case ActionEnum.GetMyState:  
                    return new ActionGetMyState(connectionId, actionType, gameId, user, leavers);                        
                case ActionEnum.GetLog:  
                    return new ActionGetLog(connectionId, actionType, gameId, user, leavers);  
                case ActionEnum.AdHocQuery:  
                    return new ActionAdHocQuery(connectionId, actionType, gameId, user, leavers, parameters);   
                case ActionEnum.Replay:  
                    return new ActionReplay(connectionId, actionType, gameId, user, leavers, parameters);                                                                                                   
                default:  
                    throw new System.Exception("7Studs User Exception: Unsupported action");
            }  
        }
    }
}
