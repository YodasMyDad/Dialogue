namespace Dialogue.Logic.Controllers.OAuthControllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Security;
    using Application;
    using Constants;
    using Models;
    using Models.ViewModels;
    using Skybrud.Social.Microsoft;
    using Skybrud.Social.Microsoft.OAuth;
    using Skybrud.Social.Microsoft.Responses.Authentication;
    using Skybrud.Social.Microsoft.WindowsLive.Scopes;

    // Create new app - https://account.live.com/developers/applications/create
    // List of existing app - https://account.live.com/developers/applications/index

    public class MicrosoftOAuthController : DialogueBaseController
    {
        public string ReturnUrl => string.Concat(AppHelpers.ReturnCurrentDomain(), Urls.GenerateUrl(Urls.UrlType.MicrosoftLogin));
        public string AuthCode => Request.QueryString["code"];
        public string AuthState => Request.QueryString["state"];
        public string AuthError => Request.QueryString["error"];
        public string AuthErrorDescription => Request.QueryString["error_description"];

        public ActionResult MicrosoftLogin()
        {
            var resultMessage = new GenericMessageViewModel();

            var input = new
            {
                Code = AuthCode,
                State = AuthState,
                Error = new
                {
                    HasError = !string.IsNullOrWhiteSpace(AuthError),
                    Text = AuthError,
                    ErrorDescription = AuthErrorDescription
                }
            };


            // Get the prevalue options
            if (string.IsNullOrEmpty(Dialogue.Settings().MicrosoftAppId) ||
                string.IsNullOrEmpty(Dialogue.Settings().MicrosoftAppSecret))
            {
                resultMessage.Message = "You need to add the Microsoft app credentials to the web.config";
                resultMessage.MessageType = GenericMessages.Danger;
            }
            else
            {

                var client = new MicrosoftOAuthClient
                {
                    ClientId = Dialogue.Settings().MicrosoftAppId,
                    ClientSecret = Dialogue.Settings().MicrosoftAppSecret,
                    RedirectUri = ReturnUrl
                };

                // Session expired?
                if (input.State != null && Session["Dialogue_" + input.State] == null)
                {
                    resultMessage.Message = "Session Expired";
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                // Check whether an error response was received from Microsoft
                if (input.Error.HasError)
                {
                    Session.Remove("Dialogue_" + input.State);
                    resultMessage.Message = AuthErrorDescription;
                    resultMessage.MessageType = GenericMessages.Danger;
                }

                // Redirect the user to the Microsoft login dialog
                if (string.IsNullOrWhiteSpace(input.Code))
                {

                    // Generate a new unique/random state
                    var state = Guid.NewGuid().ToString();

                    // Save the state in the current user session
                    Session["Dialogue_" + state] = "/";

                    // Construct the authorization URL
                    var url = client.GetAuthorizationUrl(state, WindowsLiveScopes.Emails + WindowsLiveScopes.Birthday);

                    // Redirect the user
                    return Redirect(url);
                }

                // Exchange the authorization code for an access token
                MicrosoftTokenResponse accessTokenResponse;
                try
                {
                    Session.Remove("Dialogue_" + input.State);
                    accessTokenResponse = client.GetAccessTokenFromAuthCode(input.Code);
                }
                catch (Exception ex)
                {
                    accessTokenResponse = null;
                    resultMessage.Message = $"Unable to acquire access token<br/>{ex.Message}";
                    resultMessage.MessageType = GenericMessages.Danger;
                }


                try
                {
                    if (string.IsNullOrEmpty(resultMessage.Message) || accessTokenResponse != null)
                    {
                        //MicrosoftScope debug = accessTokenResponse.Body.Scope.Items;

                        // Initialize a new MicrosoftService so we can make calls to the API
                        var service = MicrosoftService.CreateFromAccessToken(accessTokenResponse.Body.AccessToken);

                        // Make the call to the Windows Live API / endpoint
                        var response = service.WindowsLive.GetSelf();

                        // Get a reference to the response body
                        var user = response.Body;

                        var getEmail = !string.IsNullOrWhiteSpace(user.Emails?.Preferred);
                        if (!getEmail)
                        {
                            resultMessage.Message = "Unable to get email address from Microsoft account";
                            resultMessage.MessageType = GenericMessages.Danger;
                            ShowMessage(resultMessage);
                            return RedirectToUmbracoPage(Dialogue.Settings().ForumId);
                        }

                        using (UnitOfWorkManager.NewUnitOfWork())
                        {
                            var userExists = AppHelpers.UmbServices().MemberService.GetByEmail(user.Emails.Preferred);

                            if (userExists != null)
                            {
                                try
                                {
                                    // Update access token
                                    userExists.Properties[AppConstants.PropMemberMicrosoftAccessToken].Value = accessTokenResponse.Body.AccessToken;
                                    AppHelpers.UmbServices().MemberService.Save(userExists);

                                    // Users already exists, so log them in
                                    FormsAuthentication.SetAuthCookie(userExists.Username, true);
                                    resultMessage.Message = Lang("Members.NowLoggedIn");
                                    resultMessage.MessageType = GenericMessages.Success;
                                }
                                catch (Exception ex)
                                {
                                    AppHelpers.LogError(ex);
                                }
                            }
                            else
                            {

                                // Not registered already so register them
                                var viewModel = new RegisterViewModel
                                {
                                    Email = user.Emails.Preferred,
                                    LoginType = LoginType.Microsoft,
                                    Password = AppHelpers.RandomString(8),
                                    UserName = user.Name,
                                    SocialProfileImageUrl = $"https://apis.live.net/v5.0/{user.Id}/picture",
                                    UserAccessToken = accessTokenResponse.Body.AccessToken
                                };

                                return RedirectToAction("MemberRegisterLogic", "DialogueRegister", viewModel);
                            }
                        }

                    }
                    else
                    {
                        resultMessage.MessageType = GenericMessages.Danger;
                    }

                }
                catch (Exception ex)
                {
                    resultMessage.Message = $"Unable to get user information<br/>{ex.Message}";
                    resultMessage.MessageType = GenericMessages.Danger;
                }



            }


            ShowMessage(resultMessage);
            return RedirectToUmbracoPage(Dialogue.Settings().ForumId);
        }
    }
}