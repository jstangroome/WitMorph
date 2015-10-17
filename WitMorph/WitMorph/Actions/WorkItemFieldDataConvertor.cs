using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WitMorph.Actions
{
    using System.Globalization;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    public class WorkItemFieldDataConvertor
    {
        public void ConvertFieldData(Field from, Field to)
        {
            Type targetType = to.FieldDefinition.SystemType;
            if (from.Value == null)
            {
                if (!to.IsRequired)
                {
                    to.Value = null;
                    return;
                }
            }

            if (targetType == typeof(string))
            { 
                from.Value = this.ToString(to);
            }
            else if (targetType == typeof(double))
            {
                from.Value = this.ToDouble(to);
            }
            else if (targetType == typeof(int))
            {
                from.Value = this.ToInt(to);
            }
            else
            { 
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot convert field data from type {0} to {1}", from.FieldDefinition.SystemType, to.FieldDefinition.SystemType));
            }
        }

        private string ToString(Field from)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", from.Value);
        }

        private double ToDouble(Field from)
        {
            if (from.Value as string == string.Empty) return 0;

            Type sourceType = from.FieldDefinition.SystemType;
            if (sourceType == typeof(double))
            {
                return (double)(from.Value ?? 0d);
            }
            else
            {
                return Convert.ToDouble(from.Value ?? 0d);
            }
        }

        private int ToInt(Field from)
        {
            if (from.Value as string == string.Empty) return 0;

            Type sourceType = from.FieldDefinition.SystemType;
            if (sourceType == typeof(int))
            {
                return (int)(from.Value ?? 0);
            }
            else
            {
                return Convert.ToInt32(from.Value ?? 0);
            }
        }
    }
}
