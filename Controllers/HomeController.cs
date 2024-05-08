using System.Net.Http;
using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using grpc.Models;
using System.Text.Json;
using Npgsql;
using System.Data; // Add this using directive

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly ServiceProvider serviceProvider;
    private readonly string connectionString = "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=root;";
public class DatabaseContext : IDisposable
{
    private readonly string _connectionString;
    private NpgsqlConnection _connection;

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new NpgsqlConnection(_connectionString);
    }

    public void Open()
    {
        _connection.Open();
    }

    public void Dispose()
    {
        if (_connection != null && _connection.State != ConnectionState.Closed)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }

    public NpgsqlConnection GetConnection()
    {
        return _connection;
    }
}
public class Product
{
    public int id { get; set; }
    public string title { get; set; }
    public double price { get; set; }
    public string description { get; set; }
    public string category { get; set; }
    public string image { get; set; }
    public Rating rating { get; set; }
     public override string ToString()
    {
        return $"Id: {id}, Title: {title}, Price: {price}, Description: {description}, Category: {category}, Image: {image}, Rating: {rating}";
    }
}

public class Rating
{
    public double rate { get; set; }
    public int count { get; set; }
    public override string ToString()
    {
        return $"Rate: {rate}, Count: {count}";
    }
}

    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    }

    public async Task<IActionResult> Index()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("https://fakestoreapi.com/products");

        if (response.IsSuccessStatusCode)
        {
            // Parse the API response
            var responseData = await response.Content.ReadAsStringAsync();
            List<Product> productList = JsonSerializer.Deserialize<List<Product>>(responseData);

            // Console.WriteLine(typeof(jsonArray));
            // Pass the API response data to the view
            ViewBag.ApiData = productList;

            return View();
        }
        else
        {
            Console.WriteLine("Failed to fetch data from the API.");
            // Handle API error
            ViewBag.ErrorMessage = "Failed to fetch data from the API.";
            return View();
        }
    }

    public IActionResult Orders()
    {
        if (HttpContext.Session.GetString("Username") == null)
        {
            // User is not authenticated, redirect to login page
            return RedirectToAction("Login", "Home");
        }
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(string username, string password)
    {
        try
        {
            // Use DatabaseContext to establish connection
            using (var db = new DatabaseContext(connectionString))
            {
                db.Open();

                // Example query to validate user
                string sql = "SELECT COUNT(*) FROM users WHERE email = @username AND password_hash = @password";
                using (var cmd = new NpgsqlCommand(sql, db.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                                  
                        // User authenticated, retrieve complete user data
                        sql = "SELECT * FROM users WHERE email = @username";
                        using (var getUserCmd = new NpgsqlCommand(sql, db.GetConnection()))
                        {
                            getUserCmd.Parameters.AddWithValue("username", username);
                            using (var reader = getUserCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Read user data from the reader
                                    string Username = reader["username"].ToString();
                                    string name = reader["name"].ToString(); // Example: adjust field names as per your schema
                                    string email = reader["email"].ToString(); // Example: adjust field names as per your schema
                                    string userId = reader["user_id"].ToString(); // Example: adjust field names as per your schema
                                    string phone = reader["user_id"].ToString(); // Example: adjust field names as per your schema

                                    // Store user data in session
                                    HttpContext.Session.SetString("Username", Username);
                                    HttpContext.Session.SetString("name", name);
                                    HttpContext.Session.SetString("email", email);
                                    HttpContext.Session.SetString("userId", userId);
                                    HttpContext.Session.SetString("phone", phone);

                                    // You can store more user data in session if needed

                                    // Optionally, you can redirect or return success
                                }
                            }
                        }
                        return RedirectToAction("Index", "Home"); // Redirect to home page upon successful login
                    }
                    else
                    {
                        // Invalid credentials, display error
                        ViewBag.ErrorMessage = "Invalid username or password.";
                        return View();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions (e.g., database connection error)
            ViewBag.ErrorMessage = "An error occurred during login.";
            return View();
        }
    }

    public IActionResult Signup()
    {
        return View();
    }

    // POST action for handling signup form submission
    [HttpPost]
    public IActionResult Signup(string username, string password, string email, string name )
    {
        try
        {
            // Use DatabaseContext to establish connection
            using (var db = new DatabaseContext(connectionString))
            {
                db.Open();

                // Check if the username is already taken
                string checkUsernameQuery = "SELECT COUNT(*) FROM users WHERE username = @username or email = @email";
                using (var checkUsernameCmd = new NpgsqlCommand(checkUsernameQuery, db.GetConnection()))
                {
                    checkUsernameCmd.Parameters.AddWithValue("username", username);
                    checkUsernameCmd.Parameters.AddWithValue("email", email);

                    int existingUserCount = Convert.ToInt32(checkUsernameCmd.ExecuteScalar());

                    if (existingUserCount > 0)
                    {
                        ViewBag.ErrorMessage = "Username or email is already taken. Please choose a different username or email.";
                        return View();
                    }
                }

                // Insert new user record into the database
                string insertUserQuery = "INSERT INTO users (name, email, username, password_hash) VALUES (@name, @email, @username, @password)";
                using (var insertUserCmd = new NpgsqlCommand(insertUserQuery,db.GetConnection()))
                {
                    insertUserCmd.Parameters.AddWithValue("name", name);
                    insertUserCmd.Parameters.AddWithValue("email", email);
                    insertUserCmd.Parameters.AddWithValue("username", username);
                    insertUserCmd.Parameters.AddWithValue("password", password);

                    int rowsAffected = insertUserCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ViewBag.SuccessMessage = "User registered successfully!";
                        return View("Login"); // Redirect to login page after successful registration
                    }
                    else
                    {
                        Console.WriteLine("faile to signup");
                        ViewBag.ErrorMessage = "Failed to register user. Please try again.";
                        return View();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            ViewBag.ErrorMessage = "An error occurred during signup.";
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
