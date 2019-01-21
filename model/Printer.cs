using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;


namespace klib.model
{
    public class Printer
    {
        public enum PrinterType
        {
            Graphics,
            Text,
            Other,
        }

        public string Name;
        public PrinterType Type;
        public string Alias;

        public Printer(string name)
        {
            Name = name;
        }

        public bool ValidPrinter()
        {
            var list = ListOfPrinters();

            return list.Where(t => t == Name).Any();
        }

        public static List<string> ListOfPrinters()
        {
            var list = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
                list.Add(printer);

            return list;
        }

    }
}
