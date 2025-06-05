# MVC Architecture

Authentication and authorization are part of Learning Objective 3 (desired).  

## 12 Overview

- Authentication and authorization: 
  - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-7.0#reacting-to-back-end-changes
  - https://learn.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-7.0
- Multi-tier application

### 12.1 Exercise setup

**SQL Server Setup**  

Do the following in SQL Server Management Studio:

-   download the script: https://pastebin.com/jtJfak9E
-   in the script, **change the database name to Exercise12 and use it**
-   execute it to create database, its structure and some test data

**Project starter**

> The following is already completed as a project starter:
>
> -   Model and repository (database context) setup
> -   Launch settins setup
> -   Created basic CRUD views and functionality (Genre, Artist, Song)
> -   Implemented validation and labeling using viewmodels
>
> For details see the previous exercises.

Unpack the project starter archive and open the solution in the Visual Studio.  
Set up the connection string and run the application.  
Check if the application is working (e.g. navigation, list of songs, adding a new song).

> In case it's not working, check if you completed the instructions correctly.

### 12.2 Add cookie authentication services and middleware

Prerequisite to implementation of the MVC authentication is adding services and middleware for the cookie authentication. Cookie authentication is the way to authenticate users in your MVC application.  

Perform steps of the "Add cookie authentication" chapter found in the link:
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-7.0#add-cookie-authentication

In details:
- Add authentication service
- Add authentication and authorization middleware (authentication is already present in project template)
- Mark `ArtistController` and `GenreController` with `[Authorize]` attribute and see what happens when you try to open these pages the navigation.  
Look at the URL.  

  > You can observe default URL redirection that happens because user is not authenticated.

- Change middleware settings:
  ```C#
  builder.Services.AddAuthentication()
    .AddCookie(options =>
      {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/User/Forbidden";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
      })
  ```

- see what happens now when you click on `Privacy` in the navigation - look at the URL

  > Now the redirection URL changes because we provided custom links instead of defaults.

Notes:
- You don't need `app.MapRazorPages();` middleware because we don't use razor pages
- When middleware detects it needs to redirect to login, default LoginPath is `/Account/Login`
- When middleware detects it needs to redirect to logout, default LogoutPath is `/Account/Logout`
- When middleware detects it needs to redirect to access denited page, it default AccessDeniedPath is `/Account/AccessDenied`
- Depending on your authentication endpoints you can change these defaults, as you did
- See documentation for other settings (`ExpireTimeSpan`, `SlidingExpiration`)

### 12.3 Create the authentication cookie

This section is based on the following content:
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-7.0#create-an-authentication-cookie

Perform the steps:
- Create empty `UserController`
- Create `Login` action in `UserController` and pass `string returnUrl` parameter to it
- Create empty Login view and add this content:
  ```HTML
  <h2>You have been logged in.</h2>
  <a asp-action="Index" asp-controller="Home" class="btn btn-outline-info">Go home</a>
  ```
- Update the `_Layout.cshtml` to include `Login` to the navigation

  > Note: This action will now accept middleware redirect and the `returnUrl` to the page you just wanted to reach, that was `/home/privacy`. The idea now is use the simplest way to create the authentication cookie to be able to reach the secured page (here: `/Home/Privacy`). You will refine this later using real credentials.

By clicking Login button in the navigation, user is not logged in yet, just the message is displayed.  
Create the "blank" authentication cookie in the login action:

  ```C#
  public IActionResult Login(string returnUrl)
  {
      var claims = new List<Claim>();

      var claimsIdentity = new ClaimsIdentity(
          claims, 
          CookieAuthenticationDefaults.AuthenticationScheme);

      var authProperties = new AuthenticationProperties();
      
      // We need to wrap async code here into synchronous since we don't use async methods
      Task.Run(async () =>
          await HttpContext.SignInAsync(
              CookieAuthenticationDefaults.AuthenticationScheme,
              new ClaimsPrincipal(claimsIdentity),
              authProperties)
      ).GetAwaiter().GetResult();

      if (returnUrl != null)
          return LocalRedirect(returnUrl);
      else
          return return RedirectToAction("Index", "Home");
  }
  ```

When you click `Login`, action will create new empty cookie that will allow you to reach the protected pages.  

When you click `Genres` or `Artists`, middleware will pass the return URL to `Login` action. At the end of the action, you locally redirect to that URL.  

Open Development Tools in the browser, select "Application" tab and find the cookie.  

### 12.4 Remove the authentication cookie

User also needs to be able to execute the logout.  
Implement `Logout` action and `Logout.cshtml` template.

- Implement action:

  ```C#
  public IActionResult Logout()
  {
      Task.Run(async () =>
          await HttpContext.SignOutAsync(
              CookieAuthenticationDefaults.AuthenticationScheme)
      ).GetAwaiter().GetResult();

      return View();
  }
  ```

