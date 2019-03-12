using System;
using System.Collections.Generic;

namespace klib.Implement
{
    /// <summary>
    /// Interface to implement DataBase interface.
    /// </summary>
    public interface IDBClient : System.IDisposable
    {
        #region properties
        string LastCommand { get;}
        #endregion
        #region execute
        int DoQuery(string sql, params object[] values);
        int DoQueryManager(string qname, params object[] values);
        bool NoQuery(string sql, params object[] values);
        #endregion
        #region resources
        object[] FixValues(object[] values, bool manipulation = false);
        #endregion
        #region result
        DicDymanic Fields(bool upper_columns = true);
        IEnumerable<T> Fields<T>() where T : Implement.BaseModel,  new();
        Dynamic Field(object index);
        int Version();
        #endregion
        #region cursors
        bool IsFirstLine { get; }
        int TotalRows { get; }
        int TotalColumns { get; }
        bool Next();

        void First();
        void Last();
        #endregion
    }

    public abstract class DBFix
    {
        internal class Command
        {
            public string[] commands;
            public string val;
        }
        public static string OpenTag => "/*[";
        public static string MiddleTag => ":*/";
        public static string CloseTag => "/*]*/";

        public object[] FixValues(object[] values, bool manipulation = false)
        {
            var descr = manipulation ? "''''" : "''";
            var open = manipulation ? "''" : "'";

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    values[i] = "null";
                    continue;
                }

                var a = values[i].GetType().Name;
                switch (values[i].GetType().Name)
                {
                    case "DateTime":
                        values[i] = ((DateTime)values[i]).ToString("yyyy-MM-dd HH:mm:ss"); break;
                }

                values[i] = NSTag(values[i].ToString());
                values[i] = $"{open}{values[i].ToString().Replace("'", descr)}{open}";
            }

            return values;
        }

        public string Tags(string sql)
        {
            sql = NSTag(sql);
            return sql;
        }

        private Command Commands(string val)
        {
            var command = new Command();
            // get the cammands
            var comm_string = val.Substring(OpenTag.Length - 1, val.IndexOf(MiddleTag) - OpenTag.Length);
            command.commands = comm_string.Split('|');
            command.val = val.Substring(val.IndexOf(MiddleTag) + MiddleTag.Length, val.IndexOf(CloseTag) - val.IndexOf(MiddleTag) + MiddleTag.Length);

            return command;
            //var val = value.Substring(value.IndexOf(MiddleTag, i) + MiddleTag.Length, value.IndexOf(CloseTag, i);
            //var commandString = value.Substring(i + 3, value.IndexOf(MiddleTag, i));

            //var commands = commandString.Split('|');
        }

        /// <summary>
        /// Special tag to convert Namespace
        /// U_!!, @!! and [!!]
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        private string NSTag(string value)
        {
            value = value
                .Replace("U_!!", $"U_{klib.R.Company.NS}")
                .Replace("[!!]", $"U_{klib.R.Company.NS}")
                .Replace("@!!", $"@{klib.R.Company.NS}");
            return value;

        }

        #region Special tags
        private string SpecialTags(string value)
        {
            var wild_char = OpenTag[0];
            for(int i = 0; i < value.Length; i = i + 3)
            {
                // find the special char

                if (value[i] != wild_char)
                    continue;

                // Verify if char is open tag
                if (value.Substring(i, 3) != OpenTag)
                    continue;

                var command = Commands(value.Substring(i, value.IndexOf(CloseTag, i) + CloseTag.Length));
            }



            throw new NotImplementedException();
        }


        private string TagComments(string value)
        {

            throw new NotImplementedException();
        }

        #endregion
    }
}
