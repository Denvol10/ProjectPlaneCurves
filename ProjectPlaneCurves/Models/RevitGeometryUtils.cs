﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        // Получение грани с помощью пользовательского выбора
        public static Face GetFaceBySelection(UIApplication uiapp, out string elementIds)
        {
            Document doc = uiapp.ActiveUIDocument.Document;
            Selection sel = uiapp.ActiveUIDocument.Selection;

            var selectedFace = sel.PickObject(ObjectType.Face, "Select Face");
            string faceSymbolReference = selectedFace.ConvertToStableRepresentation(doc);
            elementIds = faceSymbolReference;

            if(doc.IsFamilyDocument)
            {
                Face face = doc.GetElement(selectedFace).GetGeometryObjectFromReference(selectedFace) as Face;

                return face;
            }

            return GetFaceByReference(doc, selectedFace);
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

        // Проверка на то существует грань в модели
        public static bool IsFaceExistInModel(Document doc, string faceRepresentation)
        {
            if (string.IsNullOrEmpty(faceRepresentation))
            {
                return false;
            }

            try
            {
                Reference reference = Reference.ParseFromStableRepresentation(doc, faceRepresentation);
                if (reference is null)
                {
                    return false;
                }
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            return true;
        }

        // Получение точки на грани на основе грани и точки в плане
        public static XYZ GetPointOnFace(Face face, XYZ planePoint)
        {
            Line verticalLine = Line.CreateUnbound(planePoint, XYZ.BasisZ);

            IntersectionResultArray interResult;
            var compResult = face.Intersect(verticalLine, out interResult);

            if(compResult == SetComparisonResult.Overlap)
            {
                foreach(var elem in interResult)
                {
                    if(elem is IntersectionResult result)
                    {
                        return result.XYZPoint;
                    }
                }
            }

            return null;
        }

        // Получение точек на ребрах граней (только для ребер в виде прямой линии)
        public static List<double> GetParametersOnPolyCurveByEdges(PolyCurve polyCurve, Face face)
        {
            var parameters = new List<double>();
            var edgeArrays = face.EdgeLoops;
            foreach(EdgeArray edgeArray in edgeArrays)
            {
                foreach(Edge edge in edgeArray)
                {
                    if(edge.AsCurve() is Line line)
                    {
                        XYZ startPoint = line.GetEndPoint(0);
                        XYZ endPoint = line.GetEndPoint(1);
                        XYZ startPlanePoint = new XYZ(startPoint.X, startPoint.Y, 0);
                        XYZ endPlanePoint = new XYZ(endPoint.X, endPoint.Y, 0);
                        Line planeLine = Line.CreateBound(startPlanePoint, endPlanePoint);
                        double parameter;
                        if(polyCurve.Intersect(planeLine, out _, out parameter))
                        {
                            parameters.Add(parameter);
                        }    
                    }
                }
            }

            return parameters;
        }

        // Получение поверхности экземпляра элемента из Reference
        public static Face GetFaceByReference(Document doc, Reference reference)
        {
            string faceSymbolReference = reference.ConvertToStableRepresentation(doc);
            string[] elementInfo = faceSymbolReference.Split(':');
            int faceId = int.Parse(elementInfo.ElementAt(elementInfo.Length - 2));

            Element element = doc.GetElement(reference.ElementId);
            Options options = new Options();
            var elementGeometry = element.get_Geometry(options);
            var elementGeometryInstance = elementGeometry.OfType<GeometryInstance>().First();
            var faceArrays = elementGeometryInstance.GetInstanceGeometry().OfType<Solid>().Select(s => s.Faces);
            foreach (var faceArray in faceArrays)
            {
                foreach (var faceObject in faceArray)
                {
                    if (faceObject is Face face)
                    {
                        if (face.Id == faceId)
                        {
                            return face;
                        }
                    }
                }
            }

            return null;
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
