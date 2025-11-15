using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure middleware pipeline in the correct order:
// 1. Error handling middleware (catches unhandled exceptions)
app.UseErrorHandling();

// 2. Authentication middleware (validates tokens)
app.UseTokenAuthentication();

// 3. Logging middleware (logs all requests and responses)
app.UseRequestLogging();

// GET /api/users - Optimized with performance improvements
app.MapGet("/api/users", () =>
{
    try
    {
        var users = UserRepository.GetAll();
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving users: {ex.Message}");
        return Results.StatusCode(500);
    }
});

// GET /api/users/{id} - Error handling for failed lookups
app.MapGet("/api/users/{id:int}", (int id) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("Invalid user ID");

        var user = UserRepository.GetById(id);
        if (user is null)
            return Results.NotFound(new { message = $"User with ID {id} not found" });

        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving user {id}: {ex.Message}");
        return Results.StatusCode(500);
    }
});

// POST /api/users - Input validation and error handling
app.MapPost("/api/users", (User user) =>
{
    try
    {
        var validationContext = new ValidationContext(user);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = validationResults.Select(v => v.ErrorMessage).ToList();
            return Results.BadRequest(new { errors });
        }

        var created = UserRepository.Add(user);
        return Results.Created($"/api/users/{created.Id}", created);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating user: {ex.Message}");
        return Results.StatusCode(500);
    }
});

// PUT /api/users/{id} - Input validation and error handling
app.MapPut("/api/users/{id:int}", (int id, User updatedUser) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("Invalid user ID");

        var validationContext = new ValidationContext(updatedUser);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(updatedUser, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = validationResults.Select(v => v.ErrorMessage).ToList();
            return Results.BadRequest(new { errors });
        }

        var success = UserRepository.Update(id, updatedUser);
        if (!success)
            return Results.NotFound(new { message = $"User with ID {id} not found" });

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating user {id}: {ex.Message}");
        return Results.StatusCode(500);
    }
});

// DELETE /api/users/{id} - Error handling for failed lookups
app.MapDelete("/api/users/{id:int}", (int id) =>
{
    try
    {
        if (id <= 0)
            return Results.BadRequest("Invalid user ID");

        var success = UserRepository.Delete(id);
        if (!success)
            return Results.NotFound(new { message = $"User with ID {id} not found" });

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting user {id}: {ex.Message}");
        return Results.StatusCode(500);
    }
});

app.Run();

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 50 characters")]
    required public string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email format is invalid")]
    required public string Email { get; set; }
}