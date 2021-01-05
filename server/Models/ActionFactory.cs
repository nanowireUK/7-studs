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
            string roomId,
            string user,
            string leavers,
            string parameters // only used in some cases
        )
        {
            // Only Join is allowable for games that do not exist
            if ( ServerState.RoomExists(roomId) == false && actionType != ActionEnum.Join ) {
                throw new HubException("The only allowable action against a non-existent room is to join it");
            }
            switch (actionType)  
            { 
                case ActionEnum.Open:  
                    return new ActionOpen(connectionId, actionType, roomId, user, leavers);
                case ActionEnum.Join:  
                    return new ActionJoin(connectionId, actionType, roomId, user, leavers);                    
                case ActionEnum.Rejoin:  
                    return new ActionRejoin(connectionId, actionType, roomId, user, leavers, parameters);                
                case ActionEnum.Leave:  
                    return new ActionLeave(connectionId, actionType, roomId, user, leavers);
                case ActionEnum.Start:  
                    return new ActionStart(connectionId, actionType, roomId, user, leavers);   
                case ActionEnum.Continue:  
                    return new ActionContinue(connectionId, actionType, roomId, user, leavers);     
                case ActionEnum.Spectate:  
                    return new ActionSpectate(connectionId, actionType, roomId, user, leavers);                  
                case ActionEnum.Reveal:  
                    return new ActionReveal(connectionId, actionType, roomId, user, leavers);                         
                case ActionEnum.Check:  
                    return new ActionCheck(connectionId, actionType, roomId, user, leavers);   
                case ActionEnum.Call:  
                    return new ActionCall(connectionId, actionType, roomId, user, leavers);   
                case ActionEnum.Raise:  
                    return new ActionRaise(connectionId, actionType, roomId, user, leavers, parameters);     
                case ActionEnum.Cover:  
                    return new ActionCover(connectionId, actionType, roomId, user, leavers);   
                case ActionEnum.Fold:  
                    return new ActionFold(connectionId, actionType, roomId, user, leavers);   
                case ActionEnum.BlindIntent:  
                    return new ActionBlindIntent(connectionId, actionType, roomId, user, leavers);    
                case ActionEnum.BlindReveal:  
                    return new ActionBlindReveal(connectionId, actionType, roomId, user, leavers);                      
                case ActionEnum.GetState:  
                    return new ActionGetState(connectionId, actionType, roomId, user, leavers);   
                case ActionEnum.GetMyState:  
                    return new ActionGetMyState(connectionId, actionType, roomId, user, leavers);                        
                case ActionEnum.GetLog:  
                    return new ActionGetLog(connectionId, actionType, roomId, user, leavers);  
                case ActionEnum.AdHocQuery:  
                    return new ActionAdHocQuery(connectionId, actionType, roomId, user, leavers, parameters);   
                case ActionEnum.Replay:  
                    return new ActionReplay(connectionId, actionType, roomId, user, leavers, parameters);                                                                                                   
                default:  
                    throw new System.Exception("7Studs User Exception: Unsupported action");
            }  
        }
    }
}
