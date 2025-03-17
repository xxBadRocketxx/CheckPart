// Models/CombinedInfo.cs
namespace CheckPart.Models
{
    public class CombinedInfo
    {
        public string ItemType { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string ViewName { get; set; }
        public string SheetNumber { get; set; }
        public bool? IsMatch { get; set; } // Thêm IsMatch, và cho phép null
    }
}