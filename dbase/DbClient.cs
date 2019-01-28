using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using klib.implement;
using klib.model;

namespace klib.dbase
{
    public class DbClient : implement.IDbClient
    {

        private const string LOG = "DBCLI";
        public static List<SqlConnectionStringBuilder> ConnStrings { get; protected set; } = new List<SqlConnectionStringBuilder>();
        private SqlConnection Cnn;
        private SqlDataReader DataReader;
        private bool FirstLine = false;
        public readonly int Version;
        public readonly SqlConnectionStringBuilder ConnString;
        public string LastCommand { get; protected set; }

        public bool IsFirstLine => FirstLine;

        public DbClient(string connString = null)
        {

            if (String.IsNullOrEmpty(connString) && ConnStrings.Count > 0)
                connString = ConnStrings[0].ConnectionString;

            ConnString = new SqlConnectionStringBuilder(connString);
            try
            {
                if (ConnStrings.Count < 1)
                    klib.Shell.WriteLine(R.Project.ID, LOG, $"Trying to connect {ConnString.DataSource}.{ConnString.InitialCatalog}");


                Cnn = new SqlConnection(connString);                
                Cnn.Open();

                DoQuery("SELECT @@VERSION as 'Version'");
                if (Next())
                    Version = Field(0).OnlyNumbers().ToInt();

                if (ConnStrings.Count < 1)
                    klib.Shell.WriteLine(R.Project.ID, LOG, $"SQL Server version {Version}");

                if (!ConnStrings.Where(t => t.ConnectionString == connString).Any())
                    ConnStrings.Add(ConnString);
                             
            } catch (Exception ex)
            {
                klib.Shell.WriteLine(R.Project.ID, LOG, ex);
                throw new LException(1, ex.Message);
            }
            finally
            {
            }
        }

        public int CountColumns()
        {
            return DataReader.FieldCount;
        }

        public int CountRows()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();
            if (Cnn.State == System.Data.ConnectionState.Open)
                Cnn.Close();
        }

