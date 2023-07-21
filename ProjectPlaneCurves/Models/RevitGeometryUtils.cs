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
        // Получение линий на плане с помощью пользовательского выбора
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

        // Получение линий на плане из Settings
        public static List<Curve> GetModelCurvesBySettings(Document doc, IEnumerable<int> ids)
        {
            var elementsInSettings = new List<Element>();
            foreach (var id in ids)
            {
                ElementId elemId = new ElementId(id);
                Element elem = doc.GetElement(elemId);
                elementsInSettings.Add(elem);
            }

            Options options = new Options();

            var modelCurvesElems = elementsInSettings.OfType<ModelCurve>();
            var modelCurves = modelCurvesElems.Select(e => e.get_Geometry(options).First()).OfType<Curve>().ToList();

            return modelCurves;
        }

        // Проверка на то существуют линии на плане с данными Id в модели
        public static bool IsPlaneCurvesExistInModel(Document doc, IEnumerable<int> elems)
        {
            if (elems is null)
            {
                return false;
            }

            foreach (var elem in elems)
            {
                ElementId id = new ElementId(elem);
                Element curElem = doc.GetElement(id);
                if (curElem is null || !(curElem is ModelCurve))
                {
                    return false;
                }
            }

            return true;
        }

        // Получение id элементов на основе списка в виде строки
        public static List<int> GetIdsByString(string elems)
        {
            if (string.IsNullOrEmpty(elems))
            {
                return null;
            }

            var elemIds = elems.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => int.Parse(s.Remove(0, 2)))
                         .ToList();

            return elemIds;
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
