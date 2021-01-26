using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using EmployeeList.ViewModels;

namespace EmployeeList.Controls
{
  /// <summary>
  /// Interaction logic for DocumentControl.xaml
  /// </summary>
  public partial class DocumentControl : UserControl
  {
    public DocumentControl()
    {
      InitializeComponent();
    }

    private void ClearManagerButton_Click(object sender, RoutedEventArgs e)
    {
      (DataContext as DocumentControlViewModel).ClearManager();
    }

    private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
    {
      (DataContext as DocumentControlViewModel).RemoveSelectedEmployee();
    }

    private void FillDbButton_Click(object sender, RoutedEventArgs e)
    {
      (DataContext as DocumentControlViewModel).FillDb();
    }

    private void ClearDbButton_Click(object sender, RoutedEventArgs e)
    {
      (DataContext as DocumentControlViewModel).ClearDb();
    }

    private void ResultSalaryButton_Click(object sender, RoutedEventArgs e)
    {
      (DataContext as DocumentControlViewModel).CalcResultSalary();
    }

    private void SubordinatesListBox_MouseDoubleClick(object sender, RoutedEventArgs e)
    {
      (DataContext as DocumentControlViewModel).SwitchToSubordinate();
    }

    private void ManagerTextBlock_MouseButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        (DataContext as DocumentControlViewModel).SwitchToManager();
    }
  }
}
