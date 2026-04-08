using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Identidad renovada para despistar al instalador de Windows
[assembly: AssemblyTitle("HR_Dev_Local_System")]
[assembly: AssemblyDescription("Desarrollo Local Human Resources")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Desarrollo Interno")]
[assembly: AssemblyProduct("HR_Workstation_Local")]
[assembly: AssemblyCopyright("Copyright © 2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

// ESTA LÍNEA ES LA MÁS IMPORTANTE: Es el nuevo "DNI" del programa. 
// He cambiado los valores para que Windows NO lo asocie con la versión de Drive.
[assembly: Guid("7a2b9c1d-3e4f-5a6b-7c8d-9e0f1a2b3c4d")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]

// Reiniciamos la versión a algo diferente por si acaso
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]