using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace klib
{
    public class Values
    {
        public readonly object Value;
        public int Lenght => GetLenght();

        #region Primitives
        public bool ToBool()
        {
            //00001
            var foo = Value.ToString();
            switch (foo.ToUpper())
            {
                case "1":
                case "Y":
                case "YES":
                case "S":
                case "SI":
                case "SIM":
                case "T":
                case "TRUE":
                    return true;
                default:
                    return false;

            }
        }

        public Uri ToUrl()
        {
            try
            {
                return new Uri(Value.ToString());
            }catch(System.UriFormatException ex)
            {
                throw new LException(5, Value.ToString(), ex.Message);
            }
        }
        #endregion










        /// ///////////////////////////////////////////////////////////////////////////////////
        public DirectoryInfo ToDirectory()
        {
            if (System.IO.Directory.Exists(Value.ToString()))
                return new DirectoryInfo(Value.ToString());

            switch(Value.ToString().ToUpper())
            {
                case "%TEMP%": return Shell.TempDir();
                default: throw new LException(1, $"Directory not exists or without permissions. {Value.ToString()}");
            }
        }

        public bool IsEmpty => Value == null 
            || String.IsNullOrEmpty(Value.ToString()) 
            || String.IsNullOrWhiteSpace(Value.ToString());


        public Values(object value)
        {
            Value = value;
        }

        private int GetLenght()
        {
            if(Value.GetType() == typeof(String))
                return Value.ToString().Length;
            if (Value.GetType() == typeof(Array))
                return ((Array)Value).Length;
            else
                throw new LException(1,$"({Value.GetType()}) Type of variable isn't not defined.");
        }


        public byte[] ToByteArray()
        {
            int NumberChars = Value.ToString().Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(Value.ToString().Substring(i, 2), 16);
            return bytes;
        }

        public object ToObject()
        {
            return Value;
        }

        public dynamic Dynamic()
        {
            return Value;
        }
        public int ToInt(int ifnull = 0)
        {
            try
            {
                return int.Parse(Value.ToString());
            }
            catch
            {
                return ifnull;
            }
        }

        public int? ToNInt()
        {
            try
            {
                return int.Parse(Value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(Value);
        }

        public dynamic ToAny()
        {
            return Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public DateTime ToDateTime(string format)
        {
            return DateTime.ParseExact(Value.ToString(),format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public int TimeToInt(bool addsecs = false)
        {
            if (addsecs)
                return int.Parse(ToDateTime().ToString("HHmmss"));
            else
                return int.Parse(ToDateTime().ToString("HHmm"));
        }

        public DateTime ToDateTime()
        {
            return (DateTime)Value;
        }

        public DateTime ToDate()
        {
            return ToDateTime().Date;
        }
        public double ToDouble(string point)
        {
            var val = Value.ToString();

            if (point == ".")
                val = val.Replace(",", "");
            else
                val = val.Replace(".", "");

            val = val.Replace(",", ".");
            return double.Parse(Regex.Replace(Value.ToString(), "[^0-9.]+", ""));
        }

        public Values OnlyNumbers(int def = 0)
        {
            if (IsEmpty)
                return new Values(def);

            var val = Regex.Match(ToString(), @"\d+").Value;

            if (String.IsNullOrEmpty(val))
                return new Values(def);
            else
                return new Values(val);

        }

        /// <summary>
        /// Replace information using regex. Exemple:
        /// Only char and number: "[^0-9a-zA-Z]+"
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public string RegexReplace(string pattern)
        {
            return Regex.Replace(Value.ToString(), pattern, "");
        }

        public string ToStringFormat(params object[] values)
        {
            if (values.Length > 0)
                return String.Format(Value.ToString(), values);
            else
                return Value.ToString();
        }

        public byte[] ToByte()
        {
            if (Value.GetType().Name == "Byte[]")
                return (byte[])Value;
            else
                throw new LException(1, "Value isn't byte[]");
        }

        #region DateTime
        /// <summary>
        /// Compare the dates
        /// </summary>
        /// <param name="dateTime">Date to compare (exclude the seconds)</param>     
        /// <returns>
        /// 1 - Value is great then dateTime
        /// 0 - Value and dateTime same values
        /// -1 - dateTime is great then Value
        /// </returns>
        public int Compare(DateTime dateTime)
        {
            var valDateTime = (DateTime)Value;
            var date1 = new DateTime(valDateTime.Year, valDateTime.Month, valDateTime.Day, valDateTime.Hour, valDateTime.Minute, 0);
            var date2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);                        

            return DateTime.Compare(date1, date2);
        }
        #endregion

        #region IO
        public model.Printer Printer(bool validate = true)
        {
            var printer = new model.Printer(Value.ToString());

            if (validate && !printer.ValidPrinter())
                throw new LException(1, $"Printer isn't {Value.ToString()} valid");

            return printer;
        }

        //public System.IO.DirectoryInfo Directory(bool validate = true)
        //{
        //    if (validate && !System.IO.Directory.Exists(Value.ToString()))
        //        throw new LException(1, $"Directory {Value.ToString()} not exists");

        //    return new System.IO.DirectoryInfo(Value.ToString());
        //}
        #endregion


    }

    public static class ValuesEx
    {
        public static Values Empty => new Values("");
        public static Values To(object value)
        {
            return new Values(value);
        }     
        public static string RegexReplace(object val, string pattern)
        {
            return To(val).RegexReplace(pattern);
        }

        /// <summary>
        /// Format values in characteres open and close.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="open"></param>
        /// /// <param name="close"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Format(string val, string open, string close, params object[] values)
        {
            // Query manager = @"\[\%[0-9]*\]"
            var pattern = $@"{open}[0-9]*{close}";
            int qtty = Regex.Matches(val, pattern).Count;

            for(int i = 0; i < qtty; i++)
                val = val.Replace($@"{open}{i}{close}", values[i].ToString());

            return val;
        }

        public static StreamReader ResourceUrl(Assembly assembly, Type classType, E.Resource dir)
        {
            return ResourceUrl(assembly, classType.Name, dir);
        }
        public static StreamReader ResourceUrl(Assembly  assembly, string name, E.Resource dir)
        {
            var resource = assembly.GetManifestResourceNames();
            var resourceName = resource.Where(t => t.Contains($"content.{dir.ToString()}.{name}")).FirstOrDefault();
            if (!String.IsNullOrEmpty(resourceName))
                return new StreamReader(assembly.GetManifestResourceStream(resourceName));
            else
                return null;
        }

        /// <summary>
        /// Transform @!! to Namespace 
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public static string NS(string value)
        {
            value = value.Replace("U_!!", $"U_{klib.R.Company.NS}");
            return value.Replace("@!!", $"@{klib.R.Company.NS}");
        }
    }
}
