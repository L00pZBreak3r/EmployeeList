using System;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

using EmployeeList.Helpers;

using EmployeeListLibrary;
using EmployeeListLibrary.Models;
using EmployeeListLibrary.Helpers;

namespace EmployeeList.ViewModels
{
  internal class DocumentControlViewModel : WindowViewModelBase, IDataErrorInfo
  {
    private enum EDbActionResult
    {
      Nothing,
      SuccessAdd,
      SuccessChange,
      FailAlreadyExists,
      FailEmployeeType,
      FailManagerIsSubordinate
    }

    private const string ERROR_MESSAGE_MANAGER_CONFLICT = "Кандидат в начальники уже является подчиненным данного сотрудника.";
    private const string SUCCESS_MESSAGE_DATABASE_FILLED = "База данных заполнена.";
    private const string SUCCESS_MESSAGE_DATABASE_EMPTY = "База данных очищена.";
    private const string TEXT_SALARY_FOR_ALL_EMPLOYEES = "Всех сотрудников";

    private readonly EmployeeListDBConfiguration mEmployeeListDBConfiguration;

    public DocumentControlViewModel(Window win)
      : base(win)
    {
      OkCommand = new RelayCommand(OnOkCommand, OkCommandCanExecute);
      CancelCommand = new RelayCommand(OnCancelCommand);

      mResultSalaryDate = DateTime.Now;
      mResultSalaryDateString = mResultSalaryDate.ToString("d");
      mResultSalaryDateChanged = true;
      
      mEmployeeListDBConfiguration = new EmployeeListDBConfiguration();
      mSelectedSalaryPlanItem = mEmployeeListDBConfiguration.EmployeeListDB.SalaryPlans.First();
    }

    #region OkCommand

    public ICommand OkCommand { get; set; }

    private bool OkCommandCanExecute(object p)
    {
      return string.IsNullOrEmpty(Error);
    }

    private void OnOkCommand(object p)
    {
      EDbActionResult res = AddNewEmployee();
      if ((res == EDbActionResult.SuccessAdd) || (res == EDbActionResult.SuccessChange))
      {
        OperationText = (res == EDbActionResult.SuccessAdd) ? "Сотрудник добавлен в базу." : "Данные сохранены.";
        SetOperationTextColor(Colors.Green);
      }
      else if (res == EDbActionResult.FailAlreadyExists)
      {
        OperationText = "Такой сотрудник уже занесен в базу.";
        SetOperationTextColor(Colors.Red);
      }
      else if (res == EDbActionResult.FailEmployeeType)
      {
        OperationText = "Установка категории Employee невозможна: у сотрудника есть подчиненные.";
        SetOperationTextColor(Colors.Red);
      }
      else if (res == EDbActionResult.FailManagerIsSubordinate)
      {
        OperationText = ERROR_MESSAGE_MANAGER_CONFLICT;
        SetOperationTextColor(Colors.Red);
      }
      else
      {
        OperationText = "";
        SetOperationTextColor(Colors.Black);
      }
    }

    #endregion

    #region CancelCommand

    public ICommand CancelCommand { get; set; }

    private void OnCancelCommand(object p)
    {
      CloseWindow();
    }

    #endregion

    #region SalaryPlanItemsSource

    //private ObservableCollection<SalaryPlan> mSalaryPlanItemsSource;
    public ObservableCollection<SalaryPlan> SalaryPlanItemsSource
    {
      get { return new ObservableCollection<SalaryPlan>(mEmployeeListDBConfiguration.EmployeeListDB.SalaryPlans); }
    }

    #endregion

    #region SelectedSalaryPlanItem

    private SalaryPlan mSelectedSalaryPlanItem;

    public SalaryPlan SelectedSalaryPlanItem
    {
      get { return mSelectedSalaryPlanItem; }
      set
      {
        if (!ReferenceEquals(mSelectedSalaryPlanItem, value))
        {
          mSelectedSalaryPlanItem = value;
          RaisePropertyChanged();
          RaisePropertyChanged("SubordinatesFieldVisible");
        }
      }
    }

    #endregion

