using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.UI;
using System.Xml;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;

namespace Dialogue.Logic.UserControls
{
    public partial class DialogueImporter : UserControl
    {
        protected void UploadMembers(object sender, EventArgs e)
        {
            
            if (fuXml.HasFile)
            {
                // Upload file to App_Data temp folder
                var xmlFilepath = Server.MapPath("~/App_Data/TEMP/memberimport.xml");
                fuXml.SaveAs(xmlFilepath);

                // Once saved load xml
                var xml = new XmlDocument();
                xml.Load(xmlFilepath);

                // Now loop through the members
                var allMembers = xml.SelectNodes("//member");

                // Do we have any members
                if (allMembers != null && allMembers.Count > 0)
                {
                    // Store all mapped members
                    var members = new List<MemberImport>();

                    // Yes, we have members - So map them
                    foreach (XmlNode mem in allMembers)
                    {
                        var import = MapImport(mem);
                        if (import != null)
                        {
                            members.Add(import);   
                        }
                    }

                    //TODO - Refactor - Must be a more performant way of mass creating members

                    var memService = AppHelpers.UmbServices().MemberService;
                    var memHelper = AppHelpers.UmbMemberHelper();

                    var memberGroupService = AppHelpers.UmbServices().MemberGroupService;
                    var startingGroup = memberGroupService.GetByName(AppConstants.MemberGroupDefault);
                    var adminGroup = memberGroupService.GetByName(AppConstants.AdminRoleName);

                    var unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
                    var pointService = ServiceFactory.MemberPointsService;
                    using (var unitOfWork = unitOfWorkManager.NewUnitOfWork())
                    {
                        try
                        {
                            foreach (var newmem in members)
                            {
                                //var m = memService.CreateMemberWithIdentity("username", "email", "name", AppConstants.MemberTypeAlias);

                                var userToSave = memHelper.CreateRegistrationModel(AppConstants.MemberTypeAlias);
                                userToSave.Username = ServiceFactory.BannedWordService.SanitiseBannedWords(newmem.Username);
                                userToSave.Name = userToSave.Username;
                                userToSave.UsernameIsEmail = false;
                                userToSave.Email = newmem.Email;
                                userToSave.Password = Membership.GeneratePassword(10, 1);

                                MembershipCreateStatus createStatus;
                                memHelper.RegisterMember(userToSave, out createStatus, false);

                                if (createStatus == MembershipCreateStatus.Success)
                                {
                                    // Get the umbraco member
                                    var umbracoMember = memService.GetByUsername(userToSave.Username);

                                    // Set the role/group they should be in
                                    if (newmem.IsAdmin)
                                    {
                                        // Add to admin role
                                        memService.AssignRole(umbracoMember.Id, adminGroup.Name);

                                        // Add can edit member property
                                        umbracoMember.Properties[AppConstants.PropMemberCanEditOtherUsers].Value = newmem.IsAdmin;
                                    }
                                    else
                                    {
                                        // Standard role
                                        memService.AssignRole(umbracoMember.Id, startingGroup.Name);
                                    }

                                    // Add points
                                    if (newmem.Points > 0)
                                    {
                                        // Create the DB points
                                        var points = new MemberPoints
                                        {
                                            DateAdded = DateTime.Now,
                                            MemberId = umbracoMember.Id,
                                            Points = newmem.Points
                                        };
                                        pointService.Add(points);
                                    }

                                    // Do a save on the member
                                    memService.Save(umbracoMember);
                                }
                                else
                                {
                                    //TODO- Show errors to the user
                                }

                                Server.ScriptTimeout = 600;
                            }

                            // Commit the transaction
                            unitOfWork.Commit();

                            // Yes
                            pnlResults.Controls.Add(new LiteralControl("Success - Members Imported"));
                            pnlResults.Visible = true;   
                        }
                        catch (Exception ex)
                        {
                            // Roll back database changes 
                            unitOfWork.Rollback();

                            // No
                            pnlResults.Controls.Add(new LiteralControl("There was an error during import"));
                            pnlResults.Visible = true;   
                        }
                    }
                }
                else
                {
                    // No
                    pnlResults.Controls.Add(new LiteralControl("No members found in the XML"));
                    pnlResults.Visible = true;                    
                }
            }
            else
            {
                pnlResults.Controls.Add(new LiteralControl("No file selected"));
                pnlResults.Visible = true;
            }
        }

        private static MemberImport MapImport(XmlNode node)
        {
            var m = new MemberImport();

            var username = node.SelectSingleNode("username");
            var email = node.SelectSingleNode("email");
            if (username != null && email != null)
            {
                m.Username = username.InnerText;
                m.Email = email.InnerText;

                var website = node.SelectSingleNode("website");
                if (website != null)
                {
                    m.Website = website.InnerText;
                }

                var twitter = node.SelectSingleNode("twitter");
                if (twitter != null)
                {
                    var twit = twitter.InnerText.Replace("http://", "").Replace("https://", "");
                    if (twit.IndexOf("/", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        m.Twitter = twit.Split('/')[1];

                    }
                    else
                    {
                        m.Twitter = twit;
                    }
                }

                var points = node.SelectSingleNode("points");
                if (points != null)
                {
                    try
                    {
                        m.Points = Convert.ToInt32(points.InnerText);
                    }
                    catch
                    {
                       // Don't need to do anything
                    }
                }
                var isadmin = node.SelectSingleNode("isadmin");
                if (isadmin != null)
                {
                    try
                    {
                        m.IsAdmin = Convert.ToBoolean(isadmin.InnerText);
                    }
                    catch
                    {
                        // Don't need to do anything
                    }
                }

                return m;   
            }

            return null;
        }
    }


    internal class MemberImport
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public int Points { get; set; }
        public bool IsAdmin { get; set; }
    }
}