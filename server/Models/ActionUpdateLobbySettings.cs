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

            // Update the relevant settings on the game.
            // Some changes may result in further changes
            bool anythingChanged = false;

            if ( s.Ante != G.Ante ) {
                System.Diagnostics.Debug.WriteLine("Changing Ante from "+G.Ante+" to "+s.Ante);
                G.Ante = s.Ante; 
                anythingChanged = true;
            }
            if ( s.InitialChipQuantity != G.InitialChipQuantity ) {
                System.Diagnostics.Debug.WriteLine("Changing InitialChipQuantity from "+G.InitialChipQuantity+" to "+s.InitialChipQuantity);
                G.InitialChipQuantity = s.InitialChipQuantity;
                if ( G._ContinueIsAvailable == true ) {
                    System.Diagnostics.Debug.WriteLine("Disabling the Continue action"); /// Really only necessary if players have joined in the meantime
                    G._ContinueIsAvailable = false;
                }
                anythingChanged = true;
            }
            if ( s.AcceptNewPlayers != G.AcceptNewPlayers ) {
                System.Diagnostics.Debug.WriteLine("Changing AcceptNewPlayers from "+G.AcceptNewPlayers+" to "+s.AcceptNewPlayers);
                G.AcceptNewPlayers = s.AcceptNewPlayers; 
                anythingChanged = true;
            }
             if ( s.AcceptNewSpectators != G.AcceptNewSpectators ) {
                System.Diagnostics.Debug.WriteLine("Changing AcceptNewSpectators from "+G.AcceptNewSpectators+" to "+s.AcceptNewSpectators);
                G.AcceptNewSpectators = s.AcceptNewSpectators; 
                anythingChanged = true;
            }
            if ( anythingChanged ) {
                G.RecordLastEvent("Lobby settings updated");
            }
            // No change to next action 
            G.NextAction = "Await new player or start the game";
            await Task.FromResult(0); // Just to work around compiler warning "This async method lacks 'await' operators and will run synchronously"
        }
    }     
}  
