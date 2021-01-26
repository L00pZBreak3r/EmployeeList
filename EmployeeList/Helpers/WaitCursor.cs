using System;
using System.Windows.Input;

namespace EmployeeList.Helpers
{
  class WaitCursor : IDisposable
  {
    private readonly Cursor mPreviousCursor;

    public WaitCursor(Cursor aCursor = null)
    {
      if (aCursor == null)
        aCursor = Cursors.Wait;

      mPreviousCursor = Mouse.OverrideCursor;

      Mouse.OverrideCursor = aCursor;
    }

    #region IDisposable Members

    public void Dispose()
    {
      Mouse.OverrideCursor = mPreviousCursor;
    }

    #endregion
  }
}
