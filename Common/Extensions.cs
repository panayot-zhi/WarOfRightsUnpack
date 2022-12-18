namespace WarOfRightsUnpack.Common
{
    public static class Extensions
    {
        public static string Remove(this string source, string stringToRemove)
        {
            return source.Replace(stringToRemove, string.Empty);
        }

        public static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

    }
}
