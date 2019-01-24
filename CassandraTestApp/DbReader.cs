using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cassandra;
using static System.Int32;

namespace CassandraTestApp
{
    public class DbReader : IDbReader
    {
        private readonly ICounter _counter;
        private readonly ITimedCounterReader _counterReader;
        private readonly string _keyspace = "nameslist";
        private readonly ConcurrentQueue<string> _cq = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _cqDestination = new ConcurrentQueue<string>();
        private PreparedStatement _ps;

        public DbReader(ICounter counter, ITimedCounterReader counterReader)
        {
            _counter = counter;
            _counterReader = counterReader;
        }

        public async Task Start()
        {
            Console.WriteLine("Creating session...");
            var session = CreateConnectedSession();

            ReadAllData(session);

            _ps = session.Prepare("SELECT * FROM nameslist.names WHERE name = ?");
            _ps.SetConsistencyLevel(ConsistencyLevel.Three);

            Console.WriteLine("Loading...");
            Console.WriteLine("Number of threads?");
            var readLine = Console.ReadLine();
            var nrOfThreads = Parse(readLine);
            _counterReader.StartStopWatch();

            await ConcurrentUtils.ConcurrentUtils.Times(nrOfThreads, nrOfThreads, index => LoadItemPerKey(session));
        }

        private async Task LoadItemPerKey(ISession session)
        {
            Console.WriteLine("Thread started...");

            while (_cq.TryDequeue(out var itemKey))
            {
                var ss = _ps.Bind(itemKey);

                var set = await session.ExecuteAsync(ss);
                _cqDestination.Enqueue(set.First().GetValue<string>("name"));

                _counter.IncrementCounter();
            }
        }

        private void ReadAllData(ISession session)
        {
            Console.WriteLine("Reading data...");
            var sw = new Stopwatch();
            sw.Start();

            var rowSet = session.Execute($"SELECT * FROM {_keyspace}.names");

            sw.Stop();

            Console.WriteLine("Converting...");

            foreach (var row in rowSet)
            {
                _cq.Enqueue(row.GetValue<string>("name"));
            }

            Console.WriteLine($"{rowSet.Count()} rows found in {sw.ElapsedMilliseconds} ms");
        }

        private ISession CreateConnectedSession()
        {
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
    }
}