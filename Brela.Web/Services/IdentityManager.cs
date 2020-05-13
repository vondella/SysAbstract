using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Brela.Web.Data;
using Brela.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sys.Web.Services.Interfaces;

namespace Sys.Web.Services
{
    public class IdentityManager: IIdentityManager
    {
        private  ApplicationDbContext _context;
        private  RoleManager<ApplicationRole> _roleManager  ;
        private UserManager<ApplicationUser> _userManager;
        IList<string> RoleNames;

        public IdentityManager(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public  async Task CreateGroup(string groupName)
        {
            if (await this.GroupNameExists(groupName))
            {
                throw new System.Exception("A group by that name already exists in the database. Please choose another name.");
            }

            var newGroup = new Group(groupName);
             await _context.Groups.AddAsync(newGroup);
            await _context.SaveChangesAsync();
        }
        public async Task ClearGroupRoles(int groupId)
        {
            var group = await _context.Groups.Include(x=>x.Roles).Where(x=>x.Id == groupId).FirstOrDefaultAsync();
            var groupUsers =await _context.Users.Include(x=>x.Groups).Where(u => u.Groups.Any(g => g.GroupId == group.Id)).ToListAsync();

            List<ApplicationRoleGroup> applicationRoleGroups=new List<ApplicationRoleGroup>();
            foreach (var role in group.Roles)
            {
                var currentRoleId = role.RoleId;
                applicationRoleGroups.Add(role);
                foreach (var user in groupUsers)
                {
                    // Is the user a member of any other groups with this role?
                    var groupsWithRole = user.Groups
                        .Where(g => g.Group.Roles
                            .Any(r => r.RoleId == currentRoleId)).Count();
                    // This will be 1 if the current group is the only one:
                    if (groupsWithRole == 1)
                    {
                        var roleName = await _context.Roles.Where(x => x.Id == role.RoleId).FirstAsync();
                         await RemoveFromRole(user.Id, roleName.Name);
                    }
                }

                //group.Roles.Remove(role);
            }
            // clear group roles
            foreach (var item in applicationRoleGroups)
            {
                group.Roles.Remove(item);
            }
            await _context.SaveChangesAsync();
        }
        public async Task RemoveFromRole(int userId, string roleName)
        {
            //_userManager.RemoveFromRole(userId, roleName);
            //_userManager.RemoveFromRoleAsync()
            ApplicationUser applicationUser = _context.Users.Find(userId);
           await  _userManager.RemoveFromRoleAsync(applicationUser, roleName);
        }
        public async Task ClearUserRoles(int userId)
        {
            ApplicationUser user =  await _context.Users.FindAsync(userId);
            var currentRoles = new List<ApplicationRole>();

            //currentRoles.AddRange(user.Groups.r);
            //foreach (IdentityUserRole role in currentRoles)
            //{
            //    _userManager.RemoveFromRole(userId, role.Role.Name);
            //}
        }
        public async Task DeleteRole(int roleId)
        {
            //IQueryable<ApplicationUser> roleUsers = _context.Users.Where(u => u.rAny(r => r.RoleId == roleId));
            //ApplicationRole role = _db.Roles.Find(roleId);

            //foreach (ApplicationUser user in roleUsers)
            //{
            //    RemoveFromRole(user.Id, role.Name);
            //}
            //_db.Roles.Remove(role);
            //_db.SaveChanges();
        }
        public  async Task AddUserToGroup(int userId, int groupId)
        {
            Group group = await _context.Groups.Include(x=>x.Roles).Where(x=>x.Id == groupId).FirstOrDefaultAsync();
            List<ApplicationRole> roles = await  _context.Roles.Include(x => x.Groups).ToListAsync();
            ApplicationUser user =await  _context.Users.FindAsync(userId);

            var userGroup = new ApplicationUserGroup
            {
                Group = group,
                GroupId = groupId,
                User = user,
                UserId = user.Id
            };

            foreach (ApplicationRole role in  roles)
            {
                //var rslts = _userManager.AddToRoleAsync(user, role.Name).Result;

                foreach (var item in role.Groups)
                {
                    if (item.GroupId == groupId)
                    {
                        var rslts =await  _userManager.AddToRoleAsync(user, role.Name);
                        //var v = rslts;
                    }
                }
            }
            user.Groups.Add(userGroup);
            await _context.SaveChangesAsync();
        }
        public async Task AddRoleToGroup(int groupId, string roleName)
        {

            var  group = await _context.Groups.Include(x=>x.Roles).Where(x=>x.Id ==groupId).FirstOrDefaultAsync();

            var newgroupRole = new ApplicationRoleGroup();
            ApplicationRole role=new ApplicationRole();
            if (group != null)
            {
                role = await _context.Roles.FirstAsync(r => r.Name == roleName);
                newgroupRole.GroupId = group.Id;
                newgroupRole.Group = group;
                newgroupRole.RoleId = role.Id;
                newgroupRole.Role = role;
            }
            
            if (group.Roles.Where(x=>x.RoleId==role.Id && x.GroupId ==groupId).FirstOrDefault() == null)
            {
                group.Roles.Add(newgroupRole);
                _context.SaveChanges();
            }
            
            // Add all of the users in this group to the new role:
            IList<ApplicationUser> groupUsers = await _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id)).ToListAsync();

