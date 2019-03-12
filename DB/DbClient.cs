using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using klib.Implement;
using klib.model;

namespace klib.DB
{
    public class DbClient : DBFix, IDBClient
    {
        private const string LOG = "DBCLI";
        public static List<SqlConnectionStringBuilder> StringConns { get; internal set; } = new List<SqlConnectionStringBuilder>();
        private readonly SqlConnection cnn;
        private SqlConnection Cnn
        {
            get
            {
                switch (cnn.State)
                {
                    case System.Data.ConnectionState.Closed:
                        cnn.Open();
                        return cnn;
                    case System.Data.ConnectionState.Executing:
                        var secs = 0;
                        do
                        {
                            System.Threading.Thread.Sleep(1000);
                            if (secs++ > 25)
                            {
                                var tempFile = klib.Shell.CreateTmpFile("sql");
                                klib.Shell.CreateFile(LastCommand, tempFile);
                                klib.Shell.WriteLine(R.Project.ID, LOG, $"KLib - The process is slow to execute. {tempFile.FullName}");
                            }
                        } while (cnn.State == System.Data.ConnectionState.Executing);
                        return cnn;
                    default:
                        return cnn;
                }
            }
        }
        private SqlDataReader DataReader;
        public readonly SqlConnectionStringBuilder StringConn;

        public DbClient(string string_conn = null)
        {

            
            if (String.IsNullOrEmpty(string_conn) && StringConns.Count > 0)
                StringConn = StringConns[0];
            else
            {
                StringConn = new SqlConnectionStringBuilder(string_conn);
                if (!String.IsNullOrEmpty(StringConn.Password))
                    StringConn.Password = (new klib.model.Credentials1(StringConn.UserID, StringConn.Password)).Passwd;
            }

            try
            {

                klib.Shell.WriteLine(R.Project.ID, LOG, $"Klib - Trying to connect {StringConn.DataSource}.{StringConn.InitialCatalog}");

                cnn = new SqlConnection(StringConn.ToString());
                cnn.Open();

                if (!StringConns.Where(t => t.ConnectionString == StringConn.ConnectionString).Any())
                    StringConns.Add(StringConn);

            }
            catch (Exception ex)
            {
                klib.Shell.WriteLine(R.Project.ID, LOG, ex);
                throw new KLIBException(1, ex.Message);
            }
            finally
            {
            }
        }

        public string LastCommand { get; internal set; }

        public bool IsFirstLine { get; internal set; }
        public int TotalRows => throw new NotImplementedException();

        public int TotalColumns => DataReader.FieldCount;


        public void Dispose()
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();
            if (cnn.State == System.Data.ConnectionState.Open)
                cnn.Close();
        }

        public int DoQuery(string sql, params object[] values)
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            sql = String.Format(Tags(sql), FixValues(values));
            LastCommand = sql;
            var command = new SqlCommand(sql, Cnn);
            DataReader = command.ExecuteReader();

