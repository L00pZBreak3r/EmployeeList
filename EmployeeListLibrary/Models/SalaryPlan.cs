using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EmployeeListLibrary.Models
{
  /// <summary>
  /// Contains information for a salary plan.
  /// </summary>
  public class SalaryPlan
  {
    public enum EEmployeeType
    {
      Employee,
      Manager,
      Sales
    }

    public int Id { get; set; }
    [Required]
    public string Name { get; set; } // Salary plan name.
    [Required]
    public EEmployeeType EmployeeType { get; set; } // Salary plan type.
    [Required]
    public decimal BaseRate { get; set; } // Base wage rate.
    [Required]
    public double PercentAnnual { get; set; } // Salary percentage bonus for work-years. BaseRate is used for calculation.
    [Required]
    public double PercentAnnualLimit { get; set; } // Maximal limit for the work-years salary bonus.
    [Required]
    public double PercentSubs { get; set; } // Salary percentage bonus for management. Sum of the salaries of the manager's subordinates is used for calculation.
    [Required]
    public int PercentSubsLevel { get; set; } // Maximal level of subordination used for calculation of the management bonus. If <0 then unlimited.

    public virtual ICollection<Employee> Employees { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}
