using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Collections.Generic;

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
            outputOption.IsRequired = true;

            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                fileOptions, sortOptions, outputOption
            };

            rootCommand.Description = "Merges records";

            // Note that the parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<FileInfo[], string[], FileInfo>(Startup);

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        public static void Startup(FileInfo[] files, string[] sort, FileInfo output)
        {
            // validation
            foreach (var file in files) {
                if (!file.Exists)
                {
                    Console.Error.WriteLine($"{file.FullName} does not exist.");
                    return;
                }
            }


        }
    }
}
