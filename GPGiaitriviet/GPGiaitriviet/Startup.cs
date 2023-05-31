using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GPGiaitriviet.Startup))]
namespace GPGiaitriviet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
