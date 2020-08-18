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
            string parameters // only used in some cases
        )
        {
            switch (actionType)  
            { 
                case ActionEnum.Join:  
                    return new ActionJoin(connectionId, actionType, gameId, user);
                case ActionEnum.Rejoin:  
                    return new ActionRejoin(connectionId, actionType, gameId, user, parameters);
                case ActionEnum.Start:  
                    return new ActionStart(connectionId, actionType, gameId, user);    
                case ActionEnum.Check:  
                    return new ActionCheck(connectionId, actionType, gameId, user);   
                case ActionEnum.Call:  
                    return new ActionCall(connectionId, actionType, gameId, user);   
                case ActionEnum.Raise:  
                    return new ActionRaise(connectionId, actionType, gameId, user, parameters);     
                case ActionEnum.Cover:  
                    return new ActionCover(connectionId, actionType, gameId, user);   
                case ActionEnum.Fold:  
                    return new ActionFold(connectionId, actionType, gameId, user);   
                case ActionEnum.GetState:  
                    return new ActionGetState(connectionId, actionType, gameId, user);     
                case ActionEnum.GetLog:  
                    return new ActionGetLog(connectionId, actionType, gameId, user);   
                case ActionEnum.Replay:  
                    return new ActionReplay(connectionId, actionType, gameId, user, parameters);                                                                                                   
                default:  
                    throw new System.Exception("7Studs User Exception: Unsupported action");
            }  
        }
    }
}
