using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Brela.Web.Models
{
    public class Group
    {
        public Group()
        {
            //this.Roles = new List<ApplicationRoleGroup>();
            this.Roles = new List<ApplicationRoleGroup>();
            this.Users = new List<ApplicationUserGroup>();
        }


        public Group(string name) : this()
        {
            this.Name = name;
        }


        [Key]
        [Required]
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
        //public  virtual  int SysType { get; set; }
        public virtual ICollection<ApplicationUserGroup> Users { get; set; }

        public virtual ICollection<ApplicationRoleGroup> Roles { get; set; }
    }
}
