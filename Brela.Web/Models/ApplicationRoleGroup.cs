using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brela.Web.Models
{
    public class ApplicationRoleGroup
    {
        public virtual int RoleId { get; set; }
        public virtual int GroupId { get; set; }

        public virtual ApplicationRole Role { get; set; }
        public virtual Group Group { get; set; }
    }
}
