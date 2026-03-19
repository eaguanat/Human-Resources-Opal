using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Human_Resources.Data; // Para acceder a ClassCfgAuthorizedDevices y ClassCfgUsers (para GetSha256Hash)
using System.Management; // Necesario para GetMotherboardID en este formulario también.

namespace Human_Resources.Forms
{
    public partial class frmCfgAuthorizedDevices : Page
    {
        private int idSeleccionado = 0;

        public frmCfgAuthorizedDevices()
        {
            InitializeComponent();
            LlenarGrid();
            CtrlForm(true); // Mostrar GridListado al inicio
        }

        private void LlenarGrid()
        {
            ClassCfgAuthorizedDevices obj = new ClassCfgAuthorizedDevices();
            dgListado.ItemsSource = obj.Listar(txtSearch.Text).DefaultView;
        }

        // Controlar estado del formulario (vista de lista vs. vista de edición)
        private void CtrlForm(bool showList)
        {
            GridListado.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            pnlSearch.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnAdd.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnModify.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnDelete.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnExit.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;

            GridEdicion.Visibility = showList ? Visibility.Collapsed : Visibility.Visible;
        }

        // Limpiar campos para un nuevo registro
        private void LimpiarCampos()
        {
            txtDeviceName.Clear();
            txtDeviceHash.Clear(); // Esto se llenará automáticamente para nuevos registros
            dpRegistrationDate.SelectedDate = DateTime.Today; // Establece la fecha actual por defecto
            chkIsActive.IsChecked = true;

            // Para un nuevo registro, generamos el DeviceHash automáticamente
            // y lo hacemos de solo lectura. El usuario solo ingresa el nombre.
            idSeleccionado = 0; // Asegura que es un "Add"
            txtDeviceHash.Text = GetMotherboardID();
            txtDeviceHash.IsReadOnly = true; // No permitir edición del hash existente
            dpRegistrationDate.IsEnabled = false; // La fecha de registro se auto-llena
        }

        // --- EVENTOS DE BOTONES ---

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
            lblTitulo.Text = "REGISTER NEW DEVICE";
            CtrlForm(false); // Mostrar vista de edición
            txtDeviceName.Focus();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a device to modify.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgListado.SelectedItem;
            idSeleccionado = (int)row["Id"];

            CargarDatosParaEditar(idSeleccionado);

            lblTitulo.Text = "UPDATE DEVICE";
            CtrlForm(false); // Mostrar vista de edición
            txtDeviceName.Focus();
        }

        private void CargarDatosParaEditar(int id)
        {
            ClassCfgAuthorizedDevices device = new ClassCfgAuthorizedDevices();
            if (device.ObtenerPorId(id))
            {
                txtDeviceName.Text = device.DeviceName;
                txtDeviceHash.Text = device.DeviceHash;
                dpRegistrationDate.SelectedDate = device.RegistrationDate;
                chkIsActive.IsChecked = device.IsActive;

                txtDeviceHash.IsReadOnly = true; // El hash no debe ser editable nunca
                dpRegistrationDate.IsEnabled = false; // La fecha de registro no es editable
            }
            else
            {
                MessageBox.Show("Could not load device data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnVolverInicio_Click(null, null); // Volver a la lista si hay un error
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. Validaciones ---
            if (string.IsNullOrWhiteSpace(txtDeviceName.Text)) { MessageBox.Show("Device Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtDeviceName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtDeviceHash.Text)) { MessageBox.Show("Device ID (Hash) is missing. Cannot save.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            // --- 2. Preparar objeto del dispositivo ---
            ClassCfgAuthorizedDevices device = new ClassCfgAuthorizedDevices
            {
                Id = idSeleccionado,
                DeviceHash = txtDeviceHash.Text.Trim(),
                DeviceName = txtDeviceName.Text.Trim(),
                IsActive = chkIsActive.IsChecked ?? true,
                RegistrationDate = dpRegistrationDate.SelectedDate ?? DateTime.Today // Asegura que siempre haya una fecha
            };

            int savedId = 0;
            if (idSeleccionado == 0) // Nuevo dispositivo
            {
                savedId = device.Insertar();
            }
            else // Actualizar dispositivo existente
            {
                if (device.Actualizar())
                {
                    savedId = idSeleccionado;
                }
            }

            if (savedId > 0)
            {
                MessageBox.Show("Device data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnVolverInicio_Click(null, null); // Regresar a la lista
                LlenarGrid(); // Refrescar la lista de dispositivos
            }
            else
            {
                // El mensaje de error ya debería haberse mostrado desde la capa de datos
                // MessageBox.Show("Failed to save device data. See previous errors.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a device to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgListado.SelectedItem;
            int idToDelete = (int)row["Id"];
            string deviceName = row["DeviceName"].ToString();

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete device '{deviceName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClassCfgAuthorizedDevices device = new ClassCfgAuthorizedDevices();
                if (device.Eliminar(idToDelete))
                {
                    MessageBox.Show("Device deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LlenarGrid(); // Refrescar la lista
                }
                else
                {
                    MessageBox.Show("Failed to delete device. See previous errors.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "AUTHORIZED DEVICES MANAGEMENT";
            CtrlForm(true); // Volver a la vista de lista
            LlenarGrid(); // Refrescar por si se hizo un cambio
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para cerrar la pestaña, similar a otros formularios
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

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality for Authorized Devices is not yet implemented.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LlenarGrid(); // Refrescar el grid al escribir en el buscador
        }

        // Método para obtener el ID de la placa base (similar al del Login)
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

            return ClassCfgUsers.GetSha256Hash(serial); // Usamos el método de hashing existente
        }
    }
}