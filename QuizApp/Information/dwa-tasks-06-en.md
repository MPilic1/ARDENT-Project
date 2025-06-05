# RESTful architecture (Web API)

This material is part of Learning Objective 1 (desired).

## 6 JSON Web Token (JWT)

JWT represents claims to be transferred between client and server. It is encoded set of numbers and letters that proves the user is the one who he claims to be. You can consider the token as a form of login.

JWT configuration in form of SecurityTokenDescriptor:

- https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.securitytokendescriptor?view=msal-web-dotnet-latest

JWT debugger:
- https://jwt.io/

By using JWT support in ASP.NET Core, you can:
- Stop unwanted access to resources (controllers and actions) in your project
- Allow access to secured resources to registered users only

---

To stop unwanted access to resources in your project, you have to:
- Install NuGet packages for JWT
- Use middleware in `Program.cs` to set up and configure JWT security
- Use `[Authorize]` attribute on controllers or actions that you want to secure

To allow access to secured resources in your project, you have to:
- Create and return JWT token to client in unsecured endpoint
- Use that token in Authorization header when issuing requests to secured endpoint
- Optional: set up JWT support in Swagger

To allow access to secured resources in your project only to registered users, you have to:
- Support user registration
- Create and return JWT token for the particular user
  - That can be considered as performing user login

### 6.1 Securing Controllers and Actions

1. You need to install the following packages into the project:

    ```
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8
    dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect --version 8
    ```

2. In Program.cs, set up middleware to configure JWT security. Remember to add services before building web application using ´var app = builder.Build()´, and middleware after it.

    ```C#
    // Configure JWT security services
    var secureKey = "12345678901234567890123456789012";
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o => {
            var Key = Encoding.UTF8.GetBytes(secureKey);
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Key)
            };
        });
    ```

    ```
    // Use authentication / authorization middleware
    app.UseAuthentication();
    app.UseAuthorization(); // -> this should already be present
    ```

    > If you examine the code, you can see that a secure key is used to authenticate JWT token. This key is something known only to your server and should not be shared with anyone else. Also, it should not be hardcoded. We will move the key to configuration later.

3. Create Web API read/write actions controller named SecuredController.  
Add `[Authorize]` attribute before first `Get()` action.  
Try the controller in Swagger.

    > Secured action will return `Error: Unauthorized` due to securing it with `[Authorize]` attribute. Unsecured action will return data.

4. Now mark entire controller with `[Authorize]` attribute.  
Try the controller in Swagger.  

    > All the actions of the secured controller will return `Error: Unauthorized` due to securing it with `[Authorize]` attribute.

### 6.2 Allowing Access to Secured Resources

1. Create Web API (Empty) `UserController` that is going to support JWT token creation.

2. Add new folder named `Security`, and create the `JwtTokenProvider` class in that folder.

    ```C#
    public class JwtTokenProvider
    {
        public static string CreateToken(string secureKey, int expiration, string subject = null)
        {
            // Get secret key bytes
            var tokenKey = Encoding.UTF8.GetBytes(secureKey);

            // Create a token descriptor (represents a token, kind of a "template" for token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expiration),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)                
            };

            if (!string.IsNullOrEmpty(subject))
            {
                tokenDescriptor.Subject = new ClaimsIdentity(new System.Security.Claims.Claim[]
                {
                    new System.Security.Claims.Claim(ClaimTypes.Name, subject),
                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, subject),
                });
            }

            // Create token using that descriptor, serialize it and return it
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var serializedToken = tokenHandler.WriteToken(token);
            
            return serializedToken;
        }
    }
    ```

    You will use this class to create the JWT token using the secure key.  

    > Note: You may use that class in your project and change it in the way you prefer. Note that `SecurityTokenDescriptor` allows for additional parameters. Feel free to investigate these parameters.

2. Create `GetToken()` action that returns JWT token

    ```C#
    [HttpGet("[action]")]
    public ActionResult GetToken()
    {
        try
        {
            // The same secure key must be used here to create JWT,
            // as the one that is used by middleware to verify JWT
            var secureKey = "12345678901234567890123456789012";
            var serializedToken = JwtTokenProvider.CreateToken(secureKey, 10);

            return Ok(serializedToken);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    ```

    Test this action in swagger.

    > Use https://jwt.io/ to see the token contents. There is a `Debugger` link and textbox to enter the token. You can see token contents on the right side.

3. Move secure key to the configuration.

    In `appsettings.json`, create `JWT` section and in that section add the `SecureKey` that you need in both middleware and your JWT creation.

    ```JSON
    "JWT": {
      "SecureKey": "12345678901234567890123456789012"
    }
    ```

    > In reality, when using JWT you will probably need a better secure key like, for example `E(H+MbQeThWmZq4t6w9z$C&F)J@NcRfU` :)

4. To read the configuration in your `UserController` (where you need it), you have to allow DI container to pass it to the constructor of your controller:

    ```C#
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    ```

5. Now read the key using configuration instead of using hardcoded value

    ```C#
    var secureKey = _configuration["JWT:SecureKey"];
    var serializedToken = JwtTokenProvider.CreateToken(secureKey, 10);
    ```

