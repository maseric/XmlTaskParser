using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTaskParser
{
    class Logger
    {
        public const string DEBUG   = "DEBUG";
        public const string INFO    = "INFO ";
        public const string WARN    = "WARN ";
        public const string ERR     = "ERR  ";
        public int LOGPADDLEVEL = 0;


        /// <summary>
        /// Fonction générique de log avec indentationet niveau
        /// </summary>
        /// <param name="message"></param>
        public void log(string message, string level)
        {
            String spaces = "";
            for (int i = 0; i < LOGPADDLEVEL; i++)
            {
                spaces += "  ";
            }

            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss.ff") + " - " + level +" - " + spaces + message);
        }

        /// <summary>
        /// Fonction générique de log avec indentation
        /// </summary>
        /// <param name="message"></param>
        public void log(string message)
        {
            log(message, INFO);
        }

        public void info(string message)
        {
            log(message, INFO);
        }

        public void warn(string message)
        {
            log(message, WARN);
        }

        public  void err(string message)
        {
            log(message, ERR);
        }

        public  void debug(string message)
        {
            log(message, DEBUG);
        }
    }
}
