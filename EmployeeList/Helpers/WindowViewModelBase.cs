using System.Windows;

namespace EmployeeList.Helpers
{
  internal class WindowViewModelBase : ViewModelBase
  {
    protected readonly Window mWindow;

    public WindowViewModelBase(Window win)
    {
      mWindow = win;
    }

    #region CloseWindow

    public virtual void CloseWindow(bool setDialogResult = false, bool dialogResult = false)
    {
      if (mWindow != null)
      {
        if (setDialogResult)
          mWindow.DialogResult = dialogResult;
        else
          mWindow.Close();
      }
    }

    #endregion
  }
}
