namespace UpdateCore
{
    public class PackageInfo
    {
        public string PackageName { get; set; }
        public string PackageLocation { get; set; }
        public string Destination { get; set; }
        public string Version { get; set; }
        public bool WriteVersion { get; set; }
        public bool ClearDirectory { get; set; }
        public bool RecursiveClearDirectory { get; set; }
    }
}