        public dynamic Do(dynamic alias, params object[] values)
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            throw new NotImplementedException();
        }

        public int DoQuery(string sql, params object[] values)
        {

            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            sql = String.Format( ValuesEx.NS(sql),FixValues(values));
            LastCommand = sql;
            var command = new SqlCommand(sql, Cnn);
            DataReader = command.ExecuteReader();

            if(DataReader.HasRows)
            {
                FirstLine = true;
                DataReader.Read();
                return 1;                
            }
            else
                return DataReader.HasRows ? 1 : 0;
        }

        public int DoQueryManager(string qname, params object[] values)
        {
            var sql = @"SELECT QString FROM OUQR WHERE Qname = {0}";
            var qstring = String.Empty;

            using (var rs = new DbClient())
            {
                rs.DoQuery(sql, qname);

                if (rs.Next())
                    qstring = rs.Field(0).ToString();
                else
                    throw new LException(6, qname);
            }

            var rgx = new Regex(@"\[\%[0-9]\]");
            var foo = rgx.Matches(qstring);

            if (foo.Count != values.Length)
                throw new LException(2, $"Quantity of paramters in {qname} isn't valid ({foo.Count}/{values.Length})");

            for (int i = 0; i < foo.Count; i++)
                qstring = qstring.Replace($"[%{i}]", "{" + i + "}");


            return DoQuery(qstring, values);
        }

        public void First()
        {
            throw new NotImplementedException();
        }

        public bool Next()
        {
            if (FirstLine)
            {
                FirstLine = false;
                return true;
            }

            if (DataReader.Read())
                return true;
            else
            {
                DataReader.Close();
                return false;
            }
        }

        public void Last()
        {
            throw new NotImplementedException();
        }

        public T Load<T>()
        {
            throw new NotImplementedException();
        }

        public bool NoQuery(string sql, params object[] values)
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            sql = String.Format(ValuesEx.NS(sql), FixValues(values));

            var command = new SqlCommand(sql, Cnn);           
            return command.ExecuteNonQuery() > 0;
        }

        public Values Field(object index)
        {
            if (index.GetType().Name.ToLower() == "int32")
                return ValuesEx.To(DataReader.GetValue((int)index));
            else
                return ValuesEx.To(DataReader[index.ToString()]);
            
        }

        public Dictionary<string, Values> Fields(bool upper = true)
        {
            //0001
            var line = new Dictionary<string, Values>();
            var md5 = string.Empty;
            for (int i = 0; i < DataReader.FieldCount; i++)
            {               

                if (upper)
                {
                    var foo = ValuesEx.To(DataReader.GetValue(i));
                    line.Add(DataReader.GetName(i).ToUpper(), foo);
                    md5 += foo.ToString();
                }
                else
                {
                    var foo = ValuesEx.To(DataReader.GetValue(i));
                    line.Add(DataReader.GetName(i), foo);
                    md5 += foo.ToString();
                }
            }

            if (!String.IsNullOrEmpty(md5))
                md5 = klib.Shell.Md5Hash(md5);

            line.Add("MD5Hash", ValuesEx.To(md5));

            return line;
        }


        public void Close()
        {
            Dispose();
        }

        public object[] FixValues(object[] values, bool manipulation = false)
        {
            //00001
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

                values[i] = ValuesEx.NS(values[i].ToString());
                values[i] = $"{open}{values[i].ToString().Replace("'", descr)}{open}";
            }


            return values;
        }
    }

    public static class DbClientEx
    {


        public static List<string> GetQueriesManager(string queryManagerPreFix)
        {
            var sql = $"SELECT QName FROM OUQR WHERE Qname LIKE '{queryManagerPreFix}%'";
            return Column<string>(sql);
        }

        public static bool IsQueryManager(string qname)
        {
            var sql = "SELECT QName FROM OUQR WHERE Qname = {0}";
            using (var rs = new DbClient())
                return rs.DoQuery(sql, qname) > 0;
        }

        [Obsolete]
        public static DbClient QueryManager(string qname, params object[] values)
        {
            var sql = @"SELECT QString FROM OUQR WHERE Qname = {0}";
            var qstring = String.Empty;

            using (var rs = new DbClient())
            {
                rs.DoQuery(sql, qname);

                if (rs.Next())
                    qstring = rs.Field(0).ToString();
                else
                    throw new LException(6, qname);
            }

            var rgx = new Regex(@"\[\%[0-9]\]");
            var foo = rgx.Matches(qstring);

            if (foo.Count != values.Length)
                throw new LException(2, $"Quantity of paramters in {qname} isn't valid ({foo.Count}/{values.Length})");

            for (int i = 0; i < foo.Count; i++)
                qstring = qstring.Replace($"[%{i}]", "{" + i + "}");


            var rs1 = new DbClient();
            rs1.DoQuery(sql, values);

            return rs1;

        }

        public static DbClient QueryManager(int code, int id, params object[] values)
        {
            throw new NotImplementedException();
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
        public static int IsExists(string table, string col, object val1, bool onlyString = false, bool like = false)
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
                return cnn.DoQuery(sql);
            }
        }

        public static Dictionary<string, Values> Top1(string sql, params object[] values)
        {
            var res = new Dictionary<string, Values>();
            using (var rs = new DbClient())
            {
                if (rs.DoQuery(sql, values) > 0)
                {
                    res = rs.Fields();
                }
            }

            return res;
        }

        /// <summary>
        /// Return all column values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<T> Column<T>(string sql, params object[] values)
        {
            var res = new List<T>();
            using (var rs = new DbClient())
            {
                rs.DoQuery(sql, values);
                while(rs.Next())
                    res.Add(rs.Field(0).Dynamic());
            }

            return res;
        }

        public static Values First(string sql, params object[] values)
        {
            using (var rs = new DbClient())
            {
                rs.DoQuery(sql, values);
                if ( rs.Next())
                    return rs.Field(0);
                else
                    return klib.ValuesEx.Empty;
            }
        }

        #region Parameters
        public static Values Parameter(int code, string parameter, string diff1, string diff2, string diff3, object def)
        {
            try
            {
                CreateParameter(code, parameter, diff1, diff2, diff3, def);
                return Parameter(code, parameter, diff1, diff2, diff3);
            }
            catch (Exception ex)
            {
                klib.Shell.WriteLine(R.Project.ID, ex.Message);
                return klib.ValuesEx.To(def);
            }
        }

        public static Values Parameter(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            CreateParameter(code, parameter, diff1, diff2, diff3);

            var sql = $"{klib.R.Project.SP_PARAM} '{code}', '{parameter}', '{diff1}', '{diff2}', '{diff3}'";

            using (var cnn = new DbClient())
            {
                cnn.DoQuery(sql);
                var result = klib.ValuesEx.Empty;
                if (cnn.Next())
                    result = cnn.Fields()["VALUE"];

                if (result.IsEmpty)
                    throw new LException(5, code, parameter);
                else
                    return result;
            }
        }

        public static List<klib.model.Select> ParameterList(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            //0001
            var sql = $"{klib.R.Project.SP_PARAM} {code}, '{parameter}', '{diff1}', '{diff2}'";
            var select = new List<klib.model.Select>();

            using (var cnn = new DbClient())
            {
                cnn.DoQuery(sql);
                var result = klib.ValuesEx.Empty;
                if (cnn.Next())
                {
                    var value = cnn.Field("VALUE").ToString();
                    var id = String.Empty;
                    if (diff3 != null)
                        id = cnn.Field("DIFF3").ToString();
                    if (diff2 != null)
                        id = cnn.Field("DIFF2").ToString();
                    else if (diff1 != null)
                        id = cnn.Field("DIFF1").ToString();
                    else
                        id = cnn.Field("PARAM").ToString();

                    select.Add(new klib.model.Select(id, value));
                }


                return select;
            }
        }

        public static void CreateParameter(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null, object def = null, DateTime? dueDate = null)
        {
            //00001
            var exists = false;

            exists = DbClientEx.IsExists("@!!_PARAM0", "Code", code) > 0;

            if (!exists)
            {
                using (var rs = new DbClient())
                {
                    var docEntry = DbClientEx.First("SELECT MAX(DocEntry) as LastDEntry FROM [@!!_PARAM0]").ToInt() + 1;
                    var sql = "INSERT INTO [@!!_PARAM0] (Code, DocEntry, Name, U_PROJECT) VALUES ({0},{1},{2},{3})";
                    rs.NoQuery(sql, code, docEntry, code, code);
                }
            }

            using (var rs = new DbClient())
            {
                var sql = $@"
SELECT   1 
FROM     [@!!_PARAM1] 
WHERE    Code = '{code}'
    AND  UPPER(U_Param) = UPPER('{parameter}')
    AND  ISNULL(U_DIFF1,'') = '{diff1}'
    AND  ISNULL(U_DIFF2,'') = '{diff2}'
    AND  ISNULL(U_DIFF3,'') = '{diff3}'
";

                exists = rs.DoQuery(sql) > 0;

                if (!exists)
                {
                    klib.Shell.WriteLine(R.Project.ID, $"DBCLI : Preparing the {parameter} {diff1}/{diff2}/{diff3} param");
                    var lastLine = DbClientEx.First("SELECT MAX(LineId) FROM [@!!_PARAM1] WHERE Code = {0}", code).ToInt();

                    sql = @"
INSERT INTO [@!!_PARAM1] (Code, LineId, Object, U_PARAM, U_DIFF1, U_DIFF2, U_DIFF3, U_ORDER, U_VALUE)
VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8})

";
                    rs.NoQuery(sql, code, lastLine + 1, $"{klib.R.Company.NS}_PARAM", parameter, diff1, diff2, diff3, 0, def);
                }

            }


        }

        public static void CreateParameter(Type p)
        {
            foreach (var proper in p.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var name = proper.Name;
                var foo = proper.GetValue(null, null);

            }
        }
        #endregion
    }
}
