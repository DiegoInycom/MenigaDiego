using System.Diagnostics;

namespace Ibercaja.Aggregation
{
    public class AssemblyVersionLogContextProperty
    {
        private static string _assemblyVersion;

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_assemblyVersion))
            {
                _assemblyVersion = GetAssemblyVersion();
            }

            return _assemblyVersion;
        }

        private string GetAssemblyVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}
