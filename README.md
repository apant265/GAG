# SQL Stored Procedure Generator

A modern WPF application for generating SQL Server stored procedures with a clean, progressive disclosure UI.

## Features

- **Progressive Disclosure UI**: Step-by-step workflow that reveals sections as you progress
- **Database Schema Discovery**: Automatically loads tables, columns, primary keys, and foreign keys
- **Multiple Procedure Types**:
  - `GetAll` - Retrieve all records from a table
  - `GetByAttributes` - Custom filtered queries
  - `Save` - Upsert operations (Insert/Update)
  - `Delete` - Remove records by criteria
- **Modern Design**: Material Design-inspired interface with smooth interactions
- **Async Operations**: Non-blocking database operations for responsive UX
- **Clean Architecture**: MVVM pattern with CommunityToolkit.Mvvm source generators

## Technology Stack

- **.NET 8.0** (Windows)
- **WPF** (Windows Presentation Foundation)
- **CommunityToolkit.Mvvm 8.2.2** - Modern MVVM with source generators
- **Microsoft.Data.SqlClient 5.2.2** - SQL Server connectivity

## Project Structure

```
GAG Proc Generator/
├── Constants/
│   └── DatabaseConstants.cs        # Centralized string constants and SQL queries
├── Converters/
│   └── BoolToColorConverter.cs     # UI value converters for data binding
├── Helpers/
│   └── SqlTypeHelper.cs            # SQL type mapping utilities (legacy)
├── Models/
│   ├── ColumnInfo.cs               # ObservableObject for table columns
│   └── ForeignKeyInfo.cs           # ObservableObject for foreign key relationships
├── Services/
│   ├── DatabaseService.cs          # Async database operations with proper disposal
│   └── SqlGenerationService.cs     # T-SQL stored procedure script generation
├── ViewModels/
│   └── MainViewModel.cs            # Main window ViewModel with CommunityToolkit.Mvvm
├── Views/
│   └── DatabaseConnectionDialog.xaml(.cs)  # Connection dialog with ViewModel
├── MainWindow.xaml(.cs)            # Main application window
├── App.xaml(.cs)                   # Application entry point
└── README.md                       # This file
```

## Naming Conventions

Following C# and WPF best practices:

### File Names
- **PascalCase** for all files: `MainViewModel.cs`, `DatabaseService.cs`
- **Match class names**: File name should match the primary class name

### Code Conventions

```csharp
// Classes, Methods, Properties (PascalCase)
public class DatabaseService
{
    public async Task<List<string>> GetTablesAsync(string connectionString) { }
}

// Private fields (camelCase, no prefix)
private readonly DatabaseService databaseService;
private string connectionString;

// Local variables and parameters (camelCase)
public void ProcessData(string tableName)
{
    var connectionString = BuildConnectionString();
    foreach (var column in columns) { }
}

// ObservableProperty attributes (private fields, camelCase)
[ObservableProperty]
private string tableName = string.Empty;
// Generates public property: public string TableName

// Constants (PascalCase)
public const string DefaultDatabase = "GlobalValues";
```

## Architecture Patterns

### MVVM with Source Generators

ViewModels use CommunityToolkit.Mvvm source generators to eliminate boilerplate:

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _tableName = string.Empty;
    // Generates: public string TableName { get; set; } with INotifyPropertyChanged

    [RelayCommand]
    private async Task LoadTableAsync() { }
    // Generates: public IAsyncRelayCommand LoadTableCommand { get; }
}
```

### Service Layer Pattern

Business logic separated into focused service classes:

**DatabaseService**
- Handles all SQL Server interactions
- Async methods with proper `await using` disposal
- Returns DTOs (List<T>) to ViewModels

**SqlGenerationService**
- Generates T-SQL stored procedure scripts
- Pure functions with no side effects
- Business logic for SQL generation separated from data access

### Async/Await Best Practices

All database operations use proper async patterns:

```csharp
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

await Application.Current.Dispatcher.InvokeAsync(() =>
{
    // UI updates on UI thread
    Columns.Clear();
    foreach (var column in columns)
        Columns.Add(column);
});
```

## Coding Practices and Formatting

### Code Style Guidelines

**Brace Style**
- Opening braces on same line for short methods
- Opening braces on new line for classes, namespaces, and properties

```csharp
// Classes and namespaces - new line
public class DatabaseService
{
    public void Method()
    {
        // Implementation
    }
}

// Inline methods - same line (when short)
public string GetValue() => "value";
```

**Spacing and Indentation**
- 4 spaces per indentation level (no tabs)
- One blank line between methods
- No blank lines between properties with attributes
- Space after keywords: `if (condition)`, `for (var i = 0; ...)`
- No space for method calls: `Method(param)`

**Expression Formatting**
```csharp
// Use var for obvious types
var connection = new SqlConnection(connectionString);
var items = new List<string>();

