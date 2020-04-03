using System.Web.UI;

namespace DotaDb
{
    public partial class _500 : System.Web.UI.Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            Response.StatusCode = 500;
        }
    }
}