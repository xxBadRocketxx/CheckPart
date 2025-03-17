using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CheckPart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CheckPart.Services.TextNote
{
    public class TextNoteSearchService : ITextNoteSearchService
    {
        private readonly Dictionary<ElementId, ViewSheet> _viewSheetCache = new Dictionary<ElementId, ViewSheet>();
        private Dictionary<ElementId, ViewSheet> _viewToSheetMap;

        // Tìm kiếm TextNote dựa trên searchText, CHỈ trong các View thuộc 2 ViewType
        public List<TextNoteInfo> FindTextNotes(Document doc, string searchText, string parameterValue) // Thêm parameterValue
        {
            _viewToSheetMap = CreateViewToSheetMap(doc);

            // Lọc View (Giữ nguyên như phiên bản 1.0)
            ElementId viewTypeId1 = new ElementId(514487);
            ElementId viewTypeId2 = new ElementId(582743);

            LogicalOrFilter viewTypeFilter = new LogicalOrFilter(
                new ElementParameterFilter(ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM), viewTypeId1)),
                new ElementParameterFilter(ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM), viewTypeId2))
            );

            List<View> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WherePasses(viewTypeFilter)
                .Cast<View>()
                .ToList();

            List<TextNoteInfo> foundItems = views.SelectMany(view =>
            {
                if (view == null) return Enumerable.Empty<TextNoteInfo>();

                return new FilteredElementCollector(doc, view.Id)
                    .OfClass(typeof(Autodesk.Revit.DB.TextNote))
                    .Cast<Autodesk.Revit.DB.TextNote>()
                    .Where(textNote => textNote != null && textNote.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(textNote =>
                    {
                        ViewSheet sheet = GetViewSheet(doc, view.Id);
                        string sheetNumber = sheet != null ? sheet.SheetNumber : "N/A";

                        // --- So sánh giá trị ---  THÊM PHẦN NÀY
                        string textNoteContent = textNote.Text;

                        // Bỏ qua dòng đầu tiên
                        string[] lines = textNoteContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        string textToCompare = string.Join(Environment.NewLine, lines.Skip(1));

                        // So sánh
                        bool isMatch = string.Equals(textToCompare.Trim(), parameterValue.Trim(), StringComparison.OrdinalIgnoreCase);

                        // --- Kết thúc phần so sánh ---

                        return new TextNoteInfo(textNote, textNote.Text, view.Name, sheetNumber) { IsMatch = isMatch }; // Đặt IsMatch
                    });
            }).ToList();

            return foundItems;
        }

        // Tạo ánh xạ ViewId sang ViewSheet (tăng tốc độ)
        //(giữ nguyên code)
        private Dictionary<ElementId, ViewSheet> CreateViewToSheetMap(Document doc)
        {
            return new FilteredElementCollector(doc)
               .OfClass(typeof(ViewSheet))
               .Cast<ViewSheet>()
               .SelectMany(sheet => sheet.GetAllPlacedViews().Select(viewId => new { ViewId = viewId, Sheet = sheet }))
               .GroupBy(x => x.ViewId)
               .ToDictionary(g => g.Key, g => g.First().Sheet);
        }

        // Lấy ViewSheet chứa View (dùng cache)
        //(giữ nguyên code)
        public ViewSheet GetViewSheet(Document doc, ElementId viewId)
        {
            if (_viewSheetCache.TryGetValue(viewId, out ViewSheet cachedSheet)) return cachedSheet;

            if (_viewToSheetMap.TryGetValue(viewId, out ViewSheet sheet))
            {
                _viewSheetCache[viewId] = sheet;
                return sheet;
            }

            return null;
        }

        // Hiển thị TextNote (chuyển đến View và chọn)
        //(giữ nguyên code)
        public void ShowTextNoteInView(UIDocument uidoc, Autodesk.Revit.DB.TextNote textNote)
        {
            Document _doc = uidoc.Document;

            ElementId viewId = textNote.OwnerViewId;
            View view = _doc.GetElement(viewId) as View;

            if (view != null && !(view is ViewSheet))
            {
                uidoc.ActiveView = view;
            }
            else //xóa GetValidView
            {
                // Tìm view hợp lệ để hiển thị textNote trong trường hợp view không hợp lệ
                View firstView = new FilteredElementCollector(_doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .FirstOrDefault(v => v.Id == textNote.OwnerViewId); // Tìm view theo OwnerViewId

                if (firstView != null)
                {
                    uidoc.ActiveView = firstView;
                }
            }
            uidoc.Selection.SetElementIds(new List<ElementId> { textNote.Id });
            uidoc.ShowElements(textNote.Id);
        }
        public void UpdateTextNote(Document doc, Autodesk.Revit.DB.TextNote textNote, string newText)
        {
            if (textNote == null) return;

            using (Transaction trans = new Transaction(doc, "Update Text Note"))
            {
                trans.Start();
                try
                {
                    textNote.Text = newText;
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating Text Note: " + ex.Message);
                    trans.RollBack();
                }
            }
        }

        public void ShowTextNoteInView(UIDocument uidoc, TextNode textNote)
        {
            throw new NotImplementedException();
        }
    }
}