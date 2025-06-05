# RESTful architecture (Web API)

This material is part of Learning Objective 2 (desired).

## 7 Using Web API Application

Static pages in ASP.Net Core:

-   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-8.0

JavaScript jQuery AJAX methods:

-   https://api.jquery.com/jQuery.ajax/
-   https://api.jquery.com/jQuery.get/

JavaScript Fetch API:

-   https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API

JavaScript localStorage:

-   https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage/

### Database setup

SQL Server Management Studio:

-   create database `Exercise7`
-   create database structure with data
    -   structure part 1 (general entities): https://pastebin.com/5MTPzxrd
    -   structure part 2 (authentication): https://pastebin.com/9wsPtAV1
    -   data: https://pastebin.com/iwuDcyKx

### Web API application setup

Unpack `dwa-tasks-7-en.zip` archive.  
We will use that solution as Web API server.

> How was that application setup created?  
> We used all that we learned about the Web API RESTful service to create this app:
>
> -   created new Web API solution in Visual Studio:
>     -   solution and project name: exercise-07
>     -   turned off HTTPS support
> -   installed EF support:
>     -   `dotnet tool install --global dotnet-ef --version 7`
>     -   `dotnet add package Microsoft.EntityFrameworkCore --version 7`
>     -   `dotnet add package Microsoft.EntityFrameworkCore.Design --version 7`
>     -   `dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 7`
> -   installed JWT support:
>     -   `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 7`
>     -   `dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect --version 7`
> -   configured EF connection string in `appsettings.json`
>     ```JSON
>     "ConnectionStrings": {
>       "AppConnStr": "server={your server};Database=Exercise7;User=sa;Password=SQL;TrustServerCertificate=True;MultipleActiveResultSets=true"
>     }
>     ```
> -   reverse engineered database (created entity classes and database context)
>     -   `dotnet ef dbcontext scaffold "Name=ConnectionStrings:AppConnStr" Microsoft.EntityFrameworkCore.SqlServer -o Models --force`
> -   set up database service in `Program.cs` to be available to DI container
>     ```C#
>     builder.Services.AddDbContext<Exercise7Context>(options => {
>         options.UseSqlServer("name=ConnectionStrings:AppConnStr");
>     });
>     ```
> -   configured JWT in `appsettings.json` (just secure key)
>     ```JSON
>     "JWT": {
>       "SecureKey": "1xvawozgzh78q2m9xpdlshegaqaspkpe"
>     }
>     ```
> -   configured JWT service and middleware in `Program.cs`:
>
>     ```C#
>     // Configure JWT security services
>     var secureKey = builder.Configuration["JWT:SecureKey"];
>     builder.Services
>         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
>         .AddJwtBearer(o => {
>             var Key = Encoding.UTF8.GetBytes(secureKey);
>             o.TokenValidationParameters = new TokenValidationParameters
>             {
>                 ValidateIssuer = false,
>                 ValidateAudience = false,
>                 IssuerSigningKey = new SymmetricSecurityKey(Key)
>             };
>         });
>     ```
>
>     ```C#
>     // Use authentication / authorization middleware
>     app.UseAuthentication();
>     app.UseAuthorization(); // -> this should already be present
>     ```
>
> -   created utility class `JwtTokenProvider` that handles JWT: https://pastebin.com/ZnD36wjb
> -   added JWT login support to `Program.cs` for Swagger service (for testing purposes): https://pastebin.com/YdqbDfWy
> -   created utility class `PasswordHashProvider` that handles password hashing cryptography: https://pastebin.com/buxhBfZx
> -   created `UserController` that handles registration and login
>     -   see `UserController` for details (explained in previous exercises)
>     -   created DTOs for both registration and login: `UserRegisterDto` and `UserLoginDto`
> -   created `AudioController` that uses `Audio` entity with 1-to-N `Genre` and M-to-N `Tag` entities
>     -   created `AudioDto`
>     -   created mapping class
> -   secured some of the actions in `AudioController` to be accessible to user that is logged
>     -   using `[Authorize]` attribute
>     -   actions `GetAll()`, `Post()`, `Put()` and `Delete()` are secured
>     -   only `Search` and `Get(int id)` action are not secured

