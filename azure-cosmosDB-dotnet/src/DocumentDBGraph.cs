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
    using System.IO;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using todo.Models;

    public static class DocumentDBGraph<T> where T : class
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;
        private static DocumentCollection graph;
        private static Encoding utf8 = Encoding.UTF8;

        public static async Task<T> GetItemAsync(string id, string vertexLabel = null)
        {
            T result = null;
            string gremlinQuery = string.Format("g.V()", utf8);
           
            //g.V().hasLabel('person')
            if (!string.IsNullOrEmpty(vertexLabel))
            {
                gremlinQuery = string.Format($"g.V().hasLabel(\'{vertexLabel}\')", utf8);
            }
            gremlinQuery += string.Format($".has('id', \'{id}\')", utf8);

            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously            
            IDocumentQuery<Vertex> vertexquery = client.CreateGremlinQuery<Vertex>(graph, gremlinQuery);
            while (vertexquery.HasMoreResults)
            {
                foreach (Vertex verquery in await vertexquery.ExecuteNextAsync<Vertex>())
                {
                    var newitem = new Item();
                    newitem.Id = (string)verquery.Id;
                    newitem.Name = (string)verquery.GetVertexProperties("name").First().Value; ;
                    newitem.Description = (string)verquery.GetVertexProperties("description").First().Value;
                    newitem.Completed = (bool)verquery.GetVertexProperties("isComplete").First().Value;
                    var itemProperty = JsonConvert.SerializeObject(newitem);
                    var item = JsonConvert.DeserializeObject<T>(itemProperty);
                    result = item;
                    //result.Add(verquery);   
                    //result.Add(verquery);
                    //result.AddRange(item);
                    Console.WriteLine($"\t {itemProperty}");
                }
            }
            Console.WriteLine();

            return await Task.FromResult<T>(result);
        }

        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            //IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
            //    UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
            //    new FeedOptions { MaxItemCount = -1 })
            //    .Where(predicate)
            //    .AsDocumentQuery();

            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { EnableCrossPartitionQuery = true })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public static string GetJson(object obj)
        {
            var serializer = new JsonSerializer();

            using (var writer = new StringWriter())
            {
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, obj);
                writer.Flush();

                return writer.ToString();
            }
        }
        public static async Task<T[]> GetAllResultsAsync<T>(IDocumentQuery<T> queryAll)
        {
            var list = new List<T>();

            while (queryAll.HasMoreResults)
            {
                var docs = await queryAll.ExecuteNextAsync<T>();

                foreach (var d in docs)
                {
                    list.Add(d);
                }
            }

            return list.ToArray();
        }       

        public static async Task<IEnumerable<T>> GetVertexItemsAsync(string vertexLabel = null)
        {
            string gremlinQuery = string.Format("g.V()", utf8);
            List<T> result = new List<T>();

            //g.V().hasLabel('person')
            if (!string.IsNullOrEmpty(vertexLabel))
            {
                gremlinQuery = string.Format($"g.V().hasLabel(\'{vertexLabel}\')", utf8);
            }               
            
            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously            
            IDocumentQuery<Vertex> vertexquery = client.CreateGremlinQuery<Vertex>(graph, gremlinQuery);
            while (vertexquery.HasMoreResults)
            {
                foreach (Vertex verquery in await vertexquery.ExecuteNextAsync<Vertex>())
                {                                 
                    var newitem = new Item();
                    newitem.Id = (string)verquery.Id;
                    newitem.Name = (string)verquery.GetVertexProperties("name").First().Value; ;
                    newitem.Description = (string)verquery.GetVertexProperties("description").First().Value;
                    newitem.Completed = (bool)verquery.GetVertexProperties("isComplete").First().Value;
                    var itemProperty = JsonConvert.SerializeObject(newitem);
                    var item = JsonConvert.DeserializeObject<T>(itemProperty);   
                    result.Add(item);
                    //result.Add(verquery);   
                    //result.Add(verquery);
                    //result.AddRange(item);
                    Console.WriteLine($"\t {itemProperty}");                    
                }
            }
            Console.WriteLine();

            return await Task.FromResult<IEnumerable<T>>(result);
        }

        public static async Task<T> InsertVertexAsync(string destVertex, Dictionary<string, string> dictionary)
        {
            T result = null;
   
            string gremlinQuery = string.Format($"g.addV(\'{destVertex}\')", utf8);
           
            //VertexProperty vertexProperty
            foreach (KeyValuePair<string, string> property in dictionary)
            {
                string propertyString = string.Format($".property(\'{property.Key}\', \'{property.Value}\')", utf8);
                gremlinQuery += propertyString;
            }

            //VertexProperty [] vertexProperty = new VertexProperty(dictionary); 

            // gremlinQuery likes g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)

            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously
            IDocumentQuery<T> addVertex = client.CreateGremlinQuery<T>(graph, gremlinQuery);
            while (addVertex.HasMoreResults)
            {
                foreach (T vertex in await addVertex.ExecuteNextAsync<T>())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    result = vertex;
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();

            return await Task.FromResult<T>(result);
        }

        public static async Task<bool> InsertVertexAsync(string destVertex, string itemProperty)
        {
            bool result = false;

            var itemPros = itemProperty.Remove(0, 1);
            itemPros = itemPros.Remove(itemPros.Length - 1, 1);
            itemPros = itemPros.Replace('\"', '\'');
            string[] strArray = itemPros.Split(',');

            string gremlinQuery = string.Format($"g.addV(\'{destVertex}\')", utf8);

            foreach (string str in strArray)
            {
                string[] item = str.Split(':');
                string propertyString = string.Format($".property({item[0]}, {item[1]})", utf8);
                gremlinQuery += propertyString;
            }          
            
            Console.WriteLine($"Running {gremlinQuery}");

            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
            // results asychronously
            IDocumentQuery<dynamic> addVertex = client.CreateGremlinQuery<dynamic>(graph, gremlinQuery);
            while (addVertex.HasMoreResults)
            {
                foreach (dynamic vertex in await addVertex.ExecuteNextAsync())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    result = true;
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();

            return await Task.FromResult<bool>(result);
        }

        public static async Task<bool> UpdateVertexAsync(string destVertex, string id, string itemProperty)
        {
            bool result = false;

            var itemPros = itemProperty.Remove(0, 1);
            itemPros = itemPros.Remove(itemPros.Length - 1, 1);
            itemPros = itemPros.Replace('\"', '\'');
            string[] strArray = itemPros.Split(',');

            string gremlinQuery = string.Format($"g.V(\'{id}\')", utf8);

            foreach (string str in strArray)
            {
                var gremUpdate = gremlinQuery;
                string[] item = str.Split(':');
                if (!item[0].Equals("\'id\'"))
                {
                    string propertyString = string.Format($".property({item[0]}, {item[1]})", utf8);
                    // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
                    // results asychronously
                    gremUpdate += propertyString;

                    Console.WriteLine($"Running {gremUpdate}");

                    IDocumentQuery<dynamic> updateVertex = client.CreateGremlinQuery<dynamic>(graph, gremUpdate);
                    while (updateVertex.HasMoreResults)
                    {
                        foreach (dynamic vertex in await updateVertex.ExecuteNextAsync())
                        {
                            // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                            // the first value
                            result = true;
                            Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                        }
                    }
                }                    
            }           
            
            Console.WriteLine();

            return await Task.FromResult<bool>(result);
        }

        public static async void CleanUpVertexAsync()
        {
            string gremlinQuery = string.Format("g.V().drop()", utf8);
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

        public static async Task<bool> DeleteVertexAsync(string id, string vertexLabel = null)
        {
            bool result = false; 
            string gremlinQuery = string.Format("g.V()", utf8);

            //g.V().hasLabel('person')
            if (!string.IsNullOrEmpty(vertexLabel))
            {
                gremlinQuery = string.Format($"g.V().hasLabel(\'{vertexLabel}\')", utf8);
            }

            gremlinQuery += string.Format($".has('id', \'{id}\').drop()", utf8);
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
                    result = true;
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(vertex)}");
                }
            }
            Console.WriteLine();

            return await Task.FromResult<bool>(result);
        }

        public static async void AddVertexEdgeAsync(string sourceVertex, string edgeLabel, string destVertext)
        {
            // gremlinQuery like "g.V('thomas').addE('knows').to(g.V('mary'))"
            string gremlinQuery = string.Format($"g.V(\'{sourceVertex}\').addE(\'{edgeLabel}\').to(g.V(\'{destVertext}\'))", utf8);
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
            string gremlinQuery = string.Format("g.V().count()", utf8);
           
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
            string gremlinQuery = string.Format("g.E().count()", utf8);

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
               graph = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
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