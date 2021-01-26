using NUnit.Framework;
using System;
using System.Linq;

using EmployeeListLibrary;
using EmployeeListLibrary.Models;

namespace Tests
{

  public class Tests
  {
    private EmployeeListDBConfiguration mEmployeeListDBConfiguration;

    [SetUp]
    public void Setup()
    {
      mEmployeeListDBConfiguration = new EmployeeListDBConfiguration();
    }

    [Test]
    public void Test_BasicSalaryPlansAvailable()
    {
      var aSalaryPlans = mEmployeeListDBConfiguration.EmployeeListDB.SalaryPlans;
      bool aRes = aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Employee) &&
        aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Manager) &&
        aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Sales);
      Assert.IsTrue(aRes, "Basic salary plans configuration is incorrect");
    }

    [Test]
    public void Test_CalcSalaryForWorkYears()
    {
      mEmployeeListDBConfiguration.ClearEmployees(null, true);

      var aSalaryPlans = mEmployeeListDBConfiguration.EmployeeListDB.SalaryPlans;
      var aSalaryPlanEmployee = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Employee);
      var aSalaryPlanManager = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Manager);
      var aSalaryPlanSales = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Sales);

      const int aMaxCount = 2;
      Employee[] aEmps = new Employee[aMaxCount * 3];
      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "e" + i.ToString(),
          FirstName = "e" + i.ToString(),
          MiddleName = "e" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanEmployee.Id
        };
        aEmps[i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }

      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "m" + i.ToString(),
          FirstName = "m" + i.ToString(),
          MiddleName = "m" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanManager.Id
        };
        aEmps[aMaxCount + i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }

      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "s" + i.ToString(),
          FirstName = "s" + i.ToString(),
          MiddleName = "s" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanSales.Id
        };
        aEmps[aMaxCount * 2 + i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }

      mEmployeeListDBConfiguration.EmployeeListDB.SaveChanges();

      var aTotal = mEmployeeListDBConfiguration.CalcTotalSalary(false, new DateTime(2020, 1, 1));
      bool aRes = (aEmps[0].Salary == 130m) &&
        (aEmps[1].Salary == 106m) &&
        (aEmps[2].Salary == 140m) &&
        (aEmps[3].Salary == 110m) &&
        (aEmps[4].Salary == 112m) &&
        (aEmps[5].Salary == 102m);

      Assert.IsTrue(aRes, "Salary calculation based on work-years failed");
    }

    [Test]
    public void Test_SalaryWithSubordinates()
    {
      mEmployeeListDBConfiguration.ClearEmployees(null, true);

      var aSalaryPlans = mEmployeeListDBConfiguration.EmployeeListDB.SalaryPlans;
      var aSalaryPlanEmployee = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Employee);
      var aSalaryPlanManager = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Manager);
      var aSalaryPlanSales = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Sales);

      const int aMaxCount = 2;
      Employee[] aEmps = new Employee[aMaxCount * 4];
      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "e" + i.ToString(),
          FirstName = "e" + i.ToString(),
          MiddleName = "e" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanEmployee.Id
        };
        aEmps[i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }

      Employee aManager2 = null;
      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "m" + i.ToString(),
          FirstName = "m" + i.ToString(),
          MiddleName = "m" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanManager.Id,
          Manager = aManager2
        };
        aManager2 = aEmployee;
        aEmps[aMaxCount + i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }

      aManager2 = null;
      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "s" + i.ToString(),
          FirstName = "s" + i.ToString(),
          MiddleName = "s" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanSales.Id,
          Manager = aManager2
        };
        aManager2 = aEmployee;
        aEmps[aMaxCount * 2 + i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }
      for (int i = 0; i < aMaxCount; i++)
      {
        var aEmployee = new Employee
        {
          LastName = "sa" + i.ToString(),
          FirstName = "sa" + i.ToString(),
          MiddleName = "sa" + i.ToString(),
          EmploymentDate = new DateTime(2008 + i * 10, 1, 1),
          SalaryPlanId = aSalaryPlanSales.Id,
          Manager = aManager2
        };
        aManager2 = aEmployee;
        aEmps[aMaxCount * 3 + i] = aEmployee;
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Add(aEmployee);
      }

      mEmployeeListDBConfiguration.EmployeeListDB.SaveChanges();

      var aTotal = mEmployeeListDBConfiguration.CalcTotalSalary(false, new DateTime(2020, 1, 1));
      bool aRes = (aEmps[0].Salary == 130m) &&
        (aEmps[1].Salary == 106m) &&
        (aEmps[2].Salary == 140m + 110m * 0.5m / 100m) &&
        (aEmps[3].Salary == 110m) &&
        (aEmps[4].Salary == 112m + (102m + 112m + 102m * 0.3m / 100m + 102m + (102m + 112m + 102m * 0.3m / 100m) * 0.3m / 100m) * 0.3m / 100m) &&
        (aEmps[5].Salary == 102m + (102m + 112m + 102m * 0.3m / 100m) * 0.3m / 100m) &&
        (aEmps[6].Salary == 112m + 102m * 0.3m / 100m) &&
        (aEmps[7].Salary == 102m) &&
        (aEmps[0].Salary +
        aEmps[1].Salary +
        aEmps[2].Salary +
        aEmps[3].Salary +
        aEmps[4].Salary +
        aEmps[5].Salary +
        aEmps[6].Salary +
        aEmps[7].Salary == aTotal);

      Assert.IsTrue(aRes, "Salary calculation based on subordinates failed");
    }

  }
}