using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CassandraTestApp
{
    public static class JsonLoader
    {
        public static async Task<List<string>> LoadJson(string inputfileName)
        {
            List<string> items;
            using (StreamReader r = new StreamReader(inputfileName))
            {
                string json = await r.ReadToEndAsync();
                items = JsonConvert.DeserializeObject<List<string>>(json);
            }

            return items;
        }
    }
}