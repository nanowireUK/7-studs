namespace SevenStuds.Models
{
    /// <summary>  
    /// The 'ActionFactory' static class  
    /// </summary>  
    public static class ActionFactory
    {
        public static Action NewAction(
            ActionEnum actionType,
            string gameId,
            string user,
            string parameters // only used in some cases
        )
        {
            switch (actionType)  
            { 
                case ActionEnum.Join:  
                    return new ActionJoin(actionType, gameId, user);
                case ActionEnum.Start:  
                    return new ActionStart(actionType, gameId, user);    
                case ActionEnum.Check:  
                    return new ActionCheck(actionType, gameId, user);   
                case ActionEnum.Call:  
                    return new ActionCall(actionType, gameId, user);   
                case ActionEnum.Raise:  
                    return new ActionRaise(actionType, gameId, user, parameters);     
                case ActionEnum.Cover:  
                    return new ActionCover(actionType, gameId, user);   
                case ActionEnum.Fold:  
                    return new ActionFold(actionType, gameId, user);   
                case ActionEnum.GetState:  
                    return new ActionGetState(actionType, gameId, user);     
                case ActionEnum.GetLog:  
                    return new ActionGetLog(actionType, gameId, user);   
                case ActionEnum.Replay:  
                    return new ActionReplay(actionType, gameId, user, parameters);                                                                                                   
                default:  
                    throw new System.Exception("Unsupported action");
            }  
        }
    }
}
