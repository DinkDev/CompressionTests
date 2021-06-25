namespace CompressionTests
{
    internal class SettingsStub
    {
        public double BundleTimeLimit => 300.0;
        public bool DoSchemaValidation => true;
        public string OutputDirectory => @"OutputDirectory";
        public string PackageFileExtension => @".ZIP";
        public int PollInterval => 0;
        public string WorkingDirectory => @"WorkingDirectory";
        public string TemporaryDirectory => @"TemporaryDirectory";
        public string BundleFileExtension => @".TAR";
        public bool SaveOutputFiles => true;
    }
}