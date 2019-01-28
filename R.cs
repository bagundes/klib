using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    public static class R
    {
        public static class Project
        {
            public static string MongoDBK => "mongodb+srv://kurumin:SX8S4p5p2QqVK7xK@kurumin-qa2p6.mongodb.net/test?retryWrites=true";
            public static System.Reflection.Assembly assembly => System.Reflection.Assembly.GetExecutingAssembly();
            public static Version Version => assembly.GetName().Version;
            public const string Namespace = "KK";
            public static int ID => Version.Major;
            public const string SP_PARAM = "KK_PARAM";
            public static string[] Resources => assembly.GetManifestResourceNames();
            public static ResourceManager LocationResx => new ResourceManager(Resources.Where(t => t.Contains("content.Location")).FirstOrDefault(), assembly);
        }

        internal static class Security
        {
            public static String MasterKey { get; set; } = "ace765acd5493cacb4e918fa848266b3";
        }
        

        public static class Company
        {
            public static String Name => "Teamsoft Limited";
            public static String AliasName => "Teamsoft";
            public static String NS => "TS";
        }
    }
}
