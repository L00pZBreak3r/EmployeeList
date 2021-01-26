using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;

using EmployeeListLibrary.EF;
using EmployeeListLibrary.Models;
using EmployeeListLibrary.Helpers;

namespace EmployeeListLibrary
{

  public class EmployeeListDBConfiguration : IDisposable
  {
    /// <summary>
    /// Can be used for configurating three base salary plans (Employee, Manager, Sales).
    /// </summary>
    public class SalaryPlanConfiguration
    {
      public decimal BaseRateEmployee = 100;
      public decimal BaseRateManager = 100;
      public decimal BaseRateSales = 100;

      public double PercentAnnualEmployee = 3;
      public double PercentAnnualManager = 5;
      public double PercentAnnualSales = 1;

      public double PercentAnnualLimitEmployee = 30;
      public double PercentAnnualLimitManager = 40;
      public double PercentAnnualLimitSales = 35;

      public double PercentSubsManager = 0.5;
      public double PercentSubsSales = 0.3;

      public int PercentSubsLevelManager = 1; //N subordinate levels are used for salary calculation
      public int PercentSubsLevelSales = -1; //-1 = unlimited, all subordinate levels are used for salary calculation
    }

    /// <summary>
    /// Can be used for initializing the test data generator.
    /// </summary>
    public struct RandomEmployeeGenaratorConfiguration
    {
      public int EmployeeCount;
      public int ManagerCount;
      public int SalesCount;
    }

    public enum EEmployeeUpdateResult
    {
      Success,
      FailRequiredField,
      FailAlreadyExists,
      FailEmployeeType,
      FailManagerIsSubordinate
    }

    public enum EEmployeeUpdateOption
    {
      None,
      AddToDbSet,
      SaveChanges
    }

    private class DbSetClearTask<T> where T : class
    {
      private readonly DbContext mDbContext;
#if USE_INMEMORY_DATABASE
      private readonly DbSet<T> mDbSet;
#endif
      private readonly Action mAction;

      public DbSetClearTask(DbContext aDbContext,
#if USE_INMEMORY_DATABASE
        DbSet<T> aDbSet,
#endif
        Action aAction = null)
      {
        mDbContext = aDbContext;
#if USE_INMEMORY_DATABASE
        mDbSet = aDbSet;
#endif
        mAction = aAction;
      }

      public void ThreadProc()
      {
#if USE_INMEMORY_DATABASE
        mDbSet.RemoveRange(mDbSet.Take(8));
        mDbContext.SaveChanges();
        if (mDbSet.Any())
        {
          mDbSet.RemoveRange(mDbSet.Take(64));
          mDbContext.SaveChanges();
        }
        while (mDbSet.Any())
        {
          mDbSet.RemoveRange(mDbSet.Take(1024));
          mDbContext.SaveChanges();
        }
#else
        var aEntityType = mDbContext.Model.FindEntityType(typeof(T));
        string aTableName = aEntityType.GetTableName();
        mDbContext.Database.ExecuteSqlRaw("DELETE FROM " + aTableName);
        /*bool saved = false;
        while (!saved)
        {
          try
          {
            // Attempt to save changes to the database
            mDbContext.SaveChanges();
            saved = true;
          }
          catch (DbUpdateConcurrencyException ex)
          {
            foreach (var entry in ex.Entries)
            {
              if (entry.State == EntityState.Modified)
                entry.State = EntityState.Deleted;
            }
          }
        }*/
#endif
        mAction?.Invoke();
      }
    }

    private const string DATABASE_NAME_DEFAULT = "EmployeeListDB";
    private const long MINIMAL_DATE_TICKS = 627667488000000000L;
    private const int RANDOM_EMPLOYEE_COUNT_DEFAULT = 1024;
    private const int RANDOM_MANAGER_COUNT_DEFAULT = 128;
    private const int RANDOM_SALES_COUNT_DEFAULT = 128;
    private const int RANDOM_MAX_NAME_LENGTH = 10;

    private bool disposedValue;
    public readonly EmployeeListDBContext EmployeeListDB;
    public DateTime CompanyDateOfEstablishment;