// Explicit types when type is not obvious
DatabaseService service = GetService();

// String interpolation preferred over concatenation
var message = $"Connected to {database}";

// Collection initializers
var list = new List<string> { "item1", "item2" };
```

**Null Handling**
```csharp
// Use null-coalescing operator
var name = column.Name ?? string.Empty;

// Null-conditional operator for chaining
var length = column?.Name?.Length ?? 0;

// Pattern matching for null checks
if (value is not null)
{
    ProcessValue(value);
}
```

**LINQ Formatting**
```csharp
// Simple queries - single line
var names = columns.Select(c => c.Name);

// Complex queries - multiple lines with proper indentation
var filtered = columns
    .Where(c => c.IsSelected)
    .OrderBy(c => c.Name)
    .Select(c => c.DataType);
```

### File Organization

**Using Directives**
- System namespaces first
- Third-party namespaces second
- Project namespaces last
- Alphabetical order within each group
- No unused usings

```csharp
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GAG_Proc_Generator.Models;
using GAG_Proc_Generator.Services;
```

**Class Member Order**
1. Constants
2. Private readonly fields
3. Private fields
4. Constructors
5. Properties (ObservableProperty attributes)
6. Public methods
7. Private methods (RelayCommand methods)
8. Helper methods

```csharp
public partial class MainViewModel : ObservableObject
{
    // Private fields
    private readonly DatabaseService databaseService;
    private readonly SqlGenerationService sqlGenerationService;

    // Constructor
    public MainViewModel()
    {
        databaseService = new DatabaseService();
        sqlGenerationService = new SqlGenerationService();
    }

    // Observable properties
    [ObservableProperty]
    private string tableName = string.Empty;

    [ObservableProperty]
    private bool isConnected;

    // Commands
    [RelayCommand]
    private async Task LoadTableAsync()
    {
        // Implementation
    }

    // Helper methods
    private string BuildConnectionString()
    {
        // Implementation
    }
}
```

### Commenting Guidelines

**When to Comment**
- Complex business logic that isn't immediately obvious
- Workarounds for known issues
- Important architectural decisions
- Public API documentation (XML comments)

**When NOT to Comment**
- Self-explanatory code
- Obvious variable declarations
- Standard patterns (MVVM, async/await)

```csharp
// AVOID - Obvious comments
// This gets the table name
var tableName = GetTableName();

// GOOD - Explains WHY, not WHAT
// GeographyElement requires special handling for GeographyType lookup
if (fk.ReferencedTable == "GeographyElement")
{
    // Special join logic
}

// GOOD - XML documentation for public APIs
/// <summary>
/// Generates a stored procedure that retrieves all records from the specified table.
/// </summary>
/// <param name="tableName">The name of the database table</param>
/// <param name="database">The database name</param>
/// <returns>A complete T-SQL CREATE PROCEDURE script</returns>
public string GenerateGetAllProcedure(string tableName, string database)
{
    // Implementation
}
```

### Error Handling

**Try-Catch Usage**
- Catch specific exceptions when possible
- Always provide user feedback in UI applications
- Log errors for debugging (when logging is available)

```csharp
try
{
    await databaseService.GetTablesAsync(connectionString);
}
catch (SqlException ex)
{
    MessageBox.Show(
        $"Database error: {ex.Message}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
}
catch (Exception ex)
{
    MessageBox.Show(
        $"Unexpected error: {ex.Message}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
}
```

### Resource Management

**IDisposable Pattern**
- Always use `using` or `await using` for IDisposable resources
- Prefer `await using` for async disposables

```csharp
// Async disposal
await using var connection = new SqlConnection(connectionString);
await using var command = new SqlCommand(query, connection);
await using var reader = await command.ExecuteReaderAsync();

// Synchronous disposal
using var stream = File.OpenRead(path);
```

### Threading and UI Updates

**Dispatcher Usage**
- Always update ObservableCollections on UI thread
- Use `Application.Current.Dispatcher.InvokeAsync` for async operations

```csharp
var items = await databaseService.GetItemsAsync();

await Application.Current.Dispatcher.InvokeAsync(() =>
{
    Items.Clear();
    foreach (var item in items)
    {
        Items.Add(item);
    }
});
```

### String Handling

**String Constants**
- Use `DatabaseConstants` and `MessageStrings` instead of magic strings
- Use `string.Empty` instead of `""`
- Use `nameof()` for property names in data binding

```csharp
// AVOID
if (status == "Connected")
    MessageBox.Show("Success!", "Info");

// GOOD
if (status == StatusStrings.Connected)
    MessageBox.Show(MessageStrings.ConnectionSuccess, MessageStrings.TitleInfo);

// Property names
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TableName)));
```

### Performance Considerations

**Collection Usage**
- Use `List<T>` for internal collections
- Use `ObservableCollection<T>` only for UI-bound collections
- Pre-allocate capacity when size is known

```csharp
// Good - known size
var items = new List<string>(columns.Count);

