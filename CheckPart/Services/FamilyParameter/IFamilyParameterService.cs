using Autodesk.Revit.DB;
using CheckPart.Models;
using System.Collections.Generic;

namespace CheckPart.Services.FamilyParameter
{
    public interface IFamilyParameterService
    {
        List<ParameterInfo> LoadFamilyParameters(Document doc, string familyName);
        string GetParameterValue(Parameter param, Document doc);
        List<string> LoadParentFamilies(Document doc, string familyName); // Thêm interface cho hàm LoadParentFamilies
    }
}