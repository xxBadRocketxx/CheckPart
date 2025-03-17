using Autodesk.Revit.DB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CheckPart.Models;
using SWC = System.Windows.Controls; // Alias for System.Windows.Controls
using Autodesk.Revit.UI;

namespace CheckPart.Services.FamilyParameter
{
    public class FamilyParameterUpdateService
    {
        public void UpdateParameterFromCellEdit(UIDocument uidoc, DataGridCellEditEndingEventArgs e, object sender, string familyNameTextBox)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Lấy thông tin về ô vừa được chỉnh sửa
                var editedCell = e.Column.GetCellContent(e.Row);
                var editedTextBox = editedCell as SWC.TextBox; // Sử dụng alias SWC
                if (editedTextBox == null) return;

                string newValue = editedTextBox.Text;

                // Lấy ParameterInfo tương ứng
                var parameterInfo = e.Row.Item as ParameterInfo; // Không cần fully qualify vì đã sửa ở CheckPartWindow.xaml.cs
                if (parameterInfo == null) return;

                // Tìm FamilySymbol và Parameter cần cập nhật
                Document doc = uidoc.Document; // Sử dụng Document từ UIDocument

                string familySymbolName = parameterInfo.Name.Split('-')[0].Trim(); //tách lấy tên familysymbol
                string parameterName = parameterInfo.Name.Split('-')[1].Trim();   //tách lấy tên parameter


                // Tải lại FamilySymbol từ document (Không cache).
                FamilySymbol familySymbol = new FilteredElementCollector(doc)
                     .OfClass(typeof(FamilySymbol))
                     .Cast<FamilySymbol>()
                     .FirstOrDefault(fs => fs.FamilyName.Equals(familySymbolName, StringComparison.OrdinalIgnoreCase));

                if (familySymbol == null)
                {
                    MessageBox.Show("FamilySymbol not found.");
                    return;
                }

                Parameter parameter = familySymbol.LookupParameter(parameterName);

                if (parameter == null || parameter.IsReadOnly)
                {
                    MessageBox.Show("Parameter not found or is read-only.");
                    return;
                }

                // Cập nhật giá trị parameter trong Transaction
                using (Transaction trans = new Transaction(doc, "Update Parameter"))
                {
                    trans.Start();
                    try
                    {
                        bool success = SetParameterValue(parameter, newValue, doc); // Truyền doc vào đây
                        if (!success)
                        {
                            MessageBox.Show("Failed to set parameter value.  Invalid type.");
                            trans.RollBack();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating parameter: " + ex.Message);
                        trans.RollBack();
                        return;
                    }
                    trans.Commit();
                }
            }
        }

        internal void UpdateParameterFromCellEdit(UIDocument uidoc, DataGridCellEditEndingEventArgs e, object sender, string text, ParameterInfo parameterInfo)
        {
            throw new NotImplementedException();
        }

        private bool SetParameterValue(Parameter param, string value, Document doc) // Thêm Document
        {
            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.Set(value);

                case StorageType.Integer:
                    if (int.TryParse(value, out int intValue))
                    {
                        return param.Set(intValue);
                    }
                    return false;

                case StorageType.Double:
                    if (double.TryParse(value, out double doubleValue))
                    {
                        return param.Set(doubleValue);
                    }
                    return false;

                case StorageType.ElementId:
                    if (int.TryParse(value, out int elementIdValue))
                    {
                        ElementId id = new ElementId(elementIdValue);
                        if (doc.GetElement(id) != null) // Kiểm tra với doc
                        {
                            return param.Set(id);
                        }
                    }
                    return false;

                case StorageType.None:
                default:
                    return false;
            }
        }
    }
}