# MVC Architecture

This material is part of Learning Objective 4 (minimum).

## 11 Overview

-   Using Query Strings
    -   Filtering
    -   Sorting
    -   Paging
-   Using Cookies: https://positiwise.com/blog/how-to-use-cookies-in-asp-net-core
-   Using Session: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-8.0
-   Using TempData

### 11.1 Exercise setup

**SQL Server Setup**

Do the following in SQL Server Management Studio:

-   use the script to create database, its structure and some test data: https://pastebin.com/jtJfak9E

**Project starter**

> The following is already completed as a project starter:
>
> -   Model and repository setup
> -   Launch settins setup
> -   Created basic CRUD views and functionality (Genre, Artist, Song)
> -   Implemented validation and labaling using viewmodels
>
> For details see the previous exercises.

Unpack the project starter archive and open the solution in the Visual Studio.  
Set up the connection string and run the application.  
Check if the application is working (e.g. navigation, list of songs, adding a new song).

> In case it's not working, check if you completed the instructions correctly.

### 11.2 Query Strings: Passing Data as Part of URL Query String

When you expect of your URL to contain state, query string is usualy a good candidate to store that state.  
For example, let's create an endpoint that will return only songs with duration between particular two values.  
Use `Index()` method as a template, because it returns list of songs and we need that.  
You also neet two values to constrain song duration by:

-   min
-   max

Make them nullable, so the user can opt to use only min or only max duration.

> Note: we "borrow" `Index` view here.

```
public ActionResult GetSongsByDuration(int? min, int? max)
{
    try
    {
        IEnumerable<Audio> songs = _context.Audios
            .Include(x => x.Genre)
            .Include(x => x.Artist);

        if (min.HasValue) {
            songs = songs.Where(x => x.Duration >= min.Value);
        }

        if (max.HasValue)
        {
            songs = songs.Where(x => x.Duration <= max.Value);
        }

        var songVms =
            songs.Select(x => new SongVM
            {
                Id = x.Id,
                Title = x.Title,
                Year = x.Year,
                ArtistId = x.ArtistId,
                ArtistName = x.Artist.Name,
                GenreId = x.GenreId,
                GenreName = x.Genre.Name,
                Duration = x.Duration,
                Url = x.Url
            })
            .ToList();

        return View("Index", songVms);
    }
    catch (Exception ex)
    {
        throw ex;
    }
}
```

Test it:

-   http://localhost:6555/Song/GetSongsByDuration?min=194&max=249
-   http://localhost:6555/Song/GetSongsByDuration?min=194
-   http://localhost:6555/Song/GetSongsByDuration?max=249

### 11.3 Example: Filtering, Sorting and basic Paging Collections

The same principle can be used to filter results, and even sort them in the same request. Also, after filtering and sorting you can take just one slice of the result to avoid retrieving entire resultset.

Examples:

-   http://localhost:6555/Song/GetSongsByDuration?q=best
-   http://localhost:6555/Song/GetSongsByDuration?sortby=name
-   http://localhost:6555/Song/GetSongsByDuration?page=1&count=10
-   http://localhost:6555/Song/GetSongsByDuration?q=the&sortby=genre&page=1&count=10

For input filtering parameter `string q`, you could simply filter song Title like this:

```
if (!string.IsNullOrEmpty(q))
{
    songs = songs.Where(x => x.Title.Contains(q));
}
```

Ordering (sorting) input parameter `string sortby` could be used like this:

```
switch (orderBy.ToLower())
{
    case "id":
        songs = songs.OrderBy(x => x.Id);
        break;
    case "title":
        songs = songs.OrderBy(x => x.Title);
        break;
    //...year, duration, genre, artist...
}
```

Paging parameters `page` and `count` could be used like this:

```
songs = songs.Skip((page - 1) * size).Take(size); // if pages start from 1
```

In order to work incrementally on songs collection, you need to use LINQ interface that provides methods for querying database: `IQueryable<T>`

```
public ActionResult Search(string q, string orderBy = "", int page = 1, int size = 10)
{
    try
    {
        IQueryable<Audio> songs = _context.Audios
            .Include(x => x.Genre)
            .Include(x => x.Artist);

        if (!string.IsNullOrEmpty(q))
        {
            songs = songs.Where(x => x.Title.Contains(q));
        }

        //...sorting, filtering...

        return View("Index", songVms);
    }
    catch (Exception ex)
    {
        throw ex;
    }
}
```

