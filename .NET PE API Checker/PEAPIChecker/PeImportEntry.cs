namespace PEEXEAPIChecker
{
    internal sealed class PeImportEntry
    {
        public PeImportEntry(string dllName, string functionName)
        {
            DllName = dllName;
            FunctionName = functionName;
        }

        public string DllName { get; private set; }

        public string FunctionName { get; private set; }

        public string ImportName
        {
            get { return string.Format("{0}!{1}", DllName, FunctionName); }
        }
    }
}