    #region EmployeesItemsSource

    private ObservableCollection<Employee> mEmployeesItemsSource;
    public ObservableCollection<Employee> EmployeesItemsSource
    {
      get { return mEmployeesItemsSource; }
    }

    #endregion

    #region SelectedEmployeesItem

    private Employee mSelectedEmployeesItem;

    public Employee SelectedEmployeesItem
    {
      get { return mSelectedEmployeesItem; }
      set
      {
        if (!ReferenceEquals(mSelectedEmployeesItem, value))
        {
          OperationText = "";
          mResultSalary = 0m;
          mSelectedEmployeesItem = value;
          mLastName = mSelectedEmployeesItem?.LastName;
          mFirstName = mSelectedEmployeesItem?.FirstName;
          mMiddleName = mSelectedEmployeesItem?.MiddleName;
          mSalarySubjectName = (mResultSalaryForAll) ? TEXT_SALARY_FOR_ALL_EMPLOYEES : mSelectedEmployeesItem?.ToString();
          mEmploymentDate = mSelectedEmployeesItem?.EmploymentDate ?? DateTime.Now;
          mEmploymentDateString = mEmploymentDate.ToString("d");
          mSelectedManagerItem = mSelectedEmployeesItem?.Manager;
          mSelectedSalaryPlanItem = mSelectedEmployeesItem?.SalaryPlan;
          RaisePropertyChanged();
          RaisePropertyChanged("SubordinatesItemsSource");
          RaisePropertyChanged("LastName");
          RaisePropertyChanged("FirstName");
          RaisePropertyChanged("MiddleName");
          RaisePropertyChanged("EmploymentDate");
          RaisePropertyChanged("SelectedManagerItem");
          RaisePropertyChanged("SelectedSalaryPlanItem");
          RaisePropertyChanged("SubordinatesFieldVisible");
          RaisePropertyChanged("SalarySubjectName");
          RaisePropertyChanged("ResultSalary");
        }
      }
    }

    #endregion

    #region SelectedManagerItem

    private Employee mSelectedManagerItem;

