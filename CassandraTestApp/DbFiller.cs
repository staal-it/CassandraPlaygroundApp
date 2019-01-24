using Cassandra;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CassandraTestApp
{
    public class DbFiller : IDbFiller
    {
        private readonly ICounter _counter;
        private readonly ITimedCounterReader _counterReader;
        private readonly int _numberOfFilesToLoad;

        public DbFiller(ICounter counter, ITimedCounterReader counterReader)
        {
            _counter = counter;
            _counterReader = counterReader;
            _numberOfFilesToLoad = 182;
        }

        public async Task Start()
        {
            var cq = LoadFilesInQueue();

            var session = CreateConnectedSession();

            Console.WriteLine("(Re)Create database? 'y' or 'n'");
            var readLine = Console.ReadLine();

            if (readLine.ToLower() == "y")
            {
                PrepareDatabase(session);
            }

            _counterReader.StartStopWatch();
            await ConcurrentUtils.ConcurrentUtils.Times(_numberOfFilesToLoad, _numberOfFilesToLoad, index => ImportFile(session, cq));
        }

        private ConcurrentQueue<string> LoadFilesInQueue()
        {
            Console.WriteLine("Loading files in queue...");

            var dirInfo = new DirectoryInfo(@"nameFiles");
            var files = dirInfo.GetFiles("*.json");

            var cq = new ConcurrentQueue<string>();

            foreach (var file in files.Take(_numberOfFilesToLoad))
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
                            " WITH replication = {'class' : 'NetworkTopologyStrategy', 'DC1' : 2, 'DC2' : 2 };");

            session.ChangeKeyspace(keyspace);

            session.Execute($"CREATE TABLE {keyspace}.names (name varchar primary key) WITH comment = 'A table with names'; ");
            Console.WriteLine("Done preparing database.");
        }

        private ISession CreateConnectedSession()
        {
            Console.WriteLine("Creating session...");

            var e1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);
            var e2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4001);
            var e3 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);
            var e4 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4003);
            var e5 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4004);
            var e6 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4005);

            var cluster = Cluster.Builder()
                .AddContactPoints(e1, e2, e3, e4, e5, e6)
                .WithLoadBalancingPolicy(new TokenAwarePolicy(new DCAwareRoundRobinPolicy()))
                .Build();

            var session = cluster.Connect();
            return session;
        }

        private async Task ImportFile(ISession session, ConcurrentQueue<string> cq)
        {
            var r = new Random();

            var ps = session.Prepare("INSERT INTO nameslist.names (name) VALUES (?)");
            ps.SetConsistencyLevel(ConsistencyLevel.Any);

            while (cq.TryDequeue(out var fileName))
            {
                Thread.Sleep(r.Next(0, 100));

                Console.WriteLine($"Reading file: {fileName}");

                var names = await JsonLoader.LoadJson("nameFiles/" + fileName);

                foreach (var lastName in names)
                {
                    var ss = ps.Bind(lastName);

                    await session.ExecuteAsync(ss);

                    _counter.IncrementCounter();
                }
            }
        }
    }
}