using Newtonsoft.Json;
using SourceSchemaParser.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Models
{
    public class GameItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        [JsonProperty("secret_shop")]
        [JsonConverter(typeof(StringToBoolJsonConverter))]
        public bool SecretShop { get; set; }
        [JsonProperty("side_shop")]
        [JsonConverter(typeof(StringToBoolJsonConverter))]
        public bool SideShop { get; set; }
        [JsonProperty("recipe")]
        [JsonConverter(typeof(StringToBoolJsonConverter))]
        public bool IsRecipe { get; set; }
    }
}