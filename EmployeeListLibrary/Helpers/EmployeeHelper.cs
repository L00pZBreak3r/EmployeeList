using System;
using System.Collections.Generic;
using System.Linq;

using EmployeeListLibrary.Models;

namespace EmployeeListLibrary.Helpers
{
  public static class EmployeeHelper
  {
    /// <summary>
    /// Tests if aManagerCandidate is a subordinate of aEmployee.
    /// </summary>
    /// <param name="aEmployee"></param>
    /// <param name="aManagerCandidate"></param>
    /// <returns></returns>
    public static bool IsSubordinate(Employee aEmployee, Employee aManagerCandidate)
    {
      if (aEmployee != null)
      {
        if (ReferenceEquals(aManagerCandidate, aEmployee))
          return true;

        Employee aManager = aManagerCandidate?.Manager;
        while (aManager != null)
        {
          if (ReferenceEquals(aManager, aEmployee))
            return true;
          aManager = aManager.Manager;
        }
      }
      return false;
    }
  }
}
