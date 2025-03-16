using System.Text;
using FinancePal.Models;
using Microsoft.AspNetCore.Mvc;
using FinancePal.Services;
using System.Diagnostics;
using FinancePal.Calculations.FinancePal.Calculations;

namespace FinancePal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FinancePalDbContext _context;

    private readonly Authentifier _authentifier;
    private FinanceCal _financeCalculator;

    public HomeController(ILogger<HomeController> logger, FinancePalDbContext context, Authentifier authentifier)
        {
            _logger = logger;
            _context = context;
            _authentifier = authentifier; 
            _financeCalculator = new FinanceCal();
        }

    // GET: Home Page
    [HttpGet]
    public IActionResult Index()
    {
    //Check if a user is logged in by retrieving the username from the session 
       var logged_user = HttpContext.Session.GetString("LoggedInUser");
       var user = _context.Users.FirstOrDefault(u => u.Username == logged_user);
       
       // If the user is not logged in, redirect them to the login page
        if (string.IsNullOrEmpty(logged_user)){
            return RedirectToAction("Login");
        }
        // Retrieve all expenses associated with the logged-in user, ordered by date descending
        var expenses = _context.Expenses
            .Where(e => e.UserId == user.Id)
            .OrderByDescending(e => e.Date) 
            .ToList();

        // Group expenses by category and calculate the total amount for each category
        var categoryExpenseData = expenses
            .Where(e => e.Category != "salary")  // Remove Category salary from the pie chart
            .GroupBy(e => e.Category)
            .Select(g => new{
                Category = g.Key,
                TotalAmount = g.Sum(e => e.Amount)
            })
            .ToList();
        
        // Setup data for showing pie graph for Categories& Expenses
        var x = categoryExpenseData.Select(e => e.Category).ToArray();
        var y = categoryExpenseData.Select(e => e.TotalAmount).ToArray();
        //Setup Color for Pie Chart
        var pieColors = new string[]
        {
            "#b91d47", "#00aba9", "#2b5797", "#e8c3b9", "#1e7145", "#f39c12", "#8e44ad"
        };
        //Send pie chart and User amount information to front-end
        ViewBag.XValues = x;
        ViewBag.YValues = y;
        ViewBag.PieColors = pieColors;
        ViewBag.Balance = user.Amount;

        return View(expenses);
    }

    // POST: Add Expense to database
    [HttpPost]
    public IActionResult Index(Expense model)
    {
        //Check if a user is logged in by retrieving the username from the session 
        var logged_user = HttpContext.Session.GetString("LoggedInUser");
        var user = _context.Users.FirstOrDefault(u => u.Username == logged_user);
        //Check if user exists
        if (user != null)
        {
            //Change the balance
            user.Amount = _financeCalculator.CalcBalance(user.Amount, model.Amount, model.Category); // Use FinanceCal
        
            //Update the User database
            _context.Users.Update(user);
            _context.SaveChanges();

            //Update the Expense database
            model.UserId = user.Id;
            model.Date = DateTime.Now;
            _context.Expenses.Add(model);
            _context.SaveChanges();
            _logger.LogInformation($"Expense added: User ID: {model.UserId}, Category: {model.Category}, Amount: {model.Amount}");

        }
        else
        {
            _logger.LogWarning("User not found.");
        }

        return RedirectToAction("Index");
    }


    // GET: SignUp Page
    [HttpGet]
    public IActionResult SignUp(){
        return View();
    }

    // POST: Handling SignUp
    [HttpPost]
    public IActionResult SignUp(User model){
        //Check if user exists in system
        var user_exist = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user_exist == null)
        {
            //Encrypt the password
            string salt;
            string hashedPassword = _authentifier.HashPassword(model.Password, out salt);
            model.Password = hashedPassword;
            model.Salt = salt;

            //add the new user to the database
            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
        //Send error message to the frontend if the user exists
        ViewBag.ErrorMessage = "A user with this email already exists.";
        return View();
    }

    // GET: Login Page
    [HttpGet]
    public IActionResult Login(){
        return View();
    }

    // POST: Handling Login
    [HttpPost]
    public IActionResult Login(string username, string password){
        var user = _context.Users
            .FirstOrDefault(u => u.Username == username);
            
        //Check if username exists in the database
        if (user != null){
            //Check if the password is correct
            if (_authentifier.ValidatePassword(password, user.Password, user.Salt)){
                HttpContext.Session.SetString("LoggedInUser", user.Username);
                return RedirectToAction("Index");
            }
        }
        ViewBag.ErrorMessage = "The username or password is invalid"; // Send error to the front end
        return View();

    }
    //POST: Handling LogOut
    [HttpPost]
    public IActionResult Logout(){
        //End session for the logged user
        HttpContext.Session.Remove("LoggedInUser");
        return RedirectToAction("Login");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
