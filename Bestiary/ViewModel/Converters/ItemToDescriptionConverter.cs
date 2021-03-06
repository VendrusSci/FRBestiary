﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Bestiary.ViewModel.Converters
{
    public class ItemToDescriptionConverter : IValueConverter
    {
        private string GetDescription(object fieldObject)
        {
            FieldInfo fieldInfo = fieldObject.GetType().GetField(fieldObject.ToString());

            if(fieldInfo == null)
            {
                return null;
            }

            object[] attributes = fieldInfo.GetCustomAttributes(false);
            if(attributes.Length == 0)
            {
                return fieldObject.ToString();
            }
            else
            {
                DescriptionAttribute attribute = attributes[0] as DescriptionAttribute;
                return attribute.Description;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetDescription(value) ?? value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
