using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using StudentMobile.Models;

namespace StudentMobile.ViewModels
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "present" => Colors.Green,
                    "absent" => Colors.Red,
                    "late" => Colors.Orange,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
