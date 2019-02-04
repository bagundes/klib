using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    public static class FormatTo
    {
        #region CSV file
        public static string CSV(string delimeted, string escape, object[] values)
        {
            var vals = new Values[values.Length];

            for(int i = 0; i < vals.Length; i++)            
                vals[i] = ValuesEx.To(values[i]);

            return CSV(delimeted, escape, vals);
        }

        /// <summary>
        /// Create the csv line. 
        /// </summary>
        /// <param name="delimeted"></param>
        /// <param name="escape"></param>
        /// <param name="values">You can force the value to type the string, you just need to add the value of the string in comments.</param>
        /// <returns></returns>
        public static string CSV(string delimeted, string escape, Values[] values)
        {
            var line = new List<string>();

            foreach(var value in values)
            {
                var forceString = value.Comments.Contains("STRING");

                if (forceString || !value.IsNumber())
                {
                    var val = value.ToString();

                    if (val.Contains(escape))
                        val = val.Replace(escape, $"{escape}{escape}");

                    line.Add($"{escape}{val}{escape}");
                }
                else
                    line.Add(value.ToDecimal().ToString());
            }

            return String.Join(delimeted,line.ToArray());
        }
        #endregion

    }
}
