using Autodesk.Revit.DB;
using CheckPart.Models;
using System.Collections.Generic;
using Autodesk.Revit.UI;

namespace CheckPart.Services.TextNote
{
    // Services/TextNote/ITextNoteSearchService.cs
    public interface ITextNoteSearchService
    {
        List<TextNoteInfo> FindTextNotes(Document doc, string searchText, string parameterValue); // Thêm string parameterValue
        ViewSheet GetViewSheet(Document doc, ElementId viewId);
        void ShowTextNoteInView(UIDocument uidoc, TextNode textNote);
        void UpdateTextNote(Document doc, Autodesk.Revit.DB.TextNote textNote, string newText); // THÊM LẠI DÒNG NÀY
    }
}