# RESTful architecture (Web API)

This material is part of Learning Objective 2 (minimum).

## 5 State Storage (cont.)

Context in Entity Framework:

-   https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/

Navigation properties in Entity Framework:

-   https://learn.microsoft.com/en-us/ef/core/modeling/relationships
-   https://learn.microsoft.com/en-us/ef/core/modeling/relationships/navigations

Model Validation in ASP.NET Web API:

-   https://learn.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api
-   https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-8.0

### 5.1 Exercise setup

To review the last exercise, the following steps have been done to set up the database access in the project:

-   you ensured that `dotnet ef` tool is installed
-   you installed required NuGet packages into your project
-   you used `dotnet ef dbcontext scaffold` command to generate database context and models
-   you configured the connection string in the configuration
-   you registered required type (database context) in the dependency injection
-   you used the database context in your controller to retrieve the data

Use the project that you created and set up during the last exercise. It's available on Infoeduka.

> Note that you will have to set up the database again (delete/create) and use the script from the last exercise to re-create the database structure. Also, use the script found in PasteBin URL to fill the table with data.
>
> You will need to properly configure the database access using appsettings.json configuration file.
>
> Your project and database are prepared for the database access when you succesfully use get endpoint to retrieve data from the database.

Additionally, let's add one more table with 1-to-N relation to the existing table into the database. The following script will add both the table and the relation to the existing table to the database. And fill it with data. Also, it will add references from `Notification` to `NotificationType`.

```SQL
CREATE TABLE dbo.NotificationType (
  Id int NOT NULL IDENTITY (1, 1),
  [Name] nvarchar(256) NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE dbo.NotificationType
ADD CONSTRAINT PK_NotificationType PRIMARY KEY CLUSTERED (
  Id
)
GO

ALTER TABLE dbo.[Notification]
ADD NotificationTypeId int NULL
GO

ALTER TABLE dbo.[Notification]
ADD CONSTRAINT FK_Notification_NotificationType
FOREIGN KEY (NotificationTypeId)
REFERENCES dbo.NotificationType(Id)
GO

SET IDENTITY_INSERT [dbo].[NotificationType] ON
GO
INSERT [dbo].[NotificationType] ([Id], [Name]) VALUES (1, N'Normal')
INSERT [dbo].[NotificationType] ([Id], [Name]) VALUES (2, N'Prioritized')
INSERT [dbo].[NotificationType] ([Id], [Name]) VALUES (3, N'Urgent')
GO
SET IDENTITY_INSERT [dbo].[NotificationType] OFF
GO

UPDATE [Notification]
SET NotificationTypeId = 3
WHERE CreatedAt BETWEEN '2023-03-01' AND '2023-03-02'
GO

UPDATE [Notification]
SET NotificationTypeId = 2
WHERE CreatedAt BETWEEN '2023-03-02' AND '2023-03-05'
GO

UPDATE [Notification]
SET NotificationTypeId = 1
WHERE NotificationTypeId IS NULL
GO
```

Finally, use the `dotnet ef` command to regenerate models. Pay attention to set up the correct connection string. Also, pay attention to run the command in the project folder.

```
dotnet ef dbcontext scaffold "server=.;Database=Exercise4;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
```

### 5.2 CRUD Implementation

Basic CRUD implementation is essentially the same as when using static field. The important difference is saving changes to database after performing changes.

Implement CRUD for `NotificationType` table. Don't forget to use constructor to get database context from DI container (see how it's done in `Notification` controller).

1. Implement `Get()` action
    - instead of static member use `_context.NotificationTypes` as collection
2. Implement `Get(int id)` action
    - instead of static member use `_context.NotificationTypes` as collection
3. Implement `Post([FromBody] NotificationType value)` action
    - you don't need to calculate the next id, the database will do that for you (see: `IDENTITY(1,1)`)
    - after adding new notification type, call `_context.SaveChanges()`
4. Implement `Put(int id, [FromBody] NotificationType value)` action
    - after modifying notification type, call `_context.SaveChanges()`
5. Implement `Delete(int id)` action
    - after removing notification type, call `_context.SaveChanges()`

> Observe that there is a `notifications` member in the `NotificationType` class. This is a **navigation property**. It's sometimes a useful feature, but sometimes we don't need it
>
> -   when we want to see the related data, we need that navigation property and then we need to **include** that referenced data
> -   when we don't want to see the related data, we need to map data to DTO that does not have the navigation property

### 5.3 Including referenced data

To include the referenced data into the resultset, use `.Include()`

```C#
[HttpGet("{id}")]
public ActionResult<NotificationType> Get(int id)
{
    try
    {
        var result =
            _context.NotificationTypes
                .Include(x => x.Notifications)
                .FirstOrDefault(x => x.Id == id);

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}
```

