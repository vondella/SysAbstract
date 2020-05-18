using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Brela.Web.Data;
using Brela.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sys.Web.Services;
using Sys.Web.Services.Interfaces;

namespace Brela.Web.Controllers
{
    public class GroupsController : Controller
    {
        private ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityManager _identityManager;
        private readonly IMapper _mapper;
        private Serilog.ILogger _logger;
            //,IdentityManager identityManager
        public GroupsController(ApplicationDbContext context, IdentityManager identityManager,HttpContextAccessor httpContextAccessor,Serilog.ILogger logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _identityManager = identityManager;
            _mapper = mapper;
        }

        //[Authorize(Roles = "Admin, CanEditGroup, CanEditUser")]
        public ActionResult Index()
        {
            
            return View(_context.Groups.ToList());
        }

        //[Authorize(Roles = "Admin, CanEditGroup, CanEditUser")]
        public ActionResult Details(int? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //Group group = db.Groups.Find(id);
            //if (group == null)
            //{
            //    return HttpNotFound();
            //}
            //return View(group);
            var  group=_context.Groups.Include(x=>x.Roles).Where(x=>x.Id == id).FirstOrDefault();

            return Content(group.Roles.ToList()[0].RoleId.ToString());
        }


        //[Authorize(Roles = "Admin, CanEditGroup")]
        public ActionResult Create()
        {
            return View();
        }


        //[Authorize(Roles = "Admin, CanEditGroup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Group group)
        {
            if (ModelState.IsValid)
            {
                _context.Groups.Add(group);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            //var userDTO = _mapper.Map<UserDTO>(user);
            _logger.ForContext("User", _httpContextAccessor.HttpContext.User.Identity.Name).Information("Data Added Successfully");
            return View(group);
        }

        //[Authorize(Roles = "Admin, CanEditGroup")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Group group = _context.Groups.Find(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }


        //[Authorize(Roles = "Admin, CanEditGroup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<ActionResult> Edit(Group group)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(group).State = EntityState.Modified;
               await  _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(group);
        }


        //[Authorize(Roles = "Admin, CanEditGroup")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Group group =  await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }


        //[Authorize(Roles = "Admin, CanEditGroup")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await  _identityManager.DeleteGroup(id);
           await _identityManager.RemoveUserRole();
            return RedirectToAction("Index");
        }


        //[Authorize(Roles = "Admin, CanEditGroup")]
        public async Task<ActionResult> GroupRoles(int id)
        {
            var group = await _context.Groups.Include(x=>x.Roles).Where(x=>x.Id == id).FirstOrDefaultAsync();
            var model = new SelectGroupRolesViewModel(group, _context);
            return View(model);
            //return Content(group.Name);
        }


        [HttpPost]
        //[Authorize(Roles = "Admin, CanEditGroup")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GroupRoles(SelectGroupRolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                //var idManager = new IdentityManager();
                //var Db = new ApplicationDbContext();
                await _identityManager.ClearGroupRoles(model.GroupId);
                var group =await _context.Groups.FindAsync(model.GroupId);
                //_identityManager.ClearGroupRoles(model.GroupId);
                // Add each selected role to this group:
                foreach (var role in model.Roles)
                {
                    if (role.Selected)
                    {
                        await _identityManager.AddRoleToGroup(group.Id, role.RoleName);
                    }
                    //else
                    //{
                    //    _identityManager.RemoveRoleFromGroup(group.Id, role.RoleName);
                    //}
                }
                return RedirectToAction("index");
            }
            return View();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}