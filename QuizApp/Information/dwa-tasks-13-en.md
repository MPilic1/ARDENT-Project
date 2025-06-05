# MVC Architecture

This material is part of Learning Objective 4 (desired).

## 13 Overview

- Multi-tier architecture
- AutoMapper
- Services and repositories

### 13.1 Exercise setup

**SQL Server Setup**  

Do the following in SQL Server Management Studio:

-   download the script: https://pastebin.com/jtJfak9E
-   in the script, **change the database name to Exercise13 and use it**
-   execute it to create database, its structure and some test data
-   execute additional script: https://pastebin.com/SeHBs1BA

**Project starter**

> The following is already completed as a project starter:
>
> -   Model and repository (database context) setup
> -   Launch settins setup
> -   Created basic CRUD views and functionality (Genre, Artist, Song)
> -   Implemented validation and labeling using viewmodels
> -   Authentication and authorization
>
> For details see the previous exercises.

Unpack the project starter archive and open the solution in the Visual Studio.  
Set up the connection string and run the application.  
Check if the application is working (e.g. navigation, list of songs, adding a new song).

> In case it's not working, check if you completed the instructions correctly.

### 13.2 Setting up multi-tier architecture

In multi-tier architecture, one tier depends on another. ASP.NET solution implements this in for of projects - dependencies are projects.

- Create a `Class Library` project in your solution named `ex13.BL`. Use the same framework version.  
- This project will be the one that your main project depends on, so add it to your main project dependencies.  
- That new project will be the one that uses database directly, so install database support to that project.
  ```
  dotnet add package Microsoft.EntityFrameworkCore --version 7
  dotnet add package Microsoft.EntityFrameworkCore.Design --version 7
  dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 7
  ```
- EF connection string is still configured in `appsettings.json` of the main project, so you don't need to configure it again
- Also, `Program.cs` of the main project already contains service setup, so you don't have to do it again
- You need to reverse engineer the database context and models in your new project. **Pay attention to cd into that project, not the main one.**
    ```
    dotnet ef dbcontext scaffold "{your-full-connection-string}" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
    ```

    > Note: the name-reference here would not work since scaffold tool doesn't know about the configuration in  the main project. Use full connection string and replace it with name-reference in context afterwards.
- Remove `Models` folder with its contents from the main project
  - You will now need to replace `using` directives for `exercise_13.Models` with `ex13.BL.Models`, since your database models are in entirely different namespace that is in a new project
  - Also, get rid of the `ErrorViewModel` and its view

Test and confirm that your application still works.  
Now it's using a separate tier for database access.  

### 13.3 Using multi-tier architecture to support Web API and MVC project

One of benefits of using multi-tier architecture is to be able to reuse a tier. For example:
- ASP.NET MVC project that uses BL tier
- ASP.NET Web API project that uses BL tier

Let's create ASP.NET project that user this same BL tier. It will be ASP.NET Web API project.  
The configuration you need to have:  
- `ex13.Web` - the MVC project, depends on ex13.BL
- `ex13.Api` - the Web API project, depends on ex13.BL
- `ex13.BL` - the Business Layer 

So:
- rename the `exercise-13` project to `ex13.Web`
- add Web API project with name `ex13.Api`
- add appropriate dependency to the new Web API project
  - now both `ex13.Web` and `ex13.Api` depend on `ex13.BL`

For `ex13.Api` project:
- install database access packages
- set up connection string
- add db context to services (in `Program.cs`, use name-reference)
- add `GenreController` **Web API** controller to the project
- support database context in `GenreController`
- return database genres from `GET Get()` action

Use Swagger to make sure that you can retrieve database genres.

> To run the new project you need to either set it as a startup project or set the solution to run the currently selected project (current selection). You can also set multiple startup projects and run both Web API and MVC projects together.

Use MVC application to change the genre name.  
Use Web API application to verify that the name has been changed.  

### 13.4 AutoMapper

This kind of code is used too many times in the application:
  ```C#
  var genreVms = _context.Genres.Select(x => new GenreVM {
      Id = x.Id,
      Name = x.Name,
  }).ToList();
  ```

Here, database model is mapped to viewmodel just to pass it to view. There is more elegant solution - AutoMapper.  

