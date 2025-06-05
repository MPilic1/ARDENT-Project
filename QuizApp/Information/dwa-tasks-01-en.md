# Development of Web Applications Workbook 

## 1 RESTful architecture (Web API)

### 1.1 Web API application - basic concepts

Basic (and other) concepts can be found on the following links:

- https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-8.0&tabs=visual-studio
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-8.0

### 1.2 Creating a new Web API application

Create a new RESTful Web API application with the following characteristics:
  - Name: MyFirstWebApi
  - No authentication and HTTPS
  - Use controllers and Swagger

Run the app in the Chrome browser, then run it in the Edge browser.

> Which access points are implemented in the application?

Test the access point using the Postman interface.  
Test the access point using the Swagger interface.

> What is the name of the data shape that the access point returns?

### 1.3 Managing application launch via launchSettings.json

For the RESTful web api application you created in the last task, make the following changes:
  - set the application port to 5123
  - delete the "IIS Express" profile
  - change name of the profile
  - add a new profile that does not launch the browser when the application is launched

> What do we use profiles for in the Visual Studio .NET environment?

### 1.4 Playing with WeatherForecast controller

For the RESTful web api application you created in the last task, make the following changes:
  - change the route information of the controller  
    `[Route("MyData")]`

    > Which changes can you see?

  - change the HTTP method information of the action  
    `[HttpGet("...")]` => `[HttpPost()]`

    > Which changes can you see?

  - change the return value of the action  
    - return string from the action
    - return number from the action
    - return array of strings from the action
    - return array of numbers from the action
    - return list of numbers from the action
    - implement simple class with number, string and array, initialize it and return it from the action

    > What can you say about value that is returned from action?
