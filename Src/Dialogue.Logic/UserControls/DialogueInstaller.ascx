<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DialogueInstaller.ascx.cs" Inherits="Dialogue.Logic.UserControls.DialogueInstaller" %>

<asp:Panel ID="pnlMainText" runat="server" Visible="True">
<h2>Almost Done...</h2>
<p>Dialogue needs to setup a few more things to complete, please click the button below to complete the installation</p>
<p>On completion, the results of the installer will appear below.</p>
<p>
    <asp:Button ID="btnInstall" runat="server" Text="Click Here To Complete Installation" OnClick="CompleteInstallation" style="font-size: large; padding: 12px 25px;" Width="400px" />
</p>    
</asp:Panel>

<asp:Panel ID="InstallerSuccessfull" runat="server" Visible="False">
    
    <h1>Successfully Installed</h1>
    <p>Dialogue sucessfully installed, you can now use the forum.</p>

</asp:Panel>
<asp:Panel ID="InstallerResultPanel" runat="server" Visible="False">
    
    <h4>Installer Results</h4>
    
    <asp:Literal ID="litResults" runat="server" />

</asp:Panel>
