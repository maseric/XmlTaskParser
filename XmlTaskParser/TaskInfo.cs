using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlTaskParser
{
    class TaskInfo
    {

        public const string OUPUT_SEPARATOR = ";";
        String Serveur;
        String Nom;
        public string Version { get; set; }
        public String Auteur { get; set; }
        public String Description { get; set; }
        public int nbTriggers { get; set; }
        public int nbActions { get; set; }
        public List<Trigger> triggers = new List<Trigger>();
        public List<Action> actions = new List<Action>();
        public const string HEADER = "serveur;tache;nb triggers;nb actions;fréquence;start boundary;limite;Actions";





        public TaskInfo(String Serveur, String Nom)
        {
            this.Serveur = Serveur;
            this.Nom = Nom;
        }

        public TaskInfo()
            : this("unknown", "unknown")
        {
        }


        public override String ToString()
        {

            string ret = (Serveur == "Serveur" ? "" : Serveur + OUPUT_SEPARATOR) + Nom + OUPUT_SEPARATOR /*+ (Description==null?"":Description)*/ + nbTriggers + OUPUT_SEPARATOR + nbActions + ";[";

            List<string> sTrig = new List<string>();
            foreach (Trigger trig in triggers)
            {
                sTrig.Add(trig.ToString());
            }
            ret += string.Join(Trigger.OUPUT_SEPARATOR, sTrig.ToArray());

            ret += "]";
            return ret;
        }

        public List<string> ToString2()
        {

            List<string> ret = new List<string>();
            //si pas de trigger gérer un output qd même
            if (triggers.Count > 0)
            {
                // cas où il y a un/des trigger/s
                foreach (Trigger trig in triggers)
                {

                    List<string> sAct = new List<string>();
                    foreach (Action action in actions)
                    {                        
                        sAct.Add(action.ToString());
                    }

                    ret.Add((Serveur == "Serveur" ? "" : Serveur + OUPUT_SEPARATOR) + Nom + OUPUT_SEPARATOR + nbTriggers + OUPUT_SEPARATOR + nbActions + OUPUT_SEPARATOR + trig.ToString2(OUPUT_SEPARATOR) + OUPUT_SEPARATOR + "[" + string.Join(Action.OUPUT_SEPARATOR, sAct.ToArray()) + "]");
                }
            }
            else
            {
                // cas où il n'y a pas de triggers
                List<string> sAct = new List<string>();
                foreach (Action action in actions)
                {                   
                    sAct.Add(action.ToString());
                }

                ret.Add((Serveur == "Serveur" ? "" : Serveur + OUPUT_SEPARATOR) + Nom + OUPUT_SEPARATOR + nbTriggers + OUPUT_SEPARATOR + nbActions + OUPUT_SEPARATOR + OUPUT_SEPARATOR + OUPUT_SEPARATOR + OUPUT_SEPARATOR + "[" + string.Join(Action.OUPUT_SEPARATOR, sAct.ToArray()) + "]");
            }

            return ret;
        }

    }
}
