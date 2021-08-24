using System;

namespace RecordMergerAPI.Models
{
    public class RecordItem
    {
        public long Id { get; set; }
        public string Line { get; set; }
        public char Delimiter { get; set; } 
    }

    public class RecordItemDTO
    {
        public string Line { get; set; }
    }

    public class CSV
    {
        public string Body { get; set; }
    }
}