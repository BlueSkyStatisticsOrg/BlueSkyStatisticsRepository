using System;
using System.ComponentModel;

namespace BSky.Controls
{
    public class ConditionConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Condition))
                return true;

            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is Condition)
            {

                Condition so = (Condition)value;

                return string.Format("{0} {1} {2}", so.PropertyName, so.Operator, so.Value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    Condition co = new Condition();
                    co.PropertyName = "Test";
                    co.Operator = ConditionalOperator.Equals;
                    co.Value = "5";
                    return co;
                }
                catch
                {
                    throw new ArgumentException(
                        "Can not convert '" + (string)value +
                                           "' to type SpellingOptions");
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

}
