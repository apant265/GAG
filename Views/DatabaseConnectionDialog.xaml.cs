using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GAG_Proc_Generator.Constants;
using GAG_Proc_Generator.Services;
using Microsoft.Data.SqlClient;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using Application = System.Windows.Application;
using Window = System.Windows.Window;

namespace GAG_Proc_Generator.Views;

public partial class DatabaseConnectionDialog : Window
{
    public DatabaseConnectionDialog()
    {
        InitializeComponent();
        DataContext = new DatabaseConnectionViewModel();
    }

    public string ConnectionString => ((DatabaseConnectionViewModel)DataContext).ConnectionString;
}

public partial class DatabaseConnectionViewModel : ObservableObject
{
    private readonly DatabaseService databaseService;

    [ObservableProperty]
    private string server = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> databases = new();

    [ObservableProperty]
    private string selectedDatabase = string.Empty;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string status = "Not connected";

    public string ConnectionString { get; private set; } = string.Empty;

    public DatabaseConnectionViewModel()
    {
        databaseService = new DatabaseService();
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            Status = "Connecting...";

            var databases = await databaseService.GetDatabasesAsync(BuildConnectionString());

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Databases.Clear();

                foreach (var db in databases)
                {
                    Databases.Add(db);
                }

                IsConnected = true;
                Status = "Connected successfully!";
            });
        }
        catch (Exception ex)
        {
            Status = $"Connection failed: {ex.Message}";
            MessageBox.Show(
                $"Connection failed: {ex.Message}",
                MessageStrings.TitleError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        if (string.IsNullOrEmpty(SelectedDatabase))
        {
            MessageBox.Show(
                MessageStrings.SelectDatabase,
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        ConnectionString = BuildConnectionString();

        if (Application.Current.MainWindow is Window window)
        {
            foreach (Window win in Application.Current.Windows)
            {
                if (win is DatabaseConnectionDialog dialog)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private static void Cancel()
    {
        foreach (Window win in Application.Current.Windows)
        {
            if (win is DatabaseConnectionDialog dialog)
            {
                dialog.DialogResult = false;
                dialog.Close();
                break;
            }
        }
    }

    private string BuildConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = Server,
            IntegratedSecurity = true,
            Encrypt = true,
            TrustServerCertificate = true
        };

        if (!string.IsNullOrEmpty(SelectedDatabase))
        {
            builder.InitialCatalog = SelectedDatabase;
        }

        return builder.ConnectionString;
    }
}