    public Employee SelectedManagerItem
    {
      get { return mSelectedManagerItem; }
      set
      {
        if (!ReferenceEquals(mSelectedManagerItem, value))
        {
          mSelectedManagerItem = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region SubordinatesItemsSource

    //private ObservableCollection<Employee> mSubordinatesItemsSource;
    public ObservableCollection<Employee> SubordinatesItemsSource
    {
      get { return (mSelectedEmployeesItem?.Subordinates != null) ? new ObservableCollection<Employee>(mSelectedEmployeesItem.Subordinates) : null; }
    }

    #endregion

    #region SelectedSubordinatesItem

    private Employee mSelectedSubordinatesItem;

    public Employee SelectedSubordinatesItem
    {
      get { return mSelectedSubordinatesItem; }
      set
      {
        if (!ReferenceEquals(mSelectedSubordinatesItem, value))
        {
          mSelectedSubordinatesItem = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    private void ClearFields()
    {
      OperationText = "";
      mResultSalaryDateChanged = true;      
      mResultSalary = 0m;
      mSelectedEmployeesItem = null;
      mLastName = null;
      mFirstName = null;
      mMiddleName = null;
      mSalarySubjectName = (mResultSalaryForAll) ? TEXT_SALARY_FOR_ALL_EMPLOYEES : null;
      mEmploymentDate = DateTime.Now;
      mEmploymentDateString = null;
      mSelectedManagerItem = null;
      mSelectedSalaryPlanItem = null;
      RaisePropertyChanged("EmployeesItemsSource");
      RaisePropertyChanged("SelectedEmployeesItem");
      RaisePropertyChanged("SubordinatesItemsSource");
      RaisePropertyChanged("LastName");
      RaisePropertyChanged("FirstName");
      RaisePropertyChanged("MiddleName");
      RaisePropertyChanged("EmploymentDate");
      RaisePropertyChanged("SelectedManagerItem");
      RaisePropertyChanged("SelectedSalaryPlanItem");
      RaisePropertyChanged("SubordinatesFieldVisible");
      RaisePropertyChanged("SalarySubjectName");
      RaisePropertyChanged("ResultSalary");
    }

    public void ClearManager()
    {
      mSelectedManagerItem = null;
      RaisePropertyChanged("SelectedManagerItem");
    }

    private void ClearEmployeesCompleteAction()
    {
      mWindow.Dispatcher.Invoke(() =>
      {
        SetOperationTextColor(Colors.Green);
        OperationText = SUCCESS_MESSAGE_DATABASE_EMPTY;
      });
    }

    public void FillDb()
    {
      EmployeeListDBConfiguration.RandomEmployeeGenaratorConfiguration aConfig = new EmployeeListDBConfiguration.RandomEmployeeGenaratorConfiguration();
      mEmployeeListDBConfiguration.AddRandomEmployees(aConfig);
      mResultSalaryDateChanged = true;
      mEmployeesItemsSource = new ObservableCollection<Employee>(mEmployeeListDBConfiguration.EmployeeListDB.Employees);
      RaisePropertyChanged("EmployeesItemsSource");
      SetOperationTextColor(Colors.Green);
      OperationText = SUCCESS_MESSAGE_DATABASE_FILLED;
    }

    public void ClearDb()
    {
      mEmployeeListDBConfiguration.ClearEmployees(ClearEmployeesCompleteAction);
      mEmployeesItemsSource = null;
      ClearFields();
    }
    
    public void SwitchToSubordinate()
    {
      if (mSelectedSubordinatesItem != null)
        SelectedEmployeesItem = mSelectedSubordinatesItem;
    }

    public void SwitchToManager()
    {
      if (mSelectedManagerItem != null)
        SelectedEmployeesItem = mSelectedManagerItem;
    }

    public void CalcResultSalary()
    {
      mResultSalary = 0m;
      if (mResultSalaryDate < mEmployeeListDBConfiguration.CompanyDateOfEstablishment)
      {
        OperationText = "Неправильно заполнено поле ДАТА РАСЧЕТА ЗАРПЛАТЫ.";
        SetOperationTextColor(Colors.Red);
      }
      else if ((mSelectedEmployeesItem == null) && !mResultSalaryForAll)
      {
        OperationText = "Не выбран сотрудник для расчета зарплаты.";
        SetOperationTextColor(Colors.Red);
      }
      else
      {
        Employee aSalaryEmployee = (mResultSalaryForAll) ? null : mSelectedEmployeesItem;
        if (mResultSalaryDateChanged)
        {
          using (new WaitCursor())
          {
            mResultSalary = mEmployeeListDBConfiguration.CalcTotalSalary(mResultSalaryDateChanged, mResultSalaryDate, aSalaryEmployee);
          }
          mTotalEmployeesSalary = (mResultSalaryForAll) ? mResultSalary : 0m;
          mResultSalaryDateChanged = false;
        }
        else
        {
          if ((aSalaryEmployee != null) || (mTotalEmployeesSalary <= 0m))
          {
            using (new WaitCursor())
            {
              mResultSalary = mEmployeeListDBConfiguration.CalcTotalSalary(mResultSalaryDateChanged, mResultSalaryDate, aSalaryEmployee);
            }
            if (mResultSalaryForAll)
              mTotalEmployeesSalary = mResultSalary;
          }
          mResultSalary = (mResultSalaryForAll) ? mTotalEmployeesSalary : aSalaryEmployee.Salary;
        }
      }
      RaisePropertyChanged("ResultSalary");
    }

    private EDbActionResult AddNewEmployee()
    {
      EDbActionResult res = EDbActionResult.Nothing;
      if (mAddingMode)
      {
        var aAddRes = mEmployeeListDBConfiguration.CreateNewEmployee(out Employee aEmployee, EmployeeListDBConfiguration.EEmployeeUpdateOption.SaveChanges, mSelectedSalaryPlanItem.Id, mLastName, mFirstName, mMiddleName, mEmploymentDate, mSelectedManagerItem);
        if (aAddRes == EmployeeListDBConfiguration.EEmployeeUpdateResult.Success)
        {
          res = EDbActionResult.SuccessAdd;
          mResultSalaryDateChanged = true;
          mEmployeesItemsSource = new ObservableCollection<Employee>(mEmployeeListDBConfiguration.EmployeeListDB.Employees);
          RaisePropertyChanged("EmployeesItemsSource");
        }
        else if (aAddRes == EmployeeListDBConfiguration.EEmployeeUpdateResult.FailAlreadyExists)
        {
          res = EDbActionResult.FailAlreadyExists;
        }
      }
      else
      {
        var aEmployee = mSelectedEmployeesItem;
        if (aEmployee != null)
        {
          var aAddRes = mEmployeeListDBConfiguration.UpdateEmployee(aEmployee, EmployeeListDBConfiguration.EEmployeeUpdateOption.SaveChanges, mSelectedSalaryPlanItem.Id, mLastName, mFirstName, mMiddleName, mEmploymentDate, mSelectedManagerItem);
          if (aAddRes == EmployeeListDBConfiguration.EEmployeeUpdateResult.Success)
          {
            res = EDbActionResult.SuccessChange;
            mResultSalaryDateChanged = true;
            mEmployeesItemsSource = new ObservableCollection<Employee>(mEmployeeListDBConfiguration.EmployeeListDB.Employees);
            RaisePropertyChanged("EmployeesItemsSource");
          }
          else
          {
            switch (aAddRes)
            {
              case EmployeeListDBConfiguration.EEmployeeUpdateResult.FailEmployeeType:
                res = EDbActionResult.FailEmployeeType;
                break;
              case EmployeeListDBConfiguration.EEmployeeUpdateResult.FailManagerIsSubordinate:
                res = EDbActionResult.FailManagerIsSubordinate;
                break;
            }
          }
        }
      }
      return res;
    }
    
    public void RemoveSelectedEmployee()
    {
      if (mSelectedEmployeesItem != null)
      {
        mEmployeeListDBConfiguration.EmployeeListDB.Employees.Remove(mSelectedEmployeesItem);
        mEmployeeListDBConfiguration.EmployeeListDB.SaveChanges();
        ClearFields();
      }
    }

    public string this[string columnName]
    {
      get
      {
        string result = string.Empty;
        switch (columnName)
        {
          case "LastName":
            result = ValidateLastName();
            break;
          case "FirstName":
            result = ValidateFirstName();
            break;
          case "EmploymentDate":
            result = ValidateEmploymentDate();
            break;
          case "SelectedManagerItem":
            result = ValidateManager();
            break;
          case "ResultSalaryDate":
            result = ValidateResultSalaryDate();
            break;
        }
        return result;

      }
    }

    private string ValidateAll()
    {
      var errors = new[]
      {
                ValidateLastName(),
                ValidateFirstName(),
                ValidateEmploymentDate(),
                ValidateManager()
            };
      return string.Join("\n", errors.Where(error => !string.IsNullOrEmpty(error)));
    }

    private string ValidateLastName()
    {
      if (string.IsNullOrWhiteSpace(mLastName))
        return "Поле ФАМИЛИЯ должно быть заполнено.";
      return string.Empty;
    }

    private string ValidateFirstName()
    {
      if (string.IsNullOrWhiteSpace(mFirstName))
        return "Поле ИМЯ должно быть заполнено.";
      return string.Empty;
    }

    private string ValidateEmploymentDate()
    {
      if (mEmploymentDate < mEmployeeListDBConfiguration.CompanyDateOfEstablishment)
        return "Неправильно заполнено поле ДАТА ПОСТУПЛЕНИЯ НА РАБОТУ.";
      return string.Empty;
    }

    private string ValidateManager()
    {
      if (EmployeeHelper.IsSubordinate(mSelectedEmployeesItem, mSelectedManagerItem))
        return ERROR_MESSAGE_MANAGER_CONFLICT;
      return string.Empty;
    }

    private string ValidateResultSalaryDate()
    {
      if (mResultSalaryDate < mEmployeeListDBConfiguration.CompanyDateOfEstablishment)
        return "Неправильно заполнено поле ДАТА РАСЧЕТА ЗАРПЛАТЫ.";
      return string.Empty;
    }

    public string Error => ValidateAll();

    #region SubordinatesFieldVisible

    public bool SubordinatesFieldVisible
    {
      get { return (mSelectedSalaryPlanItem != null) && (mSelectedSalaryPlanItem.EmployeeType != SalaryPlan.EEmployeeType.Employee); }
    }

    #endregion

    #region LastName

    private string mLastName;

    public string LastName
    {
      get { return mLastName; }
      set
      {
        OperationText = "";
        if (mLastName != value)
        {
          mLastName = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region FirstName

    private string mFirstName;

    public string FirstName
    {
      get { return mFirstName; }
      set
      {
        OperationText = "";
        if (mFirstName != value)
        {
          mFirstName = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region MiddleName

    private string mMiddleName;

    public string MiddleName
    {
      get { return mMiddleName; }
      set
      {
        OperationText = "";
        if (mMiddleName != value)
        {
          mMiddleName = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region EmploymentDate

    private DateTime mEmploymentDate;
    private string mEmploymentDateString;

    public string EmploymentDate
    {
      get { return mEmploymentDateString; }
      set
      {
        OperationText = "";
        if (mEmploymentDateString != value)
        {
          mEmploymentDateString = value;
          RaisePropertyChanged();
          DateTime.TryParse(value, out mEmploymentDate);
        }
      }
    }

    #endregion

    #region OkCommandText

    public string OkCommandText
    {
      get { return (mAddingMode) ? "Добавить" : "Сохранить"; }
    }

    #endregion

    #region AddingMode

    private bool mAddingMode;

    public bool AddingMode
    {
      get { return mAddingMode; }
      set
      {
        if (mAddingMode != value)
        {
          mAddingMode = value;
          RaisePropertyChanged();
          RaisePropertyChanged("OkCommandText");
        }
      }
    }

    #endregion

    #region ResultSalaryDate

    private DateTime mResultSalaryDate;
    private string mResultSalaryDateString;
    private bool mResultSalaryDateChanged;

    public string ResultSalaryDate
    {
      get { return mResultSalaryDateString; }
      set
      {
        if (mResultSalaryDateString != value)
        {
          mResultSalaryDateString = value;
          RaisePropertyChanged();
          DateTime.TryParse(value, out mResultSalaryDate);
          mResultSalaryDateChanged = true;
        }
      }
    }

    #endregion

    #region ResultSalary

    private decimal mResultSalary;
    private decimal mTotalEmployeesSalary;

    public decimal ResultSalary
    {
      get { return mResultSalary; }
    }

    #endregion

    #region ResultSalaryForAll

    private bool mResultSalaryForAll;

    public bool ResultSalaryForAll
    {
      get { return mResultSalaryForAll; }
      set
      {
        if (mResultSalaryForAll != value)
        {
          mResultSalaryForAll = value;
          RaisePropertyChanged();
          mSalarySubjectName = (mResultSalaryForAll) ? TEXT_SALARY_FOR_ALL_EMPLOYEES : mSelectedEmployeesItem?.ToString();
          RaisePropertyChanged("SalarySubjectName");
        }
      }
    }

    #endregion

    #region SalarySubjectName

    private string mSalarySubjectName;

    public string SalarySubjectName
    {
      get { return mSalarySubjectName; }
    }

    #endregion

    #region OperationText

    private string mOperationText;

    public string OperationText
    {
      get { return mOperationText; }
      set
      {
        if (mOperationText != value)
        {
          mOperationText = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region OperationTextColor

    private SolidColorBrush mOperationTextColor = new SolidColorBrush(Colors.Red);

    public SolidColorBrush OperationTextColor
    {
      get { return mOperationTextColor; }
      set
      {
        if (mOperationTextColor != value)
        {
          mOperationTextColor = value;
          RaisePropertyChanged();
        }
      }
    }

    private void SetOperationTextColor(Color aValue)
    {
      OperationTextColor.Color = aValue;
      RaisePropertyChanged("OperationTextColor");
    }

    #endregion
  }
}