## 7.1 Enabling static pages in the project

When retrieving data from web API server, we usually use _static_ HTML pages to retrieve data from our server.  
Static pages are conventionally in `wwwroot` folder in project root.

-   create `wwwroot` folder in project root
-   you can see that it looks differently, it's a special folder
-   create `audios.html` static HTML page in `wwwroot` folder (right click, Add > New Item..., filter "html")
-   add content `<h1>Audio list</h1><hr />` to the page
-   you also need to support static files using the appropriate middleware
    -   add `app.UseStaticFiles()` middleware to Program.cs
    -   add it before `app.UseAuthentication()` to avoid checking for JWT authentication token when retrieving static files; if you would add it after, you would need to provide JWT token for each of the static files

Test your page:

-   run the application
-   open http://localhost:5127/audios.html in the browser

Note that you will use JavaScript to retrieve the data from your endpoint.  
You can always use jQuery to help you with the requests and DOM handling:

-   go to https://releases.jquery.com/, section **jQuery 3.x**
-   click minified version and copy the script tag
-   paste it to your HTML just before body tag ends

## 7.2 Retrieving data from unsecured source

1. Find the exact endpoint address

    It's easy to construct it using routing attributes, but you can also use Swagger.

    - run application
    - use `/api/Audio/{id}` endpoint in Swagger to get one particular audio (e.g. id=3)
    - note the `Request URL` field, it contains the URL
        - e.g. `http://localhost:5127/api/Audio/3`

2. Check if JavaScript works

    Make sure you really can use JavaScript in your setup

    - add script tag after the first `jQuery` script tag
    - add the following script:
        ```JavaScript
        $(function () {
            console.log("DOM ready");
        });
        ```
    - in Development Tools of the browser, check if the appropriate message is in the output after page loads

3. Issue the request and log the response

    Now you can replace the single `console.log()` call with retrieval of the data from your server endpoint:

    ```JavaScript
    $(function () {
        let url = "http://localhost:5127/api/Audio/3";
        $.get(url, function (data) {
            console.log(data);
        });
    });
    ```

    > Note that you can also use native JavaScript Fetch API without jQuery. For that you async function in your script
    >
    > ```JavaScript
    > document.addEventListener("DOMContentLoaded", async function () {
    >     let url = "http://localhost:5127/api/Audio/3";
    >     const response = await fetch(url);
    >     const data = await response.json();
    >     console.log(data);
    > });
    > ```

## 7.3 Handling errors

To handle errors in jQuery request, you need to add `.fail()` error handler just after `$.get()`. It is done in a fluent interface manner.

```JavaScript
$(function () {
    let url = "http://localhost:5127/api/Audio/5";
    $.get(url, function (data) {
      console.log(data);
    }).fail(function() {
      console.error("There was an error while trying to load your data");
    })
});
```

> Note: when using Fetch API, handling errors is done using standard try/catch syntax.
> Example:
>
> ```JavaScript
> try {
>   const response = await fetch(url);
>   const data = await response.json();
>   console.log(data);
> } catch (error) {
>   console.error(`Request error: ${error.message}`);
> }
> ```

## 7.4 Using DOM

Let's use DOM handling to show data in our page.
Add placeholder for data rendering into your page.

```HTML
<div id="placeholder"></div>
```

Instead using `console.log`, render data using jQuery.  
For example:

```JavaScript
$("#placeholder").append("<ul>");
$ul = $("#placeholder ul");
$ul.append(`<li>Id: ${data.id}</li>`);
$ul.append(`<li>Name: ${data.title}</li>`);
$ul.append(`<li><a href="${data.url}">Play song</a></li>`);
```

## 7.5 Retrieving data from a secured source

