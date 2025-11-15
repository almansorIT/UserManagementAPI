
public static class UserRepository
{
    private static List<User> Users = new();
    private static int nextId = 1;
    // Cache for performance optimization on GET requests
    private static Dictionary<int, User> UserCache = new();

    public static List<User> GetAll()
    {
        try
        {
            return Users;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all users: {ex.Message}");
            throw;
        }
    }

    public static User? GetById(int id)
    {
        try
        {
            if (id <= 0)
                return null;

            // Check cache first for performance
            if (UserCache.TryGetValue(id, out var cachedUser))
                return cachedUser;

            var user = Users.FirstOrDefault(u => u.Id == id);
            
            // Cache the result for future lookups
            if (user != null)
                UserCache[id] = user;

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user by ID {id}: {ex.Message}");
            throw;
        }
    }

    public static User Add(User user)
    {
        try
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.Id = nextId++;
            Users.Add(user);
            
            // Cache the new user
            UserCache[user.Id] = user;
            
            Console.WriteLine($"User added: {user.Username} ({user.Email})");
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user: {ex.Message}");
            throw;
        }
    }

    public static bool Update(int id, User updatedUser)
    {
        try
        {
            if (id <= 0)
                return false;

            if (updatedUser == null)
                throw new ArgumentNullException(nameof(updatedUser));

            var user = GetById(id);
            if (user == null)
                return false;

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            
            // Update cache
            UserCache[id] = user;
            
            Console.WriteLine($"User updated: {user.Username} ({user.Email})");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user {id}: {ex.Message}");
            throw;
        }
    }

    public static bool Delete(int id)
    {
        try
        {
            if (id <= 0)
                return false;

            var user = GetById(id);
            if (user == null)
                return false;

            Users.Remove(user);
            
            // Remove from cache
            UserCache.Remove(id);
            
            Console.WriteLine($"User deleted with ID: {id}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user {id}: {ex.Message}");
            throw;
        }
    }
}