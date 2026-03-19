using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization; // Necesitas este using arriba
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data.SqlClient; // <--- Esta es la que define SqlConnection
using Microsoft.Win32; // Para OpenFileDialog
using System.IO;       // Para MemoryStream
using System.Windows.Media.Imaging; // Para BitmapImage


namespace Human_Resources.Forms
{
    public partial class frmCompany : Page
    {
        int idSeleccionado = 0;
        byte[] imagenBytes = null;

        public frmCompany()
        {
            InitializeComponent();
            LlenarGrid();
            CargarCatalogos();
            CtrlForm(true);

        }

        private void LlenarGrid()
        {
            Human_Resources.Data.ClassCompany obj = new Human_Resources.Data.ClassCompany();
            dgListado.ItemsSource = obj.Listar(txtSearch.Text).DefaultView;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Solo permite caracteres del 0 al 9
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Controlar estado de formulario
        private void CtrlForm(bool rtState)
        {
            
            GridListado.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;
            pnlSearch.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;
            btnPrint.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;
            BtnAdd.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;
            BtnModify.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;
            BtnDelete.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;
            BtnExit.Visibility = (rtState == true) ? Visibility.Visible : Visibility.Collapsed;

            //
            btnPrint.IsEnabled = (rtState && dgListado.Items.Count > 0);
            BtnModify.IsEnabled = (rtState && dgListado.Items.Count > 0);
            BtnDelete.IsEnabled = (rtState && dgListado.Items.Count > 0);

            GridEdicion.Visibility = (rtState == true) ? Visibility.Collapsed : Visibility.Visible;

        }
        private void CargarCatalogos()
        {
            try
            {
                cmbState.ItemsSource = new Human_Resources.Data.ClassGeoState().Listar().DefaultView;
                cmbState.DisplayMemberPath = "Description"; cmbState.SelectedValuePath = "Id";

                cmbRegion.ItemsSource = new Human_Resources.Data.ClassGeoRegion().Listar().DefaultView;
                cmbRegion.DisplayMemberPath = "Description"; cmbRegion.SelectedValuePath = "Id";
            }
            catch { }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => LlenarGrid();

        private void cmbState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbState.SelectedValue != null)
            {
                cmbCity.ItemsSource = new Human_Resources.Data.ClassGeoCity().ListarPorEstado((int)cmbState.SelectedValue).DefaultView;
                cmbCity.DisplayMemberPath = "Description"; cmbCity.SelectedValuePath = "Id";
            }
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
            idSeleccionado = 0;
            lblTitulo.Text = "NEW COMPANY RECORD";
            CtrlForm(false);  // Controlar estado del formulario
        }

        private void LimpiarCampos()
        {
            txtName.Clear(); txtAddress.Clear();txtPhone.Clear(); txtZip.Clear(); txtEmail.Clear(); 
            cmbState.SelectedItem = null; cmbCity.SelectedItem = null; cmbRegion.SelectedItem = null;
            imgLogo.Source = null;
            imagenBytes = null;
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgListado.SelectedItem;
            idSeleccionado = (int)row["id"];

            Human_Resources.Data.ClassCompany obj = new Human_Resources.Data.ClassCompany();
            // Lógica para llenar campos desde la fila y cargar idiomas
            CargarDatosParaEditar(idSeleccionado);

            lblTitulo.Text = "UPDATE COMPANY";
            CtrlForm(false);  // Controlar estado del formulario
        }

        private void CargarDatosParaEditar(int id)
        {
            Human_Resources.Data.ClassCompany obj = new Human_Resources.Data.ClassCompany();

            if (obj.ObtenerPorId(id))
            {
                // 1. Datos Personales
                txtName.Text = obj.Name;
                txtAddress.Text = obj.Address;
                txtZip.Text = obj.ZipCode;
                txtPhone.Text = obj.Phone;
                txtEmail.Text = obj.Email;
                imagenBytes = obj.Logo;
                MostrarImagen(imagenBytes);

                // 2. Combos Geográficos (Asignamos el valor, el combo se encarga del resto)
                cmbState.SelectedValue = obj.IdGeoState;
                // Forzamos la carga de ciudades para ese estado antes de asignar la ciudad
                if (obj.IdGeoState != null)
                {
                    cmbCity.ItemsSource = new Human_Resources.Data.ClassGeoCity().ListarPorEstado((int)obj.IdGeoState).DefaultView;
                    cmbCity.SelectedValue = obj.IdGeoCity;
                }
                cmbRegion.SelectedValue = obj.IdGeoRegion;

            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) { MessageBox.Show("Name is required"); return; }

            Human_Resources.Data.ClassCompany SN = new Human_Resources.Data.ClassCompany();
            SN.Id = idSeleccionado;
            SN.Name = txtName.Text.Trim();
            SN.Address = txtAddress.Text;
            SN.IdGeoState = (int?)cmbState.SelectedValue;
            SN.IdGeoCity = (int?)cmbCity.SelectedValue;
            SN.IdGeoRegion = (int?)cmbRegion.SelectedValue;
            SN.ZipCode = txtZip.Text;
            SN.Phone = txtPhone.Text;
            SN.Email = txtEmail.Text;
            SN.Logo = imagenBytes;

            int idRes = (idSeleccionado == 0) ? SN.Insertar() : (SN.Actualizar() ? idSeleccionado : 0);

            if (idRes > 0)
            {
                MessageBox.Show("Saved Successfully");
                BtnVolverInicio_Click(null, null);
                LlenarGrid();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgListado.SelectedItem;
            if (MessageBox.Show("Delete this record?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (new Human_Resources.Data.ClassCompany().Eliminar((int)row["id"])) LlenarGrid();
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "DEFINITION OF COMPANY";
            CtrlForm(true);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var principal = Window.GetWindow(this) as MainWindow;
            if (principal != null)
            {
                TabItem t = null;
                foreach (TabItem item in principal.tcPrincipal.Items)
                    if (item.Content is Frame f && f.Content == this) { t = item; break; }
                if (t != null) principal.tcPrincipal.Items.Remove(t);
            }
        }


        private void btnLoadLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (ofd.ShowDialog() == true)
            {
                // 1. Convertir archivo a Bytes
                imagenBytes = File.ReadAllBytes(ofd.FileName);

                // 2. Mostrar la imagen en el control Image
                MostrarImagen(imagenBytes);
            }
        }

        private void MostrarImagen(byte[] datos)
        {
            if (datos == null || datos.Length == 0) { imgLogo.Source = null; return; }

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

        private void BtnPrint_Click(object sender, RoutedEventArgs e) 
        {  }


    }
}