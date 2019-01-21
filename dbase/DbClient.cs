using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using klib.model;

namespace klib.dbase
{
    public class DbClient : implement.IDbClient
    {
        private static String defServer;
        public static String DefServer
        {
            get
            {
                if (String.IsNullOrEmpty(defServer))
                    throw new LException(1, "Server default wasn't define in DbClient");
                else
                    return defServer;
            }
            set
            {
                defServer = value;
            }
        }
        public static List<model.Select> ConnStrings { get; protected set; } = new List<model.Select>();
        private SqlConnection Cnn;
        private SqlDataReader DataReader;

        public DbClient(string connString = null)
        {

            if (String.IsNullOrEmpty(connString))
                connString = ConnStrings.Where(t => t.Name == defServer).Select(t => t.Value.ToString()).FirstOrDefault();
            
            Cnn = new SqlConnection(connString);
            
            try
            {
                Cnn.Open();
                if(!ConnStrings.Where(t => t.Name == Cnn.DataSource).Any())
                    ConnStrings.Add(new model.Select(Cnn.DataSource, ValuesEx.To(connString)));

                if (ConnStrings.Count == 1 && string.IsNullOrEmpty(defServer))
                    defServer = Cnn.DataSource;

            } catch (Exception ex)
            {
                throw new LException(1, ex.Message);
                Cnn.Close();
            }
            finally
            {
                //Cnn.Close();
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
            throw new NotImplementedException();
        }

        public int DoQuery(string sql, params object[] values)
        {

            sql = String.Format(sql, values);

            var command = new SqlCommand(sql, Cnn);
            DataReader = command.ExecuteReader();

            return DataReader.HasRows ? 1 : 0;
        }

        public void Fist()
        {
            throw new NotImplementedException();
        }

        public bool Next()
        {
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
            for(int i = 0; i < values.Length; i++)
                values[i] = values[i].ToString().Replace("'", "''");

            sql = String.Format(sql, values);
            var command = new SqlCommand(sql, Cnn);           
            return command.ExecuteNonQuery() > 0;
        }

        public Values Field(object index)
        {
            if (index.GetType().Name.ToLower() == "int")
                return ValuesEx.To(DataReader.GetValue((int)index));
            else
                return ValuesEx.To(DataReader[index.ToString()]);
            
        }

        public Dictionary<string, Values> Fields(bool upper = true)
        {
            var line = new Dictionary<string, Values>();
            for (int i = 0; i < DataReader.FieldCount; i++)
            {
                if(upper)
                    line.Add(DataReader.GetName(i).ToUpper(), ValuesEx.To(DataReader.GetValue(i)));
                else
                    line.Add(DataReader.GetName(i), ValuesEx.To(DataReader.GetValue(i)));
            }

            return line;
        }

        public void Close()
        {
            Dispose();
        }

        public object[] FixValues(object[] values, bool manipulation = false)
        {
            throw new NotImplementedException();
        }
    }

    public static class DbClientEx
    {
        public static Values Parameter(int code, string parameter, string diff1, string diff2, string diff3, object def)
        {
            try
            {
                return Parameter(code, parameter, diff1, diff2, diff3);
            } catch
            {
                return ValuesEx.To(def);
            }
        }

        public static Values Parameter(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            var sql = $"{R.Project.SP_PARAM} {code}, '{parameter}', '{diff1}', '{diff2}', '{diff3}'";

            using (var cnn = new DbClient())
            {
                cnn.DoQuery(sql);
                var result = ValuesEx.Empty;
                if (cnn.Next())
                    result = cnn.Fields()["VALUE"];

                if (result.IsEmpty)
                    throw new LException(4, code, parameter);
                else
                    return result;
            }
        }

    }
}