- Add `Logout.cshtml` template:

  ```HTML
  <h2>You have been logged out.</h2>
  <a asp-action="Index" asp-controller="Home" class="btn btn-outline-info">Go home</a>
  ```

- Update the `_Layout.cshtml` to include `Log Out` button in the navigation

Now click `Log Out` button.

Open Development Tools in the browser, select "Application" tab and see that the cookie is no more.

### 12.5 Implement register form

Implementing register and login form is done the same way as you already did it in Web API, except now there are user interface and cookie involved. In Web API you had Swagger and JWT instead.  

**Open exercise 6 for details.**  

To be precise, you need the following:
- Register and login viewmodels 
  - Use the same classes that you used as DTOs before, but rename them for example from `UserDTO` to `UserVM`
- The same `PasswordHashProvider` helper 
  - Don't forget the `Security` folder

Now support the `Register` functionality:
- Implement empty `GET Register()` and `POST Register()` methods as parts of `UserController`
  - POST method accepts `UserVM userVm` parameter
- Auto-generate the `Register` view (`Create` template, `UserVM` model) and fine-tune the view (remove id, set correct type for password field...)
- Add the `Register` link to layout page
- In POST method, implement the same algorithm for registering user as for Web API (find POST Register in the **exercise 6**)
  - Pay attention to support the database access in controller
- At the end of the algorithm, don't return `Ok()`, but redirect to `Index` action of the `Home` controller

Test the registration - verify that there is data in the database for the registered user.

### 12.6 Implement login form

Now support the `Login` functionality:
- You already have the `GET Login()` method, and you also need `POST Login()` method
  - POST method accepts `LoginVM loginVm` parameter (create `LoginVM`, you just need username and password there)
- Move the cookie creation logic to the POST method - it doesn't make sense to create cookie before the user actually logs in
  - You need to support the `returnUrl` in viewmodel
  - Use a hidden field in the form to maintain the `returnUrl` data until user logs in
  ```
  public IActionResult Login(string returnUrl)
  {
      var loginVm = new LoginVM
      {
          ReturnUrl = returnUrl
      };

      return View();
  }
  ```
- Auto-generate the `Login` view (`Create` template, overwrite) over the existing view, and fine-tune the view (set correct type for hidden and password fields...)
- At the start of the POST method, now implement the same algorithm for finding and verifying the user in the database as for Web API. With one difference - return View in case of an error.
  ```C#
  // Try to get a user from database
  var existingUser = _context.Users.FirstOrDefault(x => x.Username == loginVm.Username);
  if (existingUser == null)
  {
      ModelState.AddModelError("", "Invalid username or password");
      return View();
  }

  // Check is password hash matches
  var b64hash = PasswordHashProvider.GetHash(loginVm.Password, existingUser.PwdSalt);
  if (b64hash != existingUser.PwdHash)
  {
      ModelState.AddModelError("", "Invalid username or password");
      return View();
  }
  ```
- Instead of creating and and returning JWT token, create the proper cookie
  ```C#
  // Create proper cookie with claims
  var claims = new List<Claim>() {
      new Claim(ClaimTypes.Name, loginVm.Username),
      new Claim(ClaimTypes.Role, "User")
  };

  var claimsIdentity = new ClaimsIdentity(
      claims,
      CookieAuthenticationDefaults.AuthenticationScheme);

  var authProperties = new AuthenticationProperties();

  Task.Run(async () =>
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        authProperties)
  ).GetAwaiter().GetResult();
  ```

Test it and verify that registered user can now log in.

### 12.7 Display the login info to the user

You need to peek into HttpContext for that, same as you did in case of Web API. However, it's a bit different if you want to do it in layout page (you need that information in the entire web site). In any view (and also `_Layout.cshtml`), `HttpContext` can be found in `ViewContext`.

Add the following code after the navigation `<ul>` in the layout page:
  ```HTML
  @{
      var userName = this.ViewContext.HttpContext.User?.Identity.Name ?? "(user not logged in)";
  }
  <div class="d-flex">
      <div class="badge bg-primary">@userName</div>
  </div>
  ```

Now the user should be able to see whether she/he is logged in or not - the username will be displayed.

### 12.8 Adapt the navigation state according to the user authentication

Normally, state of navigation adapts whether the user is logged in or not.
- If user is not logged in, she/he needs to have `Log In` button displayed  
- In case of user is logged in, just `Log Out` should be visible  

To maintain that state, you need to control it in `_Layout.cshtml`.  
For example:

```C#
@* Start of _Layout.html *@
@{
    var user = this.ViewContext.HttpContext.User;
    bool loggedIn = false;
    string username = "";
    if (user != null && !string.IsNullOrEmpty(user.Identity.Name))
    {
        loggedIn = true;
        username = user.Identity.Name;
    }
}
```

