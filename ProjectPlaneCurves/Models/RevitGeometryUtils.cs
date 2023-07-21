using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPlaneCurves.Models
{
    internal class RevitGeometryUtils
    {
        // Получение линий на плане
        public static List<Curve> GetModelCurvesBySelection(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var selectedElements = sel.PickObjects(ObjectType.Element, new ModelCurveClassFilter(), "Select Curves");
            var elements = selectedElements.Select(r => uiapp.ActiveUIDocument.Document.GetElement(r)).OfType<ModelCurve>();

            Options options = new Options();
            elementIds = ElementIdToString(elements);
            var modelCurves = elements.Select(e => e.get_Geometry(options).First()).OfType<Curve>().ToList();

            return modelCurves;
        }

        // Метод получения строки с ElementId
        private static string ElementIdToString(IEnumerable<Element> elements)
        {
            var stringArr = elements.Select(e => "Id" + e.Id.IntegerValue.ToString()).ToArray();
            string resultString = string.Join(", ", stringArr);

            return resultString;
        }
    }
}