            if (groupUsers != null)
            {
                foreach (ApplicationUser user in groupUsers)
                {
                    if (!( await _userManager.IsInRoleAsync(user, roleName)))
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                    }
                }
            }
          
        }
        public async Task RemoveRoleFromGroup(int groupId, string roleName)
        {
            var group = await _context.Groups.Include(x => x.Roles).Where(x => x.Id == groupId).FirstOrDefaultAsync();

            var newgroupRole = new ApplicationRoleGroup();
            ApplicationRole role = new ApplicationRole();
            if (group != null)
            {
                role =await  _context.Roles.FirstAsync(r => r.Name == roleName);
                newgroupRole.GroupId = group.Id;
                newgroupRole.Group = group;
                newgroupRole.RoleId = role.Id;
                newgroupRole.Role = role;
            }

            if (group.Roles.Where(x => x.RoleId == role.Id && x.GroupId == groupId).FirstOrDefault() == null)
            {
                group.Roles.Remove(newgroupRole);
                _context.SaveChanges();
            }

            // Add all of the users in this group to the new role:
            IList<ApplicationUser> groupUsers =await  _context.Users.Where(u => u.Groups.Any(g => g.GroupId == group.Id)).ToListAsync();

            if (groupUsers != null)
            {
                foreach (ApplicationUser user in groupUsers)
                {
                    if (await _userManager.IsInRoleAsync(user, roleName))
                    {
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                    }

                }
            }
        }
        public async Task ClearUserGroups(int userId, int groupId)
        {
            //ClearUserRoles(userId);
            Group group = await _context.Groups.Include(x=>x.Roles).FirstAsync(x=>x.Id == groupId);
            ApplicationUser user = await _context.Users.Include(x=>x.Groups).FirstOrDefaultAsync(x=>x.Id==userId);
            RoleNames = new List<string>();
            foreach (ApplicationRoleGroup role in group.Roles.ToList())
            {
                var roleName = await _context.Roles.FirstAsync(x => x.Id == role.RoleId);
                RoleNames.Add(roleName.Name);
            }

           

            var userGroup = user.Groups.Where(x => x.GroupId == groupId && x.UserId == userId).FirstOrDefault();
            if (userGroup != null)
            {
                user.Groups.Remove(userGroup);
                await _context.SaveChangesAsync();
            }

            //user.Groups.Clear();
            //await _context.SaveChangesAsync();
        }
        public async Task ClearAllUserGroups(int userId)
        {
            await ClearUserRoles(userId);
            ApplicationUser user =await  _context.Users.Include(x => x.Groups).FirstOrDefaultAsync(x => x.Id == userId);
           var list = user.Groups.Where(x=>x.UserId == userId).ToList();
           foreach (var item in list)
           {
               user.Groups.Remove(item);
           }
            await _context.SaveChangesAsync();
        }
        public async Task DeleteGroup(int groupId)
        {
            Group group = await _context.Groups.FindAsync(groupId);
            foreach (var user  in _context.Users)
            {
                await ClearUserGroups(user.Id,groupId);
            }

            // Clear the roles from the group:
           await  ClearGroupRoles(groupId);
             _context.Groups.Remove(group);
           await _context.SaveChangesAsync();
        }
        public async Task RemoveUserRole()
        {
           IList<ApplicationUser> users = await _context.Users.ToListAsync();
           foreach (var user in users)
           {
               foreach (var name in RoleNames)
               {
                   await _userManager.RemoveFromRoleAsync(user, name);
               }
            }
           
        }
        public async  Task<IdentityResult> CreateRole(string name, string description)
        {
            return  await _roleManager.CreateAsync(new ApplicationRole(name, description));
        }
        public async Task<IdentityResult> CreateUser(ApplicationUser user, string password)
        {
            return  await _userManager.CreateAsync(user, password);
        }
        public async Task<bool> GroupNameExists(string groupName)
        {
            return  await _context.Groups.AnyAsync(gr => gr.Name == groupName);
        }
    }
}
