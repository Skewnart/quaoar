using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Quaoar.Converters
{
    public class TypeToReadableTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return "Texte";
            else if (value is Bitmap)
                return "Image";
            else if (value is string[])
            {
                string[] arr = value as string[];
                if (arr.Length > 0)
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(Path.GetFullPath(arr[0])))
                            return $"Fichier{(arr.Length > 1 ? "s" : "")}";
                    }
                    catch (Exception) { }
                }
            }

            return "Type inconnu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
