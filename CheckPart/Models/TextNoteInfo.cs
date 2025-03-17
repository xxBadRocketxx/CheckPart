using Autodesk.Revit.DB;

namespace CheckPart.Models
{
    public class TextNoteInfo
    {
        public TextNote TextNote { get; set; }
        public string Text { get; set; }
        public string ViewName { get; set; }
        public string SheetNumber { get; set; }
        public bool IsMatch { get; set; } // Đảm bảo có thuộc tính này

        // Constructor
        public TextNoteInfo(TextNote textNote, string text, string viewName, string sheetNumber)
        {
            TextNote = textNote;
            Text = text;
            ViewName = viewName;
            SheetNumber = sheetNumber;
            IsMatch = true; // Mặc định là true (khớp)
        }
    }
}