using Human_Resources.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Human_Resources.Forms
{
    public partial class frmLogin : Window
    {
        public frmLogin()
        {
            InitializeComponent();

            try
            {
                // Preguntamos: ¿Esta app fue instalada por ClickOnce?
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    var version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    lblVersion.Text = $"Versión: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                else
                {
                    // Si estamos en Visual Studio, ponemos este texto
                    lblVersion.Text = "Development Mode (Local)";
                }
            }
            catch (Exception)
            {
                // Si algo falla al leer la versión, que no se cierre la app
                lblVersion.Text = "Versión: N/A";
            }
        }

        private async void FrmLogin_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Estado inicial: Mostrar carga
            LoadingPanel.Visibility = Visibility.Visible;
            LoginFieldsPanel.Visibility = Visibility.Collapsed;

            string pcID = GetMotherboardID();

            // 2. Validación asíncrona contra Azure
            bool esValido = await Task.Run(() => ValidarEquipo(pcID));

            // 3. Intercambio de paneles
            LoadingPanel.Visibility = Visibility.Collapsed;
            LoginFieldsPanel.Visibility = Visibility.Visible;

            if (!esValido)
            {
                txtError.Visibility = Visibility.Visible;
                txtError.Text = "EQUIPO NO AUTORIZADO (Acceso Restringido)\nID: " + pcID;

                txtError.Cursor = Cursors.Hand;
                txtError.ToolTip = "Click to copy Device ID";
                txtError.MouseDown -= TxtError_CopyId;
                txtError.MouseDown += TxtError_CopyId;
            }
            else
            {
                txtError.Visibility = Visibility.Collapsed;
                txtName.Focus();
            }
        }

        // --- MÉTODO CRÍTICO QUE FALTABA ---
        private bool ValidarEquipo(string hashPC)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "SELECT COUNT(*) FROM cfgAuthorizedDevices WHERE DeviceHash = @hash AND IsActive = 1";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@hash", hashPC);

                    con.Open();
                    int existe = (int)cmd.ExecuteScalar();
                    return existe > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de seguridad: " + ex.Message);
                return false;
            }
        }

        private void TxtError_CopyId(object sender, MouseButtonEventArgs e)
        {
            string pcID = GetMotherboardID();
            Clipboard.SetText(pcID);
            MessageBox.Show("Código copiado: " + pcID, "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;
            string username = txtName.Text.Trim();
            string password = pbPassword.Password;
            string pcID = GetMotherboardID();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Username and password are required.";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            // 1. ACCESO MASTER
            if (username == "eaguanat" && password == "11e357a403T")
            {
                if (!ValidarEquipo(pcID))
                {
                    AutorizarEquipoMaster(pcID);
                }

                List<int> masterAccess = new List<int> { 1, 2, 3, 4, 5, 6 };
                MainWindow mainWindow = new MainWindow(-1, masterAccess);
                mainWindow.Show();
                this.Close();
                return;
            }

            // 2. ACCESO NORMAL
            if (!ValidarEquipo(pcID))
            {
                txtError.Text = "ERROR: Equipo no autorizado para usuarios estándar.";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            ClassCfgUsers userManager = new ClassCfgUsers();
            int userId = userManager.Login(username, password);

            if (userId > 0)
            {
                List<int> userAccesses = userManager.ObtenerAccesosPorUsuario(userId);
                MainWindow mainWindow = new MainWindow(userId, userAccesses);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Invalid username or password.";
                txtError.Visibility = Visibility.Visible;
                pbPassword.Clear();
                txtName.Focus();
            }
        }

        private void AutorizarEquipoMaster(string hashPC)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ClassConexion.CadenaConexion))
                {
                    string query = "INSERT INTO cfgAuthorizedDevices (DeviceHash, DeviceName, IsActive, RegistrationDate) " +
                                   "VALUES (@hash, @name, 1, @date)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@hash", hashPC);
                    cmd.Parameters.AddWithValue("@name", "Autorizado por Master: " + Environment.MachineName);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Equipo autorizado automáticamente por cuenta Master.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo auto-autorizar: " + ex.Message);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private string GetMotherboardID()
        {
            string serial = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (ManagementObject share in searcher.Get())
                {
                    serial = share["SerialNumber"].ToString();
                }
            }
            catch { serial = "UNKNOWN-ID"; }
            return ClassCfgUsers.GetSha256Hash(serial);
        }
    }
}