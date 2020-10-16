using Newtonsoft.Json;
using System;
using System.IO;

namespace dNetCoreRedis
{
    class LogWriter
    {
        private string _logPath;

        public LogWriter(string directory ,string logFileName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory ?? throw new ArgumentNullException(nameof(directory)));
            
            void CreateDirectoriesParent(DirectoryInfo info)
            {
                if (!info.Exists)
                {
                    CreateDirectoriesParent(info.Parent);
                    info.Create();
                }

               _logPath = info.FullName + logFileName ?? throw new ArgumentNullException(nameof(logFileName));
                Console.WriteLine(_logPath);
            }

            CreateDirectoriesParent(directoryInfo);

        }

        public void Write(string log)
        {
            var path = _logPath + $"_{DateTime.Now.Hour}h_{DateTime.Now.Minute}m_{DateTime.Now.Second}s"
                + ".txt";

            using (StreamWriter writer = new StreamWriter(path))
            {
                Console.WriteLine("Write log in " + path);

                writer.WriteLineAsync(JsonConvert.SerializeObject(new
                {
                    @DateTime = DateTime.Now,
                    log = log
                }));
            }
        }
    }
}
