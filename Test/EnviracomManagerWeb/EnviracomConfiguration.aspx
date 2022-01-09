<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EnviracomConfiguration.aspx.cs" Inherits="EnviracomManagerWeb.EnviracomConfiguration" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<% = m_objHomeSeer.GetPageHeader("Enviracom Manager Configuration", "", "", false, false, false, false, false)%>		
<body>
    <form id="form1" runat="server">
    <asp:TextBox ID="NunberOfZones" runat="server"></asp:TextBox>
    <asp:GridView ID="m_gvThermostatGridView" runat="server" BackColor="White" 
        BorderColor="#999999" BorderStyle="None" BorderWidth="1px" CellPadding="3" 
        GridLines="Vertical">
        <AlternatingRowStyle BackColor="Gainsboro" />
        <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
        <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
        <RowStyle BackColor="#EEEEEE" ForeColor="Black" />
        <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
        <SortedAscendingCellStyle BackColor="#F1F1F1" />
        <SortedAscendingHeaderStyle BackColor="#0000A9" />
        <SortedDescendingCellStyle BackColor="#CAC9C9" />
        <SortedDescendingHeaderStyle BackColor="#000065" />
    </asp:GridView>
    <asp:GridView ID="m_gvUnitGridView" runat="server">
    </asp:GridView>
    </form>
    </body>
    <% = m_objHomeSeer.GetPageFooter(false)%>
</html>
