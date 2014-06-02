<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Control Language="C#" AutoEventWireup="true" 
    Inherits="FlyingHippo.AutoComplete.Fields.AutoCompleteFieldEditor, $SharePoint.Project.AssemblyFullName$" 
    CompilationMode="Always" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %>


        <wssuc:InputFormControl runat="server" LabelText="List to search against" LabelAssociatedControlId="ddlSearchList">
            <Template_Control>
                <table>
                    <tr>
                        <td><asp:DropDownList ID="ddlSearchList" runat="server" AutoPostBack="true" /></td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>

        <wssuc:InputFormControl runat="server" LabelText="Column to display data" LabelAssociatedControlId="ddlDisplayColumn">
            <Template_Control>
                <table>
                    <tr>
                        <td><asp:DropDownList ID="ddlDisplayColumn" runat="server" AutoPostBack="false" /></td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>

        <wssuc:InputFormControl runat="server" LabelText="Columns to perform search against" LabelAssociatedControlId="lbSearchColumns">
            <Template_Control>
                <table>
                    <tr>
                        <td><asp:ListBox SelectionMode="Multiple" ID="lbSearchColumns" runat="server" /></td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>

        <wssuc:InputFormControl runat="server" LabelText="" LabelAssociatedControlId="chkAllowMultipleDLF">
            <Template_Control>
                <table>
                    <tr>
                        <td>
                            <asp:CheckBox ID="chkAllowMultipleDLF" runat="server" EnableViewState="true" />
                            <asp:Label ID="Label2" AssociatedControlID="chkAllowMultipleDLF" Text="Allow Multiple Values" runat="server" />
                        </td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>