<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DialogueImporter.ascx.cs" Inherits="Dialogue.Logic.UserControls.DialogueImporter" %>
<div class="propertypane">

    <div class="propertyItem">
        <div class="memberimport">
            <h3>Import Members</h3>
            <p>You can import members from your previous forum into Dialogue, just create an XML file using the same structure as show in the example and upload it using the form below</p>
            <p><asp:FileUpload ID="fuXml" runat="server" /></p>
            <p><asp:Button ID="btnUpload" runat="server" Text="Import Members" OnClick="UploadMembers" /></p>
            <asp:Panel ID="pnlResults" runat="server" Visible="False">
                
            </asp:Panel>
            <h4>Xml Example Structure</h4>
            <pre>
&lt;members&gt;
    &lt;member&gt;
        &lt;username&gt;Username&lt;/username&gt;
        &lt;email&gt;email@email.com&lt;/email&gt;
        &lt;website&gt;&lt;/website&gt;
        &lt;twitter&gt;&lt;/twitter&gt;
        &lt;points&gt;1200&lt;/points&gt;
        &lt;isadmin&gt;true&lt;/isadmin&gt;
    &lt;/member&gt;
    &lt;member&gt;
        &lt;username&gt;Another&lt;/username&gt;
        &lt;email&gt;another@email.com&lt;/email&gt;
        &lt;website&gt;&lt;/website&gt;
        &lt;twitter&gt;&lt;/twitter&gt;
        &lt;points&gt;0&lt;/points&gt;
        &lt;isadmin&gt;false&lt;/isadmin&gt;
    &lt;/member&gt;
&lt;/members&gt;
           </pre>

        </div>
    </div>

</div>


