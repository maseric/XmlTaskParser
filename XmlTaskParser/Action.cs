using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTaskParser
{
    class Action
    {

        public static string OUPUT_SEPARATOR = ",";
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }


        public override String ToString()
        {

            string ret = "{";

            if (!object.ReferenceEquals(Command, null))
            {
                ret += "cmd=" + Command;
            }

            if(!object.ReferenceEquals(Arguments, null))
            {
                ret += " " + Arguments;
            }
            if (!object.ReferenceEquals(WorkingDirectory, null))
            {
                ret += OUPUT_SEPARATOR + "rep=" + WorkingDirectory;
            }

            ret += "}";
            return ret;
        }

    }
}
