using System.Windows;

using EmployeeList.Helpers;

namespace EmployeeList.ViewModels
{
  class MainWindowViewModel : ViewModelBase
  {
    private readonly Window mWindow;

    public MainWindowViewModel(Window win)
    {
      mWindow = win;
      mDocumentControlModel = new DocumentControlViewModel(mWindow);
    }

    #region DocumentControlModel

    private readonly DocumentControlViewModel mDocumentControlModel;
    public DocumentControlViewModel DocumentControlModel
    {
      get
      {
        return mDocumentControlModel;
      }
    }
    #endregion
  }
}
