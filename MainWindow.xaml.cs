using System.Windows;
using GAG_Proc_Generator.ViewModels;

namespace GAG_Proc_Generator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}