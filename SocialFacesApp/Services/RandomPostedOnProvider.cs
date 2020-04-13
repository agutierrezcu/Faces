using System;
using SocialFacesApp.Services.Contracts;

namespace SocialFacesApp.Services
{
    public class RandomPostedOnProvider : IProvidePostedOnDate
    {
        private readonly Random _intervalDaysRandom = new Random();

        // It could be stored as a settings
        private const int DaysOffset = 5;

        public DateTime Get()
        {
            var postedOn = DateTime.UtcNow.Date.AddDays(_intervalDaysRandom.Next(-DaysOffset, DaysOffset));
            return postedOn;
        }
    }
}