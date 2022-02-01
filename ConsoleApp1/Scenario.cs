using System;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp1;

namespace test
{
    class Scenario
    {
        public static object ScenarioID { get; private set; }
        public static MongoClient client =
            new MongoClient(
                "mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        private static void Main(string[] args)
        {

            Console.WriteLine(" args:- 1.Count, 2.is seq, 3.is parallel 4. PageSize");

            //Console.WriteLine("1. SaveScheduleActual      args:- Count, is seq, is parallel");
            //Console.WriteLine("2. GETScheduleActualwithoutjoin ");
            //Console.WriteLine("3. GETScheduleActual ");
            //Console.WriteLine("4. UPDActualMovement ");
            //Console.WriteLine("5. DELScheduleActual ");
            //Console.WriteLine("6. GETScheduleActualpagination args :- pageSize");
            //Console.WriteLine("7. ReadWriteScheduleActual args:- Count, is seq, is parallel");
            //int input = Convert.ToInt32(args[0]);
            List<Tuple<double, double, double, double, double, double, double>> observation =
                new List<Tuple<double, double, double, double, double, double, double>>();
            var inputList = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
            Console.WriteLine("Running 5 times.........................");
            for (int i = 0; i < 5; i++)
            {
                double res1 = 0, res2 = 0, res3 = 0, res4 = 0, res5 = 0, res6 = 0, res7 = 0;
                for (int j = 0; j < inputList.Count; j++)
                {
                    var input = inputList[j];
                    ; switch (input)
                    {
                        case 1:
                            int count = Convert.ToInt32(args[0]);
                            bool isSeq = Convert.ToBoolean(args[1]);
                            bool isParallel = Convert.ToBoolean(args[2]);

                            Console.WriteLine("Calling SAVEScheduleActual");
                            res1 = SAVEScheduleActual("C:\\output\\Maintenance.json", count, isSeq, isParallel);

                            break;
                        case 2:
                            Console.WriteLine("Calling GETScheduleActualwithoutjoin");
                            res2 = GETScheduleActualwithoutjoin();
                            break;
                        case 3:
                            Console.WriteLine("Calling GETScheduleActual");
                            res3 = GETScheduleActual();
                            break;
                        case 4:
                            Console.WriteLine("Calling UPDActualMovement");
                            res4 = UPDActualMovement();
                            break;
                        case 5:
                            Console.WriteLine("Calling DELScheduleActual");
                            res5 = DELScheduleActual();
                            break;
                        case 6:
                            int pageSize = Convert.ToInt32(args[1]);

                            Console.WriteLine("Calling GETScheduleActualpagination");
                            res6 = GETScheduleActualpagination(pageSize);
                            break;
                        case 7:
                            int count1 = Convert.ToInt32(args[0]);
                            bool isSeq1 = Convert.ToBoolean(args[1]);
                            bool isParallel1 = Convert.ToBoolean(args[2]);
                            Console.WriteLine("Calling ReadWriteScheduleActual");
                            res7 = ReadWriteScheduleActual("C:\\output\\Maintenance.json", count1, isSeq1, isParallel1);
                            break;

                        default:
                            break;
                    }

                    var r = Tuple.Create(res1, res2, res3, res4, res5, res6, res7);
                    observation.Add(r);
                }
            }

            Console.WriteLine("Best out of 5 run by a single client is :");
            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("1. SaveScheduleActual " +observation.Select(x=>x.Item1).Min());
            Console.WriteLine("2. GETScheduleActualwithoutjoin " + observation.Select(x => x.Item2).Min());
            Console.WriteLine("3. GETScheduleActual " + observation.Select(x => x.Item3).Min());
            Console.WriteLine("4. UPDActualMovement " + observation.Select(x => x.Item4).Min());
            Console.WriteLine("5. DELScheduleActual " + observation.Select(x => x.Item5).Min());
            Console.WriteLine("6. GETScheduleActualpagination " + observation.Select(x => x.Item6).Min());
            Console.WriteLine("7. ReadWriteScheduleActual " + observation.Select(x => x.Item7).Min());





        }

        //private static void SAVEScheduleActualScenario()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());
        //    DateTime LRT;
        //    Console.Write("enter MaintenanceType: ");
        //    LRT = Convert.ToDateTime(Console.ReadLine());

        //    var mapping = db.GetCollection<BsonDocument>("ActualScheduleScenarioLRT");

        //    var docmapping = new BsonDocument
        //    {   {"ScenarioID", ScenarioID},
        //        {"LastRefreshedTime",LRT}
        //    };
        //    mapping.InsertOne(docmapping);
        //}
        private static double SAVEScheduleActual(string path, int count, bool seq, bool parallel)
        {
            try
            {

                IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

                var collection = db.GetCollection<BsonDocument>("ScheduleActual");


                string contents = File.ReadAllText(path);


                var stopWatch = new Stopwatch();
                var doc = BsonDocument.Parse(contents);
                List<BsonDocument> data = new List<BsonDocument>();
                for (int i = 1; i <= count; i++)
                {
                    BsonDocument pf1 = doc.DeepClone().AsBsonDocument;
                    data.Add(pf1);
                }

                stopWatch.Start();

                if (!seq)
                    collection.InsertManyAsync(data).Wait();
                else
                {
                    if (parallel)
                    {
                        data.ParallelForEachAsync(
                            async item =>
                            {
                                try
                                {
                                    await collection.InsertOneAsync(item);

                                }
                                catch (Exception ex)
                                {
                                }
                            }).Wait();
                    }
                    else
                    {
                        for (int i = 1; i <= count; i++)
                        {
                            collection.InsertOne(data[i]);
                        }
                    }

                    //collection.InsertMany//Async(records).Wait();
                    stopWatch.Stop();
                    Console.WriteLine($"Time elapsed in milliseconds to write {stopWatch.Elapsed.TotalMilliseconds}");
                    return stopWatch.Elapsed.TotalMilliseconds;
                    //Console.Read();
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
            }

            return 0;
        }

        private static double ReadWriteScheduleActual(string path, int count, bool seq, bool parallel)
        {
            var sa = SAVEScheduleActual(path, count, seq, parallel);
            var sa1 = GETScheduleActual();
            return sa + sa1;
        }

        //String ScheduleID;
        //Console.Write("enter ScheduleID: ");
        //ScheduleID = Console.ReadLine();

        //DateTime LRT;
        //Console.Write("enter LRT: ");
        //LRT = Convert.ToDateTime(Console.ReadLine());
        //int LocationID;
        //Console.Write("enter LocationID: ");
        //LocationID = Convert.ToInt32(Console.ReadLine());
        //int StopTime;
        //Console.Write("enter StopTime: ");
        //StopTime = Convert.ToInt32(Console.ReadLine());

        private static double GETScheduleActual()
        {
            MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
            IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

            var collection = db.GetCollection<BsonDocument>("ScheduleActual");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var lookup = new BsonDocument("$lookup",
                             new BsonDocument("from", "MaintenanceType")
                                         .Add("localField", "maintenancetypeid")
                                         .Add("foreignField", "maintenancetypeid")
                                         .Add("as", "ScheduleActualCollection"));

            var match = new BsonDocument("$match", new BsonDocument("maintenancetypeid", 1));

            var pipeline = new[] { match, lookup };
            var option = new AggregateOptions() { AllowDiskUse = true };
            var ddTask = collection.AggregateAsync<BsonDocument>(pipeline, option).Result;
            stopWatch.Stop();

            int countOfRead = 0;
            var enumerator = ddTask.ToEnumerable().GetEnumerator();
            while (enumerator.MoveNext())
            {
                countOfRead++;
            }
            Console.WriteLine($"Record count is  {countOfRead}");
            Console.WriteLine($"Time elapsed in milliseconds to read {stopWatch.Elapsed.TotalMilliseconds}");
            return stopWatch.Elapsed.TotalMilliseconds;
        }

        private static double GETScheduleActualwithoutjoin()
        {
            MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
            IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

            var collection = db.GetCollection<BsonDocument>("ScheduleActual");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var filter = Builders<BsonDocument>.Filter.Eq("maintenancetypeid", 1);
            var res = collection.FindAsync(filter).Result;
            stopWatch.Stop();

            int countOfRead = 0;
            while (res.MoveNext())
            {
                countOfRead++;
            }
            Console.WriteLine($"Record count is  {countOfRead}");

            Console.WriteLine($"Time elapsed in milliseconds to read {stopWatch.Elapsed.TotalMilliseconds}");
            return stopWatch.Elapsed.TotalMilliseconds;
        }

        private static double GETScheduleActualpagination(int pageSize)
        {
            MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
            IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

            var collection = db.GetCollection<BsonDocument>("ScheduleActual");

            int page = 1;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            List<BsonDocument> data = new
                List<BsonDocument>();
            var filter = Builders<BsonDocument>.Filter.Eq("maintenancetypeid", 1);
            int dCount = data.Count;
            while (true)
            {
                dCount = data.Count;
                var res = collection.Find(filter).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync().Result;
                data.AddRange(res);
                if (dCount == data.Count || res.Count < pageSize)
                    break;
                page++;
            }
            stopWatch.Stop();
            Console.WriteLine($"Record count is  {dCount}");

            Console.WriteLine($"Time elapsed in milliseconds to read {stopWatch.Elapsed.TotalMilliseconds}");
            return stopWatch.Elapsed.TotalMilliseconds;

        }
        private static double UPDActualMovement()
        {
            MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
            IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

            var collection = db.GetCollection<BsonDocument>("ScheduleActual");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var filter = Builders<BsonDocument>.Filter.Eq("maintenancetypeid", 1); //& (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

            var update = Builders<BsonDocument>.Update.Set("maintenancetypeid", 0);

            var updateTask = collection.UpdateManyAsync(filter, update);
            updateTask.Wait(CancellationToken.None);
            stopWatch.Stop();
            var uResult = updateTask.Result.IsAcknowledged ? updateTask.Result.ModifiedCount : 0;
            Console.WriteLine($"Updated count is  {uResult}");

            Console.WriteLine($"Time elapsed in milliseconds to write {stopWatch.Elapsed.TotalMilliseconds}");
            return stopWatch.Elapsed.TotalMilliseconds;
        }
        private static double DELScheduleActual()
        {
            MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
            IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

            var collection = db.GetCollection<BsonDocument>("ScheduleActual");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            //var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));
            var filter = Builders<BsonDocument>.Filter.Eq("maintenancetypeid", 0);

            var delTask = collection.DeleteManyAsync(filter);
            delTask.Wait();
            stopWatch.Stop();
            delTask.Wait(CancellationToken.None);
            stopWatch.Stop();
            var dResult = delTask.Result.IsAcknowledged ? delTask.Result.DeletedCount : 0;
            Console.WriteLine($"Updated count is  {dResult}");
            Console.WriteLine($"Time elapsed in milliseconds to delete {stopWatch.Elapsed.TotalMilliseconds}");
            return stopWatch.Elapsed.TotalMilliseconds;
        }

        //private static void DELScheduleStatus()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    String ScheduleID;
        //    Console.Write("enter ScheduleID: ");
        //    ScheduleID = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleStatus");

        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

        //    var update = Builders<BsonDocument>.Update.Set("State", "Deleted");

        //    collection.UpdateMany(filter, update);
        //    stopWatch.Stop();
        //    Console.WriteLine($"Time elapsed in seconds to write {stopWatch.Elapsed.TotalMilliseconds / 1000}");
        //}

        //private static void DELScheduleDetail()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    String ScheduleID;
        //    Console.Write("enter ScheduleID: ");
        //    ScheduleID = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleDetail");

        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

        //    var update = Builders<BsonDocument>.Update.Set("State", "Deleted");

        //    collection.UpdateMany(filter, update);

        //}
        //private static void DELScheduleViapoint()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    String ScheduleID;
        //    Console.Write("enter ScheduleID: ");
        //    ScheduleID = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleViapoint");

        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

        //    var update = Builders<BsonDocument>.Update.Set("State", "Deleted");

        //    collection.UpdateMany(filter, update);

        //}

        //private static void DELScheduleLink()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    String ScheduleID;
        //    Console.Write("enter ScheduleID: ");
        //    ScheduleID = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleLink");

        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

        //    var update = Builders<BsonDocument>.Update.Set("State", "Deleted");

        //    collection.UpdateMany(filter, update);

        //}
        //private static void DELScheduleAudit()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    String ScheduleID;
        //    Console.Write("enter ScheduleID: ");
        //    ScheduleID = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleAudit");

        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

        //    var update = Builders<BsonDocument>.Update.Set("State", "Deleted");

        //    collection.UpdateMany(filter, update);

        //}

        //private static void DELScheduleActivityMaster()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    String ScheduleID;
        //    Console.Write("enter ScheduleID: ");
        //    ScheduleID = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleActivityMaster");

        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & (Builders<BsonDocument>.Filter.Eq("State", "Inactive") | Builders<BsonDocument>.Filter.Eq("State", "Active"));

        //    var update = Builders<BsonDocument>.Update.Set("State", "Deleted");

        //    collection.UpdateMany(filter, update);

        //}

        //private static void GETScheduleStatus()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleStatus")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleStatusCollection"));
        //    var unwind1 = new BsonDocument("$unwind", "$ScheduleStatusCollection");
        //    var unwind2 = new BsonDocument("$unwind", "$ScheduleStatusCollection.State");
        //    var unwindmatch = new BsonDocument("$match", new BsonDocument("ScheduleStatusCollection.State", "Active"));
        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup, unwind1, unwind2, unwindmatch };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });
        //    stopWatch.Stop();
        //    Console.WriteLine($"Time elapsed in seconds to write {stopWatch.Elapsed.TotalMilliseconds / 1000}");


