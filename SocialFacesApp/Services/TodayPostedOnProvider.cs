using System;
using SocialFacesApp.Services.Contracts;

namespace SocialFacesApp.Services
{
    public class TodayPostedOnProvider : IProvidePostedOnDate
    {
        public DateTime Get()
        {
            return DateTime.UtcNow.Date;
        }
    }
}