```HTML
<!--Navigation-->
@if (!loggedIn)
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="User" asp-action="Login">Log In</a>
    </li>
} 
else
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="User" asp-action="Logout">Log Out @username</a>
    </li>
}
```

Also, remove the register link from layout and add it to the `Login.cshtml`:
```HTML
<div class="form-group">
    <a asp-controller="User" asp-action="Register">Not a user? Register here</a>
</div>
```

### 12.9 Authorization: supporting application roles

Let's first support roles in the database.

```SQL
CREATE TABLE UserRole (
	Id int NOT NULL IDENTITY (1, 1),
	[Name] nvarchar(50) NOT NULL,
	CONSTRAINT PK_UserRole PRIMARY KEY (Id)
)
GO

SET IDENTITY_INSERT UserRole ON
GO

INSERT INTO UserRole (Id, [Name])
VALUES 
	(1, 'Admin'), 
	(2, 'User')
GO

SET IDENTITY_INSERT UserRole OFF
GO

ALTER TABLE [USER] 
ADD RoleId int NULL
GO

UPDATE [USER]
SET RoleId = 2
GO

ALTER TABLE [USER] 
ALTER COLUMN RoleId int NOT NULL
GO

ALTER TABLE dbo.[USER] 
ADD CONSTRAINT FK_USER_UserRole FOREIGN KEY (RoleId) 
REFERENCES dbo.UserRole (Id)
GO
```

Although many roles can exist, the script supports just 2 roles:
- Admin
- User

Rebuild database context an models to support new table.  

```PowerShell
dotnet ef dbcontext scaffold "Name=ConnectionStrings:ex12cs" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
```

If you look at the `POST Login`, you can see that claims are set there, when creating the cookie.  

Role claim is hardcoded there, and it should be set from user data in database.
- Replace hardcoded value with `existingUser.Role.Name`
- **Don't forget to include role into resultset when retrieving data from database (context)**

> You can always check if the role claim is set by looking into `HttpContext.User` 
> - `HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value`

> Note: when registering user, you need to e.g. add user `RoleId = 2` now to avoid getting exception when registering new user. Actually, because of not having role information in the `User` entity instance, it would default to `0`, and there is no such role id in the database. The code would throw exception when user registers.

### 12.10 Authorization: adapting layout according to the role

You can differentiate the layout according to the role of a current user. Just switch the layout in `_ViewStart.cshtml` template.  
By default, your layout is hardcoded as...
  ```
  @{
      Layout = "_Layout";
  }
  ```

Let's change it to switch the layout:
  ```
  @{
      Layout = "_Layout";

      var user = ViewContext.HttpContext.User;
      if (user == null)
      {
          Layout = "_Layout";
      }
      else if (user.IsInRole("Admin"))
      {
          Layout = "_LayoutAdmin";
      }
      else if (user.IsInRole("User"))
      {
          Layout = "_LayoutUser";
      }
  }
  ```

Now, create `_LayoutAdmin.cshtml` and `LayoutUser.cshtml` templates and change some Bootstrap properties in each.

First, *visualy* change link in `Index.cshtml` into button with `class="btn btn-primary"`.  
Then, register `admin` user in the database.  
In database, manually change `admin` role id from 2 to 1.

**_LayoutUser.cshtml**

Remove `Genre`, `Artist`, `Song` and `Search` from navigation.  

**_LayoutUser.cshtml**

Force-change Bootstrap button layout 
```
  <style>
      .btn-primary, 
      .btn-primary:hover, 
      .btn-primary:active, 
      .btn-primary:visited {
          background-color: #8064A2 !important;
      }
  </style>
```
> There are [better ways to do that](https://stackoverflow.com/questions/28261287/how-to-change-btn-color-in-bootstrap).

Change the navbar class from `bg-white` to `bg-info`.  

Remove `Genre`, `Artist` and `Song` from navigation.  

Log in as `user1` and observe the change.  

**_LayoutAdmin.cshtml**

Change the navbar class from `bg-white` to `bg-dark`.  
Change the navbar class from `navbar-light` to `navbar-dark`.  
Remove `text-dark` classes from nav link items.  

Log in as `admin` and observe the change.

> What would be the better way to implement adding administrator to the application?

### 12.11 Authorization: redirecting the user to the appropriate route according to the role

See Login() action.  
At the end of action simply redirect to appropriate controller according to the role.

  ```C#
  if (loginVm.ReturnUrl != null)
      return LocalRedirect(loginVm.ReturnUrl);
  else if (existingUser.Role.Name == "Admin")
      return RedirectToAction("Index", "AdminHome");
  else if (existingUser.Role.Name == "User")
      return RedirectToAction("Index", "Home");
  else
      return View();
  ```

### 12.12 Authorization: constraining controllers and actions to the particular role

See **exercise 6**.  
The same system works for MVC as for Web API.
