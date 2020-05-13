using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brela.Web.Data;
using Brela.Web.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Sys.Web.Services;

namespace Sys.Web.Data
{
    public class Seed
    {
      
        public  static string[] _initialGroupNames =
            new string[] { "SuperAdmins", "GroupAdmins", "UserAdmins", "Users" };
        public  static  string[] _superAdminRoleNames =
            new string[] { "Admin", "CanEditUser", "CanEditGroup", "CanEditRole", "User" };

        static string _initialUserName = "root";
        static string _InitialUserFirstName = "root";
        static string _initialUserLastName = "root";
        static string _initialUserEmail = "root@root.com";

        public static async Task SeedData(ApplicationDbContext context,IdentityManager identityManager)
        {
            if (! await context.Groups.AnyAsync())
            {
                await AddGroups(identityManager);
            }
            if (! await context.Roles.AnyAsync())
            {
                await AddRoles(identityManager);
            }
            if (!await context.Users.AnyAsync())
            {
                await AddUsers(identityManager);
            }
        }
        public  static async  Task AddUsers(IdentityManager identityManager)
        {
            var newUser = new ApplicationUser()
            {
                UserName = _initialUserEmail,
                FirstName = _InitialUserFirstName,
                LastName = _initialUserLastName,
                Email = _initialUserEmail
            };
            await  identityManager.CreateUser(newUser, "Passw0rd123!");
        }
        public static async Task AddRoles(IdentityManager _identityManager)
        {
           await  _identityManager.CreateRole("Admin", "Global Access");
           await _identityManager.CreateRole("CanEditUser", "Add, modify, and delete Users");
           await _identityManager.CreateRole("CanEditGroup", "Add, modify, and delete Groups");
           await _identityManager.CreateRole("CanEditRole", "Add, modify, and delete roles");
           await _identityManager.CreateRole("User", "Restricted to business domain activity");
        }
        public static async Task AddGroups(IdentityManager _identityManager)
        {
            foreach (var groupName in _initialGroupNames)
            {
                await _identityManager.CreateGroup(groupName);
            }
        }
        public static  async Task AddUsersToGroups(ApplicationDbContext context, IdentityManager identityManager)
        {
            var user = await context.Users.FirstAsync(u => u.UserName == _initialUserName);
            var group = await context.Groups.FirstAsync(x=>x.Name == "SuperAdmins");
            await identityManager.AddUserToGroup(user.Id, group.Id);
        }
        public static async Task AddRolesToGroup(ApplicationDbContext context, IdentityManager identityManager)
        {
            var group = await context.Groups.FirstAsync(x => x.Name == "SuperAdmins");

            foreach (var name in _superAdminRoleNames.ToList())
            {
                await identityManager.AddRoleToGroup(group.Id, name);
            }
        }
    }
}
