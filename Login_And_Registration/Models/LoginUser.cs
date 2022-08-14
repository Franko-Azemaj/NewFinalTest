#pragma warning disable CS8618
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Login_And_Registration.Models;
public class LoginUser
{
    // No other fields!
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; } 
}