// ObservableCollection only for data binding
[ObservableProperty]
private ObservableCollection<ColumnInfo> columns = new();
```

**Async Best Practices**
- Suffix async methods with `Async`
- Always `await` async calls (don't use `.Result` or `.Wait()`)
- Use `Task.WhenAll` for parallel operations

```csharp
// Sequential when order matters
var tables = await databaseService.GetTablesAsync(connectionString);
var columns = await databaseService.GetColumnsAsync(connectionString, tableName);

// Parallel when independent
var tablesTask = databaseService.GetTablesAsync(connectionString);
var databasesTask = databaseService.GetDatabasesAsync(connectionString);
await Task.WhenAll(tablesTask, databasesTask);
```

## Usage Workflow

### 1. Connect to Database
- Click **"Connect to SQL Server"**
- Enter server address (e.g., `localhost` or `server.database.windows.net`)
- Click **"Connect"** to retrieve databases
- Select database from dropdown
- Click **"Next"** to proceed

### 2. Select Table
- Choose table from dropdown (appears after connection)
- Click **"Load Table"** to retrieve schema

### 3. Enter Header Information
- **Initials**: Developer initials
- **Story Number**: Work item or ticket number
- **Description**: Change description

### 4. Select Columns & Foreign Keys
- Check columns to include in custom procedures
- Check foreign keys to JOIN in queries

### 5. Generate Procedures
**Standard Procedures** (no selection needed):
- **GetAll**: Retrieves all records with optional FK joins
- **Save**: Upsert operation (Insert/Update)
- **Delete**: Remove records

**Custom Procedures** (requires column selection):
- **GetByAttributes**: Filter by selected columns

## Generated SQL Format

Each procedure includes:
- Standardized header with metadata
- Test execution string with parameter declarations
- Revision history section
- Proper parameter handling and null safety

Example:
```sql
/****************************************************************************************
PROCEDURE:   GlobalValues.uspTableName_GetAll
DESCRIPTION: Get all TableName records
APPLICATION: GlobalValues.API
DATABASE:    GlobalValues

TEST STRING:
EXEC GlobalValues.uspTableName_GetAll;

*******************************************************************************
REVISION HISTORY:
Initials      Date            Story Number    Description
=========================================================================================
AP            01/13/2025      US-12345        Initial creation
*****************************************************************************************/
```

## Building the Project

```bash
dotnet restore
dotnet build
```

## Running the Application

```bash
dotnet run
```

Or build and run from Visual Studio 2022+.

## Best Practices Implemented

### Code Quality
- CommunityToolkit.Mvvm source generators (no boilerplate)
- Proper async/await with ConfigureAwait
- `await using` for IDisposable resources
- Null-safe operations with nullable reference types
- Separation of concerns (Services, ViewModels, Views)
- UI thread dispatching for ObservableCollection modifications

### WPF Patterns
- MVVM with data binding
- ICommand pattern via RelayCommand
- Value converters for UI logic
- Progressive disclosure (conditional visibility)
- Resource dictionaries for consistent styling
- DockPanel and ScrollViewer for proper layout

### Database
- Parameterized queries (SQL injection prevention)
- Proper connection disposal with `await using`
- Error handling with user feedback
- Async database operations

## Configuration

Easily customize via `DatabaseConstants.cs`:

```csharp
public static class DatabaseConstants
{
    public const string DefaultApplication = "GlobalValues.API";
    public const string DefaultDatabase = "GlobalValues";
    public const string ProcedurePrefix = "usp";
}
```

## Security Considerations

- Windows Authentication by default
- SQL Server encryption support (TrustServerCertificate)
- Parameterized database queries
- No credential storage
- Proper disposal of database resources

## Future Enhancements

Potential improvements:
- SQL Server authentication option
- Export to multiple formats (JSON, CSV)
- Batch procedure generation for multiple tables
- Custom procedure templates
- Git integration for version control
- Procedure comparison and diff
- Multi-database support (PostgreSQL, MySQL)
- Dark mode theme

## Contributing

Follow the established patterns:
1. Use `[ObservableProperty]` and `[RelayCommand]` attributes
2. Keep ViewModels clean - delegate to Services
3. Use `DatabaseConstants` instead of magic strings
4. Follow C# naming conventions (PascalCase/camelCase)
5. Use async/await for I/O operations
6. Dispatch UI updates to UI thread
7. Add XML documentation for public APIs

## License

Internal use only.

## Support

For issues or questions, please contact the development team.

---

**Last Updated**: October 2025
**Version**: 1.0
