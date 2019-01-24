using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CassandraTestApp
{
    public class FileCreator : IFileCreator
    {
        public async Task Start()
        {
            await WriteFilesPerFirstAndLastNameLetter();
        }

        private async Task WriteFilesPerFirstAndLastNameLetter()
        {
            Console.WriteLine("Creating input files...");
            var fistNames = await JsonLoader.LoadJson("InputFiles/first-names.json");
            var lastNames = await JsonLoader.LoadJson("InputFiles/names.json");

            Directory.CreateDirectory("nameFiles");

            var completeNames = new List<string>();

            string startFirstName = fistNames.First();
            foreach (string fistName in fistNames)
            {
                foreach (var lastName in lastNames)
                {
                    var name = fistName + " " + lastName;

                    completeNames.Add(name.Replace("'", "''"));
                }

                var currentFirstname = fistName;

                if (currentFirstname != startFirstName)
                {
                    var json = JsonConvert.SerializeObject(completeNames);

                    File.WriteAllText("nameFiles/" + startFirstName + "-names.json", json);

                    startFirstName = currentFirstname;

                    completeNames.Clear();
                }
            }

            File.WriteAllText("nameFiles/ZZZZZ-names.json", JsonConvert.SerializeObject(completeNames));

            Console.WriteLine("Done creating input files...");
        }
    }
}