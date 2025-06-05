# MVC Architecture

This material is part of Learning Objective 4 (minimum).

Validation attributes:

-   https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-8.0#built-in-attributes

## 10 MVC Architecture and Model support

-   ViewModel
-   Model with related data
-   Labeling attributes
-   Model validation and validation attributes
-   Server model validation
-   Client model validation
-   Validation labels and validation summary

### 10.1 Exercise setup

Database setup: create database `Exercise10` with the following structure.

```SQL
CREATE DATABASE Exercise10
GO

USE Exercise10
GO

CREATE TABLE Genre (
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [Name] [nvarchar](256) NOT NULL,
  [Description] [nvarchar](max) NOT NULL,
  PRIMARY KEY ([Id])
)
GO

SET IDENTITY_INSERT Genre ON
GO

INSERT INTO Genre (Id, [Name], [Description])
VALUES
  (1, 'Rock', 'Otherwise known as ‘Rock & Roll,’ rock music has been a popular genre since the early 1950s.'),
  (2, 'Jazz', 'Identifiable with blues and swing notes, Jazz has origins in European and West African culture.'),
  (3, 'Electronic Dance Music', 'Typically referred to as EDM, this type of music is created by DJs who mix a range of beats and tones to create unique music.'),
  (4, 'Dubstep', 'Dubstep is an electronic dance music subgenre that originated in the late 1990s’ in South London.'),
  (5, 'Techno', 'Techno is yet another sub-genre of electronic dance music. This genre became popular in Germany towards the end of the 1980s and was heavily influenced by house music, funk, synthpop, and futuristic fiction.'),
  (6, 'Rhythm and Blues (R&B)', 'R & B is one of the world’s top music genres combining gospel, blues, and jazz influences.'),
  (7, 'Country', 'Country music is another one of the world’s top music genres. Originating in the 1920s, Country has its roots in western music and American folk.'),
  (8, 'Pop', 'The term ‘Pop’ is derived from the word ‘popular.’ Therefore, Pop music is a genre that contains music generally favored throughout society.'),
  (9, 'Indie Rock', 'In terms of genre, Indie Rock lies somewhere between pop music and rock and roll.'),
  (10, 'Electro', 'Electro blends electronic music and hip hop to create music that is similar to disco in sound.')
GO

SET IDENTITY_INSERT Genre OFF
GO

CREATE TABLE Artist (
  Id int NOT NULL IDENTITY (1, 1),
  [Name] nvarchar(256) NOT NULL
  CONSTRAINT PK_Artist
    PRIMARY KEY (Id)
)

SET IDENTITY_INSERT Artist ON
GO

INSERT INTO Artist (Id, [Name])
VALUES
  (1, 'Tina Turner'),
  (2, 'Van Halen'),
  (3, 'DJ Snake'),
  (4, 'Louis Armstrong')
GO

SET IDENTITY_INSERT Artist OFF
GO

CREATE TABLE Song (
  Id int NOT NULL IDENTITY (1, 1),
  [Name] nvarchar(256) NOT NULL,
  [Year] int NULL,
  GenreId int NOT NULL,
  ArtistId int NOT NULL,
  DeletedAt datetime2(7) NULL,
  CONSTRAINT PK_Song
    PRIMARY KEY (Id),
  CONSTRAINT FK_Song_Genre
    FOREIGN KEY(GenreId)
    REFERENCES dbo.Genre (Id),
  CONSTRAINT FK_Song_Artist
    FOREIGN KEY(ArtistId)
    REFERENCES dbo.Artist (Id)
)

SET IDENTITY_INSERT Song ON
GO

INSERT INTO Song (Id, [Name], [Year], GenreId, ArtistId)
VALUES
  (1, 'What''s Love Got to Do with It', 1984, 8, 1),
  (2, 'The Best', 1989, 8, 1),
  (3, 'Jump', 1984, 1, 2),
  (4, 'Lean On', 2015, 10, 3),
  (5, 'What a Wonderful World', 1967, 2, 4),
  (6, 'We Have All The Time In The World', 1969, 2, 4)
GO

SET IDENTITY_INSERT Song OFF
GO
```

The following is already completed as a project starter:

