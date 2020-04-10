using System;
using Newtonsoft.Json;
using SocialShared.Converters;

namespace SocialFacesApp.Models
{
    [Serializable]
    public class HappinessPerDayProjection
    {
        [JsonConverter(typeof(PostedOnConverter))]
        [JsonProperty("id")]
        public DateTime PostedOn { get; set; }

        public decimal PeopleCount { get; set; }

        public decimal HappinessAmount { get; set; }

        public decimal HappinessAverage => HappinessAmount / PeopleCount;

        public void UpdateHappinessInfo(HappinessPerDayProjection newInfo)
        {
            if (newInfo == null)
            {
                throw new ArgumentNullException($"Param {nameof(newInfo)} can not be null");
            }

            HappinessAmount += newInfo.HappinessAmount;
            PeopleCount += newInfo.PeopleCount;
        }
    }
}