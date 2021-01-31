using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Avalonia.Controls
{
    [TypeConverter(typeof(ItemVirtualizingCacheConverter))]
    public class ItemVirtualizingCache
    {
        public enum CacheLengthUnitEnum { Page, Pixel, Item }
        const int DefaultForward = 1;
        const int DefaultBack = 1;
        public ItemVirtualizingCache()
        {

        }
        public int CacheAfter
        {
            get;
            set;
        }
        public int CacheBefore
        {
            get;
            set;
        }
        public int CacheAfterExtra { get; set; }
        public int CacheBeforeExtra { get; set; }
        public CacheLengthUnitEnum CacheLengthUnit { get; set; }

        //public static VirtualizingCache Parse(string str)
        //{
        //    int[] ia = str.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
        //    var virt= new VirtualizingCache();
        //    virt.CacheAfter = ia.Length > 0 ? ia[0] : DefaultForward;
        //    virt.CacheBefore = ia.Length > 1 ? ia[1] : DefaultBack;
        //}

        public double GetBackCacheSize(double viewPort, double item)
        {
            return 100;
            switch (CacheLengthUnit)
            {
                case CacheLengthUnitEnum.Page:
                    return CacheBefore * viewPort;
                case CacheLengthUnitEnum.Pixel:
                    return CacheBefore;
                case CacheLengthUnitEnum.Item:
                    return CacheBefore * item;
            }
            return 10;
        }
        public double GetFwdCacheSize(double viewPort, double item)
        {
            return 50;
            switch (CacheLengthUnit)
            {
                case CacheLengthUnitEnum.Page:
                    return CacheAfter * viewPort;
                case CacheLengthUnitEnum.Pixel:
                    return CacheAfter;
                case CacheLengthUnitEnum.Item:
                    return CacheAfter * item;
            }
            return 10;
        }
        public override string ToString()
        {
            return $"{CacheLengthUnit} {CacheAfter},{CacheBefore} {CacheAfterExtra},{CacheBeforeExtra}";
        }
    }

    internal class ItemVirtualizingCacheConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string vals)
                return new ItemVirtualizingCache();
            return base.ConvertFrom(context, culture, value);
        }
    }
}
