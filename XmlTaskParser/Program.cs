﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace XmlTaskParser
{
    /// <summary>
    ///  programme pricipal
    /// </summary>
    class Program
    {

       // private static int LOGPADDLEVEL = 0;
        private static Logger logger=new Logger();
        private static bool isOutputMode = false;
        private static string outputFile;
        // option -f
        private const int MODE_FILE = 1;
        // option -p
        private const int MODE_PATH = 2;
        // option -s
        private const int MODE_SERVER = 3;
        private static int mode;
        private static string argument;

       

        static void Main(string[] args)
        {
            try
            {                
                if (args.Count() != 2 && args.Count() != 4)
                {
                    usage();
                }
                else
                {
                    parseOpt(args[0].ToLowerInvariant(), args[1]);

                    if (args.Count() == 4)
                    {                        
                        parseOpt(args[2].ToLowerInvariant(), args[3]);
                    }

                    action();
                    

                }

            }
            catch (Exception ex)
            {
                logger.err(ex.Message);
            }

        }

        ///<summary>
        /// Méthode permettant de tester si l'élément à affecter est nul
        ///</summary>
        ///
        private static string setSafeStr(XElement el)
        {
            string ret = "not found";
            if (object.ReferenceEquals(el, null))
            {
               // logger.debug("not found");
            }
            else
            {
                //  log("setting " + el.Name.LocalName + " to " + el.Value);
                ret = el.Value;
            }
            return ret;
        }
        /*

        /// <summary>
        /// Fonction générique de log avec indentation
        /// </summary>
        /// <param name="message"></param>
        private static void log(string message)
        {
            String spaces = "";
            for (int i = 0; i < LOGPADDLEVEL; i++)
            {
                spaces += "  ";
            }

            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss.ff") + " - " + spaces + message);
        }
        /// <summary>
        /// Fonction générique de log avec indentation
        /// </summary>
        /// <param name="message"></param>
        private static void log(string message,string level)
        {
            String spaces = "";
            for (int i = 0; i < LOGPADDLEVEL; i++)
            {
                spaces += "  ";
            }

            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss.ff") + " - " + spaces + message);
        }

        /// <summary>
        /// Fonction générique de log d'erreur
        /// </summary>
        /// <param name="message"></param>
        private static void err(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Erreur : " + message);
        }
        */
        private static void usage()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Usage : XmlTaskParser [OPTIONS]");
            Console.WriteLine("Analyse les fichiers XML issus du plannificateur de taches Windows > exporter");
            Console.WriteLine("  OPTION :");
            Console.WriteLine("    -p path, explorer recursivement path pour y trouver des fichiers XML a parser");
            Console.WriteLine("    -f file, file : fichier XML a parser");
            Console.WriteLine("    -s path, mode server");
            Console.WriteLine("             path : path ou se trouvent les servers a analyser");
            Console.WriteLine("             exemple : XmlTaskParser -s .");
            Console.WriteLine("    -o outfile, genere un fichier csv dans outfile");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void parseFile(string file, string serveur)
        {

            if (!File.Exists(file))
            {
                logger.err("Fichier non trouve : " + file);
            }
            else
            {
                logger.LOGPADDLEVEL++;

                //  Console.WriteLine(" Lecture du fichier " + args[0]);
                logger.debug("Lecture du fichier " + file);



                XElement task = XElement.Load(file);

                if (task.Name.LocalName == "Task") // le fichier XML est bien une balise Task
                {
                    logger.LOGPADDLEVEL++;




                    TaskInfo taskInfo = new TaskInfo(serveur, Path.GetFileNameWithoutExtension(file));

                    taskInfo.Version = task.Attribute("version").Value;


                    // on récupère le xmlns pour rechercher dans le fichier XML (merci Linq...)
                    XNamespace ns = task.GetDefaultNamespace();

                    // récupère les éléments fils de la balise task
                    IEnumerable<XElement> tags = task.Elements();

                    foreach (var tag in tags)
                    {
                        //----------------------------------------------------------------------------------------
                        // RegistrationInfo
                        if (tag.Name.LocalName == "RegistrationInfo")
                        {
                            logger.LOGPADDLEVEL++;
                            //"Author"                                    
                            taskInfo.Auteur = setSafeStr(tag.Element(ns + "Author"));

                            //"Description"                                    
                            taskInfo.Description = setSafeStr(tag.Element(ns + "Description"));
                            logger.LOGPADDLEVEL--;
                        }

                        //----------------------------------------------------------------------------------------
                        // Triggers
                        if (tag.Name.LocalName == "Triggers")
                        {
                            logger.LOGPADDLEVEL++;

                            // CalendarTrigger + flitre Enabled = true (Planification calendaire active)
                            // UNION
                            // TimeTrigger + flitre Enabled = true (Planification par réptition active)
                            var triggers = (from trig in tag.Elements(ns + "CalendarTrigger")                                           
                                           where trig.Element(ns + "Enabled").Value == "true"
                                           select trig).Union(
                                           from trig in tag.Elements(ns + "TimeTrigger")
                                           where trig.Element(ns + "Enabled").Value == "true"
                                           select trig);

                            foreach (XElement xele in triggers)
                            {
                                

                                Trigger trig = new Trigger();

                                IEnumerable<XElement> calTags = xele.Elements();
                                foreach (var calTag in calTags)
                                {

                                    // date de début de planification du trigger
                                    if (calTag.Name.LocalName == "StartBoundary")
                                    {
                                        logger.LOGPADDLEVEL++;
                                        trig.startHour = DateTime.ParseExact(calTag.Value, "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                        // log("start boundary : " + trig.startHour.ToString("HH:mm:ss"));

                                        logger.LOGPADDLEVEL--;
                                    }

                                    if (calTag.Name.LocalName == "ExecutionTimeLimit")
                                    {
                                        logger.LOGPADDLEVEL++;                                      
                                        TimeSpan ts=XmlConvert.ToTimeSpan(calTag.Value);
                                        trig.ExecutionTimeLimit = ((ts.Days * 24 + ts.Hours).ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2"));
                                        logger.LOGPADDLEVEL--;
                                    }

                                    if (calTag.Name.LocalName == "ScheduleByDay")
                                    {
                                        logger.LOGPADDLEVEL++;
                                        //log("ScheduleByDay");
                                        trig.planif = Trigger.PLANIF_JOUR;
                                        trig.daysInterval = int.Parse(calTag.Element(ns + "DaysInterval").Value);
                                        //log("DaysInterval : " + calTag.Element(ns + "DaysInterval").Value);
                                        logger.LOGPADDLEVEL--;
                                    }

                                    if (calTag.Name.LocalName == "ScheduleByWeek")
                                    {
                                        logger.LOGPADDLEVEL++;                                        
                                        trig.planif = Trigger.PLANIF_SEMAINE;                                        
                                        IEnumerable<XElement> days = calTag.Elements(ns + "DaysOfWeek").Elements();
                                        foreach (var day in days)
                                        {
                                            logger.LOGPADDLEVEL++;

                                            string parsedDay = "gluglu";
                                            switch (day.Name.LocalName)
                                            {
                                                case "Monday": parsedDay = "Lun";
                                                    break;
                                                case "Tuesday": parsedDay = "Mar";
                                                    break;
                                                case "Wednesday": parsedDay = "Mer";
                                                    break;
                                                case "Thursday": parsedDay = "Jeu";
                                                    break;
                                                case "Friday": parsedDay = "Ven"; 
                                                    break;
                                                case "Saturday": parsedDay = "Sam";
                                                    break;
                                                case "Sunday": parsedDay = "Dim";
                                                    break;
                                                default: parsedDay = "merdi";
                                                    break;
                                            }

                                            trig.daysOfWeek.Add(parsedDay);
                                            logger.LOGPADDLEVEL--;
                                        }
                                        //TODO : parser WeeksInterval
                                        trig.weeksInterval = int.Parse(calTag.Element(ns + "WeeksInterval").Value);
                                        //log("WeeksInterval : " + calTag.Element(ns + "WeeksInterval").Value);

                                        logger.LOGPADDLEVEL--;
                                    }
                                    if (calTag.Name.LocalName == "ScheduleByMonth")
                                    {
                                        //TODO : parser ScheduleByMonth
                                        trig.planif = Trigger.PLANIF_MOIS;
                                    }

                                    if (calTag.Name.LocalName == "ScheduleByMonthDayOfWeek")
                                    {
                                        // TODO : parser ScheduleByMonthDayOfWeek
                                        trig.planif = Trigger.PLANIF_MOIS_JOUR;
                                    }

                                    if (calTag.Name.LocalName == "Repetition")
                                    {
                                        logger.LOGPADDLEVEL++;     
                                        logger.log("TimeTrigger");
                                        
                                        trig.planif = Trigger.PLANIF_REPET;
                                        IEnumerable<XElement> subTags = calTag.Elements();
                                        foreach (var subTag in subTags)
                                        {
                                            logger.LOGPADDLEVEL++;                                            
                                            switch (subTag.Name.LocalName)
                                            {
                                                case "Interval":
                                                    TimeSpan ts = XmlConvert.ToTimeSpan(subTag.Value);
                                                    trig.repetInterval = ((ts.Days * 24 + ts.Hours).ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2"));
                                                    logger.debug("repetInterval : "+subTag.Value);
                                                    break;                                                
                                            }

                                            //trig.daysOfWeek.Add(repetInterval);
                                            logger.LOGPADDLEVEL--;
                                        }


                                        logger.LOGPADDLEVEL--;     
                                    }

                                }

                                logger.debug("trigger : " + trig.ToString());
                                taskInfo.triggers.Add(trig);

                            }


                            taskInfo.nbTriggers = triggers.Count();
                          //  log("nb declencheurs actifs : " + taskInfo.nbTriggers);

                            logger.LOGPADDLEVEL--;

                        }


                        //----------------------------------------------------------------------------------------
                        // Actions
                        if (tag.Name.LocalName == "Actions")
                        {
                            logger.LOGPADDLEVEL++;

                            // Exec
                            var execs = from exec in tag.Elements(ns + "Exec")                                   
                                        select exec;

                            foreach(XElement xele in execs)
                            {
                                Action action = new Action();

                                //logger.log(xele.ToString());
                                IEnumerable<XElement> execTags = xele.Elements();
                                foreach (var execTag in execTags)
                                {
                                    // tag command
                                    if (execTag.Name.LocalName == "Command")
                                    {
                                        logger.LOGPADDLEVEL++;
                                        action.Command = execTag.Value;
                                        //logger.debug("command : "+action.Command);
                                        logger.LOGPADDLEVEL--;
                                    }

                                    // tag Arguments
                                    if (execTag.Name.LocalName == "Arguments")
                                    {
                                        logger.LOGPADDLEVEL++;
                                        action.Arguments = execTag.Value;                                        
                                        logger.LOGPADDLEVEL--;
                                    }

                                    // tag working directory
                                    if (execTag.Name.LocalName == "WorkingDirectory")
                                    {
                                        logger.LOGPADDLEVEL++;
                                        action.WorkingDirectory = execTag.Value;
                                        logger.LOGPADDLEVEL--;
                                    }
                                }


                                logger.debug("action : " + action.ToString());
                                taskInfo.actions.Add(action);
                            }
                            
                            taskInfo.nbActions = execs.Count();
                        //    log("nb actions : " + taskInfo.nbActions);
                            logger.LOGPADDLEVEL--;
                        }
                        // Fin Actions
                        //----------------------------------------------------------------------------------------

                    }

                    // boucle d'affichage
                    foreach (string taskTrig in taskInfo.ToString2())
                    {
                        logger.info(taskTrig);
                        if (isOutputMode)
                        {
                           // System.IO.File.WriteAllText(outputFile, taskTrig);
                            System.IO.File.AppendAllText(outputFile, taskTrig+Environment.NewLine);
                            
                        }
                    }
                }
                else
                {
                    logger.LOGPADDLEVEL++;
                    // si pas tag "Task" => le fichier XML n'est pas bon
                    logger.err("Erreur : pas de balise 'Task' trouvee. Abandon." + file);
                }
                logger.LOGPADDLEVEL--;

            }
            logger.LOGPADDLEVEL--;
        }





        private static void recurse(string path, bool processServeur)
        {

            string serveur = "Serveur";
            // parcours récursif du répertoir parent
            try
            {
                foreach (string xmlFile in Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories))
                {
                    logger.debug("@@@ Parsing file " + xmlFile);


                    // option permettant de recuperer le serveur depuis le chemin
                    if (processServeur)
                    {
                        serveur = Program.processServeur(xmlFile);
                    }


                    parseFile(xmlFile, serveur);

                }
                logger.LOGPADDLEVEL--;
            }
            catch (Exception ex)
            {
                logger.err(ex.Message);
            }
        }


        private static string processServeur(string path)
        {
            // TODO : commenter le fonctionnement + ajouter option -s dans usage            
            return (path.Split(Path.DirectorySeparatorChar).Length > path.Split(Path.AltDirectorySeparatorChar).Length) ? path.Split(Path.DirectorySeparatorChar)[1] : path.Split(Path.AltDirectorySeparatorChar)[1];                        
        }

        private static void parseOpt(string option, string arg)
        {            
            switch (option)
            {
                case "-f":
                    // option file            
                    mode = MODE_FILE;
                    arg = "Serveur";
                    break;
                case "-s":
                    // option path + serveur
                    mode = MODE_SERVER;
                    argument = arg;
                    break;
                case "-p":
                    // option path
                    mode = MODE_PATH;
                    argument = arg;
                    break;
                case "-o":
                    //option output
                    isOutputMode = true;
                    outputFile = arg;
                    logger.debug("output to "+outputFile);
                    if (File.Exists(outputFile))
                    {
                        logger.warn("Le fichier "+outputFile+" va etre ecrase");
                        File.WriteAllText(outputFile, TaskInfo.HEADER+Environment.NewLine,System.Text.Encoding.UTF8);
                    }
                    


                    break;
            }
            logger.debug("mode : "+mode+", arg : "+argument);
        }


        private static void action()
        {

            switch (mode)
            {
                case MODE_FILE:
                    // option file            
                    logger.debug("@@@ DEBUT - mode file");
                    parseFile(argument, "Serveur");
                    logger.LOGPADDLEVEL = 0;
                    logger.debug("@@@ FIN");
                    // Console.ReadLine();
                    break;
                case MODE_SERVER:
                    // option path + serveur
                    logger.debug("@@@ DEBUT - mode path + serveur");
                    logger.LOGPADDLEVEL++;

                    if (Path.IsPathRooted(argument))
                    {
                        throw new ArgumentException("Le chemin passe en parametre doit etre relatif : " + argument);
                    }

                    recurse(argument, true);

                    logger.LOGPADDLEVEL = 0;
                    logger.debug("@@@ FIN");
                    break;
                case MODE_PATH:
                    // option path
                    logger.debug("@@@ DEBUT - mode path");
                    logger.LOGPADDLEVEL++;

                    recurse(argument, false);

                    logger.LOGPADDLEVEL = 0;
                    logger.debug("@@@ FIN");
                    break;               
                default:
                    usage();
                    return;

            }

        }
    }
}
