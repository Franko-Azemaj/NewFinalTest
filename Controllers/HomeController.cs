using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewFinalTest.Models;
using Microsoft.EntityFrameworkCore ;
using Microsoft.AspNetCore.Identity;

namespace NewFinalTest.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;
    public HomeController(ILogger<HomeController> logger,MyContext context)
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

    [HttpPost("Register")]
    public IActionResult Register(User user)
    {
        // Check initial ModelState
        if (ModelState.IsValid)
        {
            // If a User exists with provided email
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                // Manually add a ModelState error to the Email field, with provided
                // error message
                ModelState.AddModelError("UserName", "UserName already in use!");
               
                return View("index");
                // You may consider returning to the View at this point
            }
              PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);
             _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("userId", user.UserId );
            return RedirectToAction("hobbies");
        }
        return View("index");

    }

    [HttpGet("hobbies")]
    public IActionResult hobbies()
    {
        IList<Hobby> hobbies =_context.Hobbies
        .Include(h => h.Enthusiasts)
        .ToList();

        return View(hobbies);
    }

    [HttpGet("Register")]
    public IActionResult Register(){

        
        if(HttpContext.Session.GetInt32("userId") == null){
          
              return View();
            }
            
        return RedirectToAction("hobbies");

    }

    [HttpPost("Login")]
    public IActionResult LoginSubmit(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            // If initial ModelState is valid, query for a user with provided email
            var userInDb = _context.Users.FirstOrDefault(u => u.UserName == userSubmission.UserName);
            // If no user exists with provided email
            if (userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("User", "Invalid UserName/Password");
                return View("index");
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View("index");
                // handle failure (this should be similar to how "existing email" is handled)
            }
            HttpContext.Session.SetInt32("userId", userInDb.UserId );

            return RedirectToAction("hobbies");
        }
        
        return View("index");
    }

     [HttpGet("logout")]
    public IActionResult Logout(){

        HttpContext.Session.Clear();
        return RedirectToAction("index");
    }

    [HttpGet("hobbies/new")]
    public IActionResult Create(){

        return View("newHobbies");
    }

    [HttpPost("hobbies/new")]
    public IActionResult Create(Hobby newHobby){
         if (ModelState.IsValid)
        {
            int id = (int)HttpContext.Session.GetInt32("userId");

            newHobby.UserId =id;
            _context.Hobbies.Add(newHobby);

            _context.SaveChanges();

            IList<Hobby> hobbies = _context.Hobbies.ToList();

            return RedirectToAction("hobbies",hobbies);
        }
        
        return View("newHobbies");
    }

    [HttpGet("hobbies/{id}")]
    public IActionResult Show(int id )
    {
        


        Hobby hobby = _context.Hobbies.
            Include(e => e.Creator)
            .Include(h => h.Enthusiasts)
            .ThenInclude(e=> e.UseriQePelqen)
            .Where(e => e.HobbyId ==id)
            .First();

        return View("HobbiesDetailed",hobby);
    }

    [HttpPost("addHobby")]
    public IActionResult addEnthusiast([FromForm] int hobbyId )
    {
        int idFromSession = (int)HttpContext.Session.GetInt32("userId");

        Enthusiast newEnthusiast = new Enthusiast(){

            UserId =idFromSession,
            HobbyId = hobbyId

        };

        _context.Enthusiasts.Add(newEnthusiast);
        _context.SaveChanges();



        return Redirect("hobbies/");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
