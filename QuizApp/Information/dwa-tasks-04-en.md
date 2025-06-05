# RESTful architecture (Web API)

This material is part of Learning Objective 2 (minimum).

## 4 State Storage

### 4.1 Set up Database Access

1. Open Microsoft SQL Server Management Studio

   - Connect to the available server (localhost\SQLEXPRESS) using **SQL Server Authenication** (user:sa, pwd: SQL)
   - Create a database `Exercise4` with the default settings provided
   - In this database, create a Notification table with the following schema:

     ```SQL
     CREATE TABLE [dbo].[Notification](
       [Id] [int] IDENTITY(1,1) NOT NULL,
       [Guid] [uniqueidentifier] NOT NULL,
       [CreatedAt] [datetime2](7) NOT NULL,
       [UpdatedAt] [datetime2](7) NULL,
       [Receiver] [nvarchar](256) NOT NULL,
       [Subject] [nvarchar](256) NULL,
       [Body] [nvarchar](max) NOT NULL,
       [SentAt] [datetime2](7) NULL,
       CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED  (
         [Id] ASC
       )
     )

     ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_Guid]  DEFAULT (newid()) FOR [Guid]
     GO

     ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_CreatedAt]  DEFAULT (getutcdate()) FOR [CreatedAt]
     GO
     ```

   - Fill the table with some data: https://pastebin.com/tG9m4guk

2. In Visual Studio create a new RESTful Web API application with the following characteristics:

   - Name (solution): exercise-4
   - Project type: Web API
   - Project name: exercise-4-1
   - No authentication and HTTPS
   - Let it use controllers and Swagger

   Also:

   - Delete unnecessary `WeatherForecast` controller and model
     > Sometimes when last controller is removed, folder `Controllers` disappears. You can simply switch `Solution Explorer` to `Show All Files` and include the folder into the project
   - Make the Web API port 5123

3. Install tooling and packages needed for database access

   When a project needs database access via Entity Framework, this installation is the first thing you need to do. You need to install required packages, otherwise you won't be able to use Entity Framework in your project.  
   This is something you need to do for every project that requires direct database access.

   - Open Package Manager Console in VS.Net (Tools > NuGet Package Manager > Packet Manager Console)
   - Install the required `ef` tool via the dotnet command
     ```
     dotnet tool install --global dotnet-ef --version 8
     ```
     > If that tool is already installed, you wil get an error message.   
     > `dotnet : Tool 'dotnet-ef' is already installed.`
     >
     > That's ok.
   - `cd` into the project folder
   - Install the following packages in the project
     - Microsoft.EntityFrameworkCore
     - Microsoft.EntityFrameworkCore.Design
     - Microsoft.EntityFrameworkCore.SqlServer
     > Notes:  
     > - If you get error `dotnet : Could not find any project in ...`, then you are not in the project folder
     > - If you get error `dotnet : Found more than one project in ...`, open your project folder and delete the .csproj file with `Backup` in its name
   - Example installation for one package:
     ```
     dotnet add package Microsoft.EntityFrameworkCore --version 8
     ```
   - Do that for all 3 packages and check if they are installed in `Solution Explorer`, `Dependencies` folder
      > You need to `File > Save All` to save the solution/project with added dependencies
   - Check if all 3 dependencies are present in your `Dependencies > Packages` in Solution Explorer

4. Prepare EF context and models using the `dotnet ef` tool

   This step is usually the part of the "database first" scenario, when you start with the database and generate models from it.

    > Note: When using "code first" scenario, this is not needed. We won't be using "code first" scenario, so we need this step.

   - Verify that the tool is working correctly using the following command in Package Manager console (must not print an error): `dotnet ef`

    > Notes:
    > - If using the tool results in an error, restart the Visual Studio and reopen the project.  
    > - It might happen that on systems where you don't have admin priviledge, you still can't use the tool. The tool is installed in folder `C:\Users\{your-username}\.dotnet\tools\dotnet-ef.exe`. You can use it then as `{...full path...}\dotnet-ef`
    >   - use `whoami` command to find out what's your user name
    > - if you suspect you don't have the tool installed at all, you can use `dotnet tool list -g` to check that

   - Auto-generate models for the database using an appropriate connection string (check and edit if needed)

     ```
     # Authentication using windows user (not in classroom, probably your PC at home)
     dotnet ef dbcontext scaffold "server=.;Database=Exercise4;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer -o Models

     # If dotnet ef is not available...
     C:\Users\lecturerf6\.dotnet\tools\dotnet-ef.exe dbcontext scaffold ...
     ```

     ```
     # Authentication using SQL Authentication user (in classroom)
     dotnet ef dbcontext scaffold "server=.\SQLEXPRESS;Database=Exercise4;User=sa;Password=SQL;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer -o Models

     # If dotnet ef is not available...
     C:\Users\lecturerf6\.dotnet\tools\dotnet-ef.exe dbcontext scaffold ...
     ```

     > Notes:
     > - if you ger error `No project was found. Change the current working directory or use the --project option.`, it means that you are not in the project folder
     > - if you get error `Cannot open database ... requested by the login. The login failed.`, most likely you have the incorrect name of the database in your connection string

