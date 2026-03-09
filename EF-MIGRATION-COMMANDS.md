# EF Migration Commands - Easy Guide

## Step 1: Navigate to Project
```powershell
cd Desktop\SAMS-CLONING-PROJECT\AdminWebPage
```

## Step 2: Common Commands

### Add New Migration
```powershell
dotnet ef migrations add YourMigrationName
```

### Update Database
```powershell
dotnet ef database update
```

### See All Migrations
```powershell
dotnet ef migrations list
```

### Remove Last Migration (if not applied to database)
```powershell
dotnet ef migrations remove
```

## Step 3: Important Notes

### Before Running Commands
- **STOP** any running applications first
- Make sure you're in the correct directory
- Run `dotnet build` if you get errors

### Common Problems & Solutions
1. **"Build failed"** → Stop running app, then try again
2. **"Cannot find path"** → Use full path: `cd Desktop\SAMS-CLONING-PROJECT\AdminWebPage`
3. **"File is locked"** → Close Visual Studio or stop running processes
4. **"Invalid file names detected"** → Rename image files to lowercase (e.g., MKA.png → mka.png)

## Setup for New Computer
```powershell
# Install EF tools (one time only)
dotnet tool install --global dotnet-ef

# Navigate to project
cd Desktop\SAMS-CLONING-PROJECT\AdminWebPage

# Update database
dotnet ef database update
```

## Quick Workflow
1. Make changes to your models
2. Add migration: `dotnet ef migrations add YourChanges`
3. Update database: `dotnet ef database update`
4. Test your application

- Always run migration commands from the **AdminWebPage** directory
- The AdminWebPage project contains all the migrations
- Make sure SQL Server LocalDB is installed on all computers
- Connection string should be the same on all machines for consistency
