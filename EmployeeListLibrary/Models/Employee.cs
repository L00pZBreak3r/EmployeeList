using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using EmployeeListLibrary.Helpers;

namespace EmployeeListLibrary.Models
{
  /// <summary>
  /// Contains information for a employee.
  /// </summary>
  public class Employee : IEquatable<Employee>
  {
    public int Id { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    [Required]
    public DateTime EmploymentDate { get; set; }
    [Required]
    public int SalaryPlanId { get; set; }

    public virtual SalaryPlan SalaryPlan { get; set; }

    public int? ManagerId { get; set; }

    public virtual Employee Manager { get; set; }
    public virtual ICollection<Employee> Subordinates { get; }
    
    [NotMapped]
    public decimal Salary { get; set; } = -1m;
    [NotMapped]
    public double SubordinatesTotalSalary { get; set; } = -1.0;

    public bool Equals(Employee other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return (StringHelper.CompareNames(LastName, other.LastName, true) == 0) && (StringHelper.CompareNames(FirstName, other.FirstName, true) == 0) && (StringHelper.CompareNames(MiddleName, other.MiddleName, true) == 0);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals(obj as Employee);
    }

    public override int GetHashCode()
    {
      return (LastName?.GetHashCode() ?? 0) | (FirstName?.GetHashCode() ?? 0) | (MiddleName?.GetHashCode() ?? 0);
    }

    public override string ToString()
    {
      return string.Join(' ', LastName, FirstName, MiddleName);
    }
  }
}