6. To read the configuration in `Program.cs` (where you also need it), you just read it directly from app builder:

    ```C#
    var secureKey = builder.Configuration["JWT:SecureKey"];
    ```

7. To allow Swagger to accept the JWT token, you need to configure its UI using Swagger service configuration in `Program.cs`

    ```C#
    builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1",
            new OpenApiInfo { Title = "RWA Web API", Version = "v1" });

        option.AddSecurityDefinition("Bearer",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter valid JWT",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

        option.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
    });
    ```

8. Now test the JWT in Swagger

    - Use `GetToken()` action to get the JWT
    - Use `Authorize` button on the top of the page to save JWT into the browser
    - Try the secured endpoint now

    > Note: JWT is sent using the `Authorization` header in HTTP request. You can observe that by examining request headers in Development Tools of the browser

### 6.3 Suporting User Registration

You don't want to issue JWT to the unknown user, because any user with JWT can have access to your secured resources. This means that you need a list of your registered users (read: database table) with passwords.  

The important thing is not to have password in the database as a clear text, but the cryptographic hash of that password. When user tries to log in, the password that he enters will be compared with the cryptographic hash and if it matches, he will be allowed to get JWT.

So, first we support EF database in our project.  
This is the same thing we did in our workshop 4.  

1. Let's create database `Exercise6` and a table for users

    ```SQL
    CREATE TABLE [dbo].[User](
      [Id] [int] IDENTITY(1,1) NOT NULL,
      [Username] [nvarchar](50) NOT NULL, -- Use this for login
      [PwdHash] [nvarchar](256) NOT NULL, -- Use to check password hash
      [PwdSalt] [nvarchar](256) NOT NULL, -- Additional level of security (random string)
      [FirstName] [nvarchar](256) NOT NULL,
      [LastName] [nvarchar](256) NOT NULL,
      [Email] [nvarchar](256) NOT NULL,
      [Phone] [nvarchar](256) NULL,
      CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED (
        [Id] ASC
      )
    )
    ```

2. Let's use `Package Manager Console` to install database support and configure it. **Pay attention to properly change the connection string.**

    ```
    dotnet tool install --global dotnet-ef --version 8

    dotnet add package Microsoft.EntityFrameworkCore --version 8
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 8
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8

    dotnet ef dbcontext scaffold "server=.;Database=Exercise6;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
    ```

3. Cut/paste connection string from generated database context `Exercise6Context` to `appsettings.json`:

    ```JSON
    "ConnectionStrings": {
      "ex6cs": "{here-goes-the-pasted-connection-string}"
    }
    ```

4. In `Exercise6Context.cs` you should have this:

    ```C#
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=ConnectionStrings:ex6cs");
    ```

5. In `Program.cs` you should add this:

    ```C#
    builder.Services.AddDbContext<Exercise6Context>(options => {
        options.UseSqlServer("name=ConnectionStrings:ex6cs");
    });
    ```

6. Now we can support user registration in `UserController.cs`. First we use DI to get the database context.

    ```C#
    private readonly IConfiguration _configuration;
    private readonly Exercise6Context _context;

    public UserController(IConfiguration configuration, Exercise6Context context)
    {
        _configuration = configuration;
        _context = context;
    }
    ```

7. We need DTO class for the user, so we create `Dtos` folder and `UserDto` class inside:

    ```C#
    public class UserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User name is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password should be at least 8 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name should be between 2 and 50 characters long")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name should be between 2 and 50 characters long")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Provide a correct e-mail address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Provide a correct phone number")]
        public string Phone { get; set; }
    }
    ```

8. To support cryptographic operation of creating salt and hash values, we will create the utility class `PasswordHashProvider` in `Security` folder:

    ```C#
    public class PasswordHashProvider
    {
        public static string GetSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
            string b64Salt = Convert.ToBase64String(salt);

            return b64Salt;
        }

        public static string GetHash(string password, string b64salt)
        {
            byte[] salt = Convert.FromBase64String(b64salt);

            byte[] hash =
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8);
            string b64Hash = Convert.ToBase64String(hash);

            return b64Hash;
        }
    }
    ```

9. Now we can register user, using created DTO and the cryptographic utility class. Note that we are being careful to avoid duplicate user registration by checking the existing username.

    ```C#
    [HttpPost("[action]")]
    public ActionResult<UserDto> Register(UserDto userDto)
    {
        try
        {
            // Check if there is such a username in the database already
            var trimmedUsername = userDto.Username.Trim();
            if (_context.Users.Any(x => x.Username.Equals(trimmedUsername)))
                return BadRequest($"Username {trimmedUsername} already exists");

            // Hash the password
            var b64salt = PasswordHashProvider.GetSalt();
            var b64hash = PasswordHashProvider.GetHash(userDto.Password, b64salt);

            // Create user from DTO and hashed password
            var user = new User
            {
                Id = userDto.Id,
                Username = userDto.Username,
                PwdHash = b64hash,
                PwdSalt = b64salt,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone,
            };

            // Add user and save changes to database
            _context.Add(user);
            _context.SaveChanges();

            // Update DTO Id to return it to the client
            userDto.Id = user.Id;

            return Ok(userDto);

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    ```

    > Note: the `Register` endpoint is meant for self-registration.  
    > If you secure `Register` endpoint, users won't be allowed to register.

