using System;

namespace Dialogue.Logic.Models.ViewModels
{
    public class SubscribeEmailViewModel
    {
        public string Id { get; set; }
        public string SubscriptionType { get; set; }
    }

    public class UnSubscribeEmailViewModel
    {
        public string Id { get; set; }
        public string SubscriptionType { get; set; }
    }
}