    public EmployeeListDBConfiguration(SalaryPlanConfiguration aSalaryConfig = null, string aDatabaseName = null)
    {
      if (string.IsNullOrWhiteSpace(aDatabaseName))
        aDatabaseName = DATABASE_NAME_DEFAULT;

      if (aSalaryConfig == null)
        aSalaryConfig = new SalaryPlanConfiguration();

      var aSalaryPlan1 = new SalaryPlan
      {
        Name = "Employee",
        EmployeeType = SalaryPlan.EEmployeeType.Employee,
        BaseRate = aSalaryConfig.BaseRateEmployee,
        PercentAnnual = aSalaryConfig.PercentAnnualEmployee,
        PercentAnnualLimit = aSalaryConfig.PercentAnnualLimitEmployee,
        PercentSubs = 0,
        PercentSubsLevel = 0
      };

      var aSalaryPlan2 = new SalaryPlan
      {
        Name = "Manager",
        EmployeeType = SalaryPlan.EEmployeeType.Manager,
        BaseRate = aSalaryConfig.BaseRateManager,
        PercentAnnual = aSalaryConfig.PercentAnnualManager,
        PercentAnnualLimit = aSalaryConfig.PercentAnnualLimitManager,
        PercentSubs = aSalaryConfig.PercentSubsManager,
        PercentSubsLevel = aSalaryConfig.PercentSubsLevelManager
      };

      var aSalaryPlan3 = new SalaryPlan
      {
        Name = "Sales",
        EmployeeType = SalaryPlan.EEmployeeType.Sales,
        BaseRate = aSalaryConfig.BaseRateSales,
        PercentAnnual = aSalaryConfig.PercentAnnualSales,
        PercentAnnualLimit = aSalaryConfig.PercentAnnualLimitSales,
        PercentSubs = aSalaryConfig.PercentSubsSales,
        PercentSubsLevel = aSalaryConfig.PercentSubsLevelSales
      };

#if USE_INMEMORY_DATABASE
      var options = new DbContextOptionsBuilder<EmployeeListDBContext>()
       .UseInMemoryDatabase(databaseName: aDatabaseName)
       .Options;

      EmployeeListDB = new EmployeeListDBContext(options);

      EmployeeListDB.SalaryPlans.Add(aSalaryPlan1);
      EmployeeListDB.SalaryPlans.Add(aSalaryPlan2);
      EmployeeListDB.SalaryPlans.Add(aSalaryPlan3);
      EmployeeListDB.SaveChanges();
#else
      EmployeeListDB = new EmployeeListDBContext(aDatabaseName);
      EmployeeListDB.Database.EnsureCreated();
      
      var aSalaryPlans = EmployeeListDB.SalaryPlans;
      bool aRes = aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Employee) &&
        aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Manager) &&
        aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Sales);
      if (!aRes)
      {
        EmployeeListDB.SalaryPlans.Add(aSalaryPlan1);
        EmployeeListDB.SalaryPlans.Add(aSalaryPlan2);
        EmployeeListDB.SalaryPlans.Add(aSalaryPlan3);
        EmployeeListDB.SaveChanges();
      }
