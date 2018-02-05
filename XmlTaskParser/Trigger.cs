using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTaskParser
{
    class Trigger
    {

        public static string OUPUT_SEPARATOR = ",";
        public const String PLANIF_JOUR = "jour";
        public const String PLANIF_SEMAINE = "semaine";
        public const String PLANIF_MOIS = "mois";
        public const String PLANIF_MOIS_JOUR = "mois/jour";
        public const String PLANIF_REPET = "repetition";

        public int daysInterval { get; set; }
        public int weeksInterval { get; set; }
        public string repetInterval { get; set; }
        public System.DateTime startHour { get; set; }
        public string ExecutionTimeLimit { get; set; }
        public List<string> daysOfWeek = new List<string>();
        public string planif { get; set; }

        public Trigger()
        {
            ExecutionTimeLimit = "";
        }

        public override String ToString()
        {

            string ret = "{" + planif + OUPUT_SEPARATOR;
            switch (planif)
            {
                case PLANIF_JOUR:
                    if (daysInterval == 1)
                    {
                        ret += "quotidien";
                    }
                    else
                    {
                        ret += "tous les " + daysInterval + "jours";
                    }

                    break;
                case PLANIF_SEMAINE:
                    if (weeksInterval == 1)
                    {
                        ret += "hebdomadaire";
                    }
                    else
                    {
                        ret += "toutes les " + weeksInterval + " semaines";
                    }

                    ret += "{";
                    /*
                    foreach (string day in daysOfWeek)
                    {
                        ret += day + OUPUT_SEPARATOR;
                    }
                    */
                    ret += string.Join(OUPUT_SEPARATOR, daysOfWeek.ToArray());
                    ret += "}";
                    break;
                case PLANIF_REPET:
                    ret += "TimeTrigger";
                    break;

            }
            ret += OUPUT_SEPARATOR + startHour.ToString("HH:mm:ss");

            ret += OUPUT_SEPARATOR + ExecutionTimeLimit;

            ret += "}";

            return ret;
        }

        public string ToString2(string outterSep)
        {

            string ret = planif + OUPUT_SEPARATOR;
            switch (planif)
            {
                case PLANIF_JOUR:
                    if (daysInterval == 1)
                    {
                        ret += "quotidien";
                    }
                    else
                    {
                        ret += "tous les " + daysInterval + "jours";
                    }

                    break;
                case PLANIF_SEMAINE:
                    if (weeksInterval == 1)
                    {
                        ret += "hebdomadaire";
                    }
                    else
                    {
                        ret += "toutes les " + weeksInterval + " semaines";
                    }

                    ret += "{";
                    ret += string.Join(OUPUT_SEPARATOR, daysOfWeek.ToArray());
                    ret += "}";
                    break;
                case PLANIF_REPET:
                    ret += repetInterval;
                    break;

            }
            ret += outterSep + startHour.ToString("HH:mm:ss");

            ret += outterSep + ExecutionTimeLimit;

            

            return ret;
        }
    }
}
