using System;
using System.Threading.Tasks;
using System.Configuration;
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
                    ContainerResponse Cresp = await ourDatabase.CreateContainerIfNotExistsAsync(gamesContainerId, "/gameId", 400);
                    if ( Cresp.StatusCode == HttpStatusCode.OK )
                    {
                        Console.WriteLine("Container '{0}' already exists\n", Cresp.Container.Id);
                    }
                    else if ( Cresp.StatusCode == HttpStatusCode.Created )
                    {
                        Console.WriteLine("Container '{0}' has been created\n", Cresp.Container.Id);
                    }
                    ourGamesContainer = Cresp.Container;       
        
                    dbStatus = DatabaseConnectionStatusEnum.ConnectionEstablised;
                }
                catch(Exception ex) 
                {
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Database connection could not be established. Continuing without database. Reported exception = {0}\n", ex.Message);
                    dbStatus = DatabaseConnectionStatusEnum.ConnectionFailed;
                }
            }
            return dbStatus == DatabaseConnectionStatusEnum.ConnectionEstablised;
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
                roomId = g.ParentRoom().RoomId,
                docType = "GameHeader",
                docSeq = 0,
                administrator = g.Participants[g.GetIndexOfAdministrator()].Name,
                startTimeUtc = g.StartTime,
                endTimeUtc = DateTimeOffset.MaxValue, // should be set at end of game
                playersInOrderAtStartOfGame = players,
                // Set values that depend on the other values
                gameId = g.ParentRoom().RoomId + "-" + g.StartTime.ToString(),
                id = "GameHeader" + 0               
            };

            //Console.WriteLine("Hello World, about to write game header at "+DateTime.Now.ToString());
            // try
            // {
            //     // Read the item to see if it exists.  
            //     ItemResponse<DocOfTypeGameHeader> readResponse = await this.ourGamesContainer.ReadItemAsync<DocOfTypeGameHeader>(
            //         gameHeader.id, 
            //         new PartitionKey(gameHeader.gameId));
            //     Console.WriteLine("Item in database with id: {0} already exists\n", readResponse.Resource.id);
            // }
            // catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            // {
                // Create an item in the container representing the game header. Note we provide the value of the partition key for this item
                ItemResponse<DocOfTypeGameHeader> createResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeGameHeader>(
                    gameHeader, 
                    new PartitionKey(gameHeader.gameId));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created GameHeader in database with id: {0} Operation consumed {1} RUs.\n", createResponse.Resource.id, createResponse.RequestCharge);
                this.consumedRUs += createResponse.RequestCharge; // Add this to our total
            // }
        }

        public async Task RecordGameLogAction(Game g, GameLogAction gla)
        {
            // -------------------------------------------------------------------------------------------
            // Store a game header document for the new game

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return; }

            DocOfTypeGameLogAction gameLogAction = new DocOfTypeGameLogAction
            {
                roomId = g.ParentRoom().RoomId,
                docType = "Action",
                docSeq = gla.ActionNumber,
                // Set values that depend on the other values
                gameId = g.ParentRoom().RoomId + "-" + g.StartTime.ToString(),
                id = "Action" + gla.ActionNumber,  
                action = gla      
            };

            // Console.WriteLine("About to write game log action at "+DateTime.Now.ToString());
            // try
            // {
            //     // Read the item to see if it exists.  
            //     ItemResponse<DocOfTypeGameLogAction> readResponse = await this.ourGamesContainer.ReadItemAsync<DocOfTypeGameLogAction>(
            //         gameLogAction.id, 
            //         new PartitionKey(gameLogAction.gameId));
            //     Console.WriteLine("Item in database with id: {0} already exists\n", readResponse.Resource.id);
            // }
            // catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            // {
                // Create an item in the container representing the game header. Note we provide the value of the partition key for this item
                ItemResponse<DocOfTypeGameLogAction> createResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeGameLogAction>(
                    gameLogAction, 
                    new PartitionKey(gameLogAction.gameId));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created GameLogAction item in database with id: {0} Operation consumed {1} RUs.\n", createResponse.Resource.id, createResponse.RequestCharge);
                this.consumedRUs += createResponse.RequestCharge; // Add this to our total
            // }
        }

        public async Task RecordDeck(Game g)
        {
            // -------------------------------------------------------------------------------------------
            // Store the deck used for the last hand, along with the provisional game results

            bool dbExists = await this.DatabaseConnectionHasBeenEstablished();
            if ( dbExists == false ) { return; }

            DocOfTypeDeck details = new DocOfTypeDeck
            {
                roomId = g.ParentRoom().RoomId,
                docType = "EndOfHandDetails",
                docSeq = g.HandsPlayedIncludingCurrent,
                // Set values that depend on the other values
                gameId = g.ParentRoom().RoomId + "-" + g.StartTime.ToString(),
                id = "EndOfHandDetails" + g.HandsPlayedIncludingCurrent,  
                deck = g.SnapshotOfDeckForCurrentHand,  
                //lobbyData = g.LobbyData
            };

            // Console.WriteLine("About to write game log action at "+DateTime.Now.ToString());
            // try
            // {
            //     // Read the item to see if it exists.  
            //     ItemResponse<DocOfTypeGameLogAction> readResponse = await this.ourGamesContainer.ReadItemAsync<DocOfTypeGameLogAction>(
            //         gameLogAction.id, 
            //         new PartitionKey(gameLogAction.gameId));
            //     Console.WriteLine("Item in database with id: {0} already exists\n", readResponse.Resource.id);
            // }
            // catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            // {
                var options = new JsonSerializerOptions { WriteIndented = true, };
                options.Converters.Add(new JsonStringEnumConverter(null /*JsonNamingPolicy.CamelCase*/));
                string jsonString = JsonSerializer.Serialize(details, options);

                // Create an item in the container representing the game header. Note we provide the value of the partition key for this item
                ItemResponse<DocOfTypeDeck> createResponse = await this.ourGamesContainer.CreateItemAsync<DocOfTypeDeck>(
                    details, 
                    new PartitionKey(details.gameId));
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created GameLogAction item in database with id: {0} Operation consumed {1} RUs.\n", createResponse.Resource.id, createResponse.RequestCharge);
                this.consumedRUs += createResponse.RequestCharge; // Add this to our total
            // }
        }                
    }
    //     // </Main>

    //     // <GetStartedDemoAsync>
    //     /// <summary>
    //     /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
    //     /// </summary>
    //     public async Task GetStartedDemoAsync()
    //     {
    //         // Create a new instance of the Cosmos Client

    //         await this.CreateDatabaseAsync();
    //         await this.CreateContainerAsync();
    //         await this.ScaleContainerAsync();
    //         await this.AddItemsToContainerAsync();
    //         await this.QueryItemsAsync();
    //         await this.ReplaceFamilyItemAsync();
    //         await this.DeleteFamilyItemAsync();
    //         await this.DeleteDatabaseAndCleanupAsync();
    //     }
    //     // </GetStartedDemoAsync>

    //     // <CreateDatabaseAsync>
    //     /// <summary>
    //     /// Create the database if it does not exist
    //     /// </summary>
    //     private async Task CreateDatabaseAsync()
    //     {
    //         // Create a new database
    //         this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
    //         Console.WriteLine("Created Database: {0}\n", this.database.Id);
    //     }
    //     // </CreateDatabaseAsync>

    //     // <CreateContainerAsync>
    //     /// <summary>
    //     /// Create the container if it does not exist. 
    //     /// Specify "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
    //     /// </summary>
    //     /// <returns></returns>
    //     private async Task CreateContainerAsync()
    //     {
    //         // Create a new container
    //         this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/LastName", 400);
    //         Console.WriteLine("Created Container: {0}\n", this.container.Id);
    //     }
    //     // </CreateContainerAsync>

    //     // <ScaleContainerAsync>
    //     /// <summary>
    //     /// Scale the throughput provisioned on an existing Container.
    //     /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
    //     /// </summary>
    //     /// <returns></returns>
    //     private async Task ScaleContainerAsync()
    //     {
    //         // Read the current throughput
    //         int? throughput = await this.container.ReadThroughputAsync();
    //         if (throughput.HasValue)
    //         {
    //             Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
    //             int newThroughput = throughput.Value + 100;
    //             // Update throughput
    //             await this.container.ReplaceThroughputAsync(newThroughput);
    //             Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
    //         }
            
    //     }
    //     // </ScaleContainerAsync>

    //     // <AddItemsToContainerAsync>
    //     /// <summary>
    //     /// Add Family items to the container
    //     /// </summary>
    //     private async Task AddItemsToContainerAsync()
    //     {
    //         // Create a family object for the Andersen family
    //         Family andersenFamily = new Family
    //         {
    //             Id = "Andersen.1",
    //             LastName = "Andersen",
    //             Parents = new Parent[]
    //             {
    //                 new Parent { FirstName = "Thomas" },
    //                 new Parent { FirstName = "Mary Kay" }
    //             },
    //             Children = new Child[]
    //             {
    //                 new Child
    //                 {
    //                     FirstName = "Henriette Thaulow",
    //                     Gender = "female",
    //                     Grade = 5,
    //                     Pets = new Pet[]
    //                     {
    //                         new Pet { GivenName = "Fluffy" }
    //                     }
    //                 }
    //             },
    //             Address = new Address { State = "WA", County = "King", City = "Seattle" },
    //             IsRegistered = false
    //         };

    //         try
    //         {
    //             // Read the item to see if it exists.  
    //             ItemResponse<Family> andersenFamilyResponse = await this.container.ReadItemAsync<Family>(andersenFamily.Id, new PartitionKey(andersenFamily.LastName));
    //             Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
    //         }
    //         catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    //         {
    //             // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
    //             ItemResponse<Family> andersenFamilyResponse = await this.container.CreateItemAsync<Family>(andersenFamily, new PartitionKey(andersenFamily.LastName));

    //             // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
    //             Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
    //         }

    //         // Create a family object for the Wakefield family
    //         Family wakefieldFamily = new Family
    //         {
    //             Id = "Wakefield.7",
    //             LastName = "Wakefield",
    //             Parents = new Parent[]
    //             {
    //                 new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
    //                 new Parent { FamilyName = "Miller", FirstName = "Ben" }
    //             },
    //             Children = new Child[]
    //             {
    //                 new Child
    //                 {
    //                     FamilyName = "Merriam",
    //                     FirstName = "Jesse",
    //                     Gender = "female",
    //                     Grade = 8,
    //                     Pets = new Pet[]
    //                     {
    //                         new Pet { GivenName = "Goofy" },
    //                         new Pet { GivenName = "Shadow" }
    //                     }
    //                 },
    //                 new Child
    //                 {
    //                     FamilyName = "Miller",
    //                     FirstName = "Lisa",
    //                     Gender = "female",
    //                     Grade = 1
    //                 }
    //             },
    //             Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
    //             IsRegistered = true
    //         };

    //         try
    //         {
    //             // Read the item to see if it exists
    //             ItemResponse<Family> wakefieldFamilyResponse = await this.container.ReadItemAsync<Family>(wakefieldFamily.Id, new PartitionKey(wakefieldFamily.LastName));
    //             Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
    //         }
    //         catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    //         {
    //             // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
    //             ItemResponse<Family> wakefieldFamilyResponse = await this.container.CreateItemAsync<Family>(wakefieldFamily, new PartitionKey(wakefieldFamily.LastName));

    //             // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
    //             Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);
    //         }
    //     }
    //     // </AddItemsToContainerAsync>

    //     // <QueryItemsAsync>
    //     /// <summary>
    //     /// Run a query (using Azure Cosmos DB SQL syntax) against the container
    //     /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
    //     /// </summary>
    //     private async Task QueryItemsAsync()
    //     {
    //         var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";

    //         Console.WriteLine("Running query: {0}\n", sqlQueryText);

    //         QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
    //         FeedIterator<Family> queryResultSetIterator = this.container.GetItemQueryIterator<Family>(queryDefinition);

    //         List<Family> families = new List<Family>();

    //         while (queryResultSetIterator.HasMoreResults)
    //         {
    //             FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
    //             foreach (Family family in currentResultSet)
    //             {
    //                 families.Add(family);
    //                 Console.WriteLine("\tRead {0}\n", family);
    //             }
    //         }
    //     }
    //     // </QueryItemsAsync>

    //     // <ReplaceFamilyItemAsync>
    //     /// <summary>
    //     /// Replace an item in the container
    //     /// </summary>
    //     private async Task ReplaceFamilyItemAsync()
    //     {
    //         ItemResponse<Family> wakefieldFamilyResponse = await this.container.ReadItemAsync<Family>("Wakefield.7", new PartitionKey("Wakefield"));
    //         var itemBody = wakefieldFamilyResponse.Resource;
            
    //         // update registration status from false to true
    //         itemBody.IsRegistered = true;
    //         // update grade of child
    //         itemBody.Children[0].Grade = 6;

    //         // replace the item with the updated content
    //         wakefieldFamilyResponse = await this.container.ReplaceItemAsync<Family>(itemBody, itemBody.Id, new PartitionKey(itemBody.LastName));
    //         Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.LastName, itemBody.Id, wakefieldFamilyResponse.Resource);
    //     }
    //     // </ReplaceFamilyItemAsync>

    //     // <DeleteFamilyItemAsync>
    //     /// <summary>
    //     /// Delete an item in the container
    //     /// </summary>
    //     private async Task DeleteFamilyItemAsync()
    //     {
    //         var partitionKeyValue = "Wakefield";
    //         var familyId = "Wakefield.7";

    //         // Delete an item. Note we must provide the partition key value and id of the item to delete
    //         ItemResponse<Family> wakefieldFamilyResponse = await this.container.DeleteItemAsync<Family>(familyId,new PartitionKey(partitionKeyValue));
    //         Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKeyValue, familyId);
    //     }
    //     // </DeleteFamilyItemAsync>

    //     // <DeleteDatabaseAndCleanupAsync>
    //     /// <summary>
    //     /// Delete the database and dispose of the Cosmos Client instance
    //     /// </summary>
    //     private async Task DeleteDatabaseAndCleanupAsync()
    //     {
    //         DatabaseResponse databaseResourceResponse = await this.database.DeleteAsync();
    //         // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

    //         Console.WriteLine("Deleted Database: {0}\n", this.databaseId);

    //         //Dispose of CosmosClient
    //         this.cosmosClient.Dispose();
    //     }
    //     // </DeleteDatabaseAndCleanupAsync>
    // }
}
