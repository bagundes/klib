using klib.model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace klib.core
{
    internal static class ReportSystem
    {
        public static void Send(model.Report report)
        {
            //var client = new MongoClient(R.Project.MongoDBK);
            //var dbase = client.GetDatabase(report.CID);
            //var newcol = dbase.GetCollection<model.Report>("reports");

            //report.Computer = ValuesEx.To(Machine()).ToJson();
            //newcol.InsertOne(report);
        }

        public static void Save(model.Report report)
        {

        }

        private static model.Win32_BIOS Machine()
        {
            var consulta = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            ManagementObjectCollection bios = consulta.Get();

            var item = new Win32_BIOS();
            foreach (ManagementObject obj in bios)
            {
                
                item.BiosCharacteristics = (ushort[])obj["BiosCharacteristics"];
                item.BIOSVersion = (string[])obj["BIOSVersion"];
                item.BuildNumber = (string)obj["BuildNumber"];
                item.Caption = (string)obj["Caption"];
                item.CodeSet = (string)obj["CodeSet"];
                item.CurrentLanguage = (string)obj["CurrentLanguage"];
                item.Description = (string)obj["Description"];
                item.IdentificationCode = (string)obj["IdentificationCode"];
                item.InstallableLanguages = (ushort?)obj["InstallableLanguages"];
                item.InstallDate = (DateTime?)obj["InstallDate"];
                item.LanguageEdition = (string)obj["LanguageEdition"];
                item.ListOfLanguages = (string[])obj["ListOfLanguages"];
                item.Manufacturer = (string)obj["Manufacturer"];
                item.Name = (string)obj["Name"];
                item.OtherTargetOS = (string)obj["OtherTargetOS"];
                item.PrimaryBIOS = (bool?)obj["PrimaryBIOS"];
                item.ReleaseDate = (string)obj["ReleaseDate"];
                item.SerialNumber = (string)obj["SerialNumber"];
                item.SMBIOSBIOSVersion = (string)obj["SMBIOSBIOSVersion"];
                item.SMBIOSMajorVersion = (ushort?)obj["SMBIOSMajorVersion"];
                item.SMBIOSMinorVersion = (ushort?)obj["SMBIOSMinorVersion"];
                item.SMBIOSPresent = (bool?)obj["SMBIOSPresent"];
                item.SoftwareElementID = (string)obj["SoftwareElementID"];
                item.SoftwareElementState = (ushort?)obj["SoftwareElementState"];
                item.Status = (string)obj["Status"];
                item.TargetOperatingSystem = (ushort?)obj["TargetOperatingSystem"];
                item.Version = (string)obj["Version"];
            }

            return item;
        }
    }
}
