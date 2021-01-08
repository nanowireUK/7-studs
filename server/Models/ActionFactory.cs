using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SevenStuds.Models
{
    /// <summary>  
    /// The 'ActionFactory' static class  
    /// </summary>  
    public static class ActionFactory
    {
        public static async Task<Action> NewAction(
            string connectionId,
            ActionEnum actionType,
            string roomId,
            string user,
            string leavers,
            string parameters // only used in some cases
        )
        {
            if ( roomId == "" ) {
                // User has not put in a room name ... can't do anything without that
                // Note that the client catches this exception at the time it is processing the action method, i.e. there is no need to push anything more back to the client
                throw new HubException("You tried to "+actionType.ToString().ToLower()+" but you did not specify a room name"); 
            }
            Room ourRoom = await ServerState.FindOrCreateRoom(roomId); // Find our room or create a new one if required
            Game ourGame = await ServerState.LoadOrRecoverOrCreateGame(ourRoom); 
            // Only Join is allowable for games that do not exist
            // Find out why I did this !!!!!!!!!!!!!!
            // if ( ServerState.RoomExists(roomId) == false && actionType != ActionEnum.Join ) {
            //     throw new HubException("The only allowable action against a non-existent room is to join it");
            // }
         

            switch (actionType)  
            { 
                case ActionEnum.Open:  
                    return new ActionOpen(connectionId, actionType, ourGame, user, leavers);
                case ActionEnum.Join:  
                    return new ActionJoin(connectionId, actionType, ourGame, user, leavers);                    
                case ActionEnum.Rejoin:  
                    return new ActionRejoin(connectionId, actionType, ourGame, user, leavers, parameters);                
                case ActionEnum.Leave:  
                    return new ActionLeave(connectionId, actionType, ourGame, user, leavers);
                case ActionEnum.Start:  
                    return new ActionStart(connectionId, actionType, ourGame, user, leavers);   
                case ActionEnum.Continue:  
                    return new ActionContinue(connectionId, actionType, ourGame, user, leavers);     
                case ActionEnum.Spectate:  
                    return new ActionSpectate(connectionId, actionType, ourGame, user, leavers);                  
                case ActionEnum.Reveal:  
                    return new ActionReveal(connectionId, actionType, ourGame, user, leavers);                         
                case ActionEnum.Check:  
                    return new ActionCheck(connectionId, actionType, ourGame, user, leavers);   
                case ActionEnum.Call:  
                    return new ActionCall(connectionId, actionType, ourGame, user, leavers);   
                case ActionEnum.Raise:  
                    return new ActionRaise(connectionId, actionType, ourGame, user, leavers, parameters);     
                case ActionEnum.Cover:  
                    return new ActionCover(connectionId, actionType, ourGame, user, leavers);   
                case ActionEnum.Fold:  
                    return new ActionFold(connectionId, actionType, ourGame, user, leavers);   
                case ActionEnum.BlindIntent:  
                    return new ActionBlindIntent(connectionId, actionType, ourGame, user, leavers);    
                case ActionEnum.BlindReveal:  
                    return new ActionBlindReveal(connectionId, actionType, ourGame, user, leavers);                      
                case ActionEnum.GetState:  
                    return new ActionGetState(connectionId, actionType, ourGame, user, leavers);   
                case ActionEnum.GetMyState:  
                    return new ActionGetMyState(connectionId, actionType, ourGame, user, leavers);                        
                case ActionEnum.GetLog:  
                    return new ActionGetLog(connectionId, actionType, ourGame, user, leavers);  
                case ActionEnum.AdHocQuery:  
                    return new ActionAdHocQuery(connectionId, actionType, ourGame, user, leavers, parameters);   
                case ActionEnum.Replay:  
                    return new ActionReplay(connectionId, actionType, ourGame, user, leavers, parameters);                                                                                                   
                default:  
                    throw new System.Exception("7Studs User Exception: Unsupported action");
            }  
        }
    }
}
