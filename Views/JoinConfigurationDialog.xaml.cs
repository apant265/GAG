using System.Windows;
using GAG_Proc_Generator.ViewModels;

namespace GAG_Proc_Generator.Views;

public partial class JoinConfigurationDialog : Window
{
    public JoinConfigurationViewModel ViewModel { get; }

    public JoinConfigurationDialog()
    {
        InitializeComponent();
        ViewModel = new JoinConfigurationViewModel();
        DataContext = ViewModel;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
