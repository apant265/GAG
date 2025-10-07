# SQL Stored Procedure Generator

A WPF application built in C# .NET for generating SQL stored procedures for Azure SQL Database or SQL Server, using Windows Authentication or SQL Server Authentication.

## What the Program Does

This application allows users to connect to a SQL Server database, explore its schema (tables, columns, foreign keys), and generate custom stored procedures for CRUD operations (Create, Read, Update, Delete) with full customization options.

### Key Features

- **Database Connection Management**: Connect to Azure SQL or on-premise SQL Server with flexible authentication options.
- **Schema Exploration**: Retrieve and display table columns, data types, primary keys, and foreign key relationships.
- **Procedure Generation**:
  - **GetAll**: Select all records from a table.
  - **GetByAttributes**: Filtered select based on selected columns and foreign keys.
  - **Delete**: Delete records by primary key and optional foreign keys.
  - **Save**: Insert or update records (upsert) with selected attributes and foreign keys.
- **Customization**: For each procedure, users can select which columns and foreign keys to include, and add custom header comments.
- **Automatic File Saving**: Generated SQL scripts are saved as .sql files in a `GeneratedProcs/{TableName}/` directory.
- **Existing Procedure View**: Display existing stored procedures in the selected database.
- **User-Friendly UI**: Intuitive WPF interface with tabbed procedure generation, real-time schema display, and error handling.

## How It Works

### Architecture

The application follows the MVVM (Model-View-ViewModel) pattern for clean separation of concerns:

- **Models**: `ColumnInfo` and `ForeignKeyInfo` classes represent database schema elements.
- **ViewModels**: `MainViewModel` handles business logic, data binding, and commands.
- **Views**: WPF windows and user controls for the UI.

### Installation and Setup

1. Ensure .NET 8.0 SDK is installed.
2. Clone or download the project.
3. Run `dotnet build` in the project directory.
4. Launch the app from `bin/Debug/net8.0-windows/GAG Proc Generator.exe`.

### Workflow

1. **Launch Application**: Start the WPF app.
2. **Connect to Database**:
   - Click "Connect to Database" button.
   - Enter server address, select authentication type (Windows or SQL Server).
   - For SQL Server auth, provide username and password.
   - Click "Connect" to retrieve available databases.
   - Select a database and view existing procedures.
   - Click "OK" to set the connection.
3. **Load Table Schema**:
   - Enter the table name.
   - Click "Load Table" to retrieve columns and foreign keys.
   - View the schema in the UI.
4. **Generate Procedures**:
   - Switch to the desired procedure tab (GetAll, GetByAttributes, Delete, Save).
   - Select attributes (columns) and foreign keys to include.
   - Enter header text (comments) for the procedure.
   - Click "Generate [Procedure]" to create the SQL script.
   - View the generated SQL in the display area.
   - The script is automatically saved as a .sql file.
5. **Repeat**: Load another table or generate more procedures as needed.

### Technical Details

- **Database Connection**: Uses `Microsoft.Data.SqlClient` for secure connections.
- **Schema Retrieval**: Queries system tables (`INFORMATION_SCHEMA`, `sys.databases`, `sys.procedures`, etc.) for metadata.
- **SQL Generation**: Builds T-SQL scripts with proper parameter handling, conditional logic for upserts, and null-safe filtering.
- **File Management**: Creates directories and saves files using `System.IO`.
- **UI Responsiveness**: Data binding ensures real-time updates; validation prevents invalid operations.

### Requirements

- .NET 8.0 or later
- Windows OS (for Windows Authentication)
- Access to SQL Server or Azure SQL Database
- NuGet packages: `Microsoft.Data.SqlClient`, `CommunityToolkit.Mvvm`

### Usage Tips

- Use Windows Authentication for Azure SQL if possible for security.
- Select only necessary columns and FKs to keep procedures efficient.
- Header text is added as SQL comments at the top of each procedure.
- Generated files are saved in the app's directory under `GeneratedProcs/`.

This tool streamlines database development by automating common stored procedure creation while providing flexibility for custom requirements.-e "\n## Recent Updates\n\n- Added GUI for database connection with authentication options.\n- Implemented MVVM pattern for better code organization.\n- Fixed button binding issues for seamless user interaction.\n- Enhanced error handling and user feedback." 
