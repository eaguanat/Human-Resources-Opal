using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Human_Resources
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Puedes agregar una propiedad estática para almacenar el ID del usuario loggeado si lo necesitas globalmente
        public static int LoggedInUserId { get; private set; }
        public static List<int> LoggedInUserAccesses { get; private set; }

        public App()
        {
            // Opcional: Para manejar la información del usuario en toda la aplicación
            // Esto es si no quieres pasar el userId a cada ventana, pero no lo usaremos en este ejemplo directo.
        }

        // Sobrescribe el método OnStartup para mostrar la ventana de login primero
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Forms.frmLogin loginWindow = new Forms.frmLogin();
            loginWindow.Show();
        }
    }
}