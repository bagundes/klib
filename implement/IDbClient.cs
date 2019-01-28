using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.implement
{
    public interface IDbClient : System.IDisposable
    {
        bool IsFirstLine { get;}
        dynamic Do(dynamic alias, params object[] values);
        int DoQuery(string sql, params object[] values);
        int DoQueryManager(string qname, params object[] values);
        bool NoQuery(string sql, params object[] values);
        object[] FixValues(object[] values, bool manipulation = false);
        /// <summary>
        /// Transform the columns name in upper.
        /// </summary>
        /// <param name="upper"></param>
        /// <returns></returns>
        Dictionary<string, Values> Fields(bool upper = true);
        Values Field(object index);
        bool Next();
        int CountRows();
        int CountColumns();
        T Load<T>();
        void First();
        void Last();
    }
}