> Model and repository setup (for details see previous exercise):
>
> -   Install EF packages into the project
> -   Configure EF connection string in `appsettings.json`
> -   Reverse engineer database and set up service in `Program.cs`
>
> Launch settings setup:
>
> -   Set port to 6555
>
> Create CRUD views and functionality for `GenreController`
>
> -   Use "MVC Controller with read/write actions" template for creating `GenreController`
> -   Pass db context to controller using constructor DI
> -   Create `GenreVM` (Id, Name, Description) viewmodel in `ViewModels` folder
> -   Use Add View, Razor View, Template: {required template}, Model: `GenreVM`
> -   Index, Template: List
>     -   Fix this part of Razor template to properly create action links
>         ```C#
>         @Html.ActionLink("Edit", "Edit", new { id=item.Id }) |
>         @Html.ActionLink("Details", "Details", new { id=item.Id }) |
>         @Html.ActionLink("Delete", "Delete", new { id=item.Id })
>         ```
>     -   In template, use some CSS styling or bootstrap to finetune the view
>     -   Implement GET action
> -   Details: Template: Details
>     -   Fix this part of Razor template to properly create action links
>         ```C#
>         @Html.ActionLink("Edit", "Edit", new { id = Model.Id })
>         ```
>     -   In template, use some CSS styling or bootstrap to finetune the view
>     -   Implement GET action
> -   Create: Template: Create
>     -   Update `POST` action to accept viewmodel
>     -   Implement GET and POST actions
> -   Edit: Template: Edit
>     -   Update `POST` action to accept viewmodel
>     -   Implement GET and POST actions
> -   Delete: Template: Delete
>     -   Update `POST` action to accept viewmodel
>     -   Implement GET and POST actions
> -   Update navigation in layout page - in \_Layout.cshtml, add navigation link to genre list page
>     ```HTML
>     <li class="nav-item">
>         <a class="nav-link text-dark" asp-area="" asp-controller="Genre" asp-action="Index">Genres</a>
>     </li>
>     ```
>     Create CRUD views and functionality for `ArtistController` the same way as `GenreController`.

### 10.2 Viewmodel: Create and setup controller for Song entity from the CRUD template

Use "MVC Controller with read/write actions" template for creating `SongController`.  
Pass db context to controller using constructor DI.  
Create `SongVM` viewmodel in `ViewModels` folder:

-   int Id
-   string Name
-   int Year
-   int GenreId
-   int ArtistId

### 10.3 Implement Index action to show the list of songs

On `SongController`'s `Index` action, right-click and select:

-   Add View, Razor View
-   Template: List
-   Model: `SongVM`

Implement GET action.

> HINT: you can use code already available in `GenreController` and adapt it to work on `SongController`.

Update navigation in layout page - in \_Layout.cshtml, add navigation link to song list page.

### 10.4 Fix view for list of songs

There are some problems with view:

-   there are "naked" unstyled links
-   there is `Id` in the table

To fix them, see `Views > Artists > Index.cshtml`.

### 10.5 Replace foreign key ids with related display values

We have model with data related to another model - `Song` is related to `Genre` and `Artist`.

Let's first solve the problem where `Genre` and `Artist` data are in `Id` form instead of text.  
Include `Genre` and `Artist` entities into `Linq` query.  
Then expand song viewmodel to include strings for `Genre` and `Artist` (GenreName, ArtistName).

```C#
var songVms = _context.Songs
    .Include(x => x.Genre)
    .Include(x => x.Artist)
    .Select(x => new SongVM
        {
            Id = x.Id,
            Name = x.Name,
            Year = x.Year ?? 0,
            ArtistId = x.ArtistId,
            ArtistName = x.Artist.Name,
            GenreId = x.GenreId,
            GenreName = x.Genre.Name,
    })
    .ToList();
```

Now you can replace `GenreId` with `GenreName` in `Song/Index.cshtml`.  
Do the same for `Artist`.

> Note: you need to do it in two places in view - heading (label) and data.

### 10.6 Labeling attributes: Set up display names in viewmodel

In `Index.cshtml`, the following syntax will output data:

```C#
@Html.DisplayFor(modelItem => item.Name)
```

Following syntax will output data label:

```C#
@Html.DisplayNameFor(modelItem => item.Name)
```

By default, data label is same as the data property name.  
Using [Display] attribute, you can change data label.

```C#
public class SongVM
{
    public int Id { get; set; }
    [Display(Name = "Song Name")]
    public string Name { get; set; }
    public int Year { get; set; }
    public int GenreId { get; set; }
    [Display(Name = "Genre")]
    public string GenreName { get; set; }
    public int ArtistId { get; set; }
    [Display(Name = "Artist")]
    public string ArtistName { get; set; }
}
```

Observe the change in list of songs table header.

### 10.7 Use viewmodel display naming for other views

Add Create view (`Create.cshtml`) for `Song` entity.

-   _Add View_, _Razor View_, _Template: Create_, _Model class: SongVM_
-   we don't need Id, remove that section from HTML

Use Create template, update `POST Create` action to accept `SongVM` viewmodel.  
Add new song to the database.

-   for the new song, set properties: `Name`, `Year`, `GenreId`, `ArtistId`
-   see how it's done in `POST Create` for `GenreVM`

