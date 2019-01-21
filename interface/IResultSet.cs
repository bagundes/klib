using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.Interface
{
    public interface IResultSet
    {
        int DoQuery(string sql, params object[] values);
        bool NoQuery(string sql, params object[] values);
        
    }
}
