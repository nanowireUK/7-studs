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
            string connectionId
        )
        {
            switch (actionType)  
            {  
                case ActionEnum.Join:  
                    return new ActionJoin(actionType, gameId, user, connectionId);
                case ActionEnum.Start:  
                    return new ActionStart(actionType, gameId, user, connectionId);                    
                default:  
                    throw new System.Exception("Unsupported action");
            }  
        }
    }
}
