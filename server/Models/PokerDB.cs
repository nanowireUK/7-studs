using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SevenStuds.Models
{
    public class PokerDB
    {
        public DatabaseConnectionStatusEnum dbStatus = DatabaseConnectionStatusEnum.ConnectionNotAttempted;
        public double consumedRUs = 0;
        private string EndpointUri;
        //private readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"]; // CosmosDB endpoint
        private string PrimaryKey;
        //private readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"]; // Primary key for the Azure Cosmos account.
        private CosmosClient cosmosClient; // The Cosmos client instance
        private Database ourDatabase; // The database we will create
        private Container ourGamesContainer; // The container we will create.
        private string databaseId = "SevenStuds"; // The name of our database
        private string gamesContainerId = "Games"; // The name of our container within the database
        public PokerDB() {
            // Establishes a poker DB object that we will initialise later via methods that will be called in an asynchronous context
        }
         public async Task RecordGameStart(Game g)
        {
            // -------------------------------------------------------------------------------------------
            // Store a game header document for the new game

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return; }

            List<string> players = new List<string>();
            foreach ( Participant p in g.Participants ) {
                players.Add(p.Name);
            }

            DocOfTypeGameHeader gameHeader = new DocOfTypeGameHeader
            {
                docGameId = g.GameId,
                docRoomId = g.RoomId,
                docType = "GameHeader",
                docSeq = 0,
                administrator = g.Participants[g.GetIndexOfAdministrator()].Name,
                startTimeUtc = g.StartTimeUTC,
                endTimeUtc = DateTimeOffset.MaxValue, // should be set at end of game
                playersInOrderAtStartOfGame = players,
                id = "GameHeader-" + 0               
            };

            ItemResponse<DocOfTypeGameHeader> dbResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeGameHeader>(
                gameHeader, 
                new PartitionKey(gameHeader.docGameId));

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
            Console.WriteLine("Created GameHeader in database with id: {0} Operation consumed {1} RUs. Game id = {2}.\n", 
                dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.consumedRUs += dbResponse.RequestCharge; // Add this to our total
        }

        public async Task RecordGameLogAction(Game g, GameLogAction gla)
        {
            // -------------------------------------------------------------------------------------------
            // Store a game header document for the new game

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return; }

            DocOfTypeGameLogAction gameLogAction = new DocOfTypeGameLogAction
            {
                docGameId = g.GameId,
                docRoomId = g.RoomId,
                docType = "Action",
                docSeq = gla.ActionNumber,
                // Set values that depend on the other values
                id = "Action-" + gla.ActionNumber,  
                action = gla      
            };

            ItemResponse<DocOfTypeGameLogAction> dbResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeGameLogAction>(
                gameLogAction, 
                new PartitionKey(gameLogAction.docGameId));

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
            Console.WriteLine("Created GameLogAction item in database with id: {0} Operation consumed {1} RUs. Game id = {2}.\n", 
                dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.consumedRUs += dbResponse.RequestCharge; // Add this to our total
        }

        public async Task RecordDeck(Game g, Deck d)
        {
            // -------------------------------------------------------------------------------------------
            // Store the deck being used for the current hand

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return; }

            DocOfTypeDeck details = new DocOfTypeDeck
            {
                docGameId = g.GameId,
                docRoomId = g.RoomId,
                docType = "Deck",
                docSeq = g.HandsPlayedIncludingCurrent,
                // Set values that depend on the other values
                id = "Deck-" + g.HandsPlayedIncludingCurrent,  
                deck = d,  
                //lobbyData = g.LobbyData
            };

            var options = new JsonSerializerOptions { WriteIndented = true, };
            options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
            string jsonString = JsonSerializer.Serialize(details, options);

            // Create an item in the container representing the game header. Note we provide the value of the partition key for this item
            ItemResponse<DocOfTypeDeck> dbResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeDeck>(
                details, 
                new PartitionKey(details.docGameId));
            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
            Console.WriteLine("Created GameLogAction item in database with id: {0} Operation consumed {1} RUs. Game id = {2}.\n", 
                dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.consumedRUs += dbResponse.RequestCharge; // Add this to our total

        }  
        public async Task UpsertGameState(Game g)
        {
            // -------------------------------------------------------------------------------------------
            // Store the complete game state for the current game (overwriting any previous state for this game)

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return; }

            DocOfTypeGameState gameState = new DocOfTypeGameState
            {
                docGameId = g.GameId,
                docRoomId = g.RoomId,
                docType = "GameState",
                docSeq = 0,
                // Set values that depend on the other values
                id = "GameState-0",  
                gameState = g
            };

            ItemResponse<DocOfTypeGameState> dbResponse = await this.ourGamesContainer.UpsertItemAsync<DocOfTypeGameState>(
                gameState, 
                new PartitionKey(gameState.docGameId));

            // Note that after upserting the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
            Console.WriteLine("Upserted GameState item in database with id: {0} Operation consumed {1} RUs. Game id = {2}.\n", 
                dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.consumedRUs += dbResponse.RequestCharge; // Add this to our total
        }    
        public async Task<Game> LoadGameState(string gameId)
        {
            // -------------------------------------------------------------------------------------------
            // Load the most-recently stored version of the game state for the game with the given id
            // Note that the game state has document id "GameState-0" and is overwritten with the latest game state at the end of each action.

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return null; }

            try
            {
                // Read the item. We can access the body of the item with the Resource property off the ItemResponse. 
                // We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                ItemResponse<DocOfTypeGameState> dbResponse = await this.ourGamesContainer.ReadItemAsync<DocOfTypeGameState>("GameState-0", new PartitionKey(gameId));
                Console.WriteLine("Game state for game with id '{0}' successfully loaded. Operation consumed {1} RUs.\n", 
                    dbResponse.Resource.docGameId, dbResponse.RequestCharge);
                this.consumedRUs += dbResponse.RequestCharge; // Add this to our total
                Game returnedGame = dbResponse.Resource.gameState;
                //string gameAsJson = returnedGame.AsJson();  // for inspection when debugging
                return returnedGame;
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Game state for game with id '{0}' not found.\n", gameId);
                return null;
            }            
        }  

        public async Task<Game> LoadMostRecentGameState(string roomId)
        {
            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return null; }

            var sqlQueryText = "SELECT TOP 1 * FROM c WHERE c.id = 'GameState-0' AND c.docRoomId = '"+roomId+"' ORDER BY c.docDateUtc DESC"; ////// change top TOP 1?
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<DocOfTypeGameState> queryResultSetIterator = this.ourGamesContainer.GetItemQueryIterator<DocOfTypeGameState>(queryDefinition);

            List<DocOfTypeGameState> games = new List<DocOfTypeGameState>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<DocOfTypeGameState> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                Console.WriteLine("Results loaded. Operation consumed {0} RUs.\n", currentResultSet.RequestCharge);
                this.consumedRUs += currentResultSet.RequestCharge; // Add this to our total
                foreach (DocOfTypeGameState returnedDoc in currentResultSet)
                {
                    //string gameAsJson = returnedGame.AsJson();  // for inspection when debugging
                    Console.WriteLine("Most recent game has id '{0}'.\n", returnedDoc.docGameId);
                    return returnedDoc.gameState; // Only need the first one, but left loop in as sample code
                }
            }
            Console.WriteLine("No recent historical games found for room '{0}'.\n", roomId);
            return null;
        }          
        // public async Task UpsertGameLog(Game g)
        // {
        //     // -------------------------------------------------------------------------------------------
        //     // Store the complete game log for the current game (overwriting any previous log for this game, as it is provisionaly stored at the end of each hand)

        //     bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
        //     if ( dbExists == false ) { return; }

        //     DocOfTypeGameLog gameLog = new DocOfTypeGameLog
        //     {
        //         roomId = g.RoomId,
        //         docType = "Log",
        //         docSeq = 0,
        //         // Set values that depend on the other values
        //         gameId = g.StartTimeUTC.ToString("u") + " " + g.RoomId,
        //         id = "Log-0",  
        //         log = g._GameLog      
        //     };

        //     ItemResponse<DocOfTypeGameLog> dbResponse = await this.ourGamesContainer.UpsertItemAsync<DocOfTypeGameLog>(
        //         gameLog, 
        //         new PartitionKey(gameLog.gameId));

        //     // Note that after upserting the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
        //     Console.WriteLine("Upserted GameLogAction item in database with id: {0} Operation consumed {1} RUs.\n", dbResponse.Resource.id, dbResponse.RequestCharge);
        //     this.consumedRUs += dbResponse.RequestCharge; // Add this to our total
        // }        
        public async Task<bool> DatabaseConnectionHasBeenEstablished() {
            if ( dbStatus == DatabaseConnectionStatusEnum.ConnectionNotAttempted ) {
                try
                {
                    // Get the Cosmos DB URI from the relevant environment variable
                    System.Diagnostics.Debug.WriteLine("Getting env var SevenStudsDbUri");
                    EndpointUri = Environment.GetEnvironmentVariable("SevenStudsDbUri", EnvironmentVariableTarget.Process);
                    if ( EndpointUri == null ) {
                        System.Diagnostics.Debug.WriteLine("Env var SevenStudsDbUri not found, server will continue with database functionality deactivated");
                        dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                        return false;
                    }
                    else {
                        System.Diagnostics.Debug.WriteLine("SevenStudsDbUri="+EndpointUri);
                    }

                    // Get the Cosmos DB PrimaryKey from the relevant environment variable
                    System.Diagnostics.Debug.WriteLine("Getting env var SevenStudsDbPrimaryKey");
                    PrimaryKey = Environment.GetEnvironmentVariable("SevenStudsDbPrimaryKey", EnvironmentVariableTarget.Process);
                    if ( PrimaryKey == null ) {
                        System.Diagnostics.Debug.WriteLine("Env var SevenStudsDbPrimaryKey not found, server will continue with database functionality deactivated");
                        dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                        return false;
                    }
                    else {
                        System.Diagnostics.Debug.WriteLine("SevenStudsDbPrimaryKey="+PrimaryKey);
                    }
                                        
                    // Establish connection to the DB server
                    this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "SevenStuds" });

                    // Get a link to the SevenStuds database (creating it first if necessary)
                    DatabaseResponse DBresp = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                    if ( DBresp.StatusCode == HttpStatusCode.OK ) // Accepted??
                    {
                        Console.WriteLine("Database '{0}' already exists\n", DBresp.Database.Id);
                    }
                    else if ( DBresp.StatusCode == HttpStatusCode.Created )
                    {
                        Console.WriteLine("Database '{0}' has been created\n", DBresp.Database.Id);
                    }
                    ourDatabase = DBresp.Database;

                    // Get a link to the Games container  (creating it first if necessary)
                    ContainerResponse Cresp = await ourDatabase.CreateContainerIfNotExistsAsync(gamesContainerId, "/docGameId", 400);
                    if ( Cresp.StatusCode == HttpStatusCode.OK )
                    {
                        Console.WriteLine("Container '{0}' already exists\n", Cresp.Container.Id);
                    }
                    else if ( Cresp.StatusCode == HttpStatusCode.Created )
                    {
                        Console.WriteLine("Container '{0}' has been created\n", Cresp.Container.Id);
                    }
                    ourGamesContainer = Cresp.Container;       
        
                    dbStatus = DatabaseConnectionStatusEnum.ConnectionEstablished;
                }
                catch(Exception ex) 
                {
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Database connection could not be established. Continuing without database. Reported exception = {0}\n", ex.Message);
                    dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                }
            }
            return dbStatus == DatabaseConnectionStatusEnum.ConnectionEstablished;
        }                      
    }
}