            if (DataReader.HasRows)
            {
                IsFirstLine = true;
                DataReader.Read();
                return 1;
            }
            else
                return DataReader.HasRows ? 1 : 0;
        }

        public Dynamic Field(object index)
        {
            if (index.GetType().Name.ToLower().StartsWith("int"))
                return new Dynamic(DataReader.GetValue((int)index), DataReader.GetName((int)index));
            else
                return new Dynamic(DataReader[index.ToString()], index.ToString());
        }

        public DicDymanic Fields(bool upper_columns = true)
        {
            var values = new DicDymanic();
            for (int i = 0; i < TotalColumns; i++)
                values.Add(Field(i));

            return values;
        }

        public IEnumerable<T> Fields<T>() where T : BaseModel, new()
        {
            throw new NotImplementedException();
        }

        public void First()
        {
            throw new NotImplementedException();
        }

        public void Last()
        {
            throw new NotImplementedException();
        }

        public bool Next()
        {
            if (IsFirstLine)
            {
                IsFirstLine = false;
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

        public bool NoQuery(string sql, params object[] values)
        {
            if (DataReader != null && !DataReader.IsClosed)
                DataReader.Close();

            sql = String.Format(this.Tags(sql), FixValues(values));

            var command = new SqlCommand(sql, Cnn);
            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Execute the query saved in SAP Query Manager
        /// </summary>
        /// <param name="qname"></param>
        /// <param name="values"></param>
        /// <returns></returns>
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
                    throw new KLIBException(6, qname);
            }

            var rgx = new Regex(@"\[\%[0-9]\]");
            var foo = rgx.Matches(qstring);

            if (foo.Count != values.Length)
                throw new KLIBException(2, $"Quantity of paramters in {qname} isn't valid ({foo.Count}/{values.Length})");

            for (int i = 0; i < foo.Count; i++)
                qstring = qstring.Replace($"[%{i}]", "{" + i + "}");


            return DoQuery(qstring, values);
        }

        public int Version()
        {
            using (var conn = new DbClient())
            {
                conn.DoQuery("SELECT CAST(SUBSTRING(@@VERSION, CHARINDEX('-', @@VERSION,0) + 2, CHARINDEX('.', @@VERSION,0) - CHARINDEX('-', @@VERSION,0) - 2) as int)");
                return conn.Field(0).ToInt();
            }

        }
    }

    /*
        public static class DbClientEx
    {

        
        #region Parameters
        public static Dynamic Parameter(int code, string parameter, string diff1, string diff2, string diff3, object def)
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

        public static Dynamic Parameter(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            CreateParameter(code, parameter, diff1, diff2, diff3);

            var sql = $"{klib.R.Project.SP_PARAM} '{code}', '{parameter}', '{diff1}', '{diff2}', '{diff3}'";

            using (var cnn = new DbClient())
            {
                cnn.DoQuery(sql);
                var result = klib.Dynamic.Empty;
                if (cnn.Next())
                    result = cnn.Fields()["VALUE"];

                if (result.IsEmpty)
                    throw new KLIBException(5, code, parameter);
                else
                    return result;
            }
        }

        /// <summary>
        /// Return list of values in Parameters
        /// </summary>
        /// <param name="code">Code of project</param>
        /// <param name="parameter">Parameter name</param>
        /// <param name="diff1"></param>
        /// <param name="diff2"></param>
        /// <param name="diff3"></param>
        /// <param name="dd">Diff default to name (Values.Name).
        /// -1 : default
        ///  0 : Parameter
        ///  1 : Diff1
        ///  2 : Diff2
        ///  3 : Diff3
        /// </param>
        /// <returns></returns>
        public static List<Dynamic> ParameterList1(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null, int dd = -1)
        {            
            var sql = $"{R.Project.SP_PARAM} {code}, '{parameter}', '{diff1}', '{diff2}', '{diff3}'";
            var select = new List<Dynamic>();

            using (var cnn = new DbClient())
            {
                cnn.DoQuery(sql);
                var result = klib.Dynamic.Empty;
                if (cnn.Next())
                {
                    var value = cnn.Field("VALUE").ToString();
                    var id = String.Empty;
                    if (dd > -1)
                    {
                        switch(dd)
                        {
                            case 0: id = cnn.Field("PARAM").ToString(); break;
                            case 1: id = cnn.Field("DIFF1").ToString(); break;
                            case 2: id = cnn.Field("DIFF2").ToString(); break;
                            case 3: id = cnn.Field("DIFF3").ToString(); break;
                            default: throw new KLIBException(1, "Invalid value of parameter dd");
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(diff3))
                            id = cnn.Field("DIFF3").ToString();
                        else if (!String.IsNullOrEmpty(diff2))
                            id = cnn.Field("DIFF2").ToString();
                        else if (!String.IsNullOrEmpty(diff1))
                            id = cnn.Field("DIFF1").ToString();
                        else
                            id = cnn.Field("PARAM").ToString();
                    }
                    select.Add(new klib.Dynamic(value, id));
                }


                return select;
            }
        }

        [Obsolete("190218- Use the ParameterList1")]
        public static List<klib.model.Select> ParameterList(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            //0001
            var sql = $"{klib.R.Project.SP_PARAM} {code}, '{parameter}', '{diff1}', '{diff2}'";
            var select = new List<klib.model.Select>();

            using (var cnn = new DbClient())
            {
                cnn.DoQuery(sql);
                var result = klib.Dynamic.Empty;
                if (cnn.Next())
                {
                    var value = cnn.Field("VALUE").ToString();
                    var id = String.Empty;
                    if (!String.IsNullOrEmpty(diff3))
                        id = cnn.Field("DIFF3").ToString();
                    else if (!String.IsNullOrEmpty(diff2))
                        id = cnn.Field("DIFF2").ToString();
                    else if (!String.IsNullOrEmpty(diff1))
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

            exists = klib.DB.ExtensionDb.Exists("@!!_PARAM0", "Code", code);

            if (!exists)
            {
                using (var rs = new DbClient())
                {
                    var docEntry = ExtensionDb.First("SELECT MAX(DocEntry) as LastDEntry FROM [@!!_PARAM0]").ToInt() + 1;
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
                    klib.Shell.WriteLine(R.Project.ID, LOG, $"Klib - Preparing the {parameter} {diff1}/{diff2}/{diff3} param");
                    var lastLine = ExtensionDb.First("SELECT MAX(LineId) FROM [@!!_PARAM1] WHERE Code = {0}", code).ToInt();

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
    }*/
}
