using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Web.Security;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Skybrud.Social.Google;
using Skybrud.Social.Google.OAuth;

namespace Dialogue.Logic.Controllers.OAuthControllers
{
    public class GoogleOAuthSurfaceController : BaseSurfaceController
    {
        public string ReturnUrl
        {
            get { return string.Concat(AppHelpers.ReturnCurrentDomain(), Urls.GenerateUrl(Urls.UrlType.GoogleLogin)); }
        }

        public string Callback { get; private set; }

        public string ContentTypeAlias { get; private set; }

        public string PropertyAlias { get; private set; }

        public string Feature { get; private set; }

        /// <summary>
        /// Gets the authorizing code from the query string (if specified).
        /// </summary>
        public string AuthCode
        {
            get { return Request.QueryString["code"]; }
        }

        public string AuthState
        {
            get { return Request.QueryString["state"]; }
        }

        public string AuthErrorReason
        {
            get { return Request.QueryString["error_reason"]; }
        }

        public string AuthError
        {
            get { return Request.QueryString["error"]; }
        }

        public string AuthErrorDescription
        {
            get { return Request.QueryString["error_description"]; }
        }

        public ActionResult GoogleLogin()
        {
            var resultMessage = new GenericMessageViewModel();

            Callback = Request.QueryString["callback"];
            ContentTypeAlias = Request.QueryString["contentTypeAlias"];
            PropertyAlias = Request.QueryString["propertyAlias"];
            Feature = Request.QueryString["feature"];

            if (AuthState != null)
            {
                var stateValue = Session["Dialogue_" + AuthState] as NameValueCollection;
                if (stateValue != null)
                {
                    Callback = stateValue["Callback"];
                    ContentTypeAlias = stateValue["ContentTypeAlias"];
                    PropertyAlias = stateValue["PropertyAlias"];
                    Feature = stateValue["Feature"];
                }
            }

            if (string.IsNullOrEmpty(Dialogue.Settings().GoogleClientId) ||
                string.IsNullOrEmpty(Dialogue.Settings().GoogleClientSecret))
            {
                resultMessage.Message = "You need to add the Google app credentials";
                resultMessage.MessageType = GenericMessages.Danger;
            }
            else
            {
                // Configure the OAuth client based on the options of the prevalue options
                var client = new GoogleOAuthClient
                {
                    ClientId = Dialogue.Settings().GoogleClientId,
                    ClientSecret = Dialogue.Settings().GoogleClientSecret,
                    RedirectUri = ReturnUrl
                };

                // Session expired?
                if (AuthState != null && Session["Dialogue_" + AuthState] == null)
                {
                    resultMessage.Message = "Session Expired";
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                // Check whether an error response was received from Google
                if (AuthError != null)
                {
                    resultMessage.Message = AuthErrorDescription;
                    resultMessage.MessageType = GenericMessages.Danger;
                    if (AuthState != null) Session.Remove("Dialogue_" + AuthState);
                }

                // Redirect the user to the Google login dialog
                if (AuthCode == null)
                {

                    // Generate a new unique/random state
                    var state = Guid.NewGuid().ToString();

                    // Save the state in the current user session
                    Session["Dialogue_" + state] = new NameValueCollection {
                    { "Callback", Callback},
                    { "ContentTypeAlias", ContentTypeAlias},
                    { "PropertyAlias", PropertyAlias},
                    { "Feature", Feature}
                };

                    // Declare the scope
                    var scope = new[] {
                    GoogleScope.OpenId,
                    GoogleScope.Email,
                    GoogleScope.Profile
                };

                    // Construct the authorization URL
                    var url = client.GetAuthorizationUrl(state, scope, GoogleAccessType.Offline, GoogleApprovalPrompt.Force);

                    // Redirect the user
                    return Redirect(url);
                }

                var info = new GoogleAccessTokenResponse();
                try
                {
                    info = client.GetAccessTokenFromAuthorizationCode(AuthCode);
                }
                catch (Exception ex)
                {
                    resultMessage.Message = string.Format("Unable to acquire access token<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                try
                {

                    // Initialize the Google service
                    var service = GoogleService.CreateFromRefreshToken(client.ClientIdFull, client.ClientSecret, info.RefreshToken);

                    // Get information about the authenticated user
                    var user = service.GetUserInfo();                    
                    using (UnitOfWorkManager.NewUnitOfWork())
                    {
                        var userExists = AppHelpers.UmbServices().MemberService.GetByEmail(user.Email);

                        if (userExists != null)
                        {
                            // Update access token
                            userExists.Properties[AppConstants.PropMemberGoogleAccessToken].Value = info.RefreshToken;
                            AppHelpers.UmbServices().MemberService.Save(userExists);

                            // Users already exists, so log them in
                            FormsAuthentication.SetAuthCookie(userExists.Username, true);
                            resultMessage.Message = Lang("Members.NowLoggedIn");
                            resultMessage.MessageType = GenericMessages.Success;
                        }
                        else
                        {
                            // Not registered already so register them
                            var viewModel = new RegisterViewModel
                            {
                                Email = user.Email,
                                LoginType = LoginType.Google,
                                Password = AppHelpers.RandomString(8),
                                UserName = user.Name,
                                SocialProfileImageUrl = user.Picture,
                                UserAccessToken = info.RefreshToken
                            };

                            return RedirectToAction("MemberRegisterLogic", "DialogueLoginRegisterSurface", viewModel);
                        }
                    }

                }
                catch (Exception ex)
                {
                    resultMessage.Message = string.Format("Unable to get user information<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.Danger;
                }

            }


            ShowMessage(resultMessage);
            return RedirectToUmbracoPage(Dialogue.Settings().ForumId);
        }
    } 
}