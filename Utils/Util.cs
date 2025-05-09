using System.Text.Json;
using Azure.Core.Serialization;

namespace MyList.Function.Utils
{
    public static class Util
    {
        public static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Add more settings as needed
        };

        public static string GetConnectionString()
        {
            return Environment.GetEnvironmentVariable("SqlConnectionString");
        }

        public static ObjectSerializer GetObjectSerializer()
        {
            return new JsonObjectSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}

