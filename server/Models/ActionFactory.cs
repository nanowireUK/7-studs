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
            string amount, // only used for raise
            string connectionId
        )
        {
            switch (actionType)  
            { 
                case ActionEnum.Join:  
                    return new ActionJoin(actionType, gameId, user, connectionId);
                case ActionEnum.Start:  
                    return new ActionStart(actionType, gameId, user, connectionId);    
                case ActionEnum.Check:  
                    return new ActionCheck(actionType, gameId, user, connectionId);   
                case ActionEnum.Call:  
                    return new ActionCall(actionType, gameId, user, connectionId);   
                case ActionEnum.Raise:  
                    return new ActionRaise(actionType, gameId, user, amount, connectionId);     
                case ActionEnum.Cover:  
                    return new ActionCover(actionType, gameId, user, connectionId);   
                case ActionEnum.Fold:  
                    return new ActionFold(actionType, gameId, user, connectionId);                                                                                                  
                default:  
                    throw new System.Exception("Unsupported action");
            }  
        }
    }
}
