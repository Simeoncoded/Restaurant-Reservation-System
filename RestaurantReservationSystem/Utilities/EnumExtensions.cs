using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.Utilities
{
    public static class EnumExtensions
    {
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, TEnum? selectedValue)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var values = Enum.GetValues(typeof(TEnum))
                             .Cast<TEnum>()
                             .Select(e => new
                             {
                                 Value = e,
                                 Text = e.GetType()
                                     .GetField(e.ToString())
                                     .GetCustomAttributes(typeof(DisplayAttribute), false)
                                     .FirstOrDefault() is DisplayAttribute displayAttribute
                                     ? displayAttribute.Name
                                     : e.ToString()
                             })
                             .ToList();

            return new SelectList(values, "Value", "Text", selectedValue);
        }
    }
}
