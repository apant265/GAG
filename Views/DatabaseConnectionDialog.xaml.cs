using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace GAG_Proc_Generator.Views
{
    public partial class DatabaseConnectionDialog : Window, INotifyPropertyChanged
    {
        private string _server = string.Empty;
        private ObservableCollection<string> _databases = new();
        private string _selectedDatabase = string.Empty;
        private ObservableCollection<string> _procedures = new();
        private bool _isConnected = false;
        private string _status = "Not connected";

        public string Server
        {
            get => _server;
            set { _server = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Databases
        {
            get => _databases;
            set { _databases = value; OnPropertyChanged(); }
        }

        public string SelectedDatabase
        {
            get => _selectedDatabase;
            set { _selectedDatabase = value; OnPropertyChanged(); LoadProcedures(); }
        }

        public ObservableCollection<string> Procedures
        {
            get => _procedures;
            set { _procedures = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string ConnectionString { get; private set; } = string.Empty;

        public ICommand ConnectCommand => new RelayCommand(Connect);
        public ICommand TestConnectionCommand => new RelayCommand(TestConnection);
        public ICommand OkCommand => new RelayCommand(Ok);
        public ICommand CancelCommand => new RelayCommand(() => DialogResult = false);

        public DatabaseConnectionDialog()
        {
            InitializeComponent();
            DataContext = this;
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

        private void Connect()
        {
            try
            {
                Status = "Connecting...";

                using var connection = new SqlConnection(BuildConnectionString());
                connection.Open();

                // Retrieve databases
                var command = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", connection);
                var reader = command.ExecuteReader();
                Databases.Clear();
                while (reader.Read())
                {
                    Databases.Add(reader.GetString(0));
                }
                reader.Close();

                IsConnected = true;
                Status = "Connected successfully!";
            }
            catch (Exception ex)
            {
                Status = $"Connection failed: {ex.Message}";
                System.Windows.MessageBox.Show($"Connection failed: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void TestConnection()
        {
            try
            {
                Status = "Testing connection...";

                using var connection = new SqlConnection(BuildConnectionString());
                connection.Open();

                Status = "Connection test successful!";
                System.Windows.MessageBox.Show("Connection test successful!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Status = $"Connection test failed: {ex.Message}";
                System.Windows.MessageBox.Show($"Connection test failed: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadProcedures()
        {
            if (!IsConnected || string.IsNullOrEmpty(SelectedDatabase)) return;

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                var command = new SqlCommand("SELECT name FROM sys.procedures ORDER BY name", connection);
                var reader = command.ExecuteReader();
                Procedures.Clear();
                while (reader.Read())
                {
                    Procedures.Add(reader.GetString(0));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load procedures: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Ok()
        {
            if (string.IsNullOrEmpty(SelectedDatabase))
            {
                System.Windows.MessageBox.Show("Please select a database.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            ConnectionString = BuildConnectionString();
            DialogResult = true;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;
    }
}