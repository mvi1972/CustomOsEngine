using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace OsEngine.Converters
{
    public class ConvertersGrid : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           SolidColorBrush color =Brushes.White;

            if (value is Side)
            {
                if ((Side)value == Side.Buy)
                {
                    color = Brushes.LightGreen;
                }
                else 
                {
                    color = Brushes.LightPink;  
                }
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
