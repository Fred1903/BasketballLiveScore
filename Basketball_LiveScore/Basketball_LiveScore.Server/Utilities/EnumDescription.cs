using System.ComponentModel;
using System.Reflection;

namespace Basketball_LiveScore.Server.Utilities
{
    public static class EnumDescription
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
