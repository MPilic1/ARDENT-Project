# MVC Architecture

This material is part of Learning Objective 3 (minimum).

## 8 ASP.NET MVC

Overview of ASP.NET Core MVC:

- https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-8.0


### 8.1 Creating and starting ASP.NET MVC project

In Visual Studio 2022 we create a new MVC project with the following properties:

- Project type: ASP.NET Core Web App (Model-View-Controller)
  - *NOT Web API*
- Name (solution): exercise-8
- Project name: exercise-8-1
- No authentication and HTTPS
- Enable Docker: No
- Do not use top-level statements: No

### 8.2 Observe project structure

- Web API has a very similar folder structure
- Folder `wwwroot` 
  – static files created by default
  - additional static files (srcipts, CSS stylesheets, images, documents...) can be added here
- File `Program.cs` 
  – default route is set up in parameter of `app.MapControllerRoute()` call
  - Default controller is `Home`, default action is `Index`
- Folder `Controllers`
  - In MVC actions of contollers return HTML pages
  - Open `HomeController.cs` and find `Index` action
  - While on that action, in context menu select "Go To View"
  - You will be transferred to that view
  - Obviously, `Index.cshtml` is displayed as a default view
- Change some HTML, rebuild the app and observe the result
- Folder `Views` 
  – Contains files with code that renders HTML
  - Code is known as **Razor** syntax
  - Large part of it **is** HTML
- Folder `Models` – added by default, with one model
- File `launchSettings.json`: 
  - Change the MVC site to run on port 6555
- File `appsettings.json`: 
  - Database, authentication, custom settings...

### 8.3 Routing

- Try the following routes:
  - http://localhost:6555/
  - http://localhost:6555/home/
  - http://localhost:6555/home/index/
  - http://localhost:6555/home/privacy/
  - http://localhost:6555/home/error/

- Find the `Privacy.cshtml` view and change the text to...
  "By using our services, you consent to the collection and use of your information as outlined in this policy."
- Error handling
  - The error route is just a placeholder and you can use your own error page for that
  - Remove the exception handler route (the block with `UseExceptionHandler`)
  - Instead of that, use a middleware that we already know of: `app.UseDeveloperExceptionPage()`
  - Throw a new `NotImplementedException` in `Privacy()` action and see what happens when you use that route

### 8.4 Creating an action with a view

Here we will create a web page.

- Create a new empty MVC controller named PersonController
- Create a new empty `Add()` action inside the controller
- Add a new `Person/Add.cshtml` view
  - In context menu, click *Add View...*
  - Select *Razor View – Empty*
  - Set name to `Add.cshtml`
- Paste the following code into `Add.cshtml`: https://pastebin.com/kwknkp9C
- Run the app, go to http://localhost:6555/Person/Add
- Observe the generated HTML code in browser

### 8.5 Creating HTTP POST action

Here we will create a functionality to store the data.

- We need action that handles POST request, for server to add the first name data to the collection
- Add static list of strings to the controller
  ```C#
  private static List<string> _persons = new List<string>();
  ```
- Add `[HttpPost]` action with the same name as for the GET action: `Add()`
- Add `string firstName` parameter to it (name must be the same as the one of the input textbox, in the view)
- In action code, add that first name to the list of persons
- Debug it and show that when *Add* button is pressed, name is added to the static
list

### 8.7 Displaying data in a list

Here we will create a functionality to display the stored data in a list.

- Let's show names added to the collection
- You can use `Index` action for that – add `Index.cshtml` view
- To send data to view, in action set ViewData
  ```C#
  ViewData["persons"] = _persons;
  ```
- To receive data from view, in action get collection from ViewData:
  ```C#
  @{
    List<string> persons = ViewData["persons"] as List<string>;
  }
  ```
- Use following code to generate proper HTML: https://pastebin.com/T4KPh2UJ
- After adding the data you should be able to see the collection here: http://localhost:6555/Person

### 8.8 Redirecting from POST action to list of data action (PRG pattern)

Here we will redirect page after the data is stored.

- After adding new name, redirect to show the list of added names
  - Return RedirectToAction() instead of View()
    ```C#
    return RedirectToAction("Index")
    ```
- Beneath the table of person names, add button "Add person" that will open "/Person/Add/" URL
  ```HTML
  <a href="/Person/Add/" class="btn btn-primary">Add person</a>
  ```
- Test adding the person name

  > Note: this is a simple implementation of PRG (Post-Redirect-Get) pattern widely used in MVC

### 8.9 Use model for storage – add last name and e-mail

Here we will handle storing multiple data, not just one string.

