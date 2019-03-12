using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace klib.DB
{
    /// <summary>
    /// Class to load the parameters saved in database.
    /// </summary>
    public static class Parameter
    {
        private static string LOG => "PARAM";
        
        public static DicDymanic Get(model.Parameter parameters)
        {
            
            var sql = $"{klib.R.Project.SP_PARAM} '{parameters.Code}', '{parameters.Param}', '{parameters.Diff1}', '{parameters.Diff2}', '{parameters.Diff3}'";

            var values = ExtensionDb.Top1(sql);
            if (values == null)
            {
                Create(parameters);
                values = Get(parameters);
            }

            return values;

        }

        private static void Create(model.Parameter parameters)
        {
            #region Register the code
            var sql = $"SELECT TOP 1 1 FROM [@!!_PARAM0] WHERE Code = {parameters.Code}";

            if(!ExtensionDb.Exists("@!!_PARAM0","Code", parameters.Code))
            {
                var docentry = ExtensionDb.First("SELECT MAX(DocEntry) as DocEntry FROM [@!!_PARAM0]").ToInt();
                ExtensionDb.Execute("INSERT INTO [@!!_PARAM0] (Code, DocEntry, Name, U_PROJECT) VALUES ({0},{1},{2},{3})", parameters.Code, docentry + 1, parameters.Code, parameters.Code);
            }

            #endregion

            #region Register the parameter

            klib.Shell.WriteLine(R.Project.ID,LOG, $"Klib - Creating the {parameters.Param} {parameters.Diff1}/{parameters.Diff2}/{parameters.Diff3} parameter");
            var lineid = ExtensionDb.First($"SELECT MAX(LineId) FROM [@!!_PARAM1] WHERE Code = {parameters.Code}").ToInt();           

                
                    sql = @"
INSERT INTO [@!!_PARAM1] (Code, LineId, Object, U_PARAM, U_DIFF1, U_DIFF2, U_DIFF3, U_ORDER, U_VALUE)
VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8})

";
            ExtensionDb.Execute(sql, parameters.Code,lineid + 1, $"{klib.R.Company.NS}_PARAM", parameters.Param, parameters.Diff1, parameters.Diff2, parameters.Diff3, parameters.Order, parameters.Value);                
            #endregion
        }

        public static List<Dynamic> List(model.Parameter parameters)
        {
            throw new NotImplementedException();
        }

        public static void LoadP(Type p)
        {
            foreach (var proper in p.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var name = proper.Name;
                var foo = proper.GetValue(null, null);

            }
        }
    }
}
