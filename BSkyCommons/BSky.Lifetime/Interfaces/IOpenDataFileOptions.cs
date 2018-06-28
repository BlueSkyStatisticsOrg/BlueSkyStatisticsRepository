using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSky.Lifetime.Interfaces
{
    public interface IOpenDataFileOptions
    {
        bool HasHeader { get; set; }
        bool IsBasketData { get; set; }
        char FieldSeparatorChar { get; set; }
        char DecimalPointChar { get; set; }
    }
}