- For that, you will obviously need to store different data to list, not just string
- Add model `Person`
  - `string FirstName`
  - `string LastName`
  - `string Email`
- Change `List<string>` to `List<Person>`
  - You have to change `Add()` action parameter type for that
  - You also have to change that list in `Index.cshtml`: https://pastebin.com/DXhcxtzn
  - You also have to change the form in `Add.cshtml`: adapt the code to include new data

### 8.10 Create edit view

Here we will create functionality to display stored data for editing.

- For edit view we obviously need a new action and a new view
- Problem: no identifier
  - We will must use identifier to be able to identify the model
  - Add `int Id` property to the model
  - When adding the person, autogenerate that `Id` using `Max()` LINQ expression on the list
- Add view for editing feature
  - Add `Edit(int id)` action
  - Fill `ViewData` with the required model
    ```C#
    ViewData["person"] = person
    ```
  - Duplicate file `Add.cshtml` to `Edit.cshtml`
  - In the view, get the data from `ViewData`
    ```C#
    Person person = ViewData["person"] as Person;
    ```
  - Use properties of person for values
    ```C#
    @Html.TextBox("firstName", value: person.FirstName, htmlAttributes: ...)
    ```
```C#
@{
  Person person = ViewData["person"] as Person;
}

@using (Html.BeginForm())
{
  <div class="form-group">
    @Html.Label(labelText: "First Name", expression: "firstName")
    @Html.TextBox("firstName", value: person.FirstName, htmlAttributes: new { @class = "form-control" })
  </div>
  ...
}
```
- After editing the person, try the view: http://localhost:6555/Person/Edit/1

### 8.11 Create edit POST action

Here we will create functionality to store edited data.

- Start by copying `Edit()` GET action to POST action
- Change input argument to `Person` type, to bind data that comes from user interface
- Find person from the list by Id and update its first and last name and the email
- Redirect to action "Index", same as for `Add()` action
- After adding the person, try the edit functionality: http://localhost:6555/Person/Edit/1

### 8.12 Create delete view

Here we will create functionality to display stored data for deleting.

- For delete view we also need a new action and a new view
- Add view for deleting feature
  - Copy (duplicate) `Edit(int id)` action as `Delete(int id)` action
  - Copy (duplicate) `Edit.cshtml` view as `Delete.cshtml` view
  - In the view, replace
    ```C#
    @Html.TextBox("firstName", value: person.FirstName, htmlAttributes: new { @class = "form-control" })
    ```
    ...with...
    ```C#
    @Html.Display("person.FirstName")
    ```
  - Note: the Razor expression will look into the `ViewData` and extract information from there
  - Do it for all the textboxes
- After adding the person, try the view: http://localhost:6555/Person/Delete/1
- Check what is the effect of replacing `TextBox()` with `Display()` in HTML code

### 8.13 Create delete POST action

Here we will create functionality to delete data.

- Start by copying `Delete()` GET action
- Change input argument to `Person` type, to bind data from UI
- Find person from the list by id and remove it
- Redirect to action "Index", same as for `Add()` action
- After adding the person, try the delete functionality: http://localhost:6555/Person/Delete/1

### 8.14 Wire up the links

Here we will use `_Layout.cshtml` (can be thought of as "master page") to generate navigation links.

- Open `_Layout.cshtml`, replace existing *Privacy* link with link to `Person/Index` with text *People*
- See if the link to index page works : http://localhost:6555
- In `Index.cshtml`, add two placeholder buttons for `Edit` and `Delete`
  ```HTML
  <a href="@editUrl" class="btn btn-primary">Edit</a>
  <a href="@deleteUrl" class="btn btn-danger">Delete</a>
  ```
- After `@foreach` loop enters, create and initialize required variables
  ```C#
  var editUrl = "/Person/Edit/" + person.Id;
  var deleteUrl = "/Person/Delete/" + person.Id;
  ```
- Try the functionality
- Note that there is no Cancel button on Add/Edit/Delete views – implement it

### How do we implement CRUD?

You can use the following checklist:
> - Add model
> - Add controller
> - Add data repository (here we used static collection)
> - Create Add action (GET)
> - Create Add.cshtml
> - Create Add action (POST)
> - Create Index (list) action (GET)
> - Create Index.cshtml
> - Create Edit action (GET)
> - Create Edit.cshtml
> - Create Edit action (POST)
> - Create Delete action (GET)
> - Create Delete.cshtml
> - Create Delete action (POST)
> - Wire up links in _Layout.cshtml

### 8.15 Exercise: Implement CRUD for products

- Model Product
  - Id (int)
  - Name (string)
  - Description (string)
  - Price (decimal)
  - URL (string)
