using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.DB
{
    public static class ExtensionDb
    {
        public static Implement.IDBClient Client;

        private static void CheckClient()
        {
            if (Client is null)
                throw new KLIBException(0, "It's need define the DBClient");
        }

        #region Query Manager
        public static List<string> GetQueriesManager(string queryManagerPreFix)
        {
            var sql = $"SELECT QName FROM OUQR WHERE Qname LIKE '{queryManagerPreFix}%'";
            return Column<string>(sql);
        }

        public static Dynamic First(string sql, params object[] values)
        {
            using (Client)
            {
                Client.DoQuery(sql, values);
                if (Client.Next())
                    return Client.Field(0);
                else
                    return Dynamic.Empty;
            }
        }
        #endregion

        #region Query Faster
        public static DicDymanic Top1(string sql, params object[] values)
        {
            using (Client)
            {
                if (Client.DoQuery(sql, values) > 0)
                    return Client.Fields();
            }

            return new DicDymanic();
        }

        internal static void Execute(string sql, params object[] values)
        {
            using (Client)
                Client.NoQuery(sql, values);
            
        }

        public static List<T> Column<T>(string sql, params object[] values)
        {
            var res = new List<T>();
            using (Client)
            {
                Client.DoQuery(sql, values);
                while (Client.Next())
                    res.Add(Client.Field(0).Value);
            }

            return res;
        }

        /// <summary>
        /// Verify if value exists in the table
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="col">Column to compare</param>
        /// <param name="val1">Value to find</param>
        /// <param name="onlyString">Use regex OnlyLetterAndNumbers</param>
        /// <param name="like">Use like</param>
        /// <returns>Quantity register exists</returns>
        public static bool Exists(string table, string col, object val1, bool onlyString = false, bool like = false)
        {
            var sql = $"SELECT TOP 1 1 FROM [{table}] WHERE ";

            if (onlyString)
                sql += $"dbo.RegExReplace(UPPER({col}), '{klib.E.RegexMask.OnlyLetterAndNumbers}', '', DEFAULT) = '{klib.ValuesEx.RegexReplace(val1, klib.E.RegexMask.OnlyLetterAndNumbers).ToUpper()}' ";
            else
                sql += $"UPPER({col}) = '{val1.ToString().ToUpper()}'";

            if (like)
                sql = sql.Replace("=", "LIKE");

            using (var cnn = new DbClient())
            {
                return cnn.DoQuery(sql) > 0;
            }
        }

        public static bool HasLines(string sql, params object[] values)
        {
            using (Client)
                return Client.DoQuery(sql, values) > 0;
        }
        #endregion

        #region Add/Update
        /// <summary>
        /// Get the command in the internet
        /// </summary>
        /// <param name="url"></param>
        public static void Register(Uri url)
        {
            var func = Shell.Read(url);
            using (var cnn = new DbClient())
                cnn.NoQuery(func);
        }
        #endregion
    }
}
