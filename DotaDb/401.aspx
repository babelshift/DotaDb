<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="401.aspx.cs" Inherits="TwolipsDating._401" %>

<% Response.StatusCode = 401; %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>401 Unauthorized</title>
    <link rel="stylesheet" type="text/css" href="/Content/bootstrap-yeti.css" />
</head>
<body style="margin: 0; background-color: #fcfcfc;">
    <div style="width: 800px; margin: 0 auto; text-align: center;">
        <br />
        <br />
        <h2><img src="/Content/twolipsicon.png" width="64" height="64" /> twolips dating</h2>
        <br />
        <h3><strong>401 Unauthorized</strong></h3>
        <h3>You're trying to access something without permission. Fortunately, we can <a href="#">help you</a>.</h3>
        <br />
        <br />
        <ul class="list-unstyled list-inline">
            <li><a href="#">Help and FAQ</a></li>
            <li>&bull;</li>
            <li><a href="mailto:info@twolipsdating.com">Email Us</a></li>
            <li>&bull;</li>
            <li><a href="http://www.twitter.com/twolipsdating">Tweet Us</a></li>
        </ul>
    </div>
</body>
</html>
