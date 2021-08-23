using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace RecordMerger
{
    class Program
    {
        static int Main(string[] args)
        {
            var fileOptions = new Option<FileInfo[]>(
                "--files",
                "An option whose argument is parsed as a FileInfo");
            fileOptions.AddAlias("--input");
            fileOptions.IsRequired = true;

            var sortOptions = new Option<string[]>(
                "--sort",
                "An option whose argument is parsed as a FileInfo");

            var outputOption = new Option<FileInfo>(
                "--output",
                "An option whose argument is parsed as a FileInfo");

            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                fileOptions, sortOptions, outputOption
            };

            rootCommand.Description = "Merges records";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<FileInfo[], string[], FileInfo>(TryRun);

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        public static void TryRun(FileInfo[] files, string[] sort, FileInfo output)
        {
            try
            {
                var startup = new Startup();
                string csv = startup.Run(files, sort, output);

                // if no output file just write to console
                if (output == null)
                {
                    Console.WriteLine(csv);
                }
                else
                {
                    File.WriteAllText(output.FullName, csv);
                }
            } catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
