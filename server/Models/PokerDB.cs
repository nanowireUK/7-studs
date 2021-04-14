using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocialPokerClub.Models
{
    public class PokerDB
    {
        public DatabaseModeEnum dbMode;
        public DatabaseConnectionStatusEnum dbStatus = DatabaseConnectionStatusEnum.ConnectionNotAttempted;
        private SemaphoreSlim databaseLock = new SemaphoreSlim(1,1000);
        public double ServerTotalConsumedRUs = 0;
        private string EndpointUri;
        //private readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"]; // CosmosDB endpoint
        private string PrimaryKey;
        //private readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"]; // Primary key for the Azure Cosmos account.
        private CosmosClient cosmosClient; // The Cosmos client instance
        private Database ourDatabase; // The database we will create
        private Container ourGamesContainer; // The container we will create.
        private string databaseId = "SocialPokerClub"; // The name of our database
        private string gamesContainerId = "Games"; // The name of our container within the database
        public PokerDB() {
            // Establishes a poker DB object that we will initialise later via methods that will be called in an asynchronous context
            string db_mode = Environment.GetEnvironmentVariable("SpcDbMode");
            if ( db_mode == null ) { db_mode = "Not Found"; }
            switch (db_mode) {
                case "NoDatabase":
                    Console.WriteLine("SpcDbMode=NoDatabase. Application will run without database operations.");
                    dbMode = DatabaseModeEnum.NoDatabase;
                    break;
                case "Recoverability":
                    Console.WriteLine("SpcDbMode=Recoverability. Application will record actions for logging and use in recovery.");
                    dbMode = DatabaseModeEnum.Recoverability;
                    break;
                case "Stateless":
                    Console.WriteLine("SpcDbMode=Stateless. Application will run in database-backed stateless mode.");
                    dbMode = DatabaseModeEnum.Stateless;
                    break;
                default:
                    Console.WriteLine("Env var 'SpcDbMode' has invalid value '"+db_mode+"', using default of 'NoDatabase'");
                    Console.WriteLine("SpcDbMode=NoDatabase. Application will run without database operations.");
                    dbMode = DatabaseModeEnum.NoDatabase;
                    break;
            }
        }
        public async Task<double> RecordGameStart(Game g)
        {
            // -------------------------------------------------------------------------------------------
            // Store a game header document for the new game

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return 0; }

            List<string> players = new List<string>();
            List<string> blindPlayers = new List<string>();
            foreach ( Participant p in g.Participants ) {
                players.Add(p.Name);
                if ( p.IsPlayingBlindInCurrentHand ) {
                    blindPlayers.Add(p.Name);
                }
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
                playersStartingBlind = blindPlayers,
                lobbySettings = new LobbySettings(g),
                id = "GameHeader-" + 0
            };

            ItemResponse<DocOfTypeGameHeader> dbResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeGameHeader>(
                gameHeader,
                new PartitionKey(gameHeader.docGameId));

            // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
            // Console.WriteLine("Created GameHeader in database with id: {0} Operation consumed {1} RUs. Game id = {2}.",
            //     dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.ServerTotalConsumedRUs += dbResponse.RequestCharge; // Add this to our total
            return dbResponse.RequestCharge;
        }

        public async Task<double> RecordGameLogAction(Game g, GameLogAction gla)
        {
            // -------------------------------------------------------------------------------------------
            // Store a game header document for the new game

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return 0; }

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
            // Console.WriteLine("Created GameLogAction item in database with id: {0} Operation consumed {1} RUs. Game id = {2}.",
            //     dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.ServerTotalConsumedRUs += dbResponse.RequestCharge; // Add this to our total
            return dbResponse.RequestCharge;
        }

        public async Task<double> RecordDeck(Game g, Deck d)
        {
            // -------------------------------------------------------------------------------------------
            // Store the deck being used for the current hand

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return 0; }

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
            // Console.WriteLine("Created GameLogAction item in database with id: {0} Operation consumed {1} RUs. Game id = {2}.",
            //     dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.ServerTotalConsumedRUs += dbResponse.RequestCharge; // Add this to our total
            return dbResponse.RequestCharge;
        }
        public async Task<double> UpsertGameState(Game g)
        {
            // -------------------------------------------------------------------------------------------
            // Store the complete game state for the current game (overwriting any previous state for this game)
            //Console.WriteLine("PokerDB: UpsertGameState for game " + g.GameId);

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { 
                //Console.WriteLine("BOTCH: PokerDB: UpsertGameState -> no DB available, noting game reference onto Room.ActiveGame " + g.GameId);
                g.ParentRoom().ActiveGame = g; // This is a botch to help fix replays when in Stateless mode and no DB is available
                return 0;
            }

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
            // Console.WriteLine("Upserted GameState item in database with id: {0} Operation consumed {1} RUs. Game id = {2}.",
            //     dbResponse.Resource.id, dbResponse.RequestCharge, dbResponse.Resource.docGameId);
            this.ServerTotalConsumedRUs += dbResponse.RequestCharge; // Add this to our total
            return dbResponse.RequestCharge;
        }
        public async Task<Game> LoadGameState(string gameId)
        {
            // -------------------------------------------------------------------------------------------
            // Load the most-recently stored version of the game state for the game with the given id
            // Note that the game state has document id "GameState-0" and is overwritten with the latest game state at the end of each action.
            Console.WriteLine("PokerDB: LoadGameState for game " + gameId);

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return null; }

            try
            {
                // Read the item. We can access the body of the item with the Resource property off the ItemResponse.
                // We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                ItemResponse<DocOfTypeGameState> dbResponse = await this.ourGamesContainer.ReadItemAsync<DocOfTypeGameState>("GameState-0", new PartitionKey(gameId));
                // Console.WriteLine("Game state for game with id '{0}' successfully loaded. Operation consumed {1} RUs.",
                //     dbResponse.Resource.docGameId, dbResponse.RequestCharge);
                this.ServerTotalConsumedRUs += dbResponse.RequestCharge; // Add this to our total
                Game returnedGame = dbResponse.Resource.gameState;
                returnedGame.GameLoadDbCost = dbResponse.RequestCharge;
                //string gameAsJson = returnedGame.AsJson();  // for inspection when debugging
                return returnedGame;
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Game state for game with id '{0}' not found.", gameId);
                return null;
            }
        }
        public async Task<Game> LoadMostRecentGameState(string roomId)
        {
            //Console.WriteLine("PokerDB: LoadMostRecentGameState for room " + roomId);

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return null; }

            var sqlQueryText = "SELECT TOP 1 * FROM c WHERE c.id = 'GameState-0' AND c.docRoomId = '"+roomId+"' ORDER BY c.docDateUtc DESC";
            Console.WriteLine("Running query: {0}", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<DocOfTypeGameState> queryResultSetIterator = this.ourGamesContainer.GetItemQueryIterator<DocOfTypeGameState>(queryDefinition);

            List<DocOfTypeGameState> games = new List<DocOfTypeGameState>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<DocOfTypeGameState> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                Console.WriteLine("Results loaded. Operation consumed {0} RUs.", currentResultSet.RequestCharge);
                this.ServerTotalConsumedRUs += currentResultSet.RequestCharge; // Add this to our total
                foreach (DocOfTypeGameState returnedDoc in currentResultSet)
                {
                    //string gameAsJson = returnedGame.AsJson();  // for inspection when debugging
                    Console.WriteLine("Most recent game has id '{0}'.", returnedDoc.docGameId);
                    Game returnedGame = returnedDoc.gameState;
                    returnedGame.GameLoadDbCost = currentResultSet.RequestCharge;
                    return returnedGame; // Only need the first one, but left loop in as sample code
                }
            }
            Console.WriteLine("No recent historical games found for room '{0}'.", roomId);
            return null;
        }
        public async Task<GameLog> LoadGameLog(string gameId)
        {
            Console.WriteLine("Attempting to load game log for game with id '{0}'", gameId);

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return null; }

            GameLog rebuiltLog = new GameLog();

            // (1) Get the GameHeader and start the GameLog from there
            try
            {
                // Read the item. We can access the body of the item with the Resource property off the ItemResponse.
                // We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                ItemResponse<DocOfTypeGameHeader> dbResponse = await this.ourGamesContainer.ReadItemAsync<DocOfTypeGameHeader>("GameHeader-0", new PartitionKey(gameId));
                Console.WriteLine("Game header for game with id '{0}' successfully loaded. Operation consumed {1} RUs.",
                    dbResponse.Resource.docGameId, dbResponse.RequestCharge);
                DocOfTypeGameHeader returnedDoc = dbResponse.Resource;
                rebuiltLog.roomId = returnedDoc.docRoomId;
                rebuiltLog.administrator = returnedDoc.administrator;
                rebuiltLog.startTimeUtc = returnedDoc.startTimeUtc;
                rebuiltLog.endTimeUtc = DateTimeOffset.MinValue;
                rebuiltLog.playersInOrderAtStartOfGame = new List<string>(returnedDoc.playersInOrderAtStartOfGame);
                rebuiltLog.playersStartingBlind = new List<string>(returnedDoc.playersStartingBlind);
                rebuiltLog.lobbySettings = returnedDoc.lobbySettings;
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Game header for game with id '{0}' not found.", gameId);
                return null;
            }

            // (2) Load the decks used in the game
            var deckQueryText = "SELECT * FROM c WHERE c.docType = 'Deck' AND c.docGameId = '"+gameId+"' ORDER BY c.docSeq ASC";
            Console.WriteLine("Running query: {0}", deckQueryText);
            QueryDefinition deckQueryDefinition = new QueryDefinition(deckQueryText);
            FeedIterator<DocOfTypeDeck> deckIterator = this.ourGamesContainer.GetItemQueryIterator<DocOfTypeDeck>(deckQueryDefinition);
            List<DocOfTypeDeck> decks = new List<DocOfTypeDeck>();
            while (deckIterator.HasMoreResults)
            {
                FeedResponse<DocOfTypeDeck> currentResultSet = await deckIterator.ReadNextAsync();
                Console.WriteLine("Deck loaded. Operation consumed {0} RUs.", currentResultSet.RequestCharge);
                foreach (DocOfTypeDeck returnedDoc in currentResultSet)
                {
                    Deck d = returnedDoc.deck;
                    Console.WriteLine("Loaded deck {0}: {1}.", d.DeckNumber, d.CardList);
                    d.Cards = null;
                    rebuiltLog.decks.Add(d);
                }
            }

            // (3) Load the actions used in the game
            var actionQueryText = "SELECT * FROM c WHERE c.docType = 'Action' AND c.docGameId = '"+gameId+"' ORDER BY c.docSeq ASC";
            Console.WriteLine("Running query: {0}", actionQueryText);
            QueryDefinition actionQueryDefinition = new QueryDefinition(actionQueryText);
            FeedIterator<DocOfTypeGameLogAction> actionIterator = this.ourGamesContainer.GetItemQueryIterator<DocOfTypeGameLogAction>(actionQueryDefinition);
            List<DocOfTypeGameLogAction> actions = new List<DocOfTypeGameLogAction>();
            while (actionIterator.HasMoreResults)
            {
                FeedResponse<DocOfTypeGameLogAction> currentResultSet = await actionIterator.ReadNextAsync();
                Console.WriteLine("Action loaded. Operation consumed {0} RUs.", currentResultSet.RequestCharge);
                foreach (DocOfTypeGameLogAction returnedDoc in currentResultSet)
                {
                    GameLogAction a = returnedDoc.action;
                    Console.WriteLine("Loaded action {0}: {1} by {2}.", a.ActionNumber, a.ActionType, a.UserName);
                    rebuiltLog.actions.Add(a);
                    if ( returnedDoc.docDateUtc > rebuiltLog.endTimeUtc ) {
                        rebuiltLog.endTimeUtc= returnedDoc.docDateUtc;
                    }
                }
            }
            Console.WriteLine("Reload complete.");

            return rebuiltLog;
        }
        public async Task<bool> DatabaseConnectionHasBeenEstablished() {
            if ( dbMode == DatabaseModeEnum.NoDatabase ) { 
                //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns false as DB mode is NoDatabase");
                return false; // This will never have a database connection  
            };           
            await databaseLock.WaitAsync().ConfigureAwait(false);
            int semaphoresToReleaseOnCompletion = 1; // Default unless we are the first task to use the semaphore
            if ( dbStatus == DatabaseConnectionStatusEnum.ConnectionNotAttempted ) {
                // This IS the first run through this method
                Console.WriteLine("DatabaseConnectionHasBeenEstablished called for first time (protected by semaphore)");
                semaphoresToReleaseOnCompletion = 1000;
            }
            try {
                if ( dbStatus == DatabaseConnectionStatusEnum.ConnectionNotAttempted ) {
                    try
                    {
                        // Get the Cosmos DB URI from the relevant environment variable
                        Console.WriteLine("Getting env var SpcDbUri");
                        EndpointUri = Environment.GetEnvironmentVariable("SpcDbUri", EnvironmentVariableTarget.Process);
                        if ( EndpointUri == null ) {
                            Console.WriteLine("Env var SpcDbUri not found, server will continue with database functionality deactivated");
                            dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                            //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns false as first attempt failed (no SpcDbUri var found)");
                            return false;
                        }
                        else {
                            Console.WriteLine("SpcDbUri="+EndpointUri);
                        }

                        // Get the Cosmos DB PrimaryKey from the relevant environment variable
                        Console.WriteLine("Getting env var SpcDbPrimaryKey");
                        PrimaryKey = Environment.GetEnvironmentVariable("SpcDbPrimaryKey", EnvironmentVariableTarget.Process);
                        if ( PrimaryKey == null ) {
                            Console.WriteLine("Env var SpcDbPrimaryKey not found, server will continue with database functionality deactivated");
                            dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                            //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns false as first attempt failed (no SpcDbPrimaryKey var found)");
                            return false;
                        }
                        else {
                            Console.WriteLine("SpcDbPrimaryKey="+PrimaryKey);
                        }

                        // Establish connection to the DB server
                        this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "SevenStuds" });

                        // Get a link to the SevenStuds database (creating it first if necessary)
                        DatabaseResponse DBresp = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                        if ( DBresp.StatusCode == HttpStatusCode.OK ) // Accepted??
                        {
                            Console.WriteLine("Database '{0}' already exists", DBresp.Database.Id);
                        }
                        else if ( DBresp.StatusCode == HttpStatusCode.Created )
                        {
                            Console.WriteLine("Database '{0}' has been created", DBresp.Database.Id);
                        }
                        ourDatabase = DBresp.Database;

                        // Get a link to the Games container (creating it first if necessary)
                        ContainerResponse Cresp = await ourDatabase.CreateContainerIfNotExistsAsync(gamesContainerId, "/docGameId", 400);
                        if ( Cresp.StatusCode == HttpStatusCode.OK )
                        {
                            Console.WriteLine("Container '{0}' already exists", Cresp.Container.Id);
                        }
                        else if ( Cresp.StatusCode == HttpStatusCode.Created )
                        {
                            Console.WriteLine("Container '{0}' has been created", Cresp.Container.Id);
                        }
                        ourGamesContainer = Cresp.Container;

                        dbStatus = DatabaseConnectionStatusEnum.ConnectionEstablished;
                        //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns true as first attempt was able to establish a connection");
                        return true;
                    }
                    catch(Exception ex)
                    {
                        // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                        Console.WriteLine("Database connection could not be established. Continuing without database. Reported exception = {0}", ex.Message);
                        dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                        //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns false as first attempt to establish a connection failed");
                        return false;                    
                    }
                }
                if ( dbStatus == DatabaseConnectionStatusEnum.ConnectionEstablished ) {
                    //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns true as connection was previously shown to work");
                    return true;
                }
                else {
                    //Console.WriteLine("DatabaseConnectionHasBeenEstablished returns false as connection was previously shown to have failed");
                    return false;
                }
            }
            finally
            {
                databaseLock.Release(semaphoresToReleaseOnCompletion);
            }                
        }
    }
}