#endif

    }

    /// <summary>
    /// Fills table Employees with fake test data.
    /// </summary>
    /// <param name="aConfig">The test data generator config</param>
    /// <returns></returns>
    public void AddRandomEmployees(RandomEmployeeGenaratorConfiguration aConfig)
    {
      if (aConfig.EmployeeCount <= 0)
        aConfig.EmployeeCount = RANDOM_EMPLOYEE_COUNT_DEFAULT;

      if (aConfig.ManagerCount <= 0)
        aConfig.ManagerCount = RANDOM_MANAGER_COUNT_DEFAULT;

      if (aConfig.SalesCount <= 0)
        aConfig.SalesCount = RANDOM_SALES_COUNT_DEFAULT;

      long aMinDateTicks = CompanyDateOfEstablishment.Ticks;
      long aMaxDateTicks = DateTime.Now.Ticks;
      if (aMinDateTicks <= 0L)
        aMinDateTicks = MINIMAL_DATE_TICKS;

      var aSalaryPlans = EmployeeListDB.SalaryPlans;
      bool aRes = aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Employee) &&
        aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Manager) &&
        aSalaryPlans.Any(v => v.EmployeeType == SalaryPlan.EEmployeeType.Sales);

      if (aRes)
      {
        Random aRandom = new Random();

        var aManagers = new Employee[aConfig.ManagerCount];
        var aSalaryPlanId = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Manager).Id;
        for (int i = 0; i < aConfig.ManagerCount; i++)
        {
          var aEmployee = new Employee
          {
            LastName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            FirstName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            MiddleName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            EmploymentDate = (new DateTime(DateTimeHelper.GenerateRandomLong(aRandom, aMinDateTicks, aMaxDateTicks))).Date,
            SalaryPlanId = aSalaryPlanId
          };
          aManagers[i] = aEmployee;
        }

        var aSalesmen = new Employee[aConfig.SalesCount];
        aSalaryPlanId = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Sales).Id;
        for (int i = 0; i < aConfig.SalesCount; i++)
        {
          Employee aManager = null;
          if ((i > 1) && (aRandom.Next(2) == 0))
            aManager = (aRandom.Next(2) == 0) ? aManagers[aRandom.Next(aManagers.Length)] : aSalesmen[aRandom.Next(i)];

          var aEmployee = new Employee
          {
            LastName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            FirstName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            MiddleName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            EmploymentDate = (new DateTime(DateTimeHelper.GenerateRandomLong(aRandom, aMinDateTicks, aMaxDateTicks))).Date,
            SalaryPlanId = aSalaryPlanId,
            Manager = aManager
          };
          aSalesmen[i] = aEmployee;
        }

        var aEmployees = new Employee[aConfig.EmployeeCount];
        aSalaryPlanId = aSalaryPlans.First(v => v.EmployeeType == SalaryPlan.EEmployeeType.Employee).Id;
        for (int i = 0; i < aConfig.EmployeeCount; i++)
        {
          Employee aManager = null;
          if (aRandom.Next(2) == 0)
            aManager = (aRandom.Next(2) == 0) ? aManagers[aRandom.Next(aManagers.Length)] : aSalesmen[aRandom.Next(aSalesmen.Length)];

          var aEmployee = new Employee
          {
            LastName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            FirstName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            MiddleName = StringHelper.GenerateRandomString(aRandom, RANDOM_MAX_NAME_LENGTH),
            EmploymentDate = (new DateTime(DateTimeHelper.GenerateRandomLong(aRandom, aMinDateTicks, aMaxDateTicks))).Date,
            SalaryPlanId = aSalaryPlanId,
            Manager = aManager
          };
          aEmployees[i] = aEmployee;
        }

        for (int i = 0; i < aManagers.Length; i++)
        {
          Employee aManager = null;
          if (aRandom.Next(2) == 0)
          {
            aManager = (aRandom.Next(2) == 0) ? aManagers[aRandom.Next(aManagers.Length)] : aSalesmen[aRandom.Next(aSalesmen.Length)];
            if (!EmployeeHelper.IsSubordinate(aManagers[i], aManager))
              aManagers[i].Manager = aManager;
          }

          EmployeeListDB.Employees.Add(aManagers[i]);
        }

        EmployeeListDB.Employees.AddRange(aSalesmen);
        EmployeeListDB.Employees.AddRange(aEmployees);

        EmployeeListDB.SaveChanges();
      }
      else
      {
        throw new Exception("Database contains 0 SalaryPlans.");
      }
    }

    /// <summary>
    /// Creates a new Employee entity performing all necessary validations.
    /// </summary>
    /// <param name="aNewEmployee">Newly created Employee</param>
    /// <param name="aUpdateOption">Do nothing, Add to table Employees, Add to table Employees and do SaveChanges</param>
    /// <param name="aSalaryPlanId">SalaryPlan Id</param>
    /// <param name="aLastName">Last name</param>
    /// <param name="aFirstName">First name</param>
    /// <param name="aMiddleName">Middle name</param>
    /// <param name="aEmploymentDate">Employment date</param>
    /// <param name="aManager">Manager entity</param>
    /// <returns>Success, A required field is incorrect, Employee with the same name already exists</returns>
    public EEmployeeUpdateResult CreateNewEmployee(out Employee aNewEmployee, EEmployeeUpdateOption aUpdateOption, int aSalaryPlanId, string aLastName, string aFirstName, string aMiddleName, DateTime aEmploymentDate, Employee aManager = null)
    {
      EEmployeeUpdateResult res = EEmployeeUpdateResult.FailRequiredField;
      aNewEmployee = null;
      if ((aSalaryPlanId > 0) && !string.IsNullOrWhiteSpace(aLastName) && !string.IsNullOrWhiteSpace(aFirstName) && (aEmploymentDate >= CompanyDateOfEstablishment))
      {
        SalaryPlan aSalaryPlan = EmployeeListDB.SalaryPlans.Find(aSalaryPlanId);
        if (aSalaryPlan != null)
        {
          res = EEmployeeUpdateResult.FailAlreadyExists;
          var aEmployee = new Employee
          {
            LastName = aLastName,
            FirstName = aFirstName,
            MiddleName = aMiddleName,
            EmploymentDate = aEmploymentDate,
            SalaryPlanId = aSalaryPlanId
          };
          if (!EmployeeListDB.Employees.Any(v => v.Equals(aEmployee)))
          {
            aEmployee.Manager = aManager;
            aNewEmployee = aEmployee;

            if (aUpdateOption > EEmployeeUpdateOption.None)
            {
              EmployeeListDB.Employees.Add(aEmployee);
              if (aUpdateOption == EEmployeeUpdateOption.SaveChanges)
                EmployeeListDB.SaveChanges();
            }
            res = EEmployeeUpdateResult.Success;
          }
        }
      }
      return res;
    }

    public EEmployeeUpdateResult CreateNewEmployee(out Employee aNewEmployee, EEmployeeUpdateOption aUpdateOption, int aSalaryPlanId, string aLastName, string aFirstName, string aMiddleName, DateTime aEmploymentDate, int aManagerId)
    {
      Employee aManager = null;
      if (aManagerId > 0)
        aManager = EmployeeListDB.Employees.Find(aManagerId);
      return CreateNewEmployee(out aNewEmployee, aUpdateOption, aSalaryPlanId, aLastName, aFirstName, aMiddleName, aEmploymentDate, aManager);
    }

    public EEmployeeUpdateResult CreateNewEmployee(out Employee aNewEmployee, EEmployeeUpdateOption aUpdateOption, int aSalaryPlanId, string aLastName, string aFirstName, string aMiddleName = null)
    {
      return CreateNewEmployee(out aNewEmployee, aUpdateOption, aSalaryPlanId, aLastName, aFirstName, aMiddleName, DateTime.Now);
    }

    /// <summary>
    /// Updates Employee entity performing all necessary validations.
    /// </summary>
    /// <param name="aEmployee">Employee entity to update</param>
    /// <param name="aUpdateOption">Do nothing, Do SaveChanges</param>
    /// <param name="aSalaryPlanId">SalaryPlan Id, if <= 0 then unchanged</param>
    /// <param name="aLastName">Last name, if null then unchanged</param>
    /// <param name="aFirstName">First name, if null then unchanged</param>
    /// <param name="aMiddleName">Middle name, if null then unchanged, if empty then set to null</param>
    /// <param name="aEmploymentDate">Employment date, if zero then unchanged</param>
    /// <param name="aManager">Manager entity, if null then set to null</param>
    /// <returns>Success, A required field is incorrect, EmployeeType is Employee but this Employee has subordinates, Manager is already this Employee's subordinate</returns>
    public EEmployeeUpdateResult UpdateEmployee(Employee aEmployee, EEmployeeUpdateOption aUpdateOption, int aSalaryPlanId, string aLastName, string aFirstName, string aMiddleName, DateTime aEmploymentDate, Employee aManager)
    {
      if (aSalaryPlanId <= 0)
        aSalaryPlanId = aEmployee.SalaryPlanId;
      aLastName ??= aEmployee.LastName;
      aFirstName ??= aEmployee.FirstName;
      if (aMiddleName == null)
        aMiddleName = aEmployee.MiddleName;
      else if (aMiddleName.Length == 0)
        aMiddleName = null;
      if (aEmploymentDate.Ticks <= 0L)
        aEmploymentDate = aEmployee.EmploymentDate;
      EEmployeeUpdateResult res = EEmployeeUpdateResult.FailRequiredField;
      if ((aSalaryPlanId > 0) && !string.IsNullOrWhiteSpace(aLastName) && !string.IsNullOrWhiteSpace(aFirstName) && (aEmploymentDate >= CompanyDateOfEstablishment))
      {
        SalaryPlan aSalaryPlan = EmployeeListDB.SalaryPlans.Find(aSalaryPlanId);
        if (aSalaryPlan != null)
        {
          res = EEmployeeUpdateResult.FailEmployeeType;
          if ((aSalaryPlan.EmployeeType > SalaryPlan.EEmployeeType.Employee) || (aEmployee.Subordinates == null) || !aEmployee.Subordinates.Any())
          {
            res = EEmployeeUpdateResult.FailManagerIsSubordinate;
            if ((aManager == null) || !EmployeeHelper.IsSubordinate(aEmployee, aManager))
            {
              aEmployee.LastName = aLastName;
              aEmployee.FirstName = aFirstName;
              aEmployee.MiddleName = aMiddleName;
              aEmployee.EmploymentDate = aEmploymentDate;
              aEmployee.SalaryPlanId = aSalaryPlanId;
              aEmployee.Manager = aManager;

              if (aUpdateOption == EEmployeeUpdateOption.SaveChanges)
                EmployeeListDB.SaveChanges();
              res = EEmployeeUpdateResult.Success;
            }
          }
        }
      }
      return res;
    }

    /// <summary>
    /// Updates Employee entity performing all necessary validations.
    /// </summary>
    /// <param name="aEmployee">Employee entity to update</param>
    /// <param name="aUpdateOption">Do nothing, Do SaveChanges</param>
    /// <param name="aSalaryPlanId">SalaryPlan Id, if <= 0 then unchanged</param>
    /// <param name="aLastName">Last name, if null then unchanged</param>
    /// <param name="aFirstName">First name, if null then unchanged</param>
    /// <param name="aMiddleName">Middle name, if null then unchanged, if empty then set to null</param>
    /// <param name="aEmploymentDate">Employment date, if zero then unchanged</param>
    /// <param name="aManagerId">Manager Id, if <0 then unchanged, if 0 then set to null</param>
    /// <returns>Success, A required field is incorrect, EmployeeType is Employee but this Employee has subordinates, Manager is already this Employee's subordinate</returns>
    public EEmployeeUpdateResult UpdateEmployee(Employee aEmployee, EEmployeeUpdateOption aUpdateOption, int aSalaryPlanId, string aLastName, string aFirstName, string aMiddleName, DateTime aEmploymentDate, int aManagerId = -1)
    {
      Employee aManager = null;
      if (aManagerId > 0)
        aManager = EmployeeListDB.Employees.Find(aManagerId);
      else if (aManagerId < 0)
        aManager = aEmployee.Manager;
      return UpdateEmployee(aEmployee, aUpdateOption, aSalaryPlanId, aLastName, aFirstName, aMiddleName, aEmploymentDate, aManager);
    }

    public EEmployeeUpdateResult UpdateEmployee(Employee aEmployee, EEmployeeUpdateOption aUpdateOption, int aSalaryPlanId, string aLastName, string aFirstName, string aMiddleName = null)
    {
      return UpdateEmployee(aEmployee, aUpdateOption, aSalaryPlanId, aLastName, aFirstName, aMiddleName, new DateTime(), aEmployee.Manager);
    }

    /// <summary>
    /// Clears table Employees.
    /// </summary>
    /// <param name="aAction">Action invoked after the task is complete</param>
    /// <param name="aRunInThisThread">If true then run in the current thread</param>
    public void ClearEmployees(Action aAction, bool aRunInThisThread = false)
    {
      EmployeeListDB.Employees.AsParallel().ForAll(v => { v.ManagerId = null; });
      EmployeeListDB.SaveChanges();

      DbSetClearTask<Employee> aTask = new DbSetClearTask<Employee>(EmployeeListDB,
#if USE_INMEMORY_DATABASE
        EmployeeListDB.Employees,
#endif
        aAction);

      Thread t = new Thread(new ThreadStart(aTask.ThreadProc));
      t.Start();
      if (aRunInThisThread)
        t.Join();
    }

    /// <summary>
    /// Calculates salary.
    /// </summary>
    /// <param name="aRecalc">Recalculate salary for all employees</param>
    /// <param name="aDate">Salary date</param>
    /// <param name="aEmployee">The employee whose salary is calculated. If null then calculate salary for all the employees.</param>
    /// <returns></returns>
    public decimal CalcTotalSalary(bool aRecalc, DateTime aDate, Employee aEmployee = null)
    {
      decimal res = 0m;
      if (aRecalc)
        EmployeeListDB.Employees.AsParallel().ForAll(v => { SalaryHelper.CalcEmployeeSalary(aDate, v); });
      if (aEmployee == null)
      {
        foreach (var v in EmployeeListDB.Employees)
          SalaryHelper.CalcEmployeeSalaryWithSubordinates(aDate, v);

        res = EmployeeListDB.Employees.AsEnumerable().Sum(v => v.Salary);
      }
      else
        res = SalaryHelper.CalcEmployeeSalaryWithSubordinates(aDate, aEmployee);
      return res;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          EmployeeListDB.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~EmployeeListDBConfiguration()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
