﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotaDb
{
    public partial class _403 : System.Web.UI.Page
    {
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            Response.StatusCode = 403;
        }
    }
}