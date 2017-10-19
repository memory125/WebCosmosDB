namespace todo
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Azure.Graphs;
    using Newtonsoft.Json;
    using Microsoft.Azure.Graphs.Elements;

    public static class DocumentDBGraph<T> where T : class
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;
        private static DocumentCollection graph;

        public static async Task<T> GetItemAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public static async Task<Vertex> InsertVertexAsync(string destVertex, Dictionary<string, string> dictionary)
        {
            Vertex result = null; 
            string gremlinQuery = string.Format($"g.addV({destVertex})");            
            foreach (KeyValuePair<string, string> property in dictionary)
            {
                string propertyString = string.Format($".property({property.Key}, {property.Value})");
            }
           
            // gremlinQuery likes g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)

            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously
            IDocumentQuery<Vertex> addVertex = client.CreateGremlinQuery<Vertex>(graph, gremlinQuery);
            while (addVertex.HasMoreResults)
            {
                foreach (Vertex vertex in await addVertex.ExecuteNextAsync<Vertex>())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    result = vertex;
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();

            return await Task.FromResult<Vertex>(result);
        }

        public static async void CleanUpVertexAsync()
        {
            string gremlinQuery = "g.V().drop()";
            IDocumentQuery<Vertex> clrupVertex = client.CreateGremlinQuery<Vertex>(graph, gremlinQuery);
            while (clrupVertex.HasMoreResults)
            {
                foreach (Vertex vertex in await clrupVertex.ExecuteNextAsync<Vertex>())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();
        }

        public static async void DeleteVertexAsync(string sourceVertex)
        {
            string gremlinQuery = string.Format($"g.V({sourceVertex}').drop()");
            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously
            IDocumentQuery<Vertex> deleteVertex = client.CreateGremlinQuery<Vertex>(graph, gremlinQuery);
            while (deleteVertex.HasMoreResults)
            {
                foreach (Vertex vertex in await deleteVertex.ExecuteNextAsync<Vertex>())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();
        }

        public static async void AddVertexEdgeAsync(string sourceVertex, string edgeLabel, string destVertext)
        {
            // gremlinQuery like "g.V('thomas').addE('knows').to(g.V('mary'))"
            string gremlinQuery = string.Format($"g.V({sourceVertex}).addE({edgeLabel}).to(g.V({destVertext}))");
            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously
            IDocumentQuery<Vertex> addEdgeVertex = client.CreateGremlinQuery<Vertex>(graph, gremlinQuery);
            while (addEdgeVertex.HasMoreResults)
            {
                foreach (Vertex vertex in await addEdgeVertex.ExecuteNextAsync<Vertex>())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();
        }

        public static async Task<Int64> GetVertexCountAsync()
        {
            Int64 count = 0;
            string gremlinQuery = "g.V().count()";
           
            IDocumentQuery<Int64> VertexCount = client.CreateGremlinQuery<Int64>(graph, gremlinQuery);
            while (VertexCount.HasMoreResults)
            {
                foreach (Int64  vCount in await VertexCount.ExecuteNextAsync<Int64>())
                {
                    count = vCount;
                    Console.WriteLine($"\t count:{vCount}");
                }
            }

            return await Task.FromResult<Int64>(count);
        }

        public static async Task<Int64> GetEdgeCountAsync()
        {
            Int64 count = 0;
            string gremlinQuery = "g.E().count()";

            IDocumentQuery<Int64> EdgeCount = client.CreateGremlinQuery<Int64>(graph, gremlinQuery);
            while (EdgeCount.HasMoreResults)
            {
                foreach (Int64 eCount in await EdgeCount.ExecuteNextAsync<Int64>())
                {
                    count = eCount;
                    Console.WriteLine($"\t count:{eCount}");
                }
            }

            return await Task.FromResult<Int64>(count);
        }

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                   graph = await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}