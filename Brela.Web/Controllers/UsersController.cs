using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brela.Web.Data;
using Brela.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sys.Web.Services;

namespace Sys.Web.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext _context;
        private IdentityManager _identityManager;

        public UsersController(ApplicationDbContext context,IdentityManager identityManager)
        {
            _context = context;
            _identityManager = identityManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        //[Authorize(Roles = "Admin, CanEditUser")]
        public ActionResult UserGroups(int id)
        {
            var user = _context.Users.Include(x=>x.Groups).FirstOrDefault(x=>x.Id == id);
            var model = new SelectUserGroupsViewModel(user,_context);
            return View(model);
        }
        [HttpPost]
        //[Authorize(Roles = "Admin, CanEditUser")]
        [ValidateAntiForgeryToken]
        public ActionResult UserGroups(SelectUserGroupsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.First(u => u.UserName == model.UserName);
                //_identityManager.ClearAllUserGroups(user.Id);
                foreach (var group in model.Groups)
                {
                    _identityManager.ClearUserGroups(user.Id,group.GroupId);
                    if (group.Selected)
                    {
                        _identityManager.AddUserToGroup(user.Id, group.GroupId);
                    }
                }
                return RedirectToAction("index");
            }
            return View();
        }

    }
}