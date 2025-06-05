# RESTful architecture (Web API)

This material is part of Learning Objective 1 (minimum).

## 3 Real world Actions and CRUD

The basics of the Language Integrated Query (LINQ) can be found on the following pages:

- https://learn.microsoft.com/en-us/dotnet/standard/linq/
- https://github.com/dotnet/try-samples/tree/main/101-linq-samples/src

### 3.1 Saving to Collection

1. Create a new RESTful Web API application with the following characteristics:

    - Name (solution): exercise-3
    - Project type: Web API
    - Project name: exercise-3-1
    - No authentication and HTTPS
    - Let it use controllers and Swagger

2. Create a new API controller (Installed > Common > API, API Controller - Empty) named `CollectionController` and delete `WeatherForecastController`. 

3. Add state variable to controller, that is able to hold a list of integers:  
   `private static List<int> State { get; set; } = new()`

   _NOTE: as you can see, you are able to use the shorthand initializer for the type - `new()`_

4. Create a new action `[HttpPost("[action]")] AddToState` that takes as a parameter an array of integers - `int[] Numbers`. The action adds that field of integers to `State`.  

    > You can use LINQ expression `.AddRange()`

    > To test this change, you will need debugger breakpoint and entering a JSON expression in Swagger as an array, such as `[3, 5, 8]`

5. Return `ActionResult` value type:

   - change return type to `ActionResult`
   - return `Ok()` from the method

6. Protect the code using try/catch block.   
   In case of an error return `StatusCode(500)`

### 3.2 Returning a Collection From an Action

1. Implement `[HttpGet("[action]")] GetState` action to return the list.

2. Return type should be `ActionResult<T>`:
   - use `ActionResult<List<int>>` as return type
   - return `Ok(State)` from the method
   - implement `try...catch` and return `StatusCode(500)` from catch block

### 3.3 Adding and Removing Single Item To/From Collection

1. Implement `[HttpPost("[action]")] AddItem(int number)` action to add a single number to the list.

2. Implement `[HttpDelete("[action]")] RemoveItem(int number)` action to remove all the numbers that are equal to the one passed as a parameter from the list. Use `.RemoveAll()` method for that.

### 3.4 Using Model to Save State

Model is a shape of data in an application (e.g. a building that has properties: street, type, house number).  
A model often has an identifier. The convention is that the name of the identifier is `Id`.  
Models are often saved in a separate folder, e.g. `Models`.

1. Create a new RESTful Web API application with the following characteristics:

    - Name (solution): exercise-3 (current)
    - Project type: Web API
    - Project name: exercise-3-4
    - No authentication and HTTPS
    - Let it use controllers and Swagger

2. Inside new `Models` folder create a class `Street` with properties:
     - `int Id`
     - `string Name`

3. Create a new API controller named `AddressController` by selecting the "Installed > Common > API, API Controller with read/write actions" option during creation __(not "MVC controller with read/write actions")__. 

    > Note that all the HTTP methods are implemented automatically. Also, there are two `HttpGet` actions implemented: `Get()` and `Get(int id)`. The first one is meant to retrieve collection of all the items, and the second one to retrieve one particular item by its identifier.

