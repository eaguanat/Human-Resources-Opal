using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Human_Resources.Data; // Para acceder a ClassCompany, ClassGeoState, ClassGeoCity, ClassGeoRegion, ClassCfgUsers (para hashing)


namespace Human_Resources.Forms
{
    public partial class frmCompany : Page
    {
        private byte[] imagenBytes = null; // Para el logo de la compañía

        public frmCompany()
        {
            InitializeComponent();
            CargarCatalogos(); // Carga los ComboBox de Estados, Ciudades, Regiones
            LoadCompanyData(); // Carga la información de la compañía (ID=1)
            lblTitulo.Text = "COMPANY CONFIGURATION"; // Siempre en modo de edición/configuración

            tcCompany.SelectedIndex = 0; // Asegura que la primera pestaña esté seleccionada al cargar
        }

        // Carga los ComboBoxes de filtro
        private void CargarCatalogos()
        {
            try
            {
                cmbState.ItemsSource = new ClassGeoState().Listar().DefaultView;
                cmbState.DisplayMemberPath = "Description";
                cmbState.SelectedValuePath = "Id";

                cmbRegion.ItemsSource = new ClassGeoRegion().Listar().DefaultView;
                cmbRegion.DisplayMemberPath = "Description";
                cmbRegion.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading catalogs: " + ex.Message, "Catalog Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carga la información de la compañía (ID=1)
        private void LoadCompanyData()
        {
            ClassCompany objCompany = new ClassCompany();
            if (objCompany.Obtener()) // Intenta obtener el registro con ID=1
            {
                // Cargar datos de la compañía
                txtName.Text = objCompany.Name;
                txtAddress.Text = objCompany.Address;
                cmbState.SelectedValue = objCompany.IdGeoState;
                if (objCompany.IdGeoState != null)
                {
                    cmbCity.ItemsSource = new ClassGeoCity().ListarPorEstado((int)objCompany.IdGeoState).DefaultView;
                    cmbCity.DisplayMemberPath = "Description"; cmbCity.SelectedValuePath = "Id";
                    cmbCity.SelectedValue = objCompany.IdGeoCity;
                }
                else // Limpiar ciudades si no hay estado seleccionado
                {
                    cmbCity.ItemsSource = null;
                    cmbCity.SelectedValue = null;
                }
                cmbRegion.SelectedValue = objCompany.IdGeoRegion;
                txtZip.Text = objCompany.ZipCode;
                txtPhone.Text = objCompany.Phone;
                txtEmail.Text = objCompany.Email;
                imagenBytes = objCompany.Logo;
                MostrarImagen(imagenBytes);

                // Cargar configuración de correo
                txtMailServer.Text = objCompany.MailServer;
                txtMailPort.Text = objCompany.MailPort?.ToString(); // Usar ?. para tipos anulables
                txtMailUsername.Text = objCompany.MailUsername;
                pbMailPassword.Clear(); // La contraseña de correo (pbMailPassword) NO se carga con el valor, se deja en blanco por seguridad.
                // Guardamos el hash existente en la clase para cuando se guarde sin cambiar la contraseña
                // Esto ya lo hace la propiedad MailPasswordHash al ser leída en Obtener()
                chkEnableSSL.IsChecked = objCompany.EnableSSL ?? false; // Por defecto false si es null
                txtSenderName.Text = objCompany.SenderName;

            }
            else
            {
                // Si no existe el registro (primera vez), limpiar campos para que el usuario ingrese la info
                LimpiarCampos();
                MessageBox.Show("No company data found. Please enter the company details and click Update.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Limpia todos los campos del formulario
        private void LimpiarCampos()
        {
            txtName.Clear(); txtAddress.Clear(); txtPhone.Clear(); txtZip.Clear(); txtEmail.Clear();
            cmbState.SelectedValue = null; // Usar SelectedValue para tipos anulables
            cmbCity.SelectedValue = null;
            cmbRegion.SelectedValue = null;
            imgLogo.Source = null; // Limpia la imagen mostrada
            imagenBytes = null; // Limpia los bytes de la imagen

            // Limpiar campos de correo
            txtMailServer.Clear();
            txtMailPort.Clear();
            txtMailUsername.Clear();
            pbMailPassword.Clear(); // MUY IMPORTANTE: Limpiar el PasswordBox
            chkEnableSSL.IsChecked = false;
            txtSenderName.Clear();
        }

        // --- EVENTOS DE CONTROLES ---

        private void cmbState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbState.SelectedValue != null)
            {
                cmbCity.ItemsSource = new ClassGeoCity().ListarPorEstado((int)cmbState.SelectedValue).DefaultView;
                cmbCity.DisplayMemberPath = "Description"; cmbCity.SelectedValuePath = "Id";
            }
            else
            {
                cmbCity.ItemsSource = null; // Limpia las ciudades si no hay estado seleccionado
                cmbCity.SelectedValue = null;
            }
        }

        // Validación para campos numéricos (Zip Code, Mail Port)
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // --- MANEJO DE IMAGEN (LOGO) ---

        private void btnLoadLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    // 1. Convertir archivo a Bytes
                    imagenBytes = File.ReadAllBytes(ofd.FileName);
                    // 2. Mostrar la imagen en el control Image
                    MostrarImagen(imagenBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image: " + ex.Message, "Image Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MostrarImagen(byte[] datos)
        {
            if (datos == null || datos.Length == 0)
            {
                imgLogo.Source = null;
                return;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(datos))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgLogo.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying image: " + ex.Message, "Image Display Error", MessageBoxButton.OK, MessageBoxImage.Error);
                imgLogo.Source = null;
            }
        }

        // --- BOTONES DE ACCIÓN ---

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. Validaciones ---
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Company Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtAddress.Text)) { MessageBox.Show("Address is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtAddress.Focus(); return; }
            if (cmbState.SelectedValue == null) { MessageBox.Show("State is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); cmbState.Focus(); return; }
            if (cmbCity.SelectedValue == null) { MessageBox.Show("City is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); cmbCity.Focus(); return; }

            // Validación de Email (básica)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid company email address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtEmail.Focus(); return;
            }

            // Validación de Mail Port
            int mailPortValue = 0; // Valor predeterminado
            if (!string.IsNullOrWhiteSpace(txtMailPort.Text))
            {
                if (!int.TryParse(txtMailPort.Text, out mailPortValue))
                {
                    MessageBox.Show("Mail Port must be a valid number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtMailPort.Focus(); return;
                }
            }


            // --- 2. Preparar objeto de compañía ---
            ClassCompany objCompany = new ClassCompany
            {
                Id = 1, // Siempre trabajamos con el ID 1
                Name = txtName.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                IdGeoRegion = (int?)cmbRegion.SelectedValue,
                IdGeoState = (int?)cmbState.SelectedValue,
                IdGeoCity = (int?)cmbCity.SelectedValue,
                ZipCode = txtZip.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Logo = imagenBytes,

                // Configuración de correo
                MailServer = txtMailServer.Text.Trim(),
                MailPort = string.IsNullOrWhiteSpace(txtMailPort.Text) ? (int?)null : mailPortValue,
                MailUsername = txtMailUsername.Text.Trim(),
                MailPasswordText = pbMailPassword.Password, // Obtener la contraseña de correo del PasswordBox
                EnableSSL = chkEnableSSL.IsChecked,
                SenderName = txtSenderName.Text.Trim()
            };

            // Si la contraseña del correo no se ha cambiado, necesitamos cargar el hash existente
            // La lógica en ClassCompany.Guardar() ya maneja esto:
            // Si MailPasswordText está vacío, intentará usar el MailPasswordHash que se cargó con Obtener().
            // Si es un nuevo registro y MailPasswordText está vacío, MailPasswordHash será null.
            // Si se introduce MailPasswordText, se hashea y se usa ese nuevo hash.

            if (objCompany.Guardar()) // El método Guardar() se encarga de INSERT/UPDATE
            {
                MessageBox.Show("Company data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCompanyData(); // Recargar los datos para refrescar la vista y el hash de contraseña (dejando PB en blanco)
            }
            else
            {
                // El mensaje de error ya debería haberse mostrado desde la capa de datos
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para cerrar la pestaña
            var principal = Window.GetWindow(this) as MainWindow;
            if (principal != null)
            {
                TabItem tabToClose = null;
                foreach (TabItem item in principal.tcPrincipal.Items)
                {
                    if (item.Content is Frame f && f.Content == this)
                    {
                        tabToClose = item;
                        break;
                    }
                }
                if (tabToClose != null)
                {
                    principal.tcPrincipal.Items.Remove(tabToClose);
                }
            }
        }
    }
}