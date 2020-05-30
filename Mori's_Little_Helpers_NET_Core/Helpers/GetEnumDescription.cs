using System;
using System.ComponentModel;

namespace Schweigm_NETCore_Helpers
{
    public static class EnumDescription
    {
        // Return the description of enum value
        public static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();

            // Return the value of the attribute
        }
    }
}
