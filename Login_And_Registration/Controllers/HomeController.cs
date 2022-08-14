using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Login_And_Registration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;


namespace Login_And_Registration.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost("register")]
    public IActionResult Create(User newUser)
    {

        if (ModelState.IsValid)
        {

            if (_context.Users.Any(u => u.Email == newUser.Email))
            {
                // Manually add a ModelState error to the Email field, with provided
                // error message
                ModelState.AddModelError("Email", "Email already in use!");
                return View("index");
                // You may consider returning to the View at this point
            }

            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
            _context.Add(newUser);

             _context.SaveChanges();

            return RedirectToAction("login");
        }
        else
        {
            return View("index");
        }
    }


    [HttpPost("login")]
    public IActionResult Login( LoginUser SubmitedUser)
    {

        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with provided email
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == SubmitedUser.Email);
            // If no user exists with provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("login");
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(SubmitedUser, userInDb.Password, SubmitedUser.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                // handle failure (this should be similar to how "existing email" is handled)
                ModelState.AddModelError("Password", "Invalid Password");
                return View("login");
            }

            HttpContext.Session.SetInt32("UserId",userInDb.UserId);
            int? session =HttpContext.Session.GetInt32("UserId");

            return RedirectToAction("success",new{id = session});
        }

        
            return View("login");
        

    }

    [HttpGet("login")]
    public IActionResult SignIn()
    {

        return View("login");
    }

    [HttpGet("success{id}")]
    public IActionResult Success(int id)
    {
        int? session = HttpContext.Session.GetInt32("UserId");
        if(id == session)
        {
            return View();
        }
        else{
            return RedirectToAction("index");
        }
    }

    [HttpGet("logout")]
    public IActionResult LogOut (){

        HttpContext.Session.Clear();

        return RedirectToAction("index");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
