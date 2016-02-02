<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="500.aspx.cs" Inherits="DotaDb._500" %>

<% Response.StatusCode = 500; %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>500 Internal Server Error</title>
    <link rel="stylesheet" type="text/css" href="/Content/bootstrap-lumen.min.css" />
</head>
<body style="margin: 0; background-color: #fcfcfc;">
    <div style="width: 800px; margin: 0 auto; text-align: center;">
        <br />
        <br />
        <img src="/Content/dotadb-200x200.png" />
        <br />
        <h3><strong>500 Internal Server Error</strong></h3>
        <h3>Something really bad happened. It probably isn't your fault. Maybe.</h3>
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
