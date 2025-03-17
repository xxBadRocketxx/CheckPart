using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CheckPart.Views; // Tham chiếu đến thư mục Views

namespace CheckPart
{
    [Transaction(TransactionMode.Manual)]
    public class CheckPartCommand : IExternalCommand
    {
        // Trong CheckPartCommand.cs
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument; // Lấy UIDocument hiện tại

            if (uiDoc == null)
            {
                message = "No active document."; // Hoặc xử lý lỗi
                return Result.Cancelled; // Hoặc Result.Failed
            }

            CheckPartWindow window = new CheckPartWindow(uiDoc); // Truyền UIDocument vào
            window.Show();

            return Result.Succeeded;
        }
    }
}