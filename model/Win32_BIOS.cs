using System;

namespace klib.model
{
    internal class Win32_BIOS
    {
        public Win32_BIOS()
        {
        }

        public ushort? TargetOperatingSystem { get; internal set; }
        public ushort[] BiosCharacteristics { get; internal set; }
        public string[] BIOSVersion { get; internal set; }
        public string BuildNumber { get; internal set; }
        public string Caption { get; internal set; }
        public string CodeSet { get; internal set; }
        public string CurrentLanguage { get; internal set; }
        public string Description { get; internal set; }
        public string IdentificationCode { get; internal set; }
        public ushort? InstallableLanguages { get; internal set; }
        public DateTime? InstallDate { get; internal set; }
        public string LanguageEdition { get; internal set; }
        public string[] ListOfLanguages { get; internal set; }
        public string Manufacturer { get; internal set; }
        public string Name { get; internal set; }
        public string OtherTargetOS { get; internal set; }
        public bool? PrimaryBIOS { get; internal set; }
        public string ReleaseDate { get; internal set; }
        public string SerialNumber { get; internal set; }
        public string SMBIOSBIOSVersion { get; internal set; }
        public ushort? SMBIOSMajorVersion { get; internal set; }
        public ushort? SMBIOSMinorVersion { get; internal set; }
        public bool? SMBIOSPresent { get; internal set; }
        public string SoftwareElementID { get; internal set; }
        public ushort? SoftwareElementState { get; internal set; }
        public string Status { get; internal set; }
        public string Version { get; internal set; }
    }
}