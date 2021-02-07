using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SevenStuds.Models
{  
    /// <summary>  
    /// The 'ActionUpdateLobbySettings' Class  
    /// </summary>  
    public class ActionUpdateLobbySettings : Action
    {  
        public ActionUpdateLobbySettings(string connectionId, ActionEnum actionType, Game ourGame, string user, string leavers, string updatedSettingsAsJson) 
            : base( connectionId, actionType, ourGame, user, leavers, updatedSettingsAsJson)
        {
        }
        //public string QueryType;
        public override async Task ProcessAction()
        {
            LobbySettings s = JsonSerializer.Deserialize<LobbySettings>(Parameters);

            bool anythingChanged = s.UpdateGameSettings(G);
  
            if ( anythingChanged ) {
                G.RecordLastEvent("Lobby settings updated");
            }
            // No change to next action 
            G.NextAction = "Await new player or start the game";
            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
