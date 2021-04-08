using System;
using System.Reflection;
using AT_Utils;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("Hangar")]
[assembly: AssemblyDescription("Plugin for the Kerbal Space Program")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Allis Tauri")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.


#if NIGHTBUILD
[assembly: AssemblyVersion("3.6.*")]
#else
[assembly: AssemblyVersion("3.6.2")]
#endif
[assembly: KSPAssembly("Hangar", 3, 6)]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]


namespace AtHangar
{
    public class ModInfo : KSP_AVC_Info
    {
        public ModInfo()
        {
            MinKSPVersion = new Version(1, 11, 1);
            MaxKSPVersion = new Version(1, 11, 1);

            VersionURL = "https://raw.githubusercontent.com/allista/hangar/master/GameData/Hangar/Hangar.version";
            UpgradeURL = "https://spacedock.info/mod/1000/Hangar";
            ChangeLogURL = "https://github.com/allista/hangar/blob/master/ChangeLog.md";
        }
    }
}