> In order for the process to work correctly, you need to apply filtering, sorting and paging in that order.  
> You can try applying a different order and see what happens.

Finally, let's create own view for filtering and sorting, `Search.cshtml`:

-   c/p code from `Song/Index.cshtml` (you can remove Create, Edit and Delete buttons, we don't need them here)
-   add `Search` link to layout
-   add `SearchVM` model with appropriate properties (`string Q`, `string OrderBy`, `int Page`, `int Size`) and pass it to the `Search()` action
-   `SearchVM` model also needs song collection to show data: `IEnumerable<SongVM> Songs`

    ```
    public ActionResult Search(SearchVM searchVm)
    {
        try
        {
            // ...change existing code - assign model members, including song collection...

            return View(searchVm);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    ```

-   use `SearchVM` as model for `Search.cshtml`

    -   you will need to replace various references
    -   `@Html.DisplayNameFor()` will also not work
    -   you can use e.g. `@Html.DisplayNameFor(model => model.Songs.FirstOrDefault().Title)` instead

-   just before `<table>` we need a form that will use `GET` (!) HTTP method to pass query parameters to `Search()`
    ```
    <form asp-action="Search" method="GET">
        <div class="row">
            <div class="col-8">
                <input asp-for="Q" class="form-control" placeholder="Search song" />
            </div>
            <div class="col-auto">
                <input type="submit" value="Go" class="btn btn-primary" />
            </div>
        </div>
    </form>
    ```
-   test it
-   when you make sure it works, add dropdown for sorting and size to row div
    ```
    <div class="col-auto">
        <label class="form-label mt-1">Sort by:</label>
    </div>
    <div class="col-auto">
        <select asp-for="OrderBy" class="form-select">
            <option value="id">(default)</option>
            <option>Title</option>
            <option>Year</option>
            <option>Duration</option>
            <option>Genre</option>
            <option>Artist</option>
        </select>
    </div>
    <div class="col-auto">
      <select asp-for="Size" class="form-select">
          <option>10</option>
          <option>20</option>
          <option>50</option>
      </select>
    </div>
    <div class="col-auto">
      <input type="submit" value="Go" class="btn btn-primary" />
    </div>
    ```

### 11.4 Example: Implementing Full Pager

We need more data in database:

-   use this script to fill additional data: https://pastebin.com/Ms752UY3

For full pager we need some modifications:

-   we need `int FromPager` in the model to know what is the number of page we need to start the pager from
-   we need `int ToPager` in the model to know what is the number of page we need to stop the pager at
-   we also need `int LastPage` in the model for some visual cues
-   add paging buttons (see https://getbootstrap.com/docs/5.0/components/pagination/)
-   add `Paging:ExpandPages` configuration, number of pages that can be shown before and after current page
    ```
    "Paging": {
      "ExpandPages": 5
    }
    ```
-   fill required paging data into the model
    ```
    // BEGIN PAGER
    var expandPages = _configuration.GetValue<int>("Paging:ExpandPages");
    searchVm.LastPage = (int)Math.Ceiling(1.0 * filteredCount / searchVm.Size);
    searchVm.FromPager = searchVm.Page > expandPages ?
      searchVm.Page - expandPages :
      1;
    searchVm.ToPager = (searchVm.Page + expandPages) < searchVm.LastPage ?
      searchVm.Page + expandPages :
      searchVm.LastPage;
    // END PAGER
    ```
-   for this you need song count after the filtering
    ```
    var filteredCount = songs.Count();
    ```
-   at last, you can use this HTML for navigation (explore this code!)

    ```
    <nav>
        <ul class="pagination">
            @for (int i = Model.FromPager; i <= Model.ToPager; i++)
            {
                var linkText = @i.ToString();
                if (i != 1 && i == Model.FromPager)
                {
                    linkText = "«";
                }
                else if (i != Model.LastPage && i == Model.ToPager)
                {
                    linkText = "»";
                }

                var linkClass = "page-item";
                if (i == Model.Page)
                {
                    linkClass = "page-item active";
                }
                <li class="@linkClass">
                    @Html.ActionLink(
                        @linkText,
                        "Search",
                        new {
                            q = Model.Q,
                            orderby = Model.OrderBy,
                            page = i,
                            size = Model.Size },
                        new { @class = "page-link" })
                </li>
            }
        </ul>
    </nav>
    ```

### 11.5 Cookie: Storing Data in a Custom Cookie

-   reading custom cookies from request: `string value = Request.Cookies["CookieKey"]`
    -   here, `CookieKey` is actually the cookie name
-   writing custom cookies to response: `Response.Cookies.Append("CookieKey", value)`
-   write custom cookie with option:
    ```
    var option = new CookieOptions { Expires = DateTime.Now.AddDays(14) };
    Response.Cookies.Append("CookieKey", "10", option);
    ```
-   delete cookie: `Response.Cookies.Delete("CookieKey");`

-   create the `SongYear` cookie when new song is created
    ```C#
    // POST Action
    var option = new CookieOptions { Expires = DateTime.Now.AddDays(14) };
    Response.Cookies.Append("SongYear", song.Year.ToString(), option);
    ```
-   read the `SongYear` cookie when song creation form is displayed

    ```C#
    // GET Action
    var song = new SongVM();
    int.TryParse(Request.Cookies["SongYear"], out int year);
    song.Year = year == 0 ? null : year;

    return View(song);
    ```

    ```C#
    // View
    ViewBag.SongYear = Request.Cookies["SongYear"] ?? "";
    ```

-   test: next time you want to add the new song, cookie value should be used for the year

### 11.6 Cookie Example: Persisting Search Query

Use the same technique to persist the Search Query when searching.  
Let the cookie last for 15 minutes.

```C#
// Start of Search() method
if (string.IsNullOrEmpty(searchVm.Q))
{
    searchVm.Q = Request.Cookies["query"];
}
```

```C#
// Before returning the view
var option = new CookieOptions { Expires = DateTime.Now.AddMinutes(15) };
Response.Cookies.Append("query", searchVm.Q ?? "", option);
```

Observe that you can't reset the field by removing content. Reason is that your action doesn't recognize first page load and page loading when clicking on the search button.  
Remedy: add name (and value if missing) to the button to differentiate button click from loading page using URL:

```C#
// In viewmodel
public string Submit { get; set; }
```

```HTML
<input type="submit" value="Go" name="submit" class="btn btn-primary" />
```

```C#
// Replacement code for action
if (string.IsNullOrEmpty(searchVm.Q) && string.IsNullOrEmpty(searchVm.Submit))
{
    searchVm.Q = Request.Cookies["query"];
}
```

### 11.7 Session: Using Session

To use session you need to configure services and add it to the middleware pipeline.

```C#
// Program.cs
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

```C#
// Program.cs - middleware, after authorization
app.UseSession();
```

For example, one of the possibilities of using sessions is to avoid sending requests to the database. The data is saved in the session variable, which is usually part of the server's memory space, but it can be configured and thus optimized.

```C#
private List<SelectListItem> GetGenreListItems()
{
    var genreListItemsJson = HttpContext.Session.GetString("GenreListItems");

    List<SelectListItem> genreListItems;
    if (genreListItemsJson == null)
    {
        genreListItems = _context.Genres
            .Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

        HttpContext.Session.SetString("GenreListItems", genreListItems.ToJson());
    }
    else
    {
        genreListItems = genreListItemsJson.FromJson<List<SelectListItem>>();
    }

    return genreListItems;
}

// ...

ViewBag.GenreDdlItems = GetGenreListItems();
```

> You can do the same for dropdown items for selection of artist

### 11.8 TempData: Show Result of Saving the Data in Redirected Page

`TempData` value stays in memory until the data is transferred to another action.  
Values are usually strings, so JSON (de)serialization is needed.

```C#
// In GenreController, POST Create
TempData["newGenre"] = newGenre.ToJson();
```

```C#
// In GenreController, GET Index
if (TempData.ContainsKey("newGenre"))
{
  var newGenre = ((string)TempData["newGenre"]).FromJson<GenreVM>();
}
```

Value can be used directly in the Razor template as a part of view logic.

```C#
// In Genre/Index.cshtml
GenreVM newGenre = null;
if (TempData.ContainsKey("newGenre"))
{
  newGenre = ((string)TempData["newGenre"]).FromJson<GenreVM>();
}

...

@if (newGenre != null)
{
    <div class="alert alert-primary" role="alert">
        A new genre @newGenre.Name has been created.
    </div>
}
```
