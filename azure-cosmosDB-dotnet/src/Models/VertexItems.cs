using Newtonsoft.Json;
using System.Collections.Generic;

namespace todo.Models
{
    public class VertexItems
    {
        
        public string Id { get; set; }

       
        public string Label { get; set; }

      
        public string Type { get; set; }

        
        public Data[] data;
        
    }

    public class Data
    {
        string descrption { get; set; }
        string key { get; set; }
        string value { get; set; }
    }


}