using System;
using System.Collections.Generic;
using System.Linq;

using EmployeeListLibrary.Models;

namespace EmployeeListLibrary.Helpers
{
  static class SalaryHelper
  {
    private static readonly TimeSpan mYearInterval = new TimeSpan(365, 0, 0, 0);

    /// <summary>
    /// Sums salaries of all aEmployee's subordinates down to the specified level.
    /// </summary>
    /// <param name="aEmployee"></param>
    /// <param name="aLevel">Maximum subordination level which salaries should be taken into account.</param>
    /// <returns></returns>
    private static double GetSubordinateSalarySum(Employee aEmployee, int aLevel)
    {
      double res = (double)aEmployee.Salary;
      if (aLevel > 0)
      {
        var aSubs = aEmployee.Subordinates;
        if (aSubs != null)
        {
          res += aSubs.Sum(v => GetSubordinateSalarySum(v, aLevel - 1));
        }
      }
      return res;
    }

    /// <summary>
    /// Adjusts aEmployee's Salary based on his/her subordinates' salaries if it's necessary.
    /// </summary>
    /// <param name="aDate"></param>
    /// <param name="aEmployee"></param>
    /// <returns></returns>
    public static decimal CalcEmployeeSalaryWithSubordinates(DateTime aDate, Employee aEmployee)
    {
      decimal res = aEmployee.Salary;
      if (res < 0m)
        res = CalcEmployeeSalary(aDate, aEmployee);
      double aSubsTotalSalary = aEmployee.SubordinatesTotalSalary;
      if (aSubsTotalSalary < 0d)
      {
        aSubsTotalSalary = 0d;
        var aSubs = aEmployee.Subordinates;
        if (aSubs != null)
        {
          foreach (var v in aSubs)
            CalcEmployeeSalaryWithSubordinates(aDate, v);

          aSubsTotalSalary = aSubs.Sum(v => v.SubordinatesTotalSalary + (double)v.Salary);

          var aSalaryPlan = aEmployee.SalaryPlan;
          double aSubPercent = aSalaryPlan.PercentSubs;
          int aSubMaxLevel = aSalaryPlan.PercentSubsLevel;
          if ((aSubPercent > 0d) && (aSubMaxLevel != 0))
          {
            double aSubSum = aSubsTotalSalary;
            if (aSubMaxLevel > 0)
            {
              aSubSum = aSubs.Sum(v => GetSubordinateSalarySum(v, aSubMaxLevel - 1));
            }
            res += (decimal)(aSubSum * aSubPercent / 100.0);
            aEmployee.Salary = res;
          }
        }
        aEmployee.SubordinatesTotalSalary = aSubsTotalSalary;
      }
      return res;
    }

    /// <summary>
    /// Calculates Salary for aEmployee based on his/her EmploymentDate
    /// </summary>
    /// <param name="aDate">Salary date</param>
    /// <param name="aEmployee"></param>
    /// <returns></returns>
    public static decimal CalcEmployeeSalary(DateTime aDate, Employee aEmployee)
    {
      int aYearCount = (int)((aDate - aEmployee.EmploymentDate) / mYearInterval);
      if (aYearCount < 0)
        aYearCount = 0;
      var aSalaryPlan = aEmployee.SalaryPlan;
      decimal aSalary = aSalaryPlan.BaseRate;
      double aPercentAnnual = aSalaryPlan.PercentAnnual * aYearCount;
      double aPercentAnnualLimit = aSalaryPlan.PercentAnnualLimit;
      if (aPercentAnnual > aPercentAnnualLimit)
        aPercentAnnual = aPercentAnnualLimit;
      if (aPercentAnnual > 0d)
        aSalary *= (decimal)(1.0 + aPercentAnnual / 100.0);
      aEmployee.Salary = aSalary;
      aEmployee.SubordinatesTotalSalary = -1.0;
      return aSalary;
    }

  }
}