Here we do the following steps to enable AutoMapper and simplify the mapping:
- install the AutoMapper package into the project
  - cd to your web project (it's still in `exercise-13` folder)
  ```
  dotnet add package AutoMapper
  ```
  > From AutoMapper version 13 you don't need a separate AutoMapper DI project installation 
- create AutoMapper mapping profile
  - add AutoMapper folder to your project
  - to that folder add MappingProfile class that inherits AutoMapper.Profile class
    ```C#
    using AutoMapper;

    namespace exercise_13.AutoMapper
    {
        public class MappingProfile : Profile
        {
        }
    }
    ```
  - create class constructor
  - inside constructor, create default mapping from `Genre` to `GenreVM`
    ```C#
    CreateMap<Genre, GenreVM>();
    ```
- add AutoMapper configuration to `Startup.cs`
  ```
  builder.Services.AddAutoMapper(typeof(MappingProfile));
  ```

Now the mapping can be simply done in this way:
- pass `IMapper` to the constructor via DI
  ```C#
  // ...
  private readonly IMapper _mapper;

  public GenreController(..., IMapper mapper)
  {
    // ...
    _mapper = mapper;
  }
  ```
- use `IMapper` instance to perfom the mapping
  ```C#
  // A single viewmodel
  var genreVm = _mapper.Map<GenreVM>(genre);

  // ...or a collection of viewmodels
  var genreVms = _mapper.Map<IEnumerable<GenreVM>>(genres);
  ```

Use AutoMapper to support mapping throughout entire GenreController.  

### 13.5 AutoMapper and naming conventions

Use AutoMapper to support mapping from `Audio` to `SongVM`.  
  ```C#
  // In MappingProfile
  CreateMap<Audio, SongVM>();
  ```

  ```C#
  // In SongController, GET Index()
  var songs = _context.Audios
    .Include(x => x.Genre)
    .Include(x => x.Artist);

  var songVms = _mapper.Map<IEnumerable<SongVM>>(songs);
  ```

See that `ArtistName` and `GenreName` are automatically mapped. That is part of the convention, for example `source.Artist.Name` is mapped to `destination.ArtistName`.

> That functionality is also named "flattening", meaning that more complex structure can be transformed to the less complex one and used as such.

### 13.6 AutoMapper and custom model member mapping

You can observe that audio tags (Audio.AudioTags) are not automatically mapped to tag ids (SongVM.TagIds). For example, song edit page enables user to select multiple tags and save them. If we use AutoMapper, it will not be mapped and we will lose that functionality.

The solution is to use AutoMapper `.ForMember()` customization option.
  ```
  CreateMap<Audio, SongVM>()
    .ForMember(dst => dst.TagIds, opt => opt.MapFrom(src => src.AudioTags.Select(x => x.TagId)));
  ```

> There are more customizations of the AutoMapper:
> - nested mapping support
> - "mapping after mapping"
> - reverse mapping
> - ignore properties
> - possibility to implement custom value resolver  
> 
> ...and a lot more: https://docs.automapper.org/en/stable/index.html

### 13.7 Services and repositories

As you already know, you should avoid writing business logic inside actions. Services and repositories are one of the possible encapsulations of code logic that would otherwise end up inside actions.  

Service is a general encapsulation of a _buisiness logic_.  
Repository is an encapsulation of _operation logic performed on data_.  
From ASP.NET Core point of view, only services are supported. Repositories are then just services that e.g. perform CRUD on data.  

Implementing and using a custom service would typically include the following steps:
- creating an interface
- creating an implementation class of the interface
- registering the service (interface + class) in DI container
- allowing DI container to pass the implementation of the interface via controller constructor injection
- using the implementation in action of the controller where you need it

### 13.8 Service implementation

Here is a "simple" example of how to use service in ASP.NET Core MVC:
- in `ex13.BL` project create folder `Services`
- create an interface IDiagnostics with the following members:
  - int CountSongs()
  - float CountTempPathFiles()
- implement the interface
  ```
  public class Diagnostics : IDiagnostics
  {
      private readonly Exercise13Context _context;

      public Diagnostics(Exercise13Context context)
      {
          _context = context;
      }

      public int CountSongs()
      {
          return _context.Audios.Count();
      }

      public float CountTempPathFiles()
      {
          var tempPath = Path.GetTempPath();
          return Directory.GetFiles(tempPath).Length;
      }
  }
  ```
- create MVC `DiagnosticsController`
- get the `IDiagnostics` parameter from DI container into the constructor
  ```
  public readonly IDiagnostics _diagnostics;

  public DiagnosticsController(IDiagnostics diagnostics)
  {
      _diagnostics = diagnostics;
  }
  ```
- create `DiagnosticsVM` model and use it in Index action of that controller to fill its data
  - int SongCount
  - int TempPathFileCount

  ```
  public IActionResult Index()
  {
      var diagVm = new DiagnosticsVM
      {
          SongCount = _diagnostics.CountSongs(),
          TempPathFileCount = _diagnostics.CountTempPathFiles()
      };

      return View(diagVm);
  }
  ```
- auto-generate the view (use `Details` template)
- add `Diagnostics` link to the layout view

When you try clicking the new "Diagnostics" navigation item, you should get the following error:
> Unable to resolve service for type 'ex13.BL.Services.IDiagnostics' while attempting to activate 'exercise_13.Controllers.DiagnosticsController'.

This is due to not registering the service in the DI container.  
The solution:
  ```
  builder.Services.AddScoped<IDiagnostics, Diagnostics>();
  ```

Try clicking the link now - it should properly display diagnostic data.

> There are three options for adding services:
> - Singleton
> - Scoped
> - Transient
>
> For details, see: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0#service-registration-methods

### 13.9 Repository implementation

This is an example of the repository implementation based on the existing code.

1. Create `AudioRepository` based on `IAudioRepository` interface and auto-generate the implementation.

    ```
    public interface IAudioRepository
    {
        public Audio GetAll();
        public Audio Get(int id);
        public Audio Add(string title, int? year, int genreId, int artistId, int duration, string url);
        public Audio Modify(int id, string title, int? year, int genreId, int artistId, int duration, string url, IEnumerable<int> tagIds);
        public Audio Remove(int id);
    }
    ```

2. Inject database context into `SongController` implementation

3. Inject `IAudioRepository` into `SongController`

4. Register pair `IAudioRepository` + `AudioRepository` as a service

5. Copy LINQ database request from `SongController.Index()` to `AudioRepository.GetAll()`

    ```
    public IEnumerable<Audio> GetAll()
    {
      var songs = _context.Audios
          .Include(x => x.Genre)
          .Include(x => x.Artist)
          .Include(x => x.AudioTags);

      return songs;
    }
    ```

6. In `SongController.Index()`, replace the LINQ database request with service method call.

    ```
    var songs = _audioRepo.GetAll();
    var songVms = _mapper.Map<IEnumerable<SongVM>>(songs);
    ```

Do steps 5+6 for other CRUD operations. _Pay attention that for creating, updating and deleting you need to implement POST actions._

**POST Example:**
  ```C#
  // In AudioRepository
  public Audio Add(string title, int? year, int genreId, int artistId, int duration, string url)
  {
    var audio = new Audio
    {
        Title = title,
        Year = year,
        GenreId = genreId,
        ArtistId = artistId,
        Duration = duration,
        Url = url
    };

    _context.Audios.Add(audio);

    _context.SaveChanges();

    return audio;
  }
  ```

  ```C#
  // In SongController.Create
  //...
  _audioRepo.Add(
      song.Title,
      song.Year,
      song.GenreId,
      song.ArtistId,
      song.Duration,
      song.Url);
  //...
  ```

**PUT Example:**
  ```C#
  // In AudioRepository
  public Audio Modify(int id, string title, int? year, int genreId, int artistId, int duration, string url, IEnumerable<int> tagIds)
  {
      var audio = _context.Audios.Include(x => x.AudioTags).FirstOrDefault(x => x.Id == id);
      audio.Title = title;
      audio.Year = year;
      audio.GenreId = genreId;
      audio.ArtistId = artistId;
      audio.Duration = duration;
      audio.Url = url;

      _context.RemoveRange(audio.AudioTags);
      var audioTags = tagIds.Select(x => new AudioTag { AudioId = id, TagId = x });
      foreach (var tag in audioTags)
      {
          audio.AudioTags.Add(tag);
      }

      _context.SaveChanges();

      return audio;
  }
  ```

  ```C#
  // In SongController.Edit
  //...
  _audioRepo.Modify(
      id,
      song.Title,
      song.Year,
      song.GenreId,
      song.ArtistId,
      song.Duration,
      song.Url,
      song.TagIds);
  //...
  ```

**DELETE Example:**
  ```C#
  // In AudioRepository
  public Audio Remove(int id)
  {
      var audio = _context.Audios
          .Include(x => x.AudioTags)
          .FirstOrDefault(x => x.Id == id);

      _context.RemoveRange(audio.AudioTags);
      _context.Audios.Remove(audio);

      _context.SaveChanges();

      return audio;
  }
  ```

  ```C#
  // In SongController.Edit
  //...
  _audioRepo.Remove(id);
  //...
  ```

### 13.10 Exercise: Implement additional diagnostics data

Implement additional diagnostics data and display it:
  - int CountGenres()
  - int CountArtists()

### 13.11 Exercise: Create custom caching service

Move `GetGenreListItems()` and `GetArtistListItems()` to a custom caching service and use it. Also do it for `GetTagListItems()` and implement caching there.

> Note: It makes sense to implement this in a service local to the MVC project.
> Hint: To get `HttpContext` in a service use `IHttpContextAccessor`; see: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-8.0#access-httpcontext-from-custom-components

### 13.12 Exercise: Introduce mapper to support Web API DTO objects
