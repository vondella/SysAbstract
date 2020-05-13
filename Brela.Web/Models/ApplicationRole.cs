using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Brela.Web.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base()
        {
            this.Groups = new HashSet<ApplicationRoleGroup>();
        }


        public ApplicationRole(string name, string description) : base(name)
        {
            this.Description = description;
        }
        public virtual string Description { get; set; }
        public virtual ICollection<ApplicationRoleGroup> Groups { get; set; }

    }
}
