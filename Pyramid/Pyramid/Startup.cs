using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using Pyramid.Models;

[assembly: OwinStartupAttribute(typeof(Pyramid.Startup))]
namespace Pyramid
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //Ensure that the roles are created
            CreateRoles();
        }

        /// <summary>
        /// This method will ensure that the application roles exist
        /// </summary>
        private void CreateRoles()
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                //Get the role and user managers
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<PyramidUser>(new UserStore<PyramidUser>(context));

                //Create roles if they do not exist
                //Admin role
                if (!roleManager.RoleExists("Admin"))
                {
                    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                    role.Name = "Admin";
                    roleManager.Create(role);
                }

                //User role   
                if (!roleManager.RoleExists("User"))
                {
                    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                    role.Name = "User";
                    roleManager.Create(role);

                }

                //Guest role  
                if (!roleManager.RoleExists("Guest"))
                {
                    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                    role.Name = "Guest";
                    roleManager.Create(role);
                }
            }
        }
    }
}
