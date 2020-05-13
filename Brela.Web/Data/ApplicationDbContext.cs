using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brela.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Sys.Util.SysExtensions;
using Sys.Web.Models;

namespace Brela.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            //this.loggerFactory = loggerFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public virtual DbSet<Group> Groups { get; set; }
        public DbSet<Audit> Audits { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }

            builder.Entity<ApplicationUserGroup>().HasKey((ApplicationUserGroup r) =>
                new {UserId = r.UserId, GroupId = r.GroupId});

            builder.Entity<ApplicationUserGroup>().HasOne<ApplicationUser>(user => user.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(u => u.UserId);

            builder.Entity<ApplicationUserGroup>().HasOne<Group>(user => user.Group)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.GroupId);

            builder.Entity<ApplicationRoleGroup>().HasKey((ApplicationRoleGroup gr) =>
                new {RoleId = gr.RoleId, GroupId = gr.GroupId});

            builder.Entity<ApplicationRoleGroup>().HasOne<Group>(g => g.Group)
                .WithMany(c => c.Roles)
                .HasForeignKey(c => c.GroupId);

            builder.Entity<ApplicationRoleGroup>().HasOne<ApplicationRole>(g => g.Role)
                .WithMany(c => c.Groups)
                .HasForeignKey(c => c.RoleId);
            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var temoraryAuditEntities = await AuditNonTemporaryProperties();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await AuditTemporaryProperties(temoraryAuditEntities);
            return result;

            //var auditEntries = OnBeforeSaveChanges();
            //var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            //await OnAfterSaveChanges(auditEntries);
            //return result;
        }

        async Task<IEnumerable<Tuple<EntityEntry, Audit>>> AuditNonTemporaryProperties()
        {
            ChangeTracker.DetectChanges();
            var entitiesToTrack = ChangeTracker.Entries().Where(e =>
                !(e.Entity is Audit) && e.State != EntityState.Detached && e.State != EntityState.Unchanged);

            await Audits.AddRangeAsync(
                entitiesToTrack.Where(e => !e.Properties.Any(p => p.IsTemporary)).Select(e => new Audit()
                {
                    TableName = e.Metadata.GetTableName(),
                    Action = Enum.GetName(typeof(EntityState), e.State),
                    DateTime = DateTime.Now.ToUniversalTime(),
                    Username = this.httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                    KeyValues = JsonConvert.SerializeObject(e.Properties.Where(p => p.Metadata.IsPrimaryKey())
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()),
                    NewValues = JsonConvert.SerializeObject(e.Properties
                        .Where(p => e.State == EntityState.Added || e.State == EntityState.Modified)
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()),
                    OldValues = JsonConvert.SerializeObject(e.Properties
                        .Where(p => e.State == EntityState.Deleted || e.State == EntityState.Modified)
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue).NullIfEmpty())
                }).ToList()
            );

            //Return list of pairs of EntityEntry and ToolAudit  
            return entitiesToTrack.Where(e => e.Properties.Any(p => p.IsTemporary))
                .Select(e => new Tuple<EntityEntry, Audit>(
                    e,
                    new Audit()
                    {
                        TableName = e.Metadata.GetTableName(),
                        Action = Enum.GetName(typeof(EntityState), e.State),
                        DateTime = DateTime.Now.ToUniversalTime(),
                        Username = this.httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                        NewValues = JsonConvert.SerializeObject(e.Properties.Where(p => !p.Metadata.IsPrimaryKey())
                            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty())
                    }
                )).ToList();
        }

        async Task AuditTemporaryProperties(IEnumerable<Tuple<EntityEntry, Audit>> temporatyEntities)
        {
            if (temporatyEntities != null && temporatyEntities.Any())
            {
                await Audits.AddRangeAsync(
                    temporatyEntities.ForEach(t =>
                            t.Item2.KeyValues = JsonConvert.SerializeObject(t.Item1.Properties
                                .Where(p => p.Metadata.IsPrimaryKey())
                                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue).NullIfEmpty()))
                        .Select(t => t.Item2)
                );
                await SaveChangesAsync();
            }

            await Task.CompletedTask;
        }
        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Save the Audit entry
                Audits.Add(auditEntry.ToAudit());
            }

            return SaveChangesAsync();
        }
        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Metadata.GetTableName();
                auditEntry.UserName = this.httpContextAccessor?.HttpContext?.User?.Identity?.Name;

                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        // value will be generated by the database, get the value after saving
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            // Save audit entities that have all the modifications
            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                Audits.Add(auditEntry.ToAudit());
            }

            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }
        public class AuditEntry
        {
            public AuditEntry(EntityEntry entry)
            {
                Entry = entry;
            }

            public EntityEntry Entry { get; }
            public string TableName { get; set; }
            public  string UserName { get; set; }
            public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
            public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
            public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
            public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

            public bool HasTemporaryProperties => TemporaryProperties.Any();

            public Audit ToAudit()
            {
                var audit = new Audit();
                audit.TableName = TableName;
                audit.Username = UserName;
                audit.DateTime = DateTime.UtcNow;
                audit.KeyValues = JsonConvert.SerializeObject(KeyValues);
                audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
                audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
                return audit;
            }

         }
    }
}
