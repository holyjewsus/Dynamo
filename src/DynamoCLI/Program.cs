using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Applications.StartupUtils;
using System.Diagnostics;
using Dynamo.Models;
using Dynamo.Services;
using System.Text.RegularExpressions;

namespace DynamoCLI
{
    internal class Program
    {

        protected static CommandLineArguments commandstringToArgs(string commandstring)
        {
            var argarray = commandstringToStringArray(commandstring);
            var args = CommandLineArguments.FromArguments(argarray);
            return args;
        }

        protected static string[] commandstringToStringArray(string commandstring)
        {
            //convert string to commandlineargs, 
            var m = Regex.Matches(commandstring, "([^\"]\\S*|\".+?\")\\s*");
            var list = m.Cast<Match>().Select(match => match.Value).ToList();

            //strip trailing whitespace
            var argarray = list.Select(x => x.Trim()).ToArray();
            return argarray.ToArray();
        }



        [STAThread]
        static internal void Main(string[] args)
        {

            try
            {
                var cmdLineArgs = CommandLineArguments.FromArguments(args);
                Console.WriteLine("command line args built");
                Console.WriteLine(cmdLineArgs.OpenFilePath);
                Console.WriteLine(cmdLineArgs.Verbose);

                var locale = Dynamo.Applications.StartupUtils.Locale.SetLocale(cmdLineArgs);
                Console.WriteLine(locale);
                var model = Dynamo.Applications.StartupUtils.Preloading.MakeModel(true,Dynamo.Applications.StartupUtils.Preloading.CLILibraries);
                Console.WriteLine(model.Version);
                var runner = new CommandLineRunner(model);
                Console.WriteLine("about to run");
                runner.Run(cmdLineArgs);
                Console.WriteLine("run complete");
                
            }
            catch (Exception e)
            {
                try
                {
                    DynamoModel.IsCrashing = true;
                    InstrumentationLogger.LogException(e);
                    StabilityTracking.GetInstance().NotifyCrash();

                }
                catch
                {
                }

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

    }
}
