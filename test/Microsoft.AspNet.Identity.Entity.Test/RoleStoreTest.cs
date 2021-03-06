// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Test;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.AspNet.Identity.Entity.Test
{
    public class RoleStoreTest
    {
        class ApplicationRoleManager : RoleManager<EntityRole>
        {
            public ApplicationRoleManager(IServiceProvider services, IRoleStore<EntityRole> store) : base(services, store) { }
        }

        [Fact]
        public async Task CanCreateUsingAddRoleManager()
        {
            var services = new ServiceCollection();
#if NET45
            //            services.AddEntityFramework().AddSqlServer();
            //#else
            services.AddEntityFramework().AddInMemoryStore();
#endif
            // TODO: this should construct a new instance of InMemoryStore
            var store = new EntityRoleStore<EntityRole>(new IdentityContext());
            services.AddIdentity<EntityUser, EntityRole>(s =>
            {
                s.AddRoleStore(() => store);
                s.AddRoleManager<ApplicationRoleManager>();
            });

            var provider = services.BuildServiceProvider();
            var manager = provider.GetService<ApplicationRoleManager>();
            Assert.NotNull(manager);
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(new EntityRole("arole")));
        }
        [Fact]
        public async Task CanCreateRoleWithSingletonManager()
        {
            var services = new ServiceCollection();
#if NET45
//            services.AddEntityFramework().AddSqlServer())
//#else
            services.AddEntityFramework().AddInMemoryStore();
#endif
            services.AddTransient<DbContext, IdentityContext>();
            services.AddTransient<IRoleStore<EntityRole>, EntityRoleStore<EntityRole>>();
            //todo: services.AddSingleton<RoleManager<EntityRole>, RoleManager<EntityRole>>();
            // TODO: How to configure SqlServer?
            services.AddSingleton<ApplicationRoleManager, ApplicationRoleManager>();
            var provider = services.BuildServiceProvider();
            var manager = provider.GetService<ApplicationRoleManager>();
            Assert.NotNull(manager);
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(new EntityRole("someRole")));
        }

        [Fact]
        public async Task RoleStoreMethodsThrowWhenDisposedTest()
        {
            var store = new EntityRoleStore<EntityRole, string>(new IdentityContext());
            store.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.FindByIdAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.FindByNameAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.GetRoleIdAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.GetRoleNameAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.SetRoleNameAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.CreateAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.UpdateAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.DeleteAsync(null));
        }

        [Fact]
        public async Task RoleStorePublicNullCheckTest()
        {
            Assert.Throws<ArgumentNullException>("context", () => new EntityRoleStore<EntityRole, string>(null));
            var store = new EntityRoleStore<EntityRole, string>(new IdentityContext());
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.GetRoleIdAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.GetRoleNameAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.SetRoleNameAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.CreateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await store.DeleteAsync(null));
        }

        [Fact]
        public async Task CanUpdateRoleName()
        {
            var manager = TestIdentityFactory.CreateRoleManager();
            var role = new EntityRole("UpdateRoleName");
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(role));
            Assert.Null(await manager.FindByNameAsync("New"));
            role.Name = "New";
            IdentityResultAssert.IsSuccess(await manager.UpdateAsync(role));
            Assert.NotNull(await manager.FindByNameAsync("New"));
            Assert.Null(await manager.FindByNameAsync("UpdateAsync"));
        }

        [Fact]
        public async Task CanSetUserName()
        {
            var manager = TestIdentityFactory.CreateRoleManager();
            var role = new EntityRole("UpdateRoleName");
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(role));
            Assert.Null(await manager.FindByNameAsync("New"));
            IdentityResultAssert.IsSuccess(await manager.SetRoleNameAsync(role, "New"));
            Assert.NotNull(await manager.FindByNameAsync("New"));
            Assert.Null(await manager.FindByNameAsync("UpdateAsync"));
        }

    }
}
