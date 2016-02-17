<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="500.aspx.cs" Inherits="DotaDb._500" %>

<% Response.StatusCode = 500; %>

<!DOCTYPE html>
<html>
<head>
    <title>500 Internal Server Error</title>
    <link rel="stylesheet" type="text/css" href="/Content/bootstrap-lumen.min.css" />
    <link rel="stylesheet" type="text/css" href="/Content/font-awesome.min.css" />
    <link rel="stylesheet" type="text/css" href="/Content/Site.css" />
</head>
<body style="margin: 0; background-color: #fcfcfc;">
    <div class="row" style="padding: 15px;">
        <div class="col-md-8 col-md-offset-2 col-lg-6 col-md-offset-3 col-lg-offset-3">
            <div class="panel panel-default" style="margin: 0">
                <div class="panel-body text-center">
                    <img src="../../Content/dotadb-200x200.png" class="img-responsive center-block" />
                    <h2><strong>Error 500</strong></h2>
                    <h3>Pudge seems to have eaten your request. Don't worry, we'll find him some other food.</h3>
                    <br />
                    <div class="row">
                        <div class="col-sm-2 col-sm-offset-2">
                            <a href="http://www.dotadatabase.net" class="btn btn-block btn-default" title="Home">
                                <i class="fa fa-lg fa-home"></i>
                                <span class="hidden-sm hidden-md hidden-lg">Home</span>
                            </a>
                        </div>
                        <div class="col-sm-2">
                            <a href="mailto:info@dotadatabase.net" class="btn btn-block btn-primary" title="Email Us">
                                <i class="fa fa-lg fa-send"></i>
                                <span class="hidden-sm hidden-md hidden-lg">Email</span>
                            </a>
                        </div>
                        <div class="col-sm-2">
                            <a href="http://www.twitter.com/dotadatabase" class="btn btn-block btn-twitter">
                                <i class="fa fa-lg fa-twitter"></i>
                                <span class="hidden-sm hidden-md hidden-lg">Twitter</span>
                            </a>
                        </div>
                        <div class="col-sm-2">
                            <a href="https://www.facebook.com/Dota-Database-1571638349826115/" class="btn btn-block btn-facebook">
                                <i class="fa fa-lg fa-facebook"></i>
                                <span class="hidden-sm hidden-md hidden-lg">Facebook</span>
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>