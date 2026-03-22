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
            MostrarVersionActual();
        }

        private void MostrarVersionActual()
        {
            try
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    var v = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    lblVersion.Text = $"Version: {v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                }
                else { lblVersion.Text = "Development Mode"; }
            }
            catch { lblVersion.Text = "Version: N/A"; }
        }

        private async void FrmLogin_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. INICIO VISUAL
            LoadingPanel.Visibility = Visibility.Visible;
            LoginFieldsPanel.Visibility = Visibility.Collapsed;

            // --- FLANCO 1: ACTUALIZACIÓN AUTOMÁTICA (UNIDAD G) ---
            string vDriveStr = ClassCfgUsers.ObtenerVersionUpdate();
            if (!string.IsNullOrEmpty(vDriveStr))
            {
                Version vDrive = new Version(vDriveStr);
                Version vLocal = new Version("1.0.0.0");

                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                    vLocal = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;

                if (vDrive > vLocal)
                {
                    MessageBox.Show($"Remake {vDrive} detectada.\nThe system will shut down to update..", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                    try
                    {
                        System.Diagnostics.Process.Start(@"G:\Mi unidad\Software_Deployments\Human_Resources\Installer\setup.exe");
                        Application.Current.Shutdown();
                        return;
                    }
                    catch (Exception ex) { MessageBox.Show("Error launching installer: " + ex.Message); }
                }
            }

            // --- FLANCO 2: SEGURIDAD REFORZADA (REINTENTOS AZURE) ---
            string pcID = GetMotherboardID();
            bool esValido = false;
            int intentos = 0;
            int maxIntentos = 3;

            while (intentos < maxIntentos)
            {
                intentos++;
                // Opcional: Si tienes un label de estado podrías poner: txtStatus.Text = $"Validando... ({intentos})";

                esValido = await Task.Run(() => ValidarEquipo(pcID));

                if (esValido) break; // Si tuvo éxito, salimos del bucle inmediatamente

                // Si falló el intento, esperamos 1.5 segundos antes de reintentar
                if (intentos < maxIntentos) await Task.Delay(1500);
            }

            LoadingPanel.Visibility = Visibility.Collapsed;
            LoginFieldsPanel.Visibility = Visibility.Visible;

            if (!esValido)
            {
                txtError.Visibility = Visibility.Visible;
                txtError.Text = "Server Communication Error. Please check your internet connection.";
            }
            else
            {
                txtError.Visibility = Visibility.Collapsed;
                txtName.Focus();
            }
        }

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

            // 1. ACCESO MASTER (ELIECER)
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
                txtError.Text = "ERROR: Device not authorized for standard users.";
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
                    cmd.Parameters.AddWithValue("@name", "Authorized by Master: " + Environment.MachineName);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Device automatically authorized by the Master account.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not self-authorize: " + ex.Message);
            }
        }

        private void TxtError_CopyId(object sender, MouseButtonEventArgs e)
        {
            string pcID = GetMotherboardID();
            Clipboard.SetText(pcID);
            MessageBox.Show("Code copied: " + pcID, "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    serial = share["SerialNumber"]?.ToString() ?? "UNKNOWN-ID";
                }
            }
            catch { serial = "UNKNOWN-ID"; }
            return ClassCfgUsers.GetSha256Hash(serial);
        }
    }
}
