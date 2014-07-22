using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Umbraco.Web;

namespace Dialogue.Logic.Controllers
{
    public partial class ViewMacroSurfaceController : BaseSurfaceController
    {

        
        public PartialViewResult RenderCustomViewMacros()
        {
            // Get current node
            var currentNode = AppHelpers.CurrentPage();
            if (currentNode.HasProperty(AppConstants.PropViewMacros))
            {
                // Now get the property for the custom views
                var customView = currentNode.GetPropertyValue<string>(AppConstants.PropViewMacros);
                if (!string.IsNullOrEmpty(customView))
                {
                    customView = customView.ToLower();

                    // This is a special partial result - We pass the string name/path in to the view
                    // Also we look for the view name to work out if it needs a custom model or not
                    //if (customView.Contains("contactform"))
                    //{
                    //    // Create and return a custom partial
                    //    return RenderContactForm(customView, currentNode);
                    //}

                    return PartialView(string.Concat(AppConstants.ViewMacroDirectory, customView));
                }
            }

            return null;
        }

        //public PartialViewResult RenderContactForm(string customView, IPublishedContent currentNode)
        //{
        //    // Here we can create custom logic, create custom models to go into the view
        //    var contactModel = new ContactForm{ThankYouPage = currentNode.GetPropertyValue<int>(AppConstants.PropLinkedPage)};
        //    return PartialView(string.Concat(AppConstants.ViewMacroDirectory, customView), contactModel);
        //}
    }
}