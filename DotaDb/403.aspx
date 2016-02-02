﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="403.aspx.cs" Inherits="DotaDb._403" %>

<% Response.StatusCode = 403; %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>403 Forbidden</title>
    <link rel="stylesheet" type="text/css" href="/Content/bootstrap-lumen-min.css" />
</head>
<body style="margin: 0; background-color: #fcfcfc;">
    <div style="width: 800px; margin: 0 auto; text-align: center;">
        <br />
        <br />
        <img src="/Content/dotadb-200x200.png" />
        <br />
        <h3><strong>403 Forbidden</strong></h3>
        <h3>That's off limits. Not even we can help you with that one.</h3>
        <br />
        <br />
        <ul class="list-unstyled list-inline">
            <li>&bull;</li>
            <li><a href="mailto:info@dotadatabase.net">Email Us</a></li>
            <li>&bull;</li>
            <li><a href="http://www.twitter.com/dotadatabase">Tweet to Us</a></li>
        </ul>
    </div>
</body>
</html>
