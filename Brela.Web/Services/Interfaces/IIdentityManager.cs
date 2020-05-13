using System.Threading.Tasks;
using Brela.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace Sys.Web.Services.Interfaces
{
    public interface IIdentityManager
    {
         Task CreateGroup(string groupName);
         Task  ClearGroupRoles(int groupId);
         Task RemoveFromRole(int userId, string roleName);
         Task  ClearUserRoles(int userId);
        Task DeleteRole(int roleId);
         Task AddUserToGroup(int userId, int groupId);
        Task AddRoleToGroup(int groupId, string roleName);
        Task RemoveRoleFromGroup(int groupId, string roleName);
        Task  ClearUserGroups(int userId, int groupId);
        Task ClearAllUserGroups(int userId);
        Task  DeleteGroup(int groupId);
        Task RemoveUserRole();
        Task<IdentityResult> CreateRole(string name, string description);
        Task<IdentityResult> CreateUser(ApplicationUser user, string password);



    }
}
