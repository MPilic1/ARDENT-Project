# RESTful architecture (Web API)

This material is part of Learning Objective 1 (minimum).

## 2 Basics of Controllers and Actions

Attribute routing concept and explanation:

- https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-8.0#attribute-routing-for-rest-apis

Parameter binding is explained here:

- https://learn.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api

The options for returning value from the action are explained here:

- https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-8.0

### 2.1 Action

A well written action has the following properties:

- uses the HTTP method attribute (GET, POST, PUT, DELETE) which can, if necessary, define the name of the action
- if necessary, it receives one or more parameters (parameter binding)
- logic is protected by the `try...catch` block
- returns a wrapped generic result `ActionResult<T>`, where `T` is the data type that the client actually expects
- at the point where the data is returned as correct, "wraps" it in `2XX` or `3XX` response status code; e.g. `Ok(result)` or `Redirect(url)`
- at the point where the data is returned as incorrect, "wraps" it in a `4XX` or `5XX` response status code; e.g. `BadRequest("Operation failed")` or `StatusCode(500)`
- it doesn't contain lots of code but, if needed, e.g. invokes a method that does all the work

  ``` C#
  [HttpGet("[action]")]
  public ActionResult<decimal> ReturnQuotient(decimal a, decimal b)
  {
      try
      {
          // Action logic (e.g. data handling)
          var result = a / b;

          // Return 200 OK
          return Ok(result);
      }
      catch (Exception ex)
      {
          // Exception handling code
          // ...

          // Return e.g. internal server error
          return statusCode(500);
      }
  }
  ```

### 2.2 Adding Controllers and Actions

1. Create a new RESTful web API application with the following properties:

    - Name (solution): exercise-2
    - Project type: Web API
    - Project name: exercise-2-1
    - No authentication and HTTPS
    - Let it use controllers and Swagger

    You don't have to write code from scratch to add a controller and action. Just add an empty controller using Solution Explorer.  
    Right-clicking on "Controllers" opens a context menu in which you can select "Add", then "Controller..." After that, from the just opened panel _on the left side_, select "Installed > Common > API", and from the right, the option "API Controller > Empty" __(not "MVC Controller > Empty")__.
    After that you choose a name for the controller and it will appear in the list of controllers in the project.  

1. Add the new `MathController` class to the project using the explained method.

    > Notice that `MathController` inherits from the same `ControllerBase` base class as the `WeatherForecastController` class.

    > For a controller to appear in the Swagger interface, it must be annotated with the `ApiController` attribute, as done in the `WeatherForecastController` and `MathController` classes. In addition, the controller needs a routing attribute, `Route` - and it is present in the generated controller and in the `WeatherForecastController` class. What is the difference between them?

2. Add a new `Sum` method to that class. Let the method receive two integer parameters a and b and return the sum of those two parameters.

    > For a method to appear in the Swagger interface, it must be annotated with an HTTP-method attribute, such as `[HttpGet]`. When you run the project, the new method is shown as `api/Math`, without the name `Sum`. This is because the name is not automatically included in the endpoint since we did not give the `HttpGet` attribute a route parameter.

    > Give the method a route parameter by changing `[HttpGet]` to `[HttpGet("[action]")]`.  
    > What happens in the Swagger interface?

3. Test the new endpoint in the Swagger interface.  
We see that it is possible to reach it from Swagger. We call it _endpoint_, and the controller method that is called when the endpoint is requested is called _action_.

    > We see that the new access point is not returning JSON. With primitive values, no JSON is returned. JSON is returned only in case of collections, objects or collections of objects.

### 2.3 Controller Routing

Change the controller route by adding an "operations" segment to the URL after the "api" prefix.

`Route("api/[controller]")` => `Route("api/operations/[controller]")`

Observe the change in Swagger.

### 2.4 Routing the Action

1. Implement `Multiply` and `Power` actions to multiply and power two decimal numbers. Use `[HttpGet]` attributes, meaning you don't use any routing information.

    When you start the application, the browser will show an error instead of the usual interface!

    `Failed to load API definition.`

2. You can diagnose the error in the browser Developer Tools. Open Developer Tools in the browser (F12), select the `Network` tab, refresh the page and select the `swagger.json` request that shows the error. After that, you can open the response from the server on the right and see the details of the error in it.

    `Conflicting method/path combination "GET api/operations/Math" for actions - ...`

    > _Explanation_: now we have two methods that are also two `HttpGet` actions. The Web API framework itself does not know how to distinguish between two actions according to the method names (they are called `Multiply` and `Power`). That's why when you run the application you will get the error `Failed to load API definition.`

3. That is why it is necessary to add information for routing:

    - for `Multiply` method: `[HttpGet("Multiply")]` or `[HttpGet("[action]")]`
    - for `Power` method: `[HttpGet("Power")]` or `[HttpGet("[action]")]`
  
    After that, Swagger no longer shows the error and the routing information is updated.

    > As you can see, it is possible to use the `[action]` token for routing. Thus, the name of the method is used in the name of the action: `[HttpGet("[action]")]`.

### 2.5 Input Parameter Binding

1. Web API actions can be called directly from the browser.
Open a new tab in the browser and enter the address in it:

    http://localhost:5123/api/operations/Math/Sum?a=1&b=2

    > What result is returned?

2. It is not necessary to send parameters, but you need to know what do you get in that case

    http://localhost:5123/api/operations/Math/Sum

    > What if we don't send parameters?  
    > Parameters will get default values.

    > Can an action declare a parameter with a default value?  
    > Try it.

2. What happens when we send e.g. `string` instead of `int`?

     http://localhost:5123/api/operations/Math/Sum?a=a&b=b

     > What if we send parameters of the wrong type?  
     > An error will occur when binding parameters (parameter binding).
    > `One or more validation errors occurred...`

### 2.6 Saving the State

An action can save state in one action call and use it in another. For example, this mode of operation is used when saving to the database.

> We will use a static variable instead of a database for now.

The basics of the Language Integrated Query (LINQ) can be found on the following pages:

- https://learn.microsoft.com/en-us/dotnet/standard/linq/
- https://github.com/dotnet/try-samples/tree/main/101-linq-samples/src

---

1. Create a new RESTful Web API application with the following characteristics:

    - Name (solution): exercise-2 (current one)
    - Project type: Web API
    - Project name: exercise-2-6
    - No authentication and HTTPS
    - Let it use controllers and Swagger

2. Create a controller named `StatefulController`

3. Create a static variable inside the controller

    `private static int State { get; set; } = 0;`

4. Create a new `Add` action that does not have a parameter or returns a value, but only increases the `State` variable by 1. Pass the action name as a routing parameter.

    > Which HTTP method should we use to change state?

5. Create a new `HttpGet` action named `GetState` that returns the state value.

    > Try these two actions now.

5. Try removing keyword `static` from the variable declaration.

    When you test how it works, the variable will always be 0.

    > _Reason_: on each method call, the controller is re-instantiated and a new instance is used. The old instance is collected by the Garbage Collector. But remember the static variables! Static variables "survive" when instantiating a class object, that is, their value does not change during instantiation itself!

    That's why we can use a static variable to save the state during the application's runtime.

    _Important_: When you restart the application, the static (and other) variables are reset to their initial value, because the process is restarted and the memory used by the application is deleted.

### 2.7 Exercise: Average

Create an `Average` action in the `Math` controller that takes one `int` parameter and returns the average value of all numbers from 1 to that number.

### 2.8 Exercise: Fibonacci

Create a `Fibonacci` action in the `Math` controller that takes one parameter N, the ordinal number of the fibonacci number. The action returns the N-th Fibonacci number. Save that number as a state and create a `LastFibonacci` action that retrieves the last created Fibonacci number.

### 2.9 Exercise: Repetitive Text

Create a new `TextOperation` controller and a `RepeatText` action that takes one text parameter T and one integer parameter N. The action returns N times repeated text T.

### 2.10 Exercise: The Most Frequent Letter

In the `TextOperation` controller, create the `MostFrequentCharacter` action that takes one `sentence` parameter. The action should return the character that most often appears in the input parameter.

### 2.11 Exercise: Number of characters in a sentence

Create a `SetSentence` action in the `TextOperation` controller that takes one `sentence` parameter and saves it. Also create a `CharacterFrequency` action that takes a letter as an input parameter and returns how many such letters appear in the saved sentence.

### 2.12 Exercise: ROT13

Create a `Rot13` action in the `TextOperation` controller that takes one input string parameter. The action should return a ROT13 encrypted string of characters.

> Look up how to do ROT13 encryption.
> Note: ROT13 encryption is also ROT13 decryption.
