using System.Collections.Generic;
namespace DBChangesDataAPI.Models
{
    public class ToDoItemDBChanges
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public List<KeyValuePair<string, string>> DBchanges { get; set; }
    }
}