<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="UnifiedCheckout.Index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <div id="buttonPaymentListContainer">
       <button type="button" id="checkoutEmbedded" class="btn btn-lg btn-block btn-primary" disabled="disabled">
        Loading...
       </button>
       <button type="button" id="checkoutSidebar" class="btn btn-lg btn-block btn-primary" disabled="disabled">
        Loading...
       </button>
   </div>
    <input type="hidden" id="captureContext" value= "" />

    <form id="authForm" action="/token" method="post">
       <input type="hidden" id="transientToken" name="transientToken" />
   </form>
</body>
    <script src="https://apitest.cybersource.com/up/v1/assets/0.8.0/SecureAcceptance.js"></script>
    <script src="Scripts/jquery-3.4.1.min.js"></script>
</html>
