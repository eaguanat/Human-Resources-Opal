using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
// IMPORTANTE: Necesitamos estos dos para la cultura
using System.Globalization;
using System.Threading;

namespace Human_Resources
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int LoggedInUserId { get; private set; }
        public static List<int> LoggedInUserAccesses { get; private set; }

        public App()
        {
            // Constructor vacío
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // --- CAMBIO QUIRÚRGICO: FORZAR PUNTO DECIMAL ---
            // Esto asegura que en cualquier PC (aunque esté en español) 
            // los decimales usen punto (.) para Azure SQL.
            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            // Para que WPF también use el formato en los controles visuales (fechas, monedas)
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(ci.IetfLanguageTag)
                )
            );
            // -----------------------------------------------

            Forms.frmLogin loginWindow = new Forms.frmLogin();
            loginWindow.Show();
        }
    }
}