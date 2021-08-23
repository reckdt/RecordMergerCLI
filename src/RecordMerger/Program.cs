using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
            var delimiters = new char[] {'|', ',', ' '};
            var orderBys = new string[] {"asc", "desc"};
            var columnNames = new string[] {};
            var filesDelimsDict = new Dictionary<FileInfo, char>();

            var sortOrderArr = new string[2,2];

            if (sort.Length > 2)
            {
                Console.Error.WriteLine("Only allowed to sort by a max of 2 columns.");
            }

            for (int i = 0; i < sort.Length; i++)
            {
                var arr = sort[i].Split(":");
                var sortColumnName = arr[0];
                sortOrderArr[i, 0] = sortColumnName;
                if (arr.Length == 1)
                {
                    sortOrderArr[i, 1] = "asc"; 
                }
                else
                {
                    var orderBy = arr[1].ToLower();
                    if (!orderBys.Contains(orderBy))
                    {
                        Console.Error.WriteLine("Acceptable order bys are 'asc' or 'desc'.");
                        return;
                    }
                    sortOrderArr[i, 1] = orderBy; 
                }
            }

            // validation
            foreach (var file in files)
            {
                if (!file.Exists)
                {
                    Console.Error.WriteLine($"{file.FullName} does not exist.");
                    return;
                }
                
                string header = File.ReadLines(file.FullName).First();
                foreach (var delimiter in delimiters)
                {
                    if (header.Contains(delimiter))
                    {
                        filesDelimsDict[file] = delimiter;
                        if (columnNames.Length == 0)
                        {
                            columnNames = header.Split(delimiter).Select(x => x.Trim()).ToArray();
                        }
                        else
                        {
                            var tempColumnNames = header.Split(delimiter).Select(x => x.Trim()).ToArray();
                            if (!Enumerable.SequenceEqual(columnNames, tempColumnNames))
                            {
                                Console.Error.WriteLine($"{file.Name} column names do not match previous file.");
                                return;
                            }
                            else
                            {
                                columnNames = tempColumnNames;
                            }
                        }
                        break;                        
                    }
                }

            }

            List<string[]> rows = new List<string[]>();

            foreach (var file in files) 
            {
                using (StreamReader sr = file.OpenText())
                {
                    string s = "";
                    long i = 0;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (i == 0)
                        {
                            i++;
                            continue;
                        }
                        var row = s.Split(filesDelimsDict[file]).Select(x => x.Trim()).ToArray();;
                        rows.Add(row);
                    }
                }
            }

            var columnNamesDict = new Dictionary<string, int>();

            int j = 0;
            foreach (var columnName in columnNames)
            {
                columnNamesDict[columnName] = j;
                j++;
            }
            Console.WriteLine(sortOrderArr[1, 1]);
            if (sortOrderArr[0, 0] == null)
            {
                // do nothing
            }
            else if (sortOrderArr[1, 1] == null)
            {
                var sortColumnName = sortOrderArr[0, 0];
                var sortBy = sortOrderArr[0, 1];
                var columnNumber = columnNamesDict[sortColumnName];
                if (sortBy == "asc")
                {
                    rows = rows.OrderBy(arr => arr[0]).ToList();
                }
                else if (sortBy == "desc")
                {
                    rows = rows.OrderByDescending(arr => arr[0]).ToList();
                }
            }
            else
            {
                var sortColumnName = sortOrderArr[0, 0];
                var sortColumnName2 = sortOrderArr[1, 0];
                var sortBy = sortOrderArr[0, 1];
                var sortBy2 = sortOrderArr[1, 1];
                var columnNumber = columnNamesDict[sortColumnName];
                var columnNumber2 = columnNamesDict[sortColumnName2];
                if (sortBy == "asc")
                {
                    if (sortBy2 == "asc")
                    {
                        rows = rows.OrderBy(arr => GetObject(arr[columnNumber])).ThenBy(arr => GetObject(arr[columnNumber2])).ToList();
                    }
                    else if (sortBy2 == "desc")
                    {
                        rows = rows.OrderBy(arr => GetObject(arr[columnNumber])).ThenByDescending(arr => GetObject(arr[columnNumber2])).ToList();
                    }
                }
                else if (sortBy == "desc")
                {
                    if (sortBy2 == "asc")
                    {
                        rows = rows.OrderByDescending(arr => GetObject(arr[columnNumber])).ThenBy(arr => GetObject(arr[columnNumber2])).ToList();
                    }
                    else if (sortBy2 == "desc")
                    {
                        rows = rows.OrderByDescending(arr => GetObject(arr[columnNumber])).ThenByDescending(arr => GetObject(arr[columnNumber2])).ToList();
                    }
                }
            }

            //rows = rows.OrderBy(arr => arr[0]).ToList();
            //rows = rows.OrderBy(arr => arr[0]).ThenBy(arr => arr[1]).ToList();

            foreach (var row in rows)
            {
                var line = String.Join(",", row);
                Console.WriteLine(line);
            }
        }
        
        public static dynamic GetObject(string str)
        {
            try
            {
                return DateTime.Parse(str);
            }
            catch
            {
                return str;
            }
        }
    }
}
