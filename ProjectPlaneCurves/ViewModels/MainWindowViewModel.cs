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
using System.Windows.Input;
using ProjectPlaneCurves.Infrastructure;

namespace ProjectPlaneCurves.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Проецировать линии";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Линии для проецирования в плане
        private string _planeCurvesElemIds;

        public string PlaneCurvesElemIds
        {
            get => _planeCurvesElemIds;
            set => Set(ref _planeCurvesElemIds, value);
        }
        #endregion

        #region Грань для проецирования
        private string _faceRepresentation;

        public string FaceRepresentation
        {
            get => _faceRepresentation;
            set => Set(ref _faceRepresentation, value);
        }
        #endregion

        #region Команды

        #region Получение линий для проецирования
        public ICommand GetPlaneCurvesCommand { get; }

        private void OnGetPlaneCurvesCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetPlaneCurvesBySelection();
            PlaneCurvesElemIds = RevitModel.PlaneCurvesElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetPlaneCurvesCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение грани
        public ICommand GetFaceCommand { get; }

        private void OnGetFaceCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetFaceBySelection();
            FaceRepresentation = RevitModel.FaceRepresentation;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetFaceCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Тест - получение адаптивных точек на грани
        public ICommand CreateReferencePointsOnFaceCommand { get; }

        private void OnCreateReferencePointsOnFaceCommandExecuted(object parameter)
        {
            RevitModel.CreateAdaptivePointsOnFace();
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCreateReferencePointsOnFaceCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрытие окна
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            SaveSettings();
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default["PlaneCurvesElemIds"] = PlaneCurvesElemIds;
            Properties.Settings.Default["FaceRepresentation"] = FaceRepresentation;
            Properties.Settings.Default.Save();
        }

        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация значения элементам линий на плане
            if (!(Properties.Settings.Default["PlaneCurvesElemIds"] is null))
            {
                string planeCurvesElementIdInSettings = Properties.Settings.Default["PlaneCurvesElemIds"].ToString();
                if(RevitModel.IsPlaneCurvesExistInModel(planeCurvesElementIdInSettings) && !string.IsNullOrEmpty(planeCurvesElementIdInSettings))
                {
                    PlaneCurvesElemIds = planeCurvesElementIdInSettings;
                    RevitModel.GetPlaneLinesBySettings(planeCurvesElementIdInSettings);
                }
            }
            #endregion

            #region Инициализация значения грани
            if (!(Properties.Settings.Default["FaceRepresentation"] is null))
            {
                string faceRepresentation = Properties.Settings.Default["FaceRepresentation"].ToString();
                if(RevitModel.IsFaceExistInModel(faceRepresentation) && !string.IsNullOrEmpty(faceRepresentation))
                {
                    FaceRepresentation = faceRepresentation;
                    RevitModel.GetFaceBySetings(faceRepresentation);
                }
            }
            #endregion

            #region Команды
            GetPlaneCurvesCommand = new LambdaCommand(OnGetPlaneCurvesCommandExecuted, CanGetPlaneCurvesCommandExecute);
            GetFaceCommand = new LambdaCommand(OnGetFaceCommandExecuted, CanGetFaceCommandExecute);
            CreateReferencePointsOnFaceCommand = new LambdaCommand(OnCreateReferencePointsOnFaceCommandExecuted,
                                                                   CanCreateReferencePointsOnFaceCommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel()
        { }

        #endregion
    }
}
