using Autodesk.Revit.UI;
using System.Windows;
using System.Windows.Input;
using CheckPart.Services.FamilyParameter;
using CheckPart.Services.TextNote;
using CheckPart.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using Autodesk.Revit.DB;
using System;

namespace CheckPart.Views
{
    public partial class CheckPartWindow : Window
    {
        private readonly UIDocument _uidoc;
        private readonly FamilyParameterService _familyParameterService;
        private readonly FamilyParameterUpdateService _familyParameterUpdateService;
        private readonly TextNoteSearchService _textNoteSearchService;
        private ObservableCollection<ParameterInfo> _parameterInfos;
        private ObservableCollection<TextNoteInfo> _textNoteInfos; // Thêm ObservableCollection cho TextNoteInfo

        public CheckPartWindow(UIDocument uidoc)
        {
            InitializeComponent();
            _uidoc = uidoc;
            if (_uidoc == null)
            {
                MessageBox.Show("Error: UIDocument is null."); // Hoặc xử lý lỗi khác
                Close(); // Đóng cửa sổ
                return;
            }

            // Khởi tạo các service
            _familyParameterService = new FamilyParameterService();
            _familyParameterUpdateService = new FamilyParameterUpdateService();
            _textNoteSearchService = new TextNoteSearchService();

            _parameterInfos = new ObservableCollection<ParameterInfo>();
            parameterDataGrid.ItemsSource = _parameterInfos;
            parameterDataGrid.CellEditEnding += ParameterDataGrid_CellEditEnding;


            _textNoteInfos = new ObservableCollection<TextNoteInfo>(); // Khởi tạo
            textNotesDataGrid.ItemsSource = _textNoteInfos; // Gán cho DataGrid
            textNotesDataGrid.CellEditEnding += TextNotesDataGrid_CellEditEnding; // Đăng ký sự kiện
        }



        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
                e.Handled = true;
            }
        }
        private void Search()
        {
            string searchText = FamilyNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText)) return;

            if (_uidoc.Document == null)
            {
                MessageBox.Show("Error: Document is null.");
                return;
            }

            _parameterInfos.Clear();
            _textNoteInfos.Clear();

            // 1. Lấy thông tin Parameter
            var parameterInfos = _familyParameterService.LoadFamilyParameters(_uidoc.Document, searchText);
            foreach (var info in parameterInfos)
            {
                _parameterInfos.Add(info);
            }

            // 2. Lấy giá trị Parameter để so sánh (VÍ DỤ: lấy Parameter đầu tiên)
            string parameterValueToCompare = "";
            if (parameterInfos.Count > 0)
            {
                parameterValueToCompare = parameterInfos[0].Value; // Lấy giá trị đầu tiên
            }

            // 3. Tìm kiếm TextNote và truyền giá trị Parameter để so sánh
            var textNoteInfos = _textNoteSearchService.FindTextNotes(_uidoc.Document, searchText, parameterValueToCompare);
            foreach (var info in textNoteInfos)
            {
                _textNoteInfos.Add(info); // Thêm vào _textNoteInfos
            }
            // 4. Tìm kiếm và hiển thị family cha
            parentFamiliesListView.ItemsSource = _familyParameterService.LoadParentFamilies(_uidoc.Document, searchText)
               .Where(familyName => !familyName.Equals(searchText, StringComparison.OrdinalIgnoreCase))
               .ToList();
        }

        private void ParameterDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _familyParameterUpdateService.UpdateParameterFromCellEdit(_uidoc, e, sender, FamilyNameTextBox.Text);
            Search();
        }

        private void textNotesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (textNotesDataGrid.SelectedItem is TextNoteInfo selectedItem)
            {
                _textNoteSearchService.ShowTextNoteInView(_uidoc, selectedItem.TextNote);
            }
        }

        private void TextNotesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editedCell = e.Column.GetCellContent(e.Row);
                var editedTextBox = editedCell as System.Windows.Controls.TextBox;
                if (editedTextBox == null) return;

                string newValue = editedTextBox.Text;

                var textNoteInfo = e.Row.Item as TextNoteInfo;
                if (textNoteInfo == null) return;

                // Gọi service để cập nhật TextNote
                _textNoteSearchService.UpdateTextNote(_uidoc.Document, textNoteInfo.TextNote, newValue);
            }
        }
    }
}