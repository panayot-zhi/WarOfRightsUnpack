using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;
using System.Globalization;

namespace WarOfRightsUnpack.Common
{
    public sealed class GenericAutoMap<T> : ClassMap<T>
    {
        public GenericAutoMap()
        {
            AutoMap(CultureInfo.CurrentCulture);

            foreach (var memberMap in MemberMaps)
            {
                if (Extensions.IsNullable(memberMap.Data.Type))
                {
                    memberMap.TypeConverter(new CsvNullTypeConverter());
                }
            }
        }
    }

    public class CsvNullTypeConverter: DefaultTypeConverter 
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return "NULL";
            }

            return base.ConvertToString(value, row, memberMapData);
        }
    }

    public class MySqlDecimalConverter : JsonConverter<decimal?>
    {
        public override void WriteJson(JsonWriter writer, decimal? value, JsonSerializer serializer)
        {
            if (!value.HasValue)
            {
                writer.WriteValue(value);
            }
            else
            {
                writer.WriteValue($"{value:F4}");
            }
        }

        public override decimal? ReadJson(JsonReader reader, Type objectType, decimal? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
