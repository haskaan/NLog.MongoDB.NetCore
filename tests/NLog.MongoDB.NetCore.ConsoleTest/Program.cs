using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NLog.MongoDB.NetCore.ConsoleTest
{
    public class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            int param1 = 1071;
            int param2 = 1299;
            string param3 = "param3";
            

            _logger.Trace("Trace: (param1: {0}, param2: {1})", param1, param2);
            _logger.Debug("Debug: (param1: {0}, param2: {1})", param1, param3);
            _logger.Info("Info: (param1: {0}, param2: {1})", param1, param3);
            _logger.Warn("Warn: (param1: {0}, param2: {1})", param1, param2);
            _logger.Error("Error: (param1: {0}, param2: {1})", param1, param2);
            _logger.Fatal("Fatal: (param1: {0}, param2: {1})", param1, param3);

            _logger.Log(LogLevel.Info, "LogLevel.Info: (param1: {0}, param2: {1})", param1, param2);


            string path = "dummyFile.txt";
            try
            {
                string text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reading file. Path: '{0}'.", path);
            }

            Console.ReadLine();
        }
    }
}
