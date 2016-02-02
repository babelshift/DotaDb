<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="DotaDb._404" %>

<% Response.StatusCode = 404; %>

<!DOCTYPE html>
<html>
<head>
    <title>404 Not Found</title>
    <link rel="stylesheet" type="text/css" href="/Content/bootstrap-lumen.min.css" />
</head>
<body style="margin: 0; background-color: #fcfcfc;">
    <div style="width: 800px; margin: 0 auto; text-align: center;">
        <br />
        <br />
        <img src="/Content/dotadb-200x200.png" />
        <br />
        <h3><strong>404 Not Found</strong></h3>
        <h3>We couldn't find what you were looking for. It may or may not exist.</h3>
        <br />
        <br />
        <ul class="list-unstyled list-inline">
            <li><a href="mailto:info@dotadatabase.net">Email Us</a></li>
            <li>&bull;</li>
            <li><a href="http://www.twitter.com/dotadatabase">Tweet to Us</a></li>
        </ul>
    </div>
</body>
</html>