5. In the Solution Explorer, you should see the new folder `Models`. Open the `Models` folder and observe the result of the scaffolding action:

   - Models/Exercise4Context.cs
     - inspect `DbSet` collections that represent records of the database
     - inspect `OnConfiguring()` and hardcoded connection
     - inspect `OnModelCreating()` and database schema configuration (generated)
   - Models/Notification.cs
     - inspect generated class

6. Register EF context in DI container

   When your solution needs database access via Entity Framework, this is also what you need to do. However, in case there are multiple projects, this part is done only in the **startup** project. You need to configure connection string in your settings and get rid of the hardcoded connection string. Also, you need to add dbcontext to services for DI container to resolve your db context properly.

   - open appsettings.json configuration file and add "ConnectionStrings" section to the configuration, with your connection string setting (change if needed)

     ```
     {
       ...
       "AllowedHosts": "*",
       "ConnectionStrings": {
         "Exercise4ConnStr": "server=.;Database=Exercise4;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
       }
     }
     ```

   - use that configuration in OnConfiguring() instead of hardcoded connection string; you can also remove warning

     ```
     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Exercise4ConnStr");
     ```

   - `Program.cs`: add dependency injection container registration for the database context

     > We will talk about dependency injection in our incoming lectures and use it in our further exercises.
     >
     > Add the following code block after lines that start with `builder.Services...`

     ```
     builder.Services.AddDbContext<Exercise4Context>(options => {
         options.UseSqlServer("name=ConnectionStrings:Exercise4ConnStr");
     });
     ```

     Now the project is ready for database access.

7. You will now create controller that can access database data via EF context. Read the instructions.

   - For your controller to be able to work with database, you need to receive context parameter via constructor. Framework will ensure that you get the contex parameter - it uses dependency injection for that.

   - Create a new API controller with read/write actions named `NotificationsController`
   - Add constructor to that API controller, with a single `Exercise4Context context` parameter; store that parameter to the local readonly field.

     ```
     private readonly Exercise4Context _context;

     public NotificationsController(Exercise4Context context)
     {
         _context = context;
     }
     ```

     > Note: this is typical handling of parameters passed by dependency injection. You get them via constructor and store them as a class member.

8. Retrieve data and return it to client:

   - use `Get()` action for that

     ```
     [HttpGet]
     public ActionResult<IEnumerable<Notification>> Get()
     {
         try
         {
             return Ok(_context.Notifications);
         }
         catch (Exception ex)
         {
             return StatusCode(500, ex.Message);
         }
     }
     ```

   - test the new endpoint in Swagger
   - use debugger to see what is exactly happening in the action

### 4.2 Updating Model when Database Structure Changes

Normally you would change database structure or part of it as an incremental change, while requirements are changed. This can include modifying or removing existing tables, adding new tables, changing keys etc. You need to update the database model in order to reflect your changes.

1. Update database table structure

   - Remove `UpdatedAt` column from the table
   - Change `Body` column to be of type `nvarchar(2048) NOT NULL`
   - Add `Priority` column of type `int NULL`

2. Regenerate models

   - Regenerate models for the database - use additional `--force` flag to overwrite models
     ```
     dotnet ef dbcontext scaffold "server=.;Database=Exercise4;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
     ```
     > Notes: 
     > - without `--force` flag you will get response like "The following file(s) already exist in directory", and no model or context will be refreshed
     > - after the update, database context class will again contain hardcoded connection string. To avoid that, you should be able to use parameterized connection string
     ```
     dotnet ef dbcontext scaffold "name=ConnectionStrings:Exercise4ConnStr" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
     ```
   - Supported flags:
     - `--context NewDatabaseContext` - create another database context
     - `--data-annotations` - instead of a fluent API, use validation annotations to create a model
     - `--no-build` - prevents code build before executing a command
     - `--verbose` - print details of the command execution
     - `--help` - print help
