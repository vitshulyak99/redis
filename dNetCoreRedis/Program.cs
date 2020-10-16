using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dNetCoreRedis
{
    class Program
    {
        static void Main(string[] args)
        {
            Configuration _config;
            ConnectionMultiplexer connection = null;
            List<RedisKey> oldKeys = new List<RedisKey>();
            LogWriter logWriter = new LogWriter("logs\\", $"log_{DateTime.Now.ToShortDateString().Replace("/","-")}");
            bool loop = true;
            int totalCurrentKeys = 0;

            logWriter.Write("test");

            using (StreamReader reader = new StreamReader("cfg.json"))
            {
                var confStr = reader.ReadToEnd();

                _config = JsonConvert.DeserializeObject<Configuration>(confStr);
            }

            connection = ConnectionMultiplexer.Connect(_config.Connection);

            var server = connection.GetServer(_config.Connection.Split(",")[0]);
            var keysConfigs = new List<KeyConfig>(_config.KeyConfigs);

            while (loop)
            {
                List<RedisKey> lastKeys = new List<RedisKey>();

                try
                {
                    keysConfigs.ForEach(keyConfig =>
                    {
                        var keys = server.Keys(keyConfig.DB, keyConfig.Pattern);
                        lastKeys.AddRange(keys);
                    });

                    var result = oldKeys.Where(ok => lastKeys.Any(lk => lk == ok)).ToList();

                    oldKeys.Clear();
                    oldKeys.AddRange(lastKeys);
                    totalCurrentKeys = oldKeys.Count;

                    Console.WriteLine($"TotalKeys: {totalCurrentKeys} Update time: {DateTime.Now.ToString()}");

                    if (result.Count > 0)
                    {
                        Console.Clear();
                        var jsonResult = JsonConvert.SerializeObject(result);
                        logWriter.Write(jsonResult);
                    }



                    result.ForEach(r => Console.WriteLine(r));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Task.Delay(TimeSpan.FromMinutes(_config.DelayMinutes)).Wait();
            }


            Console.ReadLine();
        }
    }

    internal struct KeyConfig
    {

        public int DB;

        public string Pattern;

        public KeyConfig(int dB, string pattern)
        {
            DB = dB;
            Pattern = pattern;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyConfig other &&
                   DB == other.DB &&
                   Pattern == other.Pattern;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DB, Pattern);
        }

        public void Deconstruct(out int dB, out string pattern)
        {
            dB = DB;
            pattern = Pattern;
        }

        public static implicit operator (int DB, string Pattern)(KeyConfig value)
        {
            return (value.DB, value.Pattern);
        }

        public static implicit operator KeyConfig((int DB, string Pattern) value)
        {
            return new KeyConfig(value.DB, value.Pattern);
        }
    }

    internal struct Configuration
    {

        public string Connection;

        public int DelayMinutes;
 
        public List<KeyConfig> KeyConfigs;

        public Configuration(string connection, int delayMinutes, List<KeyConfig> item3)
        {
            Connection = connection;
            DelayMinutes = delayMinutes;
            KeyConfigs = item3;
        }

        public override bool Equals(object obj)
        {
            return obj is Configuration other &&
                   Connection == other.Connection &&
                   DelayMinutes == other.DelayMinutes &&
                   EqualityComparer<List<KeyConfig>>.Default.Equals(KeyConfigs, other.KeyConfigs);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Connection, DelayMinutes, KeyConfigs);
        }

        public void Deconstruct(out string connection, out int delayMinutes, out List<KeyConfig> item3)
        {
            connection = Connection;
            delayMinutes = DelayMinutes;
            item3 = KeyConfigs;
        }

        public static implicit operator (string Connection, int DelayMinutes, List<KeyConfig>)(Configuration value)
        {
            return (value.Connection, value.DelayMinutes, value.KeyConfigs);
        }

        public static implicit operator Configuration((string Connection, int DelayMinutes, List<KeyConfig> keyConfigs) value)
        {
            return new Configuration(value.Connection, value.DelayMinutes, value.keyConfigs);
        }
    }
}
