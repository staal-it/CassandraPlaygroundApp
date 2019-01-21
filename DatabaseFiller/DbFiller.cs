using Cassandra;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseFiller
{
    public class DbFiller : IDbFiller
    {
        private readonly ICounter _counter;

        public DbFiller(ICounter counter)
        {
            _counter = counter;
        }

        public async Task Start()
        {
            //WriteFilesPerFirstAndLastNameLetter();
            //WriteFilesPerFirstNameLetter();

            var cq = LoadFilesInQueue();

            var session = CreateConnectedSession();

            PrepareDatabase(session);

            await ConcurrentUtils.ConcurrentUtils.Times(4500, 265, index => ImportFile(session, cq));
        }

        private List<string> LoadJson(string inputfileName)
        {
            List<string> items;
            using (StreamReader r = new StreamReader(inputfileName))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<string>>(json);
            }

            return items;
        }

        private ConcurrentQueue<string> LoadFilesInQueue()
        {
            Console.WriteLine("Loading files in queue...");

            var dirInfo = new DirectoryInfo(@"nameFiles");
            var files = dirInfo.GetFiles("*.json");

            var cq = new ConcurrentQueue<string>();

            foreach (var file in files)
            {
                cq.Enqueue(file.Name);
            }

            Console.WriteLine("Done loading files in queue.");

            return cq;
        }

        private void PrepareDatabase(ISession session)
        {
            Console.WriteLine("Prepare database...");

            var keyspace = "nameslist";
            session.Execute($"DROP KEYSPACE IF EXISTS {keyspace}");
            session.Execute("CREATE KEYSPACE " + keyspace +
                            " WITH replication = {'class' : 'NetworkTopologyStrategy', 'DC1' : 1 };");

            session.ChangeKeyspace(keyspace);

            session.Execute($"CREATE TABLE {keyspace}.names (name varchar primary key) WITH comment = 'A table with names'; ");
            Console.WriteLine("Done preparing database.");
        }

        private ISession CreateConnectedSession()
        {
            var e1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);

            //var e2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4001);
            //var e3 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);
            //var e4 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4003);
            //var e5 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4004);
            //var e6 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4005);

            var cluster = Cluster.Builder()
                .AddContactPoints(e1)
                .WithLoadBalancingPolicy(new TokenAwarePolicy(new DCAwareRoundRobinPolicy()))
                .Build();

            var session = cluster.Connect();
            return session;
        }

        private async Task ImportFile(ISession session, ConcurrentQueue<string> cq)
        {
            Console.WriteLine("Thread started...");
            var r = new Random();

            var ps = session.Prepare("INSERT INTO nameslist.names (name) VALUES (?)");
            ps.SetConsistencyLevel(ConsistencyLevel.Any);

            while (cq.TryDequeue(out var fileName))
            {
                Thread.Sleep(r.Next(0, 100));

                Console.WriteLine($"Reading file: {fileName}");

                var names = LoadJson("nameFiles/" + fileName);

                foreach (var lastName in names)
                {
                    var ss = ps.Bind(lastName);

                    await session.ExecuteAsync(ss);

                    _counter.IncrementCounter();
                }
            }
        }

        private void WriteFilesPerFirstNameLetter()
        {
            var fistNames = LoadJson("InputFiles/first-names.json");
            var lastNames = LoadJson("InputFiles/names.json");

            var completeNames = new List<string>();

            Directory.CreateDirectory("nameFiles");

            string firstnameLetter = fistNames.First().Substring(0, 1);
            foreach (string fistName in fistNames)
            {
                string currentFirstnameLetter = fistName.Substring(0, 1);

                if (currentFirstnameLetter != firstnameLetter)
                {
                    var json = JsonConvert.SerializeObject(completeNames);

                    File.WriteAllText("nameFiles/" + firstnameLetter + "-names.json", json);

                    firstnameLetter = currentFirstnameLetter;

                    completeNames.Clear();
                }

                foreach (var lastName in lastNames)
                {
                    var name = fistName + " " + lastName;

                    completeNames.Add(name.Replace("'", "''"));
                }
            }

            File.WriteAllText("nameFiles/z-names.json", JsonConvert.SerializeObject(completeNames));
        }

        private void WriteFilesPerFirstAndLastNameLetter()
        {
            Console.WriteLine("Creating input files...");
            var fistNames = LoadJson("InputFiles/first-names.json");
            var lastNames = LoadJson("InputFiles/names.json");

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