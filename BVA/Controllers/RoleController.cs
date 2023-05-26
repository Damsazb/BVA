using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BVA.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace BVA.Controllers
    {
    [Authorize(Roles = "Administrator,admin")]

    public class RoleController : Controller
    {
        readonly RoleManager<IdentityRole> roleManager;
        readonly UserManager<IdentityUser> userManager;
        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;

        }
        public IActionResult Index()
        {
            ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            if (!User.IsInRole("Administrator"))
                ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name").Where(x => x.Value != "Administrator");
            ViewData["User"] = new SelectList(userManager.Users, "UserName", "UserName");
            return View(roleManager.Roles);
        }
        public async Task<IActionResult> Add(string Name)
        {

            var identityRole = CreateRule();
            identityRole.Name = Name;
            await roleManager.CreateAsync(identityRole);
            ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            if (!User.IsInRole("Administrator"))
                ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name").Where(x => x.Value != "Administrator");
            ViewData["User"] = new SelectList(userManager.Users, "UserName", "UserName");
            return View("index", roleManager.Roles);
        }

        public async Task<IActionResult> Show()
        {

            List<UserRole> UserRole = new List<UserRole>();
            foreach (var User in userManager.Users)
            {

                var Roles = new List<string>(await userManager.GetRolesAsync(User));


                UserRole.Add(new UserRole(User.Id, User.UserName, Roles));
                //  vm.UserName = user.UserName;
            }
            return View(UserRole);
        }

        public async Task<IActionResult> DeleteUser(string UserId)
        {
            var UserName = userManager.Users.FirstOrDefault(u => u.Id == UserId).UserName;
            if (UserName != User.Identity.Name)
            {
                if (User.IsInRole("Administrator"))
                {
                    var User = userManager.Users.FirstOrDefault(u => u.Id == UserId);

                    var result2 = await userManager.DeleteAsync(User);
                }
            }
            ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            if (!User.IsInRole("Administrator"))
                ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name").Where(x => x.Value != "Administrator");
            ViewData["User"] = new SelectList(userManager.Users, "UserName", "UserName");
            return View("index", roleManager.Roles);
        }
        public async Task<IActionResult> ResetPassword(string UserId)
        {
            var UserName = userManager.Users.FirstOrDefault(u => u.Id == UserId).UserName;
            if (UserName != User.Identity.Name)
            {
                PasswordHasher<IdentityUser> hasher = new PasswordHasher<IdentityUser>();
                var User = userManager.Users.FirstOrDefault(u => u.Id == UserId);
                User.PasswordHash = hasher.HashPassword(null, "1");

                var result2 = await userManager.UpdateAsync(User);
            }
            ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            if (!User.IsInRole("Administrator"))
                ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name").Where(x => x.Value != "Administrator");
            ViewData["User"] = new SelectList(userManager.Users, "UserName", "UserName");
            return View("index", roleManager.Roles);
        }
        public async Task<IActionResult> Delete(string Id)
        {
            var role = roleManager.Roles.FirstOrDefault(u => u.Id == Id);

            var result2 = await roleManager.DeleteAsync(role);
            ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            if (!User.IsInRole("Administrator"))
                ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name").Where(x => x.Value != "Administrator");
            ViewData["User"] = new SelectList(userManager.Users, "UserName", "UserName");
            return View("index", roleManager.Roles);
        }

        public async Task<IActionResult> DeleteRoleUser(string Role, string UserId)
        {
            var user = await userManager.FindByIdAsync(UserId);
            if (!Role.Equals("Administrator"))
            {
                await userManager.RemoveFromRoleAsync(user, Role);
            }
            else if (User.IsInRole("Administrator"))
                await userManager.RemoveFromRoleAsync(user, Role);
            List<UserRole> UserRole = new List<UserRole>();
            foreach (var User in userManager.Users)
            {

                var Roles = new List<string>(await userManager.GetRolesAsync(User));


                UserRole.Add(new UserRole(User.Id, User.UserName, Roles));
                //  vm.UserName = user.UserName;

            }
            return View("Show", UserRole);
        }
        public async Task<IActionResult> AddUserRole(string UserName, string Role)
        {
            var username = userManager.Users.FirstOrDefault(u => u.UserName == UserName);
            if (User.IsInRole("Administrator") || User.IsInRole("admin") && !Role.Equals("Administrator"))
            {
                await userManager.AddToRoleAsync(username, Role);
            }
            else if (User.IsInRole("Administrator") && Role.Equals("Administrator"))
            {
                await userManager.AddToRoleAsync(username, Role);
            }
            ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            if (!User.IsInRole("Administrator"))
                ViewData["Roles"] = new SelectList(roleManager.Roles, "Name", "Name").Where(x => x.Value != "Administrator");
            ViewData["User"] = new SelectList(userManager.Users, "UserName", "UserName");
            return View("index", roleManager.Roles);
        }

        private IdentityRole CreateRule()
        {
            try
            {
                return Activator.CreateInstance<IdentityRole>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityRole)}'. " +
                    $"Ensure that '{nameof(IdentityRole)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in Role.cshtml");
            }
        }

    }
}
