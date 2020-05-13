using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brela.Web.Areas.Identity.Pages.Account;
using Brela.Web.Data;
using Brela.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sys.Web.Services;

namespace Sys.Web.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext _context;
        private IdentityManager _identityManager;
        private UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(ApplicationDbContext context,IdentityManager identityManager,
            UserManager<ApplicationUser> userManager,IEmailSender emailSender,ILogger<AccountController> logger)
        {
            _context = context;
            _identityManager = identityManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }
        [Authorize(Roles = "Admin, CanEditGroup, CanEditUser")]

        public ActionResult Index()
        {
            var users =  _context.Users;
            var model = new List<EditUserViewModel>();
            foreach (var user in users)
            {
                var u = new EditUserViewModel(user);
                model.Add(u);
            }
            return View(model);
        }

        //[Authorize(Roles = "Admin, CanEditUser")]
        public ActionResult Register()
        {
            RegisterViewModel registerViewModel=new RegisterViewModel();
            registerViewModel.GroupEditorViewModels=new List<SelectGroupEditorViewModel>();
            foreach (var group in _context.Groups)
            {
               registerViewModel. GroupEditorViewModels.Add(new SelectGroupEditorViewModel(group));
            }

            return View(registerViewModel);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin, CanEditUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
           
            //returnUrl = returnUrl ?? Url.Content("~/");
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Input.Email, Email = model.Input.Email, FirstName = model.Input.FirstName, LastName = model.Input.LastName };
                var result = await _userManager.CreateAsync(user, model.Input.Password);
                if (result.Succeeded)
                {
                    //_logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = user.Id, code = code },
                    //    protocol: Request.Scheme);

                    foreach (var item in model.GroupEditorViewModels)
                    {

                        if (item.Selected)
                        {

                            await _identityManager.AddUserToGroup(user.Id, item.GroupId);
                        }
                    }

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return RedirectToPage("RegisterConfirmation", new { email = model.Input.Email });
                    //}
                    //else
                    //{
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return LocalRedirect(returnUrl);
                    //}
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form

           model. GroupEditorViewModels = new List<SelectGroupEditorViewModel>();
           foreach (var group in _context.Groups)
            {
                model.GroupEditorViewModels.Add(new SelectGroupEditorViewModel(group));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<ActionResult> UserGroups(int id)
        {
            var user = await _context.Users.Include(x=>x.Groups).FirstAsync(u => u.Id == id);
            var model = new SelectUserGroupsViewModel(user,_context);
            return View(model);
        }
        [HttpPost]
        //[Authorize(Roles = "Admin, CanEditUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserGroups(SelectUserGroupsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user =await  _context.Users.FirstAsync(u => u.Id == model.Id);
                //-IdentityManager.ClearUserGroups(user.Id,model.);
                foreach (var group in model.Groups)
                {
                    await _identityManager.ClearUserGroups(user.Id, group.GroupId);
                    if (group.Selected)
                    {
                       await  _identityManager.AddUserToGroup(user.Id, group.GroupId);
                    }
                }
                return RedirectToAction("index");
            }

            return View();
        }

        //[Authorize(Roles = "Admin, CanEditRole, CanEditGroup, User")]
        public async Task<ActionResult> UserPermissions(int id)
        {
            var user = await _context.Users.Include(x=>x.Groups).FirstAsync(u => u.Id == id);
            IList<string> applicationRoles = await _userManager.GetRolesAsync(user);
            var model = new UserPermissionsViewModel(user,applicationRoles,_context);
            await model.SetRoleList();
            return View(model);
        }


        //[Authorize(Roles = "Admin, CanEditUser")]
        public async Task<ActionResult> Edit(int id)
        {
            var user = await _context.Users.FirstAsync(u => u.Id == id);
            var model = new EditUserViewModel(user);
            //ViewBag.MessageId = Message;
            return View(model);
        }


        [HttpPost]
        //[Authorize(Roles = "Admin, CanEditUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstAsync(u => u.UserName == model.UserName);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        //[Authorize(Roles = "Admin, CanEditUser")]
        public async  Task<ActionResult> Delete(int id )
        {
            var user =await  _context.Users.FirstAsync(u => u.Id == id);
            var model = new EditUserViewModel(user);
            if (user == null)
            {
                return NotFound();
            }
            return View(model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin, CanEditUser")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var user =await _context.Users.Include(x=>x.Groups).FirstAsync(u => u.Id == id);
           // _context.Users.Remove(user);
           //await  _context.SaveChangesAsync();
           IList<ApplicationUserGroup> _user2bremoved=new List<ApplicationUserGroup>();
           foreach (var item in user.Groups)
           {
               if (item.UserId == id)
               {
                   _user2bremoved.Add(item);
               }
           }

           foreach (var userGroups in _user2bremoved)
           {
               user.Groups.Remove(userGroups);
           }
           await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
        }

     

        protected override void Dispose(bool disposing)
        {
            if (disposing && _identityManager != null)
            {
                //_identityManager.Dispose();
                _identityManager = null;
            }
            base.Dispose(disposing);
        }

    }
}