using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace Dialogue.Logic.Application
{
    /// <summary>
    /// Usage like so on property [UmbracoDisplay("MyDictionaryKey")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class UmbracoDisplayAttribute : RangeAttribute, IClientValidatable
    {
        public UmbracoDisplayAttribute(int minimum, int maximum, string dictionaryKey)
            : base(minimum, maximum)
        {
            this.ErrorMessage = dictionaryKey;
        }

        public override string FormatErrorMessage(string name)
        {
            return AppHelpers.Lang(base.FormatErrorMessage(name));
        }


        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            // Kodus to "Chad" http://stackoverflow.com/a/9914117
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = AppHelpers.Lang(this.ErrorMessage),
                ValidationType = "range"
            };

            rule.ValidationParameters.Add("min", this.Minimum);
            rule.ValidationParameters.Add("max", this.Maximum);
            yield return rule;
        }
    }

    public class DialogueDisplayName : DisplayNameAttribute
    {
        private string _resourceValue = string.Empty;

        public DialogueDisplayName(string resourceKey)
            : base(resourceKey)
        {
            ResourceKey = resourceKey;
        }

        public string ResourceKey { get; set; }

        public override string DisplayName
        {
            get
            {
                _resourceValue = AppHelpers.Lang(ResourceKey);
                return _resourceValue;
            }
        }

        public string Name
        {
            get { return "DialogueDisplayName"; }
        }

    }

    public class ModelStateToTempDataAttribute : ActionFilterAttribute
    {
        public const string TempDataKey = "__TSD_ValidationFailures__";

        /// <summary>
        /// When a RedirectToRouteResult is returned from an action, anything in the ViewData.ModelState dictionary will be copied into TempData.
        /// When a ViewResultBase is returned from an action, any ModelState entries that were previously copied to TempData will be copied back to the ModelState dictionary.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;

            var controller = filterContext.Controller;

            var result = filterContext.Result;

            if (result is ViewResultBase)
            {
                //If there are failures in tempdata, copy them to the modelstate
                CopyTempDataToModelState(controller.ViewData.ModelState, controller.TempData);
                return;
            }
            //If we're redirecting and there are errors, put them in tempdata instead (so they can later be copied back to modelstate)
            if ((result is RedirectToRouteResult || result is RedirectResult || result is RedirectToUmbracoPageResult) && !modelState.IsValid)
            {
                CopyModelStateToTempData(controller.ViewData.ModelState, controller.TempData);
            }
        }

        private void CopyTempDataToModelState(ModelStateDictionary modelState, TempDataDictionary tempData)
        {
            if (!tempData.ContainsKey(TempDataKey)) return;

            var fromTempData = tempData[TempDataKey] as ModelStateDictionary;
            if (fromTempData == null) return;

            foreach (var pair in fromTempData.ToList())
            {
                if (modelState.ContainsKey(pair.Key))
                {
                    modelState[pair.Key].Value = pair.Value.Value;

                    foreach (var error in pair.Value.Errors.ToList())
                    {
                        modelState[pair.Key].Errors.Add(error);
                    }
                }
                else
                {
                    modelState.Add(pair.Key, pair.Value);
                }
            }
        }

        private static void CopyModelStateToTempData(ModelStateDictionary modelState, TempDataDictionary tempData)
        {
            tempData[TempDataKey] = modelState;
        }
    }

}