To retrieve data from secured source, you first need to retrieve the JWT and then use it in your request.

1. Retrieve JWT

    Let's first register user using Swagger, and then login (retrieve JWT) using JavaScript.

    For example, use this data to register:

    ```
    {
      "username": "johnny1234",
      "password": "qwertzuiop",
      "firstName": "John",
      "lastName": "Smith",
      "email": "johnsmith1234@example.com",
      "phone": "0987654321"
    }
    ```

    Now use Swagger to login and see how the login endpoint really looks like.
    For example, it will look like this: `http://localhost:5127/api/User/Login`.
    Pay attention that it's a POST endpoint, so you need to use the jQuery Ajax POST handler to perform the login from JavaScript:

    ```JavaScript
    let loginUrl = "http://localhost:5127/api/User/Login";
    let loginData = {
        "username": "johnny1234",
        "password": "qwertzuiop"
    }
    $.ajax({
        method: "POST",
        url: loginUrl,
        data: JSON.stringify(loginData),
        contentType: 'application/json' // important, otherwise it's sent as form data
    }).done(function (data) {
        console.log(data);
    }).fail(function () {
        console.error("There was an error while trying to load your data");
    });
    ```

    Now you should see JWT retrieved and displayed in your console.

    > Note: A lot of times when using Web API, you will receive automatic error 400. You can turn it off using the already known technique in `Program.cs` of your Web API server:
    >
    > ```C#
    > builder.Services.Configure<ApiBehaviorOptions>(options =>
    > {
    >   options.SuppressModelStateInvalidFilter = true;
    > });
    > ```

2. Use JWT to retrieve secured data

    Use Swagger to find the secured `GetAll()` endpoint.  
    For example, it will be: http://localhost:5127/api/Audio/GetAll

    Now after you get the JWT, use it to retrieve data from secured endpoint:

    ```JavaScript
    let loginUrl = "http://localhost:5127/api/User/Login";
    let allAudiosUrl = "http://localhost:5127/api/Audio/GetAll";
    
    let loginData = {
        "username": "johnny1234",
        "password": "qwertzuiop"
    }
    $.ajax({...})
    .done(function (tokenData) {
        console.log(tokenData);

        $.ajax({
            url: allAudiosUrl,
            headers: { "Authorization": `Bearer ${tokenData}` } // this is how you send JWT token using JavaScript
        }).done(function (allAudioData) {
            console.log(allAudioData);
        }).fail(function () {
            console.error("There was an error while trying to load your data");
        });
    })
    .fail(...);
    ```

## 7.6 Using localStorage

LocalStorage is property that gets saved in order to persist your data after the browser window is closed.  
Example:

```
localStorage.setItem("myCat", "Tom");
const cat = localStorage.getItem("myCat");
console.log(cat);
```

You can remove an item from localStorage:

```
localStorage.removeItem("myCat");
```

## 7.7 Using localStorage to support JWT login/logout

By using localStorage, you can support login/logout functionality:

-   Login: retrieving and storing JWT to localStorage
-   Logout: removing JWT from localStorage

> The example will include also HTML and CSS because we need the layout.

Example:

-   `login.html`: create `login.html` page with username and password inputs and `Login` button
-   `login.html`: on button click, either display login error or store received JWT as localStorage item and redirect to `audios.html` page

    ```HTML
    <div class="login-container">
        <label for="username"><b>Username</b></label>
        <input type="text" placeholder="Enter Username" name="username" id="username" required>

        <label for="password"><b>Password</b></label>
        <input type="password" placeholder="Enter Password" name="password" id="password" required>

        <button onclick="jwtLogin()">Login</button>
    </div>
    ```

    ```CSS
    input[type=text], input[type=password] {
        width: 100%;
        padding: 12px 20px;
        margin: 8px 0;
        display: block;
        border: 1px solid #ccc;
        box-sizing: border-box;
    }

    button {
        width: 100%;
        background-color: #04AA6D;
        color: white;
        padding: 14px 20px;
        margin: 8px 0;
        display: block;
        border: none;
        cursor: pointer;
    }

        button:hover {
            opacity: 0.8;
        }

    .login-container {
        width: 50%;
        padding: 16px;
        border: 3px solid #f1f1f1;
        margin: auto;
    }
    ```

    ```JavaScript
    function jwtLogin() {
        let loginData = {
            "username": $("#username").val(),
            "password": $("#password").val()
        }
        $.ajax({
            method: "POST",
            url: loginUrl,
            data: JSON.stringify(loginData),
            contentType: 'application/json'
        }).done(function (tokenData) {
            //console.log(tokenData);
            localStorage.setItem("JWT", tokenData);

            // redirect
            window.location.href = "audios.html";
        }).fail(function (err) {
            alert(err.responseText);
            localStorage.removeItem("JWT");
        });
    }
    ```

