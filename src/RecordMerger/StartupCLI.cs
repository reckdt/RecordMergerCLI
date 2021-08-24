using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace RecordMerger
{
    public class StartupCLI
    {
        public string Run(FileInfo[] files, string[] sort, FileInfo output)
        {
            var delimsByFiles = GetDelimsByFiles(files);
            var columnNames = GetColumnNames(delimsByFiles);
            var positionsByColumnName = GetPositionsByColumnName(columnNames);
            var sorts = GetSorts(sort, positionsByColumnName);
            var rows = GetRowsFromFiles(delimsByFiles);
            rows = SortRows(rows, sorts);

            var header = String.Join(",", columnNames);
            var csvBody = GetCSVBody(rows);
            var csv = header + Environment.NewLine + csvBody;

            return csv;
        }

        public string GetCSVBody(List<string[]> rows)
        {
            var csvBody = String.Join(Environment.NewLine, rows.Select(row => String.Join(",", row)).ToArray());

            return csvBody;
        }

        public Dictionary<FileInfo, char> GetDelimsByFiles(FileInfo[] files)
        {
            var delimsByFiles = new Dictionary<FileInfo, char>();
            var found = false;

            // checks if the header contains one of the specified delimiters
            foreach (var file in files)
            {
                if (!file.Exists)
                {
                    throw new ArgumentException($"{file.FullName} does not exist.");
                }

                string header = File.ReadLines(file.FullName).First();
                var delimiter = GetDelimiter(header);
                if (delimiter != '\0')
                {
                    delimsByFiles[file] = delimiter;
                    found = true;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException("Header does not contain a valid delimiter, only '|', ',', and ' ' accepted.");
            }

            return delimsByFiles;
        }

        public char GetDelimiter(string line)
        {
            var delimiters = new char[] {'|', ',', ' '};
            var returnedDelimiter = '\0';

            foreach (var delimiter in delimiters)
            {
                if (line.Contains(delimiter))
                {
                    returnedDelimiter = delimiter;
                    break;                        
                }
            }

            return returnedDelimiter;
        }

        public Dictionary<string, int> GetPositionsByColumnName(string[] columnNames)
        {
            var positionsByColumnNames = new Dictionary<string, int>();

            int i = 0;
            foreach (var columnName in columnNames)
            {
                positionsByColumnNames[columnName] = i;
                i++;
            }

            return positionsByColumnNames;
        }

        public string[] GetColumnNames(Dictionary<FileInfo, char> delimsByFiles)
        {
            var columnNames = new string[] { };

            // checks that the column names aren't different throughout the input files
            foreach (var item in delimsByFiles)
            {
                string header = File.ReadLines(item.Key.FullName).First();
                var tempColumnNames = header.Split(item.Value).Select(x => x.Trim()).ToArray();

                if (columnNames.Length != 0 && !Enumerable.SequenceEqual(columnNames, tempColumnNames))
                {
                    throw new InvalidOperationException("Column names throughout files do not match.");
                }
                else
                {
                    columnNames = tempColumnNames;
                }                       
            }

            return columnNames;
        }

        public Sort[] GetSorts(string[] sorts, Dictionary<string, int> positionsByColumnName)
        {
            if (sorts.Length > 2)
            {
                throw new ArgumentException("Only allowed to sort by a max of 2 columns.");
            }

            var sortsArr = new Sort[sorts.Length];
            var orderBys = new string[] {"asc", "desc"};

            // gets the sort fields and if asc or desc
            for (int i = 0; i < sorts.Length; i++)
            {
                var arr = sorts[i].Split(":");
                var columnName = arr[0];
                if (!positionsByColumnName.ContainsKey(columnName))
                {
                    throw new ArgumentException($"Sort column, {columnName}, does not exist.");
                }
                var position = positionsByColumnName[columnName];
                var sort = new Sort(position);

                if (arr.Length == 1)
                {
                    sort.By = "asc"; 
                }
                else
                {
                    var orderBy = arr[1].ToLower();
                    if (!orderBys.Contains(orderBy))
                    {
                        throw new ArgumentException($"Acceptable sorts are only 'asc' or 'desc', not {orderBy}.");
                    }
                    sort.By = orderBy; 
                }

                sortsArr[i] = sort;
            }

            return sortsArr;
        }

        public List<string> GetLines(FileInfo file)
        {
            List<string> lines = new List<string>();

            using (StreamReader sr = file.OpenText())
            {
                string line = "";
                long i = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (i == 0)
                    {
                        i++;
                        continue;
                    }
                    if (line == "")
                    {
                        continue;
                    }
                    
                    lines.Add(line);
                }
            }

            return lines;
        }

        public List<string[]> GetRows(List<string> lines, char delimiter)
        {
            List<string[]> rows = new List<string[]>();

            foreach (var line in lines) 
            {
                var row = GetRow(line, delimiter);
                rows.Add(row);
            }

            return rows;
        }

        public string[] GetRow(string line, char delimiter)
        {
            var row = line.Split(delimiter).Select(x => x.Trim()).ToArray();;
            for (int j = 0; j < row.Length; j++)
            {
                row[j] = TryGettingFormattedDate(row[j]);
            }

            return row;
        }

        public List<string[]> GetRowsFromFiles(Dictionary<FileInfo, char> delimsByFiles)
        {
            List<string[]> rows = new List<string[]>();

            foreach (var item in delimsByFiles)
            {
                var lines = GetLines(item.Key);
                var rowsToAdd = GetRows(lines, item.Value);
                rows.AddRange(rowsToAdd);
            }

            return rows;
        }


        public List<string[]> SortRows(List<string[]> rows, Sort[] sorts)
        {
            // does the sorting
            if (sorts.Length == 0)
            {
                // do nothing
            }
            else if (sorts.Length == 1 && sorts[0] != null)
            {
                var sort = sorts[0];
                if (sort.By == "asc")
                {
                    rows = rows.OrderBy(arr => GetObject(arr[sort.Position])).ToList();
                }
                else if (sort.By == "desc")
                {
                    rows = rows.OrderByDescending(arr => GetObject(arr[sort.Position])).ToList();
                }
            }
            else if (sorts.Length == 2 && sorts[0] != null && sorts[1] != null)
            {
                var sort = sorts[0];
                var sort2 = sorts[1];
                if (sort.By == "asc")
                {
                    if (sort2.By == "asc")
                    {
                        rows = rows.OrderBy(arr => GetObject(arr[sort.Position]))
                            .ThenBy(arr => GetObject(arr[sort2.Position])).ToList();
                    }
                    else if (sort2.By == "desc")
                    {
                        rows = rows.OrderBy(arr => GetObject(arr[sort.Position]))
                            .ThenByDescending(arr => GetObject(arr[sort2.Position])).ToList();
                    }
                }
                else if (sort.By == "desc")
                {
                    if (sort2.By == "asc")
                    {
                        rows = rows.OrderByDescending(arr => GetObject(arr[sort.Position]))
                            .ThenBy(arr => GetObject(arr[sort2.Position])).ToList();
                    }
                    else if (sort2.By == "desc")
                    {
                        rows = rows.OrderByDescending(arr => GetObject(arr[sort.Position]))
                            .ThenByDescending(arr => GetObject(arr[sort2.Position])).ToList();
                    }
                }
            }

            return rows;
        }

        public string TryGettingFormattedDate(string str)
        {
            try
            {
                var dateTime = DateTime.Parse(str);
                return dateTime.ToString("M/d/yyyy");
            }
            catch
            {
                return str;
            }
        }

        public dynamic GetObject(string str)
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

        public class Sort
        {
            public int Position { get; set; }
            public string By { get; set; }

            public Sort(int position)
            {
                Position = position;
            }
        }
    }
}