        //}
        //private static void GETScheduleDetail()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleDetail")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleDetailCollection"));
        //    var unwind1 = new BsonDocument("$unwind", "$ScheduleDetailCollection");
        //    var unwind2 = new BsonDocument("$unwind", "$ScheduleDetailCollection.State");
        //    var unwindmatch = new BsonDocument("$match", new BsonDocument("ScheduleDetailCollection.State", "Active"));
        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup, unwind1, unwind2, unwindmatch };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });

        //}

        //private static void GETScheduleViapoint()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleViapoint")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleViapointCollection"));
        //    var unwind1 = new BsonDocument("$unwind", "$ScheduleViapointCollection");
        //    var unwind2 = new BsonDocument("$unwind", "$ScheduleViapointCollection.State");
        //    var unwindmatch = new BsonDocument("$match", new BsonDocument("ScheduleViapointCollection.State", "Active"));
        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup, unwind1, unwind2, unwindmatch };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });

        //}

        //private static void GETScheduleLink()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleLink")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleLinkCollection"));
        //    var unwind1 = new BsonDocument("$unwind", "$ScheduleLinkCollection");
        //    var unwind2 = new BsonDocument("$unwind", "$ScheduleLinkCollection.State");
        //    var unwindmatch = new BsonDocument("$match", new BsonDocument("ScheduleLinkCollection.State", "Active"));
        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup, unwind1, unwind2, unwindmatch };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });

        //}
        //private static void GETScheduleActivityMaster()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleActivityMaster")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleActivityMasterCollection"));
        //    var unwind1 = new BsonDocument("$unwind", "$ScheduleActivityMasterCollection");
        //    var unwind2 = new BsonDocument("$unwind", "$ScheduleActivityMasterCollection.State");
        //    var unwindmatch = new BsonDocument("$match", new BsonDocument("ScheduleActivityMasterCollection.State", "Active"));
        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup, unwind1, unwind2, unwindmatch };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });

        //}

        //private static void GETScheduleAudit()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleAudit")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleAuditCollection"));
        //    var unwind1 = new BsonDocument("$unwind", "$ScheduleAuditCollection");
        //    var unwind2 = new BsonDocument("$unwind", "$ScheduleAuditCollection.State");
        //    var unwindmatch = new BsonDocument("$match", new BsonDocument("ScheduleAuditCollection.State", "Active"));
        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup, unwind1, unwind2, unwindmatch };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });

        //}

        //private static void GETScheduleNRTStop()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleNRTStop")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleNRTStopCollection"));

        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });

        //    stopWatch.Stop();
        //    Console.WriteLine($"Time elapsed in seconds to write {stopWatch.Elapsed.TotalMilliseconds / 1000}");

        //}
        //private static void GETScheduleNRTJourney()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "ScheduleNRTJourey")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "ScheduleNRTJourneyCollection"));

        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });



        //}
        //private static void GETSchedulePAXFreightStop()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "SchedulePAXFreightStop")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "SchedulePAXFreightStopCollection"));

        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });



        //}
        //private static void GETSchedulePAXFreightJourney()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int ScenarioID;
        //    Console.Write("enter ScenarioID: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var lookup = new BsonDocument("$lookup",
        //                     new BsonDocument("from", "SchedulePAXFreightJourney")
        //                                 .Add("localField", "Guid")
        //                                 .Add("foreignField", "Guid")
        //                                 .Add("as", "SchedulePAXFreightJourneyCollection"));

        //    var match = new BsonDocument("$match", new BsonDocument("ScenarioID", ScenarioID));

        //    var pipeline = new[] { match, lookup };
        //    var results = collection.Aggregate<BsonDocument>(pipeline).ToList();

        //    results.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc.ToJson());
        //    });



        //}
        //private static void GETScenario()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");

        //    int x;
        //    Console.Write("enter scenarioid: ");
        //    x = Convert.ToInt32(Console.ReadLine());
        //    Console.Write(x);
        //    var collection = db.GetCollection<BsonDocument>("Scenario");
        //    var builder = Builders<BsonDocument>.Filter;
        //    var filter = builder.Eq("ScenarioID", x);

        //    var docs = collection.Find(filter).ToList();

        //    docs.ForEach(doc =>
        //    {
        //        Console.WriteLine(doc);
        //    });
        //}

        //private static void SAVEScenario()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    int ScenarioID;
        //    Console.Write("enter scenarioid: ");
        //    ScenarioID = Convert.ToInt32(Console.ReadLine());
        //    String Guid;
        //    Console.Write("enter guid: ");
        //    Guid = Console.ReadLine();
        //    var collection = db.GetCollection<BsonDocument>("Scenario");

        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    for (int i = 1; i <= 1; i++)
        //    {
        //        try
        //        {
        //            var doc = new BsonDocument
        //    {
        //        {"ScenarioID",ScenarioID},
        //        {"Guid", Guid}
        //    };
        //            collection.InsertOne(doc);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString()); ;
        //        }
        //    }
        //    stopWatch.Stop();
        //    Console.WriteLine($"Time elapsed in seconds to write {stopWatch.Elapsed.TotalMilliseconds / 1000}");
        //}
        //private static void CRUScheduleStatus()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    BsonDocument Data;
        //    Console.Write("enter data: ");
        //    Data = BsonDocument.Parse(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("ScheduleStatus");
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    //var filter1 = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & Builders<BsonDocument>.Filter.Eq("State", "Active");
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID);

        //    var update = Builders<BsonDocument>.Update.Set("State", "Inactive");
        //    //var upsert = new UpdateOptions { IsUpsert = true };
        //    collection.UpdateMany(filter, update);

        //    for (int i = 1; i <= 1000; i++)
        //    {
        //        try
        //        {
        //            var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"State", "Active"},
        //       Data
        //    };
        //            collection.InsertOne(doc);
        //        }

        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString()); ;
        //        }
        //    }
        //    stopWatch.Stop();
        //    Console.WriteLine($"Time elapsed in seconds to write {stopWatch.Elapsed.TotalMilliseconds / 1000}");
        //}
        //private static void CRUScheduleDetail()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    BsonDocument Data;
        //    Console.Write("enter data: ");
        //    Data = BsonDocument.Parse(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("ScheduleDetail");
        //    //var filter1 = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & Builders<BsonDocument>.Filter.Eq("State", "Active");
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID);

        //    var update = Builders<BsonDocument>.Update.Set("State", "Inactive");
        //    //var upsert = new UpdateOptions { IsUpsert = true };
        //    collection.UpdateMany(filter, update);
        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"State", "Active"},
        //       Data
        //    };
        //    collection.InsertOne(doc);
        //}
        //private static void CRUScheduleViapoint()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    BsonDocument Data;
        //    Console.Write("enter data: ");
        //    Data = BsonDocument.Parse(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("ScheduleViapoint");
        //    //var filter1 = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & Builders<BsonDocument>.Filter.Eq("State", "Active");
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID);

        //    var update = Builders<BsonDocument>.Update.Set("State", "Inactive");
        //    //var upsert = new UpdateOptions { IsUpsert = true };
        //    collection.UpdateMany(filter, update);
        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"State", "Active"},
        //       Data
        //    };
        //    collection.InsertOne(doc);
        //}

        //private static void CRUScheduleLink()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    BsonDocument Data;
        //    Console.Write("enter data: ");
        //    Data = BsonDocument.Parse(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("ScheduleLink");
        //    //var filter1 = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & Builders<BsonDocument>.Filter.Eq("State", "Active");
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID);

        //    var update = Builders<BsonDocument>.Update.Set("State", "Inactive");
        //    //var upsert = new UpdateOptions { IsUpsert = true };
        //    collection.UpdateMany(filter, update);
        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"State", "Active"},
        //       Data
        //    };
        //    collection.InsertOne(doc);
        //}
        //private static void SAVEScheduleNRTStop()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    String VarbinaryData;
        //    Console.Write("enter varbinarydata: ");
        //    VarbinaryData = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleNRTStop");
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    for (int i = 1; i <= 1000; i++)
        //    {
        //        try
        //        {
        //            var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"Varbinary",VarbinaryData}
        //    };
        //            collection.InsertOne(doc);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString()); ;
        //        }
        //    }
        //    stopWatch.Stop();
        //    Console.WriteLine($"Time elapsed in seconds to write {stopWatch.Elapsed.TotalMilliseconds / 1000}");

        //}
        //private static void SAVEScheduleNRTJourney()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    String VarbinaryData;
        //    Console.Write("enter varbinarydata: ");
        //    VarbinaryData = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("ScheduleNRTJourney");

        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"Varbinary",VarbinaryData}
        //    };
        //    collection.InsertOne(doc);
        //}
        //private static void SAVESchedulePAXFreightStop()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    String VarbinaryData;
        //    Console.Write("enter varbinarydata: ");
        //    VarbinaryData = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("SchedulePAXFreightStop");

        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"Varbinary",VarbinaryData}
        //    };
        //    collection.InsertOne(doc);
        //}
        //private static void SAVESchedulePAXFreightJourney()
        //{
        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    String VarbinaryData;
        //    Console.Write("enter varbinarydata: ");
        //    VarbinaryData = Console.ReadLine();

        //    var collection = db.GetCollection<BsonDocument>("SchedulePAXFreightJourney");

        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"Varbinary",VarbinaryData}
        //    };
        //    collection.InsertOne(doc);
        //}
        //private static void CRUScheduleAudit()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    BsonDocument Data;
        //    Console.Write("enter data: ");
        //    Data = BsonDocument.Parse(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("ScheduleAudit");
        //    //var filter1 = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & Builders<BsonDocument>.Filter.Eq("State", "Active");
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID);

        //    var update = Builders<BsonDocument>.Update.Set("State", "Inactive");
        //    //var upsert = new UpdateOptions { IsUpsert = true };
        //    collection.UpdateMany(filter, update);
        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"State", "Active"},
        //       Data
        //    };
        //    collection.InsertOne(doc);
        //}

        //private static void CRUScheduleActivityMaster()
        //{

        //    MongoClient client = new MongoClient("mongodb://RailmaxWeb:Op5K4HM1A6or@dcxrmxpoc04:27017,dcxrmxpoc05:27017,dcxrmxpoc06:27017/admin?replicaSet=MainReplicaSet&ssl=false");
        //    IMongoDatabase db = client.GetDatabase("Schedule_Dummy");
        //    String ScheduleID;
        //    Console.Write("enter guid: ");
        //    ScheduleID = Console.ReadLine();
        //    BsonDocument Data;
        //    Console.Write("enter data: ");
        //    Data = BsonDocument.Parse(Console.ReadLine());

        //    var collection = db.GetCollection<BsonDocument>("ScheduleActivityMaster");
        //    //var filter1 = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID) & Builders<BsonDocument>.Filter.Eq("State", "Active");
        //    var filter = Builders<BsonDocument>.Filter.Eq("Guid", ScheduleID);

        //    var update = Builders<BsonDocument>.Update.Set("State", "Inactive");
        //    //var upsert = new UpdateOptions { IsUpsert = true };
        //    collection.UpdateMany(filter, update);
        //    var doc = new BsonDocument
        //    {   {"Guid", ScheduleID},
        //        {"State", "Active"},
        //       Data
        //    };
        //    collection.InsertOne(doc);
        //}
    }
}
