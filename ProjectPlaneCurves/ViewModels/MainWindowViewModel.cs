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

        #region Закрытие окна
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Команды
            GetPlaneCurvesCommand = new LambdaCommand(OnGetPlaneCurvesCommandExecuted, CanGetPlaneCurvesCommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel()
        { }
        #endregion
    }
}
