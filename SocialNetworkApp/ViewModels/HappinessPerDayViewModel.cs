using System;
using Newtonsoft.Json;
using SocialShared.Converters;

namespace SocialNetworkApp.ViewModels
{
    public class HappinessPerDayViewModel
    {
        [JsonConverter(typeof(PostedOnConverter))]
        public DateTime PostedOn { get; set; }

        public decimal PeopleCount { get; set; }

        public decimal HappinessAverage { get; set; }
    }
}