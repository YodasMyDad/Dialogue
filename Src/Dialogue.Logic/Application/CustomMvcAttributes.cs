using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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


}