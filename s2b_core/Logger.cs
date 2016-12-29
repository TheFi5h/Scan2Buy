using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scan2Buy
{
    class Logger
    {
        private static Logger _logger;
        private readonly StreamWriter _streamWriter;
        private readonly Random _rnd = new Random();

        public static Logger GetInstance()
        {
            return _logger ?? ( _logger = new Logger() );
        }

        private Logger()
        {
            _streamWriter = new StreamWriter("log" + +_rnd.Next(1, 100) + ".txt" );
        }

        public void Log(string message)
        {
            _streamWriter.WriteLineAsync($"{DateTime.Now} | " + message);
        }
    }
}
