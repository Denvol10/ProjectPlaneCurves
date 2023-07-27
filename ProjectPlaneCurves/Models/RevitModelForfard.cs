using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using ProjectPlaneCurves.Models;

namespace ProjectPlaneCurves
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Линии для проецирования в плане
        public PolyCurve PlaneCurves { get; set; }

        private string _planeCurvesElemIds;
        public string PlaneCurvesElemIds
        {
            get => _planeCurvesElemIds;
            set => _planeCurvesElemIds = value;
        }

        public void GetPlaneCurvesBySelection()
        {
            var curves = RevitGeometryUtils.GetModelCurvesBySelection(Uiapp, out _planeCurvesElemIds);
            PlaneCurves = new PolyCurve(curves);
        }
        #endregion

        #region Проверка на то существуют линии на плане в модели
        public bool IsPlaneCurvesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsPlaneCurvesExistInModel(Doc, elemIds);
        }
        #endregion

        #region Получение линий на плане из Settings
        public void GetPlaneLinesBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            var curves = RevitGeometryUtils.GetModelCurvesBySettings(Doc, elemIds);
            PlaneCurves = new PolyCurve(curves);
        }
        #endregion

        #region Грань для проецирования линий
        public Face FaceForProject { get; set; }

        private string _faceRepresentation;
        public string FaceRepresentation
        {
            get => _faceRepresentation;
            set => _faceRepresentation = value;
        }
        #endregion

        #region Получение грани с помощью пользовательского выбора
        public void GetFaceBySelection()
        {
            FaceForProject = RevitGeometryUtils.GetFaceBySelection(Uiapp, out _faceRepresentation);
        }
        #endregion

        #region Проверка на то существует ли грань в модели
        public bool IsFaceExistInModel(string faceRepresentation)
        {
            return RevitGeometryUtils.IsFaceExistInModel(Doc, faceRepresentation);
        }
        #endregion

        #region Получение грани из Settings
        public void GetFaceBySetings(string faceRepresentation)
        {
            Reference faceRef = Reference.ParseFromStableRepresentation(Doc, faceRepresentation);
            FaceForProject = Doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;
        }
        #endregion

        #region Тест получение адаптивных точек на грани
        public void CreateAdaptivePointsOnFace()
        {
            double boundParameter1 = 0;
            double boundParameter2 = PlaneCurves.GetLength();

            var pointParameters = GenerateParameters(boundParameter1, boundParameter2, 1.5);
            var edgeParameters = RevitGeometryUtils.GetParametersOnPolyCurveByEdges(PlaneCurves, FaceForProject);

            pointParameters.AddRange(edgeParameters);
            pointParameters = pointParameters.OrderBy(p => p).ToList();

            var pointsOnFace = new List<XYZ>();

            foreach(var parameter in pointParameters)
            {
                XYZ planePoint = PlaneCurves.GetPointOnPolycurve(parameter, out _);
                var pointOnFace = RevitGeometryUtils.GetPointOnFace(FaceForProject, planePoint);
                if(!(pointOnFace is null))
                {
                    pointsOnFace.Add(pointOnFace);
                }
            }

            using(Transaction trans = new Transaction(Doc, "Created Reference Points"))
            {
                trans.Start();
                foreach(var point in pointsOnFace)
                {
                    var refPoint = Doc.FamilyCreate.NewReferencePoint(point);
                }
                trans.Commit();
            }
        }
        #endregion

        #region Генератор параметров на поликривой
        private List<double> GenerateParameters(double bound1, double bound2, double inputStep)
        {
            var parameters = new List<double>
            { bound1 };

            double approxStep = UnitUtils.ConvertToInternalUnits(inputStep, UnitTypeId.Meters);

            int count = (int)(Math.Abs(bound2 - bound1) / approxStep + 1);

            double start = bound1;

            double step = (bound2 - bound1) / (count - 1);
            for (int i = 0; i < count - 2; i++)
            {
                parameters.Add(start + step);
                start += step;
            }

            parameters.Add(bound2);

            return parameters;
        }
        #endregion
    }
}

