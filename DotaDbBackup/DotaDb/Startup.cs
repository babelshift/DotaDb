using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DotaDb.Startup))]
namespace DotaDb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}