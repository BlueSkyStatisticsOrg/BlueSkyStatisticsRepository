using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BSky.Controls.Commands
{
    public static class CustomMenuCommands
    {
        static RoutedUICommand _exportToExcel = new RoutedUICommand("Export To Excel", "ExportToExcelCommand", typeof(CustomMenuCommands));
        public static RoutedUICommand ExportToExcel { get { return _exportToExcel; } }

        static RoutedUICommand _exportAPAToWord = new RoutedUICommand("Export To Word", "ExportAPAToWordCommand", typeof(CustomMenuCommands));
        public static RoutedUICommand ExportAPAToWord { get { return _exportAPAToWord; } }

        static RoutedUICommand _exportFGridToPDF = new RoutedUICommand("Export To PDF", "ExportFGridToPDFCommand", typeof(CustomMenuCommands));
        public static RoutedUICommand ExportFGridToPDF { get { return _exportFGridToPDF; } }
    }
}