-   `audios.html`: on page load check if there is JWT item in localStorage and if it is not, redirect to `login.html`
-   `audios.html`: add button to `audios.html` page that removes JWT item from localStorage and redirects to `login.html`

    ```HTML
    <nav>
        <h2>Audio list</h2>
        <ul>
            <li><a href="audios.html">Audios</a></li>
            <li><a href="javascript:void(0);" onclick="jwtLogout()">Logout</a></li>
        </ul>
    </nav>
    <div id="placeholder"></div>
    ```

    ```CSS
    * {
        padding: 0;
        margin: 0;
    }

    body {
        font-family: Arial, Tahoma, Serif;
        color: #263238;
    }

    nav {
        display: flex; /* 1 */
        justify-content: space-between; /* 2 */
        padding: 1rem 2rem; /* 3 */
        background: #cfd8dc; /* 4 */
    }

        nav ul {
            display: flex; /* 5 */
            list-style: none; /* 6 */
        }

        nav li {
            padding-left: 1rem; /* 7! */
        }

        nav a {
            text-decoration: none;
            color: #0d47a1
        }

            nav a:hover {
                opacity: 0.5;
            }
    ```

    ```JavaScript
    function jwtLogout() {
        localStorage.removeItem("JWT");

        // redirect
        window.location.href = "login.html";
    }
    ```

## 7.8 Example: JavaScript and Web API functionality for registering/login/logout

Implement register functionality.  
Use `register.html` page and integrate it into login page scheme (e.g. "New User Registration" link).  

> Note: After user is registered, client can call login function to return and save JWT as he does on login page.

## 7.9 Example: JavaScript interactivity using CRUD controllers

Tasks:

1. Show the audio contents in a tabular / list / card manner
2. Add the option to search the records
3. Support adding new audio content
4. Support removing audio content
5. Support modifying audio content

Interactivity rules for adding, modifying and deleting items:

-   use AJAX GET to retrieve data
-   use AJAX POST to add data
-   use AJAX PUT to modify data
-   use AJAX DELETE to remove data

> Hint: use modal to create or modify items without navigating to the other page. You can use Bootstrap modal. Također, morat ćete učitati podatke o žanru da biste odabrali okvir pri učitavanju stranice.

Use the following template for basic communication with your server:

```JavaScript
const requestUrl = "http://localhost:5127/api/{controller}"

function templateToDoSomething() {
  const requestData = {} // data that you want to send to server
  $.ajax({
      method: "{GET or POST or PUT or DELETE}",
      url: requestUrl,
      data: JSON.stringify(requestData), // for POST or PUT
      contentType: 'application/json', // for POST or PUT
      headers: { "Authorization": `Bearer ${jwt}` } // for secured endpoints
  }).done(function (responseData) {
      console.log(responseData);

      // Example 1: modify DOM
      // Example 2: fill inputs with data
      // Example 3: use your imagination :)
  }).fail(function (err) {
      console.error(err.responseText);

      // ...or: alert(err.responseText) to show error to user
      // ...or: modify DOM to show error to user
  });
}
```

**Solution**:

See the solution archive for details.
