using System;
using System.IO;

namespace DataAccess
{
    public class Logger
    {
        private static Logger _logger;
        private StreamWriter _file;

        // private constructor to not be able to create an object from outside (Singleton)
        private Logger() {}

        // Destructor to log destruction
        ~Logger()
        {
            _file = new StreamWriter("log.txt", true);      // true means appening of the text

            _file.WriteLine($"{DateTime.Now} | " + "Program Closed");    // uses the current time as a prefix + the given string
            _file.WriteLine($"------------------------------------------------------");
            _file.WriteLine($"");

            _file.Close();
        }

        // public static GetInstance method which checks if an object already exists (Singleton)
        public static Logger GetInstance()
        {
            return _logger ?? ( _logger = new Logger() );       // returns the logger if it's not null or creates a new object
        }
        
        // writes a string into the log file
        public void Log(string message)
        {
            _file = new StreamWriter("log.txt", true);      // true means appending of the text

            _file.WriteLine($"{DateTime.Now} | {message}");    // uses the current time as a prefix + the given string

            _file.Close();
        }
    }
}