Now you can create a new song, but the creation is a bit cumbersome because it uses FK ids.

> When you run and test the create functionality, you will notice that `GenreName` and `ArtistName` don't make any sense, so remove them from template.  
> Also, you will replace `GenreId` and `ArtistId` with dropdowns.

### 10.8 Related data: Replace ID textboxes with dropdowns

For e.g. `GenreId`, instead of `<input>` tag you need `<select>` tag with options.

-   replace `<input>` with `<select>` tag (select tag needs to be properly closed: `<select ... ></select>`)
-   replace `class="form-control"` with `class="form-select"` to render Bootstrap select element properly
-   now you need `<option>` data to display in the dropdown itself
    -   fill that data in `GET Create`
        ```C#
        ViewBag.GenreDdlItems =_context.Genres
          .Select(x => new SelectListItem
          {
              Text = x.Name,
              Value = x.Id.ToString()
          });
        ```
    -   reference that data in Razor template, attribute `asp-items`; it will automatically generate options
        ```HTML
        <select asp-for="GenreId" asp-items="ViewBag.GenreDdlItems" class="form-select"></select>
        ```
    -   you can add "(select item)" with empty value to force user to select an item
        ```HTML
        <select asp-for="GenreId" asp-items="ViewBag.GenreDdlItems" class="form-select">
          <option value="">(select item)</option>
        </select>
        ```
-   finally, add display attribute for GenreId to viewmodel, to display a proper label (like you did in previous task)

To the same for Artist.

### 10.9 Support edit functionality

Add Edit view for `Song` entity.  
Update view (use select instead of inputs etc.)  
Update `GET Edit` action to retrieve proper data from database and pass it to the view (see how it's done in `GET Edit` for `GenreVM`).  
For `Get Edit`, also use the same `ViewBag` properties `GenreDdlItems` and `ArtistDdlItems` as you did for `GET Create`.  
Update `Edit.cshtml` view `<select>` to show items (`asp-items`).  
Update `POST Edit` action to accept `SongVM` viewmodel (see how it's done in `POST Edit` for `GenreVM`).

### 10.10 Support display/delete functionality

You can use what you already learned to support the needed functionalities.

### 10.11 Model validation and validation attributes

_Client validation_

When you create the model using autogeneration and default values, client validation is included. This means that if user doesn't provide needed values, the errors will be presented:

-   `The Song Name field is required.`
-   `The Year field is required.`

The script that is responsible for this functionality is `_ValidationScriptsPartial.cshtml`. If you **comment out** this script from template, client validation is NOT performed, and you have to rely on server validation.

Client validation is performed immediatelly on client, which means that data doesn't have to be sent to server to be validated.

_Server validation_

We did not use server validation until now. The way to do it is to check `ModelState.IsValid` flag. Depending on the flag, you either continue creating the entity instance, or stop and return user to the point where he did not enter the proper data (just return View()).

```C#
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(SongVM song)
{
    try
    {
        if (!ModelState.IsValid) {
            return View();
        }

        var newSong = new Song
        {
            Name = song.Name,
            Year = song.Year,
            GenreId = song.GenreId,
            ArtistId = song.ArtistId,
        };

        _context.Songs.Add(newSong);

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
    catch
    {
        return View();
    }
}
```

> NOTE: now, when user enters incorrect data, there are no <option> items.  
> Fill them before returning the view.

### 10.12 Appropriate model validation using validation attributes

You can use built-in validation attributes in viewmodel to automatically check what you require and give appropriate feedback in case of an error.

E.g. for `Name` and `Year`:

```C#
[Required(ErrorMessage = "There's not much sense of having a song without the name, right?")]
public string Name { get; set; }

[Range(1000, 2024, ErrorMessage = "Invalid year for a song")]
public int Year { get; set; }
```

> Note: If you **uncomment out** the validation script from template, you can see that the client validation also works.

### 10.13 Validation labels and validation summary

Following autogenerated tags are responsible for the validation feedback:

```HTML
<span asp-validation-for="Name" class="text-danger"></span>
```

Also, there is another tag that is responsible for the feedback:

```HTML
<div asp-validation-summary="ModelOnly" class="text-danger"></div>
```

Is you switch from `ModelOnly` to `All`, you will get all the validation feedback in ul/li list that you can style by your own desire.

> Note: What is `ModelOnly`? In case when you detect error, you can add a general error message that is not related to any of the model properties.
>
> ```C#
> ModelState.AddModelError("", "Failed to create song");
> ```
>
> So, `ModelOnly` will now show these kinds of errors.
>
> Important: `asp-validation-summary` works only for server validation.