> Note: when you do this, Swagger will show you the error: `A possible object cycle was detected.` The reason is because of the serialization of the object cycle. `In Program.cs`, you need to instruct ASP.NET Web API to ignore object referencing cycles.
>
> ```
> builder.Services.AddControllers().AddJsonOptions(x =>
>   x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
> ```

Now, edit `NotificationsController` to get data and include referenced `NotificationTypes` data.

> Note that it makes more sense to include `NotificationType` into `Notification` data than vice versa.

### 5.4 Using DTO to exclude referenced data from resultset

To exclude the referenced data, you will have to create the class that doesn't have the data you want to exclude. The class is usually referenced as **Data Transfer Object** or DTO.

```C#
// Create this e.g. in new folder DTOs
public class NotificationTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// You need this in your controller
[HttpGet]
public ActionResult<IEnumerable<NotificationTypeDto>> Get()
{
    try
    {
        var result = _context.NotificationTypes;
        var mappedResult = result.Select(x =>
            new NotificationTypeDto
            {
                Id = x.Id,
                Name = x.Name
            });

        return Ok(mappedResult);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}

[HttpGet("{id}")]
public ActionResult<NotificationTypeDto> Get(int id)
{
    try
    {
        var result =
            _context.NotificationTypes
                .FirstOrDefault(x => x.Id == id);

        var mappedResult = new NotificationTypeDto
        {
            Id = result.Id,
            Name = result.Name
        };

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}
```

### 5.5 Using DTO classes to for creation, modification and deletion

Using EF created classes with the navigation properties as inputs for creation, modification and deletion is bad. You should rather send DTO classes as inputs and then map these classes to EF created classes.

So, you should bear in mind:

-   Get data collection: map collection of EF classes to collection of DTO classes
-   Get one data: map EF class to DTO class
-   Create data: accept DTO class, map it to EF class
-   Update data: accept DTO class, map it to EF class
-   Delete data: no mapping

> Note: using DTO as a class that conveys the data between client and server is **always** a good idea. Even when you need the navigation properties - you can also map the navigation properties the same way.

### 5.6 Using DTO classes for validation

DTO classes are also used for validation.  
You can use DataAnnotation namespace for that.  
Attributes in DataAnnotation namespace are used to mark validated fields in DTO.

```C#
public class NotificationTypeDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "You need to enter the name")]
    public string Name { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required(ErrorMessage = "Receiver is required.")]
    public string Receiver { get; set; } = null!;

    [Required(AllowEmptyStrings = true)]
    [StringLength(256, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
    public string? Subject { get; set; }

    [StringLength(2048, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
    public string Body { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    [Range(1, int.MaxValue)]
    public int? NotificationTypeId { get; set; }
}
```

To validate the DTO, you have to:

-   turn off automatic error 400

```
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
  options.SuppressModelStateInvalidFilter = true;
});

```

-   check `ModelState.IsValid` property in your action

```
[HttpPost]
public ActionResult<NotificationTypeDto> Post([FromBody] NotificationTypeDto value)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newNotificationType = new NotificationType
        {
            Name = value.Name,
        };

        _context.NotificationTypes.Add(newNotificationType);

        _context.SaveChanges();

        value.Id = newNotificationType.Id;

        return Ok(value);
    }
    catch (Exception)
    {
        return BadRequest();
    }
}

[HttpPost]
public ActionResult<NotificationDto> Post([FromBody] NotificationDto value)
{
    if(!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var newNotification = new Notification
    {
        UpdatedAt = value.UpdatedAt,
        Receiver = value.Receiver,
        Subject = value.Subject,
        Body = value.Body,
        SentAt = value.SentAt,
        NotificationTypeId = value.NotificationTypeId,
    };

    _context.Notifications.Add(newNotification);

    _context.SaveChanges();

    value.Id = newNotification.Id;

    return value;
}
```

### 5.7 Implementing Searching, Sorting and Paging

Search operation with sorting and paging can be implemented in the same way we did for static collection.

> _NOTE: you don't need to specify `StringComparison.OrdinalIgnoreCase` for comparison, ignoring case in text comparison is a default for SQL Server._

### 5.8 Exercise: Support CRUD for Notification table

Support CRUD for `Notification` table.
Use `NotificationDto` class as DTO to avoid navigation properties in communication between client and server.

### 5.9 Exercise: Log table

Create a new solution that supports logging to database.

Log table consists of:

-   identifier (autogenerated, primary key)
-   timestamp (date and time),
-   log level (number from 1 to 5),
-   message (text, 1024 characters)
-   error text (text, maximum supported number of characters)

Create `LogController` with actions:

-   Post(Log log) - adds log into the database table
-   Post(Log[] logs) - adds multiple logs into the database table
-   Delete(int n) - deletes first n logs
-   Get(int n, int orderBy) - retrieves last n logs, ordered by ID, timestamp or message

### 5.10 Exercise: Database of Addresses

Support address database with following entities:

-   Street (CRUD)
-   HouseNumber (CRUD)

Use DTOs.
Use validation in DTOs.
