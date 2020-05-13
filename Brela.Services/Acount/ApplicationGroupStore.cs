using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brela.Database.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Brela.Services.Acount
{
    public class ApplicationGroupStore : IDisposable
    {
        private bool _disposed;
        private GroupStoreBase _groupStore;


        public ApplicationGroupStore(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.Context = context;
            this._groupStore = new GroupStoreBase(context);
            
        }


        public IQueryable<ApplicationGroups> Groups
        {
            get
            {
                return this._groupStore.EntitySet;
            }
        }

        public DbContext Context
        {
            get;
            private set;
        }


        public virtual void Create(ApplicationGroups group)
        {
            this.ThrowIfDisposed();
            if (group == null)
            {
                throw new ArgumentNullException("role");
            }
            this._groupStore.Create(group);
            this.Context.SaveChanges();
        }


        public virtual async Task CreateAsync(ApplicationGroups group)
        {
            this.ThrowIfDisposed();
            if (group == null)
            {
                throw new ArgumentNullException("role");
            }
            this._groupStore.Create(group);
            await this.Context.SaveChangesAsync();
        }


        public virtual async Task DeleteAsync(ApplicationGroups group)
        {
            this.ThrowIfDisposed();
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this._groupStore.Delete(group);
            await this.Context.SaveChangesAsync();
        }


        public virtual void Delete(ApplicationGroups group)
        {
            this.ThrowIfDisposed();
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this._groupStore.Delete(group);
            this.Context.SaveChanges();
        }


        public Task<ApplicationGroups> FindByIdAsync(string roleId)
        {
            this.ThrowIfDisposed();
            return this._groupStore.GetByIdAsync(roleId);
        }


        public ApplicationGroups FindById(string roleId)
        {
            this.ThrowIfDisposed();
            return this._groupStore.GetById(roleId);
        }

        public Task<ApplicationGroups> FindByNameAsync(string groupName)
        {
            this.ThrowIfDisposed();
            return QueryableExtensions
                .FirstOrDefaultAsync<ApplicationGroups>(this._groupStore.EntitySet,
                    (ApplicationGroups u) => u.Name.ToUpper() == groupName.ToUpper());
        }


        public virtual async Task UpdateAsync(ApplicationGroups group)
        {
            this.ThrowIfDisposed();
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this._groupStore.Update(group);
            await this.Context.SaveChangesAsync();
        }


        public virtual void Update(ApplicationGroups group)
        {
            this.ThrowIfDisposed();
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this._groupStore.Update(group);
            this.Context.SaveChanges();
        }


        // DISPOSE STUFF: ===============================================

        public bool DisposeContext
        {
            get;
            set;
        }


        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }


        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (this.DisposeContext && disposing && this.Context != null)
            {
                this.Context.Dispose();
            }
            this._disposed = true;
            this.Context = null;
            this._groupStore = null;
        }
    }
}
