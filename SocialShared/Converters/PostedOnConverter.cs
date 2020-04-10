using System;
using Newtonsoft.Json;

namespace SocialShared.Converters
{
    public class PostedOnConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var postedOn = (DateTime) value;
            writer.WriteValue(postedOn.Ticks.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var postedOnTicks = long.Parse(reader.Value.ToString());
            return new DateTime(postedOnTicks);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }
    }
}