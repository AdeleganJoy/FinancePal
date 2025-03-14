using System.Text;
using FinancePal.Models;
using Microsoft.AspNetCore.Mvc;
using FinancePal.Services;
using System.Diagnostics;


namespace FinancePal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FinancePalDbContext _context;

    private readonly Authentifier _authentifier;

    public HomeController(ILogger<HomeController> logger, FinancePalDbContext context, Authentifier authentifier)
        {
            _logger = logger;
            _context = context;
            _authentifier = authentifier; 
        }

    // GET: Home Page
    [HttpGet]
    public IActionResult Index()
    {
       var logged_user = HttpContext.Session.GetString("LoggedInUser");
       var user = _context.Users.FirstOrDefault(u => u.Username == logged_user);
        if (string.IsNullOrEmpty(logged_user)){
            return RedirectToAction("Login");
        }

        var expenses = _context.Expenses
            .Where(e => e.UserId == user.Id)
            .OrderByDescending(e => e.Date) 
            .ToList();

        var categoryExpenseData = expenses
            .GroupBy(e => e.Category)
            .Select(g => new{
                Category = g.Key,
                TotalAmount = g.Sum(e => e.Amount)
            })
            .ToList();

        var x = categoryExpenseData.Select(e => e.Category).ToArray();
        var y = categoryExpenseData.Select(e => e.TotalAmount).ToArray();
        var barColors = new string[]
        {
            "#b91d47", "#00aba9", "#2b5797", "#e8c3b9", "#1e7145", "#f39c12", "#8e44ad"
        };

        ViewBag.XValues = x;
        ViewBag.YValues = y;
        ViewBag.BarColors = barColors;
        ViewBag.Balance = user.Amount;

        return View(expenses);
    }

    // POST: Add Expense to database
    [HttpPost]
    public IActionResult Index(Expense model)
    {
        var logged_user = HttpContext.Session.GetString("LoggedInUser");
        var user = _context.Users.FirstOrDefault(u => u.Username == logged_user);

        if (user != null)
        {
            if (model.Category == "salary"){
                user.Amount += model.Amount;
            }
            else{
                user.Amount -= model.Amount;
            }
            _context.Users.Update(user);
            _context.SaveChanges();

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
        _logger.LogInformation($"Sign-Up Info: Username - {model.Username}, Email - {model.Email}, Amount - {model.Amount}");
        var user_exist = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user_exist == null)
        {
            string salt;
            string hashedPassword = _authentifier.HashPassword(model.Password, out salt);
            model.Password = hashedPassword;
            model.Salt = salt;

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

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

        if (user != null){
            if (_authentifier.ValidatePassword(password, user.Password, user.Salt)){
                HttpContext.Session.SetString("LoggedInUser", user.Username);
                return RedirectToAction("Index");
            }
        }
        ViewBag.ErrorMessage = "The username or password is invalid";
        return View();

    }

    public IActionResult Logout(){
        HttpContext.Session.Remove("LoggedInUser");
        return RedirectToAction("Login");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
