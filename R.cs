using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    public static class R
    {
        public static class Project
        {
            public static System.Reflection.Assembly assembly => System.Reflection.Assembly.GetExecutingAssembly();
            public static Version Version => assembly.GetName().Version;
            public const string Namespace = "KK";
            public static int ID => Version.Major;
            public const string SP_PARAM = "KK_PARAM";
            public static string[] Resources => assembly.GetManifestResourceNames();
        }

        internal static class Security
        {
            public static String MasterKey { get; set; } = "ace765acd5493cacb4e918fa848266b3";
        }
        

        public static class Company
        {
            public static String Name { get; } = "Teamsoft Limited";
            public static String AliasName { get; } = "Teamsoft";
            public static String NS { get; } = "TS";
        }
    }
}
