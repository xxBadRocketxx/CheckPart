using Autodesk.Revit.DB;
using CheckPart.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckPart.Services.FamilyParameter
{
    public class FamilyParameterService : IFamilyParameterService
    {
        public List<ParameterInfo> LoadFamilyParameters(Document doc, string familyName)
        {
            if (string.IsNullOrWhiteSpace(familyName))
            {
                return new List<ParameterInfo>();
            }

            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol));

            List<FamilySymbol> matchingFamilySymbols = collector.Cast<FamilySymbol>()
                .Where(fs => fs.FamilyName.IndexOf(familyName, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (matchingFamilySymbols.Count == 0)
            {
                return new List<ParameterInfo>();
            }

            HashSet<string> parameterNamesToShow = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "IDENTITY_DESCRIPTION",
                "CONTROL_MARK",
                "desc_b",
                "desc_g",
                "mark_b",
                "mark_g"
            };

            List<ParameterInfo> parameterInfos = new List<ParameterInfo>();

            foreach (FamilySymbol familySymbol in matchingFamilySymbols)
            {
                ParameterSet parameters = familySymbol.Parameters;

                foreach (Parameter param in parameters)
                {
                    if (parameterNamesToShow.Contains(param.Definition.Name))
                    {
                        string paramValue = GetParameterValue(param, doc);
                        parameterInfos.Add(new ParameterInfo { Name = $"{familySymbol.FamilyName} - {param.Definition.Name}", Value = paramValue });
                    }
                }
            }
            return parameterInfos;
        }

        public string GetParameterValue(Parameter param, Document doc)
        {
            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.AsString();
                case StorageType.Integer:
                    return param.AsInteger().ToString();
                case StorageType.Double:
                    return param.AsDouble().ToString();
                case StorageType.ElementId:
                    ElementId id = param.AsElementId();
                    if (id.IntegerValue >= 0)
                    {
                        return doc.GetElement(id)?.Name ?? "ElementId: " + id.IntegerValue.ToString();
                    }
                    return "ElementId: " + id.IntegerValue.ToString();
                case StorageType.None:
                    return "N/A";
                default:
                    return "Unknown Storage Type";
            }
        }
        public List<string> LoadParentFamilies(Document doc, string familyName)
        {
            // 1. Tìm tất cả các FamilyInstance của family có tên chứa searchText.
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilyInstance))
                 .WhereElementIsNotElementType();  // Chỉ lấy FamilyInstance, không lấy FamilySymbol

            // Lọc theo tên family (chú ý: lọc trên FamilySymbol của FamilyInstance)
            List<FamilyInstance> familyInstances = collector.Cast<FamilyInstance>()
                .Where(fi => fi.Symbol.FamilyName.IndexOf(familyName, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();


            // 2. Lấy danh sách các Family cha (Family) của các FamilyInstance vừa tìm được.
            HashSet<string> parentFamilies = new HashSet<string>();
            foreach (FamilyInstance fi in familyInstances)
            {

                if (fi.SuperComponent == null)
                {

                    if (!parentFamilies.Contains(fi.Symbol.FamilyName)) //tránh trùng lặp
                    {
                        parentFamilies.Add(fi.Symbol.FamilyName);
                    }
                }
                else
                {
                    FamilyInstance parent = doc.GetElement(fi.SuperComponent.Id) as FamilyInstance; // Lấy family cha trực tiếp
                    if (parent != null)
                    {
                        parentFamilies.Add(parent.Symbol.FamilyName); //thêm tên Family cha.
                    }
                }
            }
            return parentFamilies.ToList();
        }


    }
}