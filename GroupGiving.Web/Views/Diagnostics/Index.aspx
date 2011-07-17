<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Diagnostics</title>
    <script src="/Scripts/jquery-1.5.1.min.js" type="text/javascript"></script>
    <script src="/scripts/diagnostics.js" type="text/javascript"></script>
</head>
<body>
    <h2>Email templates</h2>
    <p><input type="text" name="toAddress" id="toAddress" size="100" value="andrew.myhre@gmail.com" /></p>
    <ul>
        <li>
            <a href="<%=Url.Action("SendTest", "Diagnostics") %>" class="emailTest" templateName="EventActivated">send Event Activated test</a>
        </li>
        <li>
            <a href="<%=Url.Action("SendTest", "Diagnostics") %>" class="emailTest" templateName="AccountCreated">send Account Created test</a>
        </li>
        <li>
            <a href="<%=Url.Action("SendTest", "Diagnostics") %>" class="emailTest" templateName="GetYourAccountStarted">send Get Your Account Started test</a>
        </li>
        <li>
            <a href="<%=Url.Action("SendTest", "Diagnostics") %>" class="emailTest" templateName="ResetYourPassword">send Reset Password test</a>
        </li>
    </ul>


</body>
</html>