10. Now you can test your user registration by using new `Register()` Swagger endpoint.  
Example JSON DTO:

    ```JSON
    {
      "username": "johnny1234",
      "password": "qwertzuiop",
      "firstName": "John",
      "lastName": "Smith",
      "email": "johnsmith1234@example.com",
      "phone": "0987654321"
    }
    ```

### 6.4 Suporting User Login

As already mentioned, login process includes checking if user exists and if yes, getting a token. Token is usually personalized, meaning it contains claims about user (user name, role, etc...).  
After login, user gets the token and will send that token back to server when accessing a secured endpoint.  

> Note: for this to work, you need to create `UserLoginDto` class

  ```C#
  [HttpPost("[action]")]
  public ActionResult Login(UserLoginDto userDto)
  {
      try
      {
          var genericLoginFail = "Incorrect username or password";

          // Try to get a user from database
          var existingUser = _context.Users.FirstOrDefault(x => x.Username == userDto.Username);
          if (existingUser == null)
              return BadRequest(genericLoginFail);

          // Check is password hash matches
          var b64hash = PasswordHashProvider.GetHash(userDto.Password, existingUser.PwdSalt);
          if(b64hash != existingUser.PwdHash)
              return BadRequest(genericLoginFail);

          // Create and return JWT token
          var secureKey = _configuration["JWT:SecureKey"];
          var serializedToken = JwtTokenProvider.CreateToken(secureKey, 120, userDto.Username);

          return Ok(serializedToken);
      }
      catch (Exception ex)
      {
          return StatusCode(500, ex.Message);
      }
  }
  ```

  > If you secure `Login` endpoint, users won't be allowed to login.

  > Use https://jwt.io/ to see the token contents. Now you can see its name. Also, when user performs the authenticated request, the middleware takes care about the JWT contents and can provide them when you need it, along with name and other data in JWT.

### 6.5 Middleware Support for JWT Data 

Middleware takes care about the users identity passed via JWT.  
It wraps the user data into `HttpContext.User.Identity` and you can retrieve it from there.  

  ```C#
  // Just an example in SecuredController
  [HttpGet]
  public ActionResult<string> Get()
  {
      var identity = HttpContext.User.Identity as ClaimsIdentity;
      return identity.FindFirst(ClaimTypes.Name).Value;
  }
  ```

### 6.6 Support for roles

You can support role-based access control via JWT.  
For resources that need to be protected by roles, use the same `Authorize` attribute, and add roles to it.

  ```C#
  [Authorize(Roles = "Admin")]
  ```

  ```C#
  [Authorize(Roles = "Admin,User")]
  ```

For that to work, you need to add a `Role` claim when generating token.

  ```C#
  var role = "Admin" // for example...
  tokenDescriptor.Subject = new ClaimsIdentity(new System.Security.Claims.Claim[]
  {
      new System.Security.Claims.Claim(ClaimTypes.Name, subject),
      new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, subject),
      new System.Security.Claims.Claim(ClaimTypes.Role, role),
  });
  ```

### 6.7 Exercise: Create ASP.NET Web API project with a secured controller

1. Create new Web API project that uses the same `Exercise6` database.  
2. Create database support for `User`, `Product`, `Receipt` and `ReceiptItem` entities and connect the project to your database.  

    `Product` entity attributes:
    - Id (int)
    - Title (string)
    - Price (decimal)

    `Receipt` entity attributes:
    - Id (int)
    - Code (string)
    - Total (decimal)
    - IssuedAt (DateTime)

    `ReceiptItem` entity attributes:
    - Id (int)
    - ProductId (int)
    - ReceiptId (int)
    - Quantity (int)
    - Price (decimal)
    - Value (decimal)

    > Note: pay attention to properly set up foreign keys.

3. Create a controller for registering and signing in user (getting JWT).  
4. Set up JWT support in Swagger.  
5. Create CRUD controller for `Product` entity and CRUD controller for `Receipt` entity
6. Use `ReceiptItem` entities when retrieving/storing `Product` entity
   - add collection of `ReceiptItems` to `Product` entity class
   - when updating the collection, you will have to add setter to generated `ReceiptItems` navigation property member
     - `public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();`
7. Secure the `Receipt` CRUD controller.  

### 6.8 Exercise: Change password

For the previous task, add `ChangePassword` endpoint that allows the user to change its password.

### 6.9 Exercise: Support roles

For the previous task, support `Roles` entity in your database and membership of the user to a role. Roles should be `Admin` and `User`. `Admin` role should be allowed to read and write all the entities. `User` role should be allowed to read products.  
When self-registering user, set his role to `User`.  
Add a secured endpoint `PromoteUser` that allows promoting a registered user to administrator.

