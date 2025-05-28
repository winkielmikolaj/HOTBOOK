using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HOTBOOK.Models;
using HOTBOOK.Data;
using Microsoft.EntityFrameworkCore;

namespace HOTBOOK.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                // First, ensure Guest role exists
                if (!await _roleManager.RoleExistsAsync("Guest"))
                {
                    _logger.LogInformation("Creating Guest role");
                    var createRoleResult = await _roleManager.CreateAsync(new IdentityRole("Guest"));
                    if (!createRoleResult.Succeeded)
                    {
                        _logger.LogError("Failed to create Guest role. Errors: {Errors}", 
                            string.Join(", ", createRoleResult.Errors.Select(e => e.Description)));
                        return Page();
                    }
                }

                var user = new ApplicationUser 
                { 
                    UserName = Input.Email, 
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                _logger.LogInformation("Attempting to create user with email: {Email}", Input.Email);

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully with email: {Email}", Input.Email);

                    // Get the Guest role
                    var guestRole = await _roleManager.FindByNameAsync("Guest");
                    if (guestRole != null)
                    {
                        // Assign Guest role to new user using UserManager
                        _logger.LogInformation("Attempting to assign Guest role to user: {Email}", Input.Email);
                        
                        var addToRoleResult = await _userManager.AddToRoleAsync(user, "Guest");
                        if (addToRoleResult.Succeeded)
                        {
                            _logger.LogInformation("Successfully assigned Guest role to user: {Email}", Input.Email);
                            
                            // Verify role assignment
                            var userRoles = await _userManager.GetRolesAsync(user);
                            _logger.LogInformation("User {Email} has roles: {Roles}", Input.Email, string.Join(", ", userRoles));
                        }
                        else
                        {
                            _logger.LogError("Failed to assign Guest role to user: {Email}. Errors: {Errors}", 
                                Input.Email, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        _logger.LogError("Guest role not found in database");
                    }

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    _logger.LogError("Failed to create user: {Email}. Errors: {Errors}", 
                        Input.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
} 