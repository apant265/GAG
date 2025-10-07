using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GAG_Proc_Generator.ViewModels;

namespace GAG_Proc_Generator;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainViewModel;
        if (vm != null)
        {
            vm.OpenConnectionDialogCommand.Execute(null);
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Button clicked, but no command bound!", "Info", System.Windows.MessageBoxButton.OK);
    }
}