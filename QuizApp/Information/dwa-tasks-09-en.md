# MVC Architecture

This material is part of Learning Objective 3 (minimum).

## 9 ASP.NET MVC and views

Views:

-   https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview?view=aspnetcore-8.0

Overview of Razor syntax:

-   https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-8.0

HTML Helpers:

-   https://learn.microsoft.com/en-us/dotnet/api/system.web.mvc.htmlhelper?view=aspnet-mvc-5.2

Tag Helpers:

-   https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-8.0

### 9.1 Exercise setup

Database setup:

-   create database `Exercise9` with the following structure

    ```SQL
    CREATE DATABASE Exercise9
    GO

    USE Exercise9
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

    CREATE TABLE Song (
      Id int NOT NULL IDENTITY (1, 1),
      [Name] nvarchar(256) NOT NULL,
      [Year] int NULL,
      GenreId int NOT NULL,
      DeletedAt datetime2(7) NULL,
      CONSTRAINT PK_Song
        PRIMARY KEY (Id),
      CONSTRAINT FK_Song_Genre
        FOREIGN KEY(GenreId)
        REFERENCES dbo.Genre (Id)
    )

    SET IDENTITY_INSERT Song ON
    GO

    INSERT INTO Song (Id, [Name], [Year], GenreId, DeletedAt)
    VALUES
      (1, 'A-ha - Take On Me', 1985, 8, NULL),
      (2, 'Tina Turner - What''s Love Got to Do with It', 1984, 8, NULL),
      (3, 'Van Halen - Jump', 1984, 1, NULL),
      (4, 'Franz Ferdinand - Take Me Out', 2004, 9, NULL),
      (5, 'DJ Snake - Lean On', 2015, 10, NULL),
      (6, 'Louis Armstrong - What a Wonderful World', 1967, 2, NULL),
      (7, 'Deleted Song', 1967, 2, '2024-04-27 11:41:00')
    GO

    SET IDENTITY_INSERT Song OFF
    GO
    ```

Solution setup:

-   Create MVC solution without HTTPS support

Model and repository setup:

-   Install EF packages into the project
    ```
    dotnet add package Microsoft.EntityFrameworkCore --version 7
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 7
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 7
    ```
    > Don't forget to `cd` into the project folder!
-   Configure EF connection string in `appsettings.json`
    ```JSON
    "ConnectionStrings": {
      "ex9cs": "server=.;Database=Exercise9;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
    }
    ```
    > Set up the correct connection string!
-   Reverse engineer database and set up service in `Program.cs`

    ```
    dotnet ef dbcontext scaffold "Name=ConnectionStrings:ex9cs" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
    ```

    > If `dotnet ef` isn't installed, install the tool! 
    > - `dotnet tool install --global dotnet-ef --version 7`  
    > You may want to restart Visual Studio if in the classroom.

    ```C#
    builder.Services.AddDbContext<Exercise9Context>(options => {
        options.UseSqlServer("name=ConnectionStrings:ex9cs");
    });
    ```

Launch settings setup:

-   Set port to 6555

Add controllers:

-   Use "MVC Controller with read/write actions" template for creating controllers
-   Controller names: `GenreController`, `SongController`
-   Pass db context to controller using constructor DI in **both** controllers.

    -   Example for `GenreController`:

        ```C#
        private readonly Exercise9Context _context;

        public GenreController(Exercise9Context context)
        {
            _context = context;
        }
        ```

Index action:

-   For `GenreController` use `Index()` action to display genres

    -   Add empty Razor view `Index.cshtml`
    -   Use `ViewBag` to pass genres to the view

        ```
        public ActionResult Index()
        {
            ViewBag.Genres = _context.Genres;

            return View();
        }
        ```

    -   In `Index.cshtml` get genres from ViewBag
        ```
        @{
            var genres = ViewBag.Genres as IEnumerable<Genre>;
        }
        ```
    -   In `Index.cshtml`, generate HTML code from that data - use a simple ul/li HTML list
        ```HTML
        <ul class="genre-list">
            @foreach (var genre in genres)
            {
                <li>
                    @genre.Name: @genre.Description
                </li>
            }
        </ul>
        ```

-   For `SongController` use `Index()` action to display songs
    -   for each song display a song name and a year the song was released
    -   example: "A-ha - Take On Me (1985)"

Update navigation in layout page:

-   in \_Layout.cshtml, add navigation links to genre and song list page.  
    Example for `Genre` controller:
    ```HTML
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Genre" asp-action="Index">Genres</a>
    </li>
    ```

Test navigation and pages.

### 9.2 Create view with CSS styling

Here you will support a particular style in view where you need it.

-   Add named section to the layout

    ```C#
    @await RenderSectionAsync("Styles", required: false)
    ```

    > This is the way to add the code placeholder to the layout, where you want to render something from the view.

-   Add style for genre/index: add subfolder wwwroot/css/genres, and add file `index.css` to it

    ```CSS
    ul.genre-list {
        list-style-type: none;
        margin: 0;
        padding: 0;
    }

        ul.genre-list li {
            border: 1px solid black;
            margin: 1em;
            padding: 1em;
            color: #fff;
            background: rgb(2,0,36);
            background: linear-gradient(0deg, rgba(2,0,36,1) 0%, rgba(150,174,180,1) 100%);
            box-shadow: rgba(150,174,180,1) 5px 5px 5px;
        }
    ```

-   Add style link into the view - it will be rendered in the layout
    ```HTML
    @section Styles {
        <link rel="stylesheet" href="~/css/genres/index.css" />
    }
    ```
-   For styling the songs page, you can use the exactly same workflow: add stylesheet file and a link into the template for songs `Index.cshtml`

### 9.3 Use ViewData and ViewBag to pass data from controller to view

-   whatever you want to pass to the view, you can do it in action either via `ViewData` or `ViewBag`
-   for example, in the action code, passing data from action to view
    ```C#
    ViewData["genres"] = _context.Genres;
    //...is equivalent to...
    ViewBag.Genres = _context.Genres;
    ```
-   for example, referencing data in the view
    ```C#
    @{
      var genres = ViewData["genres"] as IEnumerable<Genre>;
      //...is equivalent to...
      var genres = ViewBag.Genres as IEnumerable<Genre>;
    }
    ```
-   then, rendering HTML is easy
    ```HTML
    <ul class="genre-list">
        @foreach (var genre in genres)
        {
        <li>@genre.Name: @genre.Description</li>
        }
    </ul>
    ```

### 9.4 Render data as common HTML form elements

Use `Index` action of `Song` controller to render some common HTML form elements.

Action:

```C#
ViewBag.Songs = _context.Songs;
ViewBag.ExampleText = "Some text";
ViewBag.ExampleNumber = 1987;
ViewBag.Genres = _context.Genres;
```

Razor template:

```HTML
@{
  var genres = ViewBag.Genres as IEnumerable<Genre>;
}

<!-- ...Razor code for song list... -->

<hr />

<form>
    <label>Text input: <input type="text" value="@ViewBag.ExampleText"></label><br />
    <label>Numeric input: <input type="number" value="@ViewBag.ExampleNumber"></label><br />
    <label>Select genre:
        <select>
            @foreach (var genre in genres)
            {
                <option value="@genre.Id">@genre.Name</option>
            }
        </select>
    </label>
</form>
```

### 9.5 Render data as HTML form elements using HTML helper

HTML helpers can reduce amount of code you write to display HTML in your template. There are helpers that you can just use, like `@Html.TextBox()`. There are others that you need to _feed_ with specific object instances, like `@Html.DropDownList()` that use collection of `SelectListItem` instances.

So, let's add a collection of `SelectListItem` object instances in `Index` action of `Song` and use that in template.

```C#
// Action
ViewBag.Songs = _context.Songs;
ViewBag.ExampleText = "Some text";
ViewBag.ExampleNumber = 1987;
ViewBag.Genres = _context.Genres;
ViewBag.GenreDropDownItems =
    _context.Genres.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
```

```HTML
<!-- ...existing Razor code... -->

<hr />

@using(Html.BeginForm())
{
    <text>Text input: </text>
    @Html.TextBox("ExampleText")<br />
    <text>Numeric input: </text>
    @Html.TextBox("ExampleNumber")<br />
    <text>Select genre: </text>
    @Html.DropDownList("GenreDropDownItems")
    <button type="submit">Send data</button>
}
```

### 9.6 Sending Data to Server using HTML Form

Remember:

-   Data is sent to server using `method` attribute specified in `<form>` tag (`GET` or `POST`)
-   Data is sent to servers endpoint that is specified in `action` attribute of the `<form>` tag
-   Only data that is inside form elements with `name` attributes is sent to server
-   Example:
    ```HTML
    <form action="/Song/Index" method="POST">
      <input type="text" name="Example"><!--This is sent to server, key is Example--><br />
      <input type="text"><!--This is NOT sent to server--><br />
      <select name="SelectedGenre"><!--This is sent to server, key is SelectedGenre-->
        <option value="1">One</option>
        <option value="2">Two</option>
      </select>
      <button type="submit">Send data</button>
    </form>
    ```

Let's write the code that can send/submit the data:

-   add action attribute to form: "/Genre/Index"
-   add method attribute to form: "POST"
-   add name attributes to form elements
-   add a submit button to form
-   example:

```HTML
<form action="/Song/Index" method="POST">
  <label>Text input: <input type="text" name="ExampleTxt" value="@ViewBag.ExampleText"></label><br />
  <label>Numeric input: <input type="number" name="ExampleNum" value="@ViewBag.ExampleNumber"></label><br />
  <label>
    Select genre:
    <select name="SelectedGenre">
      @foreach (var genre in genres)
      {
        <option value="@genre.Id">@genre.Name</option>
      }
    </select>
  </label>
  <button type="submit">Send data</button>
</form>
```

The POST request needs to be processed by the appropriate action:

```C#
[HttpPost]
public ActionResult Index(string ExampleTxt, int ExampleNum, string SelectedGenre)
{
    return RedirectToAction();
}
```

### 9.7 Using HTML Helpers to reduce code

Let's see how using HTML helpers reduce the code. For HTML helpers naming is important.  
**Same name is used both for ViewBag key to display the value and the generated `name` attribute.**

```C#
using(Html.BeginForm())
{
    <text>Text input: </text>
    @Html.TextBox("ExampleText")<!--The name attribute is ExampleText--><br />
    <text>Numeric input: </text>
    @Html.TextBox("ExampleNumber")<!--The name attribute is ExampleNumber--><br />
    <text>Select genre: </text>
    @Html.DropDownList("GenreDropDownItems")<!--The name attribute is GenreDropDownItems-->
    <button type="submit">Send data</button>
}
```

```C#
[HttpPost]
public ActionResult Index(string ExampleText, int ExampleNumber, int GenreDropDownItems)
{
    return RedirectToAction();
}
```

> Compare the amount of code when not using HTML helpers and when using HTML helpers.
>
> The following HTML helpers are supported:
>
> Html.ActionLink() renders `<a></a>`  
> Html.TextBox() / Html.TextBoxFor() renders `<input type="textbox">`  
> Html.TextArea() / Html.TextAreaFor() renders `<input type="textarea">`  
> Html.CheckBox() / Html.CheckBoxFor() renders `<input type="checkbox">`  
> Html.RadioButton() / Html.RadioButtonFor() renders `<input type="radio">`  
> Html.DropDownList() / Html.DropDownListFor() renders `<select><option>...</select>`  
> Html.ListBox() / Html.ListBoxFor() renders multi-select `<select><option>...</select>`  
> Html.Hidden() / Html.HiddenFor() renders `<input type="hidden">`  
> Html.Password() / Html.PasswordFor() renders `<input type="password">`  
> Html.Label() / Html.LabelFor() renders `<label>`  
> Html.Editor() / Html.EditorFor() renders Html controls based on data type of specified model property e.g. textbox for string property, numeric field for int, double or other numeric type.  
> Html.Display() / Html.DisplayFor() renders text instead of Html controls, data that specified model property contains.

### 9.8 Strongly Typed View

Preferred way of passing data to the view is to use a strongly typed view.  
For that you need a model class. Usually we refer to these kind of models as _viewmodels_. We name them like `GenreViewModel` or `GenreVM`. There are other naming conventions also.

-   create folder `ViewModels`
-   create the following classes inside that folder

    ```C#
    public class GenreVM
    {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
    }
    ```

    ```C#
    public class SongVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Year { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public bool IsActive { get; set; }
    }
    ```

-   for `Genre` controller, `Details` action, add empty view `Details.cshtml`
-   in details action, get a genre from database by parameter `id` and pass it to the strongly typed view

    ```C#
    public ActionResult Details(int id)
    {
      var genre = _context.Genres.FirstOrDefault(x => x.Id == id);

      // Map to VM
      var genreVM = new GenreVM
      {
        Id = genre.Id,
        Name = genre.Name,
        Description = genre.Description,
      };

      return View(genreVM);
    }
    ```

-   for that to work, you need to expect that type in the view itself - use `@model` Razor keyword on the very top of the `Details.cshtml` view
    ```C#
    @model exercise_8_1.ViewModels.GenreVM
    ```
-   now you can use that model in various ways
    -   with `@Model` reference
    -   with HTML Helpers
    -   with Tag Helpers
-   for example, using `@Model` reference:

    ```HTML
    @model exercise_8_1.ViewModels.GenreVM

    <p>@Model.Name</p>
    <p>@Model.Description</p>
    ```

-   use http://localhost:6555/Genre/Details/1 to view result of this action

-   you can do the same for `Details` action of `Song` controller

    -   create `Details.cshtml`
    -   in action: get song by id, map it to VM object, pass it to the view
    -   example:

    ```C#
    public ActionResult Details(int id)
    {
      var song = _context.Songs.Include(x => x.Genre).FirstOrDefault(x => x.Id == id);

      // Map to VM
      var songVM = new SongVM
      {
        Id = song.Id,
        Name = song.Name,
        Year = song.Year,
        GenreId = song.GenreId,
        GenreName = song.Genre.Name,
        IsActive = !song.DeletedAt.HasValue,
      };

      return View(songVM);
    }
    ```

    -   in view: use `@Model` to output the song details
    -   example:

    ```HTML
    @model exercise_8_1.ViewModels.SongVM

    <p>@Model.Name (@Model.Year)</p>
    <p>Genre: @Model.GenreName</p>
    @if (!Model.IsActive)
    {
        <i>This song is deleted</i>
    }
    ```

-   use http://localhost:6555/Song/Details/1 or http://localhost:6555/Song/Details/7 to view result of this action

### 9.9 Strongly Typed View and POST action

Strongly typed view has all the information about model. If form is properly configured, it can sumbit proper model information in POST request when form is submitted.

To process the submitted data, you need to implement the POST action with the same name and model as single parameter.

-   create strongly typed view for GET `Edit` action of `Genre` controller, like you did in the previous task
    -   model is same one as in `Details.cshtml` view - `exercise_8_1.ViewModels.GenreVM`
-   use the following code to generate the form in `Edit.cshtml` view:

    ```HTML
    @using (Html.BeginForm())
    {
        @Html.HiddenFor(m => m.Id)
        @Html.TextBoxFor(m => m.Name)
        <br />
        @Html.TextAreaFor(m => m.Description)
        <br />
        <button type="submit">Send data</button>
    }
    ```

    > This HTML Helper code creates same form as the following Tag Helper code:
    >
    > ```HTML
    > <form asp-controller="Genre" asp-action="Edit" method="POST">
    >     <input type="hidden" asp-for="Id" />
    >     <input type="text" asp-for="Name" />
    >     <br />
    >     <textarea asp-for="Description"></textarea>
    >     <br />
    >     <button type="submit">Send data</button>
    > </form>
    > ```
    >
    > Tag Helpers are similar to HTML tags, but can be used to reduce the code required to writhe a view just as HTML Helpers.

-   copy the code from the `Details` action
    -   the code gets the data from the database and passes theat data to the view
-   use http://localhost:6555/Genre/Edit/1 to view result
-   POST action already exists, so you don't have to add it

    -   change Genre parameter type to GenreVM, as we expect the viewmodel type from the strongly typed view
    -   add a breakpoint to action start and test the action, inspect parameters
        -   parameter `id` should be bound to data, same as entire `Genre` model
    -   implement updating data in the database

    ```C#
    var dbGenre = _context.Genres.FirstOrDefault(x => x.Id == id);
    dbGenre.Name = genre.Name;
    dbGenre.Description = genre.Description;

    _context.SaveChanges();

    return RedirectToAction(nameof(Index));
    ```

-   use http://localhost:6555/Genre/Edit/1 to test the edit functionality

### 9.10 Autogenerating Strongly Typed View - Create view

It's easy to autogenerate the strongly typed view for the particular model.

-   go to `Create` action of `Genre` controller, right click and "Add View"
-   select "Razor View"
    -   NOT "Razor View - Empty"
-   leave "Name" as `Create`
-   select "Template" as `Create` (for another action you would select another template)
-   for "Model class" select your `GenreVM` viewmodel
-   leave "DbContext class" empty
-   leave other properties as default

Now the create cshtml view opens. Inspect it.

> Notice the Tag Helpers.

When you run your app and go to http://localhost:6555/Genre/Create, it shows empty fields that need to be filled.

> That template is crude. You are expected to use knowledge of CSS formatting and HTML editing to customize the auto-generated template visually and make it more appealing.

Implement POST action.

-   input parameter should be of `GenreVM` type
-   add the mapped genre to database context and save the context

```C#
  var newGenre = new Genre
  {
    Name = genre.Name,
    Description = genre.Description,
  };

  _context.Genres.Add(newGenre);

  _context.SaveChanges();

  return RedirectToAction(nameof(Index));
```

-   try the `Create` action: http://localhost:6555/Genre/Create

> Note that action accepts `Id` parameter; it should be removed from the `Create.cshtml` template since it's automatically generated by the database

### 9.11 Exercise: Autogenerating all Strongly Typed Views for Song

For `Song` controller, autogenerate `Create`, `Edit`, `Delete` and `Details` views.

> You can also autogenerate `List` view, but you will need to overwrite the Index.cshtml that you created.

Use `SongVM` viewmodel and don't use database context.  
Inspect each autogenerated view.

> Hints:
>
> -   remove input Id form group from `Create` view
> -   in `Edit` view, make input Id as `type="hidden"`, move it outsige the form-group and delete the form-group
> -   in `Create` and `Edit`, `GenreId` is generated as free input, and we need to make that input a dropdown using `<select>` tag
>     -   in GET actions, retrieve all the genres from the database as collection of `SelectListItem` instances
>         ```C#
>         ViewBag.GenreSelect =
>           _context.Genres.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
>         ```
>     -   in views, remove `GenreName`
>     -   in views, instead of `<input>`, make `GenreId` a `<select>`
>     -   that `<select>` must not be self-closing, and must have a closing tag
>     -   in `<select>` tag, instead of `class="form-control"` use `class="form-select"`
>     -   use `asp-items` to pass data from ViewBag for rendering `<option>` tags
>         ```HTML
>         <select asp-for="GenreId" asp-items="@ViewBag.GenreSelect" class="form-select"></select>
>         ```
>         ```HTML
>         <select asp-for="GenreId" asp-items="@ViewBag.GenreSelect" class="form-select">
>           <option>(select genre)</option>
>         </select>
>         ```
> -   in `Delete`, `GenreId` is not important, but we need `GenreName`

Test:

-   http://localhost:6555/Song/Index
-   http://localhost:6555/Song/Details/1
-   http://localhost:6555/Song/Create
-   http://localhost:6555/Song/Edit/1
-   http://localhost:6555/Song/Delete/1

### 9.12 Exercise: Using Autogenerated Strongly Typed Views for CRUD Features

Implement POST actions for each of the `Create`, `Edit` and `Delete` functionalities for `Song` controller.

### 9.13 Exercise: Customizing Autogenerated Views

Customize all the auto-generated templates visually to make them more appealing.