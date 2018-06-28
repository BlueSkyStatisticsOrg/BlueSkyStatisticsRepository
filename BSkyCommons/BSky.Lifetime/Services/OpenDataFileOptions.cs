using BSky.Lifetime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSky.Lifetime.Services
{
    public class OpenDataFileOptions : IOpenDataFileOptions
    {
        //default values are assigned
        public bool HasHeader { get; set; } = true;
        public bool IsBasketData { get; set; } = false;
        public char FieldSeparatorChar { get; set; } = ',';
        public char DecimalPointChar { get; set; } = '.';

    }
}
