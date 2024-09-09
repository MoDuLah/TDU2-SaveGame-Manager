using System;
using System.Globalization;
using System.Windows.Data;

namespace TDU2SaveGameManager
{
    public class CenterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double windowWidth)
            {
                if (parameter is double contentWidth)
                {
                    // Calculate the left margin to center the content
                    return (windowWidth - contentWidth) / 2;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
