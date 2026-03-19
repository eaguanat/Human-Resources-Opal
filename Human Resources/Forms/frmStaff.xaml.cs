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

namespace Human_Resources.Forms
{
    // Esta es la clase que le falta a tu código
    public class LangStaff
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsChecked { get; set; }
        public bool CanRead { get; set; }   
        public bool CanWrite { get; set; } 
    }

    public partial class frmStaff : Page
    {
        int idSeleccionado = 0;

        public frmStaff()
        {
            InitializeComponent();
            LlenarGrid();
            CargarCatalogos();
            CtrlForm(true);

        }

        private void LlenarGrid()
        {
            Human_Resources.Data.ClassStaff obj = new Human_Resources.Data.ClassStaff();
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

            GridEdicion.Visibility = (rtState == true) ? Visibility.Collapsed : Visibility.Visible;

        }

        private void CtrlEdition() // El TabControl
        {
            tcStaff.SelectedIndex = 0; // Selecciona la primera pestaña ("User Details")
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Input,
                new Action(() => { txtName.Focus(); }));
        }
        private void CargarCatalogos()
        {
            try
            {
                cmbDepartment.ItemsSource = new Human_Resources.Data.ClassDepartment().Listar().DefaultView;
                cmbDepartment.DisplayMemberPath = "Description"; cmbDepartment.SelectedValuePath = "Id";

                cmbState.ItemsSource = new Human_Resources.Data.ClassGeoState().Listar().DefaultView;
                cmbState.DisplayMemberPath = "Description"; cmbState.SelectedValuePath = "Id";

                cmbRegion.ItemsSource = new Human_Resources.Data.ClassGeoRegion().Listar().DefaultView;
                cmbRegion.DisplayMemberPath = "Description"; cmbRegion.SelectedValuePath = "Id";

                cmbInmigration.ItemsSource = new Human_Resources.Data.ClassDocsInmigration().Listar().DefaultView;
                cmbInmigration.DisplayMemberPath = "Description"; cmbInmigration.SelectedValuePath = "Id";

                cmbPayroll.ItemsSource = new Human_Resources.Data.ClassDocsPayroll().Listar().DefaultView;
                cmbPayroll.DisplayMemberPath = "Description"; cmbPayroll.SelectedValuePath = "Id";

                cmbBank.ItemsSource = new Human_Resources.Data.ClassBanks().Listar().DefaultView;
                cmbBank.DisplayMemberPath = "Description"; cmbBank.SelectedValuePath = "Id";

                cmbBankMethod.ItemsSource = new Human_Resources.Data.ClassBanksMethod().Listar().DefaultView;
                cmbBankMethod.DisplayMemberPath = "Description"; cmbBankMethod.SelectedValuePath = "Id";

                // Idiomas
                // --- SECCIÓN CORREGIDA DE IDIOMAS EN CargarCatalogos ---
                // 1. Instanciamos la clase de datos
                Human_Resources.Data.ClassLanguage objData = new Human_Resources.Data.ClassLanguage();

                // 2. Llamamos a ListarIdiomas pero pasándole NULL o una lista vacía 
                //    para que el SQL entienda que queremos TODO el catálogo de tblLanguages
                DataTable dtTodosLosIdiomas = objData.Listar();

                // 3. Convertimos el DataTable a nuestra lista de objetos LangStaff
                //    Esto es lo que el ListBox necesita para "dibujar" los CheckBoxes
                var listaParaCheck = dtTodosLosIdiomas.AsEnumerable().Select(row => new LangStaff
                {
                    Id = Convert.ToInt32(row["id"]),
                    Description = row["Description"].ToString(),
                    IsChecked = false, // Por defecto desmarcados al cargar catálogo
                    CanRead = false,
                    CanWrite = false
                }).ToList();

                // 4. Asignamos al ListBox
                lstLanguages.ItemsSource = listaParaCheck;
            

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
        private void cmbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartment.SelectedValue == null) return;

            int idDep = Convert.ToInt32(cmbDepartment.SelectedValue);

            // CAMBIO AQUÍ: Usamos la ruta completa del modelo de datos
            List<Human_Resources.Data.ClassStaff.LicenseEntry> lista = new List<Human_Resources.Data.ClassStaff.LicenseEntry>();

            try
            {
                using (SqlConnection con = new SqlConnection(Human_Resources.Data.ClassConexion.CadenaConexion))
                {
                    string query = "SELECT id, Description FROM tblDepLicense WHERE idDepartment = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", idDep);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        lista.Add(new Human_Resources.Data.ClassStaff.LicenseEntry // RUTA COMPLETA
                        {
                            IdDepLicense = Convert.ToInt32(dr["id"]),
                            Description = dr["Description"].ToString(),
                            LicenseNumber = ""
                        });
                    }
                }
                listLicenses.ItemsSource = lista;
            }
            catch (Exception ex) { MessageBox.Show("Error loading licenses: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
            idSeleccionado = 0;
            lblTitulo.Text = "NEW STAFF RECORD";
            CtrlForm(false);  // Controlar estado del formulario
            CtrlEdition(); // Controlar la edición (enfocar el primer campo)
        }

        private void LimpiarCampos()
        {
            txtName.Clear(); txtLastName.Clear(); txtSocial.Clear(); txtAddress.Clear();
            txtPhone.Clear(); txtZip.Clear(); txtAccount.Clear();
            txtCompanyName.Clear(); txtContPersonA.Clear(); txtContPhoneA.Clear(); txtContPersonB.Clear(); txtContPhoneB.Clear();
            txtRouting.Clear(); chkActive.IsChecked = true;
            dpBirthday.SelectedDate = null; txtRate.Clear(); txtEmail.Clear();
            dpAppDay.SelectedDate = null; txtJobDes.Clear();
            dpHiredDay.SelectedDate = null; dpEndDay.SelectedDate = null;
            var list = (List<LangStaff>)lstLanguages.ItemsSource;
            if (list != null) foreach (var l in list) l.IsChecked = false;
            listLicenses.ItemsSource = null;
            cmbDepartment.SelectedItem = null;
            cmbState.SelectedItem = null; cmbCity.SelectedItem = null; cmbRegion.SelectedItem = null;
            cmbInmigration.SelectedItem = null; cmbBank.SelectedItem = null; cmbBankMethod.SelectedItem = null;
            cmbPayroll.SelectedItem = null;

            // Limpiando los idiomas
            var listLang = (List<LangStaff>)lstLanguages.ItemsSource;
            if (listLang != null)
            {
                foreach (var l in listLang)
                { l.IsChecked = false; l.CanRead = false; l.CanWrite = false; }
            }
            // ESTO ES CLAVE: Obliga al ListBox a volver a dibujar los CheckBoxes con los nuevos valores
            lstLanguages.Items.Refresh();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgListado.SelectedItem;
            idSeleccionado = (int)row["id"];

            // En un sistema real, aquí cargarías el objeto completo desde la DB por ID. 
            // Para simplificar, asumimos que Data tiene los campos o hacemos una consulta rápida.
            Human_Resources.Data.ClassStaff obj = new Human_Resources.Data.ClassStaff();
            // Lógica para llenar campos desde la fila y cargar idiomas
            CargarDatosParaEditar(idSeleccionado);

            lblTitulo.Text = "UPDATE STAFF";
            CtrlForm(false);  // Controlar estado del formulario
            CtrlEdition(); // Controlar la edición (enfocar el primer campo))
        }

        private void CargarDatosParaEditar(int id)
        {
            Human_Resources.Data.ClassStaff obj = new Human_Resources.Data.ClassStaff();

            if (obj.ObtenerPorId(id))
            {
                // 1. Datos Personales
                txtName.Text = obj.Name;
                txtLastName.Text = obj.LastName;
                dpBirthday.SelectedDate = obj.Birthday;
                dpAppDay.SelectedDate = obj.AppDay;
                dpHiredDay.SelectedDate = obj.HiredDay;
                dpEndDay.SelectedDate = obj.EndDay;
                txtSocial.Text = obj.Social;
                txtAddress.Text = obj.Address;
                txtZip.Text = obj.ZipCod;
                txtPhone.Text = obj.Phone;
                txtEmail.Text = obj.Email;
                txtRate.Text = obj.Rate.ToString("N2");

                // 2. Combos Geográficos (Asignamos el valor, el combo se encarga del resto)
                cmbDepartment.SelectedValue = obj.IdDepartment;
                cmbState.SelectedValue = obj.IdGeoState;
                // Forzamos la carga de ciudades para ese estado antes de asignar la ciudad
                if (obj.IdGeoState != null)
                {
                    cmbCity.ItemsSource = new Human_Resources.Data.ClassGeoCity().ListarPorEstado((int)obj.IdGeoState).DefaultView;
                    cmbCity.SelectedValue = obj.IdGeoCity;
                }
                cmbRegion.SelectedValue = obj.IdGeoRegion;


                // 3. Payroll y Banking
                cmbInmigration.SelectedValue = obj.IdDocsInmigration;
                cmbPayroll.SelectedValue = obj.IdDocsPayroll;
                cmbBank.SelectedValue = obj.IdBanks;
                cmbBankMethod.SelectedValue = obj.IdBanksMethod;
                txtAccount.Text = obj.AccountNumber;
                txtRouting.Text = obj.RoutingNumber;
                txtCompanyName.Text = obj.CompanyName;
                txtJobDes.Text = obj.JobDes;
                chkActive.IsChecked = obj.Active;

                // 5. Contactos de Emergencia
                txtContPersonA.Text = obj.ContactPersonA;
                txtContPhoneA.Text = obj.ContactPhoneA;
                txtContPersonB.Text = obj.ContactPersonB;
                txtContPhoneB.Text = obj.ContactPhoneB;

                // 6. Idiomas (Carga de Speak, Read y Write)
                DataTable dtIdiomasStaff = obj.ObtenerIdiomasDetallados(id); // El método que devuelve la tabla tblStaffLanguage
                var listLang = (List<LangStaff>)lstLanguages.ItemsSource;

                if (listLang != null && dtIdiomasStaff != null)
                {
                    foreach (var l in listLang)
                    {
                        // Buscamos si el idioma actual (l.Id) existe para este empleado en la tabla de la BD
                        DataRow row = dtIdiomasStaff.AsEnumerable()
                            .FirstOrDefault(r => Convert.ToInt32(r["idLanguage"]) == l.Id);

                        if (row != null)
                        {
                            l.IsChecked = true; // Si existe el registro, es que lo "Habla"

                            // Convertimos el 1 de SQL a true de C#
                            l.CanRead = row["CanRead"] != DBNull.Value && Convert.ToInt32(row["CanRead"]) == 1;
                            l.CanWrite = row["CanWrite"] != DBNull.Value && Convert.ToInt32(row["CanWrite"]) == 1;

                            Console.WriteLine($"Cargado: {l.Description} - R:{l.CanRead} W:{l.CanWrite}");
                        }
                        else
                        {
                            l.IsChecked = false;
                            l.CanRead = false;
                            l.CanWrite = false;
                        }
                    }

                    // ESTO ES CLAVE: Obliga al ListBox a volver a dibujar los CheckBoxes con los nuevos valores
                    lstLanguages.Items.Refresh();
                }

                // 7. Licencias
                listLicenses.ItemsSource = obj.ObtenerLicenciasPorEmpleado(id, obj.IdDepartment);

            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) { MessageBox.Show("Name is required"); return; }

            Human_Resources.Data.ClassStaff SN = new Human_Resources.Data.ClassStaff();
            SN.Id = idSeleccionado;
            SN.Name = txtName.Text.Trim();
            SN.LastName = txtLastName.Text.Trim();
            SN.IdDepartment = (int?)cmbDepartment.SelectedValue;
            SN.Social = txtSocial.Text.Trim();
            SN.Birthday = dpBirthday.SelectedDate;
            SN.AppDay = dpAppDay.SelectedDate;
            SN.HiredDay = dpHiredDay.SelectedDate;
            SN.EndDay = dpEndDay.SelectedDate;
            SN.Address = txtAddress.Text;
            SN.IdGeoState = (int?)cmbState.SelectedValue;
            SN.IdGeoCity = (int?)cmbCity.SelectedValue;
            SN.IdGeoRegion = (int?)cmbRegion.SelectedValue;
            SN.ZipCod = txtZip.Text;
            SN.Phone = txtPhone.Text;
            SN.Email = txtEmail.Text;
            SN.ContactPersonA = txtContPersonA.Text;
            SN.ContactPhoneA = txtContPhoneA.Text;
            SN.ContactPersonB = txtContPersonB.Text;
            SN.ContactPhoneB = txtContPhoneB.Text;
            SN.IdDocsInmigration = (int?)cmbInmigration.SelectedValue;
            SN.IdDocsPayroll = (int?)cmbPayroll.SelectedValue;
            SN.IdBanks = (int?)cmbBank.SelectedValue;
            SN.IdBanksMethod = (int?)cmbBankMethod.SelectedValue;
            SN.AccountNumber = txtAccount.Text;
            SN.RoutingNumber = txtRouting.Text;
            SN.CompanyName = txtCompanyName.Text;
            SN.JobDes = txtJobDes.Text;
            SN.Active = chkActive.IsChecked ?? true;
            // Intentamos convertir el texto a double
            if (double.TryParse(txtRate.Text, out double rateValue)) { SN.Rate = rateValue; }
            else { SN.Rate = 0.0; } // Valor por defecto si el texto no es un número válido



            int idRes = (idSeleccionado == 0) ? SN.Insertar() : (SN.Actualizar() ? idSeleccionado : 0);

            if (idRes > 0)
            {
                // 2. GUARDAR IDIOMAS
                var listaParaGuardar = (List<LangStaff>)lstLanguages.ItemsSource;
                SN.GuardarIdiomas(idRes, listaParaGuardar);

                // 3. GUARDAR LICENCIAS (FILTRADO)
                // Usamos la ruta completa para evitar el error de "no se encontró el tipo"
                var listaCompleta = (List<Human_Resources.Data.ClassStaff.LicenseEntry>)listLicenses.ItemsSource;

                if (listaCompleta != null)
                {
                    // FILTRO: Solo tomamos las que tengan el número de licencia escrito
                    var soloConDatos = listaCompleta
                                       .Where(x => !string.IsNullOrWhiteSpace(x.LicenseNumber))
                                       .ToList();

                    // Enviamos solo las que tienen información
                    SN.GuardarLicencias(idRes, soloConDatos);
                }

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
                if (new Human_Resources.Data.ClassStaff().Eliminar((int)row["id"])) LlenarGrid();
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "DEFINITION OF STAFF";
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

        private void BtnPrint_Click(object sender, RoutedEventArgs e) { /* Implementar igual que Banks con los campos de Staff */ }


        // Este maneja el PreviewTextInput
        private void txtRate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permite números y un punto decimal
            Regex regex = new Regex("[^0-9.]+");
            bool isForbidden = regex.IsMatch(e.Text);

            // Si es un punto y ya existe uno, prohibirlo
            if (e.Text == "." && ((TextBox)sender).Text.Contains("."))
            {
                e.Handled = true;
                return;
            }

            e.Handled = isForbidden;
        }
        private void txtRate_GotFocus(object sender, RoutedEventArgs e)
        {
            // Esto selecciona todo el contenido del TextBox
            txtRate.SelectAll();
        }

        // Este maneja el LostFocus
        private void txtRate_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtRate.Text, out double value))
            {
                txtRate.Text = value.ToString("N2"); // Formato 0.00
            }
            else
            {
                txtRate.Text = "0.00"; // Valor por defecto si borran todo
            }
        }
    }
}