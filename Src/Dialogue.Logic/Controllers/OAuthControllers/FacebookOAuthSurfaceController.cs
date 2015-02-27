using System;
using System.Web.Mvc;
using System.Web.Security;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Skybrud.Social.Facebook;
using Skybrud.Social.Facebook.OAuth;

namespace Dialogue.Logic.Controllers.OAuthControllers
{
    public class FacebookOAuthSurfaceController : BaseSurfaceController
    {
        public string ReturnUrl
        {
            get { return string.Concat(AppHelpers.ReturnCurrentDomain(), Urls.GenerateUrl(Urls.UrlType.FacebookLogin)); }
        }

        public string Callback { get; private set; }

        public string ContentTypeAlias { get; private set; }

        public string PropertyAlias { get; private set; }

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

        public ActionResult FacebookLogin()
        {
            var resultMessage = new GenericMessageViewModel();

            Callback = Request.QueryString["callback"];
            ContentTypeAlias = Request.QueryString["contentTypeAlias"];
            PropertyAlias = Request.QueryString["propertyAlias"];

            if (AuthState != null)
            {
                var stateValue = Session["Dialogue_" + AuthState] as string[];
                if (stateValue != null && stateValue.Length == 3)
                {
                    Callback = stateValue[0];
                    ContentTypeAlias = stateValue[1];
                    PropertyAlias = stateValue[2];
                }
            }

            // Get the prevalue options
            if (string.IsNullOrEmpty(Dialogue.Settings().FacebookAppId) || string.IsNullOrEmpty(Dialogue.Settings().FacebookAppSecret))
            {
                resultMessage.Message = "You need to add the Facebook app credentials";
                resultMessage.MessageType = GenericMessages.Danger;
            }
            else
            {

                // Settings valid move on
                // Configure the OAuth client based on the options of the prevalue options
                var client = new FacebookOAuthClient
                {
                    AppId = Dialogue.Settings().FacebookAppId,
                    AppSecret = Dialogue.Settings().FacebookAppSecret,
                    RedirectUri = ReturnUrl
                };

                // Session expired?
                if (AuthState != null && Session["Dialogue_" + AuthState] == null)
                {
                    resultMessage.Message = "Session Expired";
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                // Check whether an error response was received from Facebook
                if (AuthError != null)
                {
                    resultMessage.Message = AuthErrorDescription;
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                // Redirect the user to the Facebook login dialog
                if (AuthCode == null)
                {
                    // Generate a new unique/random state
                    var state = Guid.NewGuid().ToString();

                    // Save the state in the current user session
                    Session["Dialogue_" + state] = new[] { Callback, ContentTypeAlias, PropertyAlias };

                    // Construct the authorization URL
                    var url = client.GetAuthorizationUrl(state, "public_profile", "email"); //"user_friends"

                    // Redirect the user
                    return Redirect(url);
                }

                // Exchange the authorization code for a user access token
                var userAccessToken = string.Empty;
                try
                {
                    userAccessToken = client.GetAccessTokenFromAuthCode(AuthCode);
                }
                catch (Exception ex)
                {
                    resultMessage.Message = string.Format("Unable to acquire access token<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                try
                {
                    if (string.IsNullOrEmpty(resultMessage.Message))
                    {
                        // Initialize the Facebook service (no calls are made here)
                        var service = FacebookService.CreateFromAccessToken(userAccessToken);

                        var user = service.Users.GetUser();


                        // Try to get the email - Some FB accounts have protected passwords
                        var email = user.Body.Email;
                        // TODO - Ignore if no email - Have to check PropMemberFacebookAccessToken has a value
                        // TODO - and the me.UserName is there to match existing logged in accounts
                        
     
                        // First see if this user has registered already - Use email address
                        using (UnitOfWorkManager.NewUnitOfWork())
                        {

                            var userExists = AppHelpers.UmbServices().MemberService.GetByEmail(email);

                            if (userExists != null)
                            {
                                // Update access token
                                userExists.Properties[AppConstants.PropMemberFacebookAccessToken].Value = userAccessToken;
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
                                    Email = email,
                                    LoginType = LoginType.Facebook,
                                    Password = AppHelpers.RandomString(8),
                                    UserName = user.Body.Name,
                                    UserAccessToken = userAccessToken
                                };

                                // Get the image and save it
                                var getImageUrl = string.Format("http://graph.facebook.com/{0}/picture?type=square", user.Body.Id);
                                viewModel.SocialProfileImageUrl = getImageUrl;

                                //Large size photo https://graph.facebook.com/{facebookId}/picture?type=large
                                //Medium size photo https://graph.facebook.com/{facebookId}/picture?type=normal
                                //Small size photo https://graph.facebook.com/{facebookId}/picture?type=small
                                //Square photo https://graph.facebook.com/{facebookId}/picture?type=square

                                return RedirectToAction("MemberRegisterLogic", "DialogueLoginRegisterSurface", viewModel);
                            }
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