4. Implement the list of streets as a static variable `Streets` of the new controller (don't forget the initialization).

5. Change the `Get()` action so that it returns a list of streets.

6. Change the `Get(int id)` action so that it returns the street by identifier.

    > Use the LINQ `.First()` statement for that. For example: `Streets.First(x => x.Id == id)`.

7. Change the `Post(...)` action so that it adds the street passed to it as an input parameter. Ignore the `Id` member that was sent and calculate yourself what the next identifier is. Let that action also return that added street.

    > To calculate the next identifier you can use the LINQ operator `.Max(x => x.Id)`
    >
    > You can use the LINQ operator `.Any()` to check if there is any street in the list of streets

8. Change the `Put(...)` action so that according to the identifier that is passed, it updates the street that is also passed as an input parameter. Only the `Name` member needs to be updated. Let that action also return the updated street.

9. Change the `Delete(...)` action so that it deletes the street according to the identifier passed.

10. Delete `WeatherForecastController`. 

11. Add the `Search` action for retrieving all the streets that contain `text` input parameter in their names.

12. Change `Search` action in the way to enable sorting output by either `Id` or `Name`.

13. Change `Search` action in the way to enable retrieval of first N products, second N products and so on; for this purpose, you can use the LINQ operators `Skip()` and `Take()`. You will need additional two parameters for that, e.g. `start` and `count`.

### 3.5 Improve Implementation of Actions

Change all actions according to what we consider a good action implementation:
- return value type `ActionResult<T>`
- action protection using `try/catch`
- returning results such as `Ok(result)` or `BadRequest()` and similar
  - retun 400 if user tries to create Street with empty name
  - retun 500 from any method where applicable
- error logging within the `catch` block
  - it is not necessary to implement logging, just leave a TODO comment instead

Example:

```C#
[HttpPost]
public ActionResult<Street> Post([FromBody] Street street)
{
    try
    {    
        int maxId = Streets.Any() ? Streets.Max(x => x.Id) : 0;
        street.Id = maxId + 1;

        Streets.Add(street);

        return Ok(street);
    }
    catch (Exception)
    {
        // TODO: Log error, use details from the exception

        return BadRequest("There was a problem while updating street");
    }
}
```

### 3.6 Returning case-specific codes

In addition to `200 OK`, `400 Bad Request` or `500 Internal Server Error`, in certain cases it is important to return some other specific HTTP code.  

For example:
- `201 Created` in the case when a Web API "resource" (item) is created via the POST method
- `204 No Content` if we don't want to return details, for example when we create or update a resource
- `401 Unauthorized` if the client is not authenticated
- `403 Forbidden` if the client is authenticated but not authorized (confusing because of the name, but true)
- `404 Not Found` if the resource was not found
- `500 Internal Server Error` if there is no special explanation why the server cannot complete the request, so we return a "generic" reason

---

1. Use `201 Created` (POST)  

Instead of Ok(), use Created(). You will need URL information where the created resource can be found for that change.  

For example:

    ```C#
    ...
    var location = Url.Action(nameof(Get), new { id = street.Id });
    return Created(location, street);

    ```

2. Use `404 Not Found` (GET for Id, PUT, DELETE)

Check if the resource exists and if not, return status code 404 using `NotFound()`.

### 3.7 Using a More Complex Models to Save State

In this exercise we will store both streets and house numbers in the same structure. To do that, let's create static class with the static parameter as a repository for the data.

1. Create `Repository` folder

2. Create `StreetRepository` class in it and make it static

3. Move static `Streets` collection from `AddressController` to `AddressRepository`.

4. In `AddressController`, replace all the usage of `Streets` with `AddressRepository.Streets`.

Now you can use that structure as storage for your data from different controllers.

5. Create a controller `HouseNumberController` that uses the `HouseNumber` model with `int Number` and `string Addendum` properties.

6. Let house numbers be part of `Street` class; e.g. store them in a list.

7. Implement good `GET`, `POST` and `DELETE` actions for a particular house number.

    > NOTE: you will need additional `StreetId` property in `HouseNumber` model for POST request.
    > 
    > Why doesn't it make sense to implement `PUT` method here?

8. Implement a good `GET` action that, for a given street identifier, returns all house numbers for that street. Have house numbers sorted first by number and then by addendum.

### 3.8 Exercise: Endpoint for Recording Logs

Implement an endpoint that records logs and returns them on request. The endpoint is http://localhost:5123/api/admin/logs. Logs are logged using POST requests and retrieved using GET requests. One log contains timestamp (date and time), log level (number from 1 to 8) and message. Endpoint needs to retrieve last N logs, and that parameter is passed using `int last`. If user passed no such parameter, return 20 logs.

### 3.9 Exercise: Product Administration

Use the same project to solve this task as for the last task.

Implement an endpoint to retrieve, add, update and delete products. Let the URL be http://localhost:{port}/api/store/product. Let the product have a name, description, price, creation date and rating. To identify the product you will also need an identifier.

Also implement:
- retrieving an individual product
- searching
- paging

### 3.10 Exercise: Logging product administration

Use the same project to solve this task as for the last task.

Log changes and errors in product administration.

Test the API.
