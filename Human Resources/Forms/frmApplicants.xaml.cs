using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Human_Resources.Data; // Para acceder a las clases de datos

namespace Human_Resources.Forms
{
    public partial class frmApplicants : Page
    {
        private int idAplicanteSeleccionado = 0;

        // Variables para guardar los filtros cuando pasamos a la vista de detalle
        private int? currentFilterIdDept;
        private int? currentFilterIdState;
        private int? currentFilterIdCity;
        private int? currentFilterStatus;
        private string currentFilterZipCode;
        private string currentFilterServiceZipCodes;
        private string currentFilterFullName;


        public frmApplicants()
        {
            InitializeComponent();
            CargarCatalogosFiltro();
            LimpiarFiltros(); // Limpiar filtros al inicio para asegurar estado limpio
            CtrlForm(ViewMode.Filter); // Inicia en la vista de filtro
        }

        // Definimos los modos de vista para el formulario
        private enum ViewMode
        {
            Filter, // Solo mostrando el área de filtros y botones de filtro
            Results, // Mostrando filtros deshabilitados + DataGrid de resultados + botones de resultados
            Detail // Mostrando filtros deshabilitados + Detalle del aspirante + botones de detalle
        }

        // Controlar la visibilidad de los paneles y botones según el modo de vista
        private void CtrlForm(ViewMode mode)
        {
            // Oculta/Muestra el panel de filtros
            bdrFilterControls.Visibility = (mode == ViewMode.Detail) ? Visibility.Collapsed : Visibility.Visible;

            // Oculta los paneles de contenido dinámico (resultados y detalle)
            pnlResultadosBusqueda.Visibility = Visibility.Collapsed;
            pnlDetalle.Visibility = Visibility.Collapsed;

            // Oculta todos los botones de acción inferior por defecto
            BtnBuscar.Visibility = Visibility.Collapsed;
            BtnResetFiltro.Visibility = Visibility.Collapsed;
            BtnSalirFiltro.Visibility = Visibility.Collapsed; // Este siempre es el de salir de la TAB

            BtnVerDetalle.Visibility = Visibility.Collapsed;
            BtnHomeResultados.Visibility = Visibility.Collapsed;

            BtnActualizarStatus.Visibility = Visibility.Collapsed;
            BtnEliminarDetalle.Visibility = Visibility.Collapsed;
            BtnBackDetalle.Visibility = Visibility.Collapsed;


            // Ajusta el estado de los controles de filtro y los paneles/botones de contenido
            switch (mode)
            {
                case ViewMode.Filter:
                    lblTitulo.Text = "APPLICANTS MANAGEMENT - SEARCH";
                    SetFilterControlsEnabled(true); // Habilita los filtros
                    BtnBuscar.Visibility = Visibility.Visible;
                    BtnResetFiltro.Visibility = Visibility.Visible;
                    BtnSalirFiltro.Visibility = Visibility.Visible; // Salir de la TAB
                    break;

                case ViewMode.Results:
                    pnlResultadosBusqueda.Visibility = Visibility.Visible;
                    lblTitulo.Text = "APPLICANTS MANAGEMENT - RESULTS";
                    SetFilterControlsEnabled(false); // Deshabilita los filtros
                    BtnVerDetalle.Visibility = Visibility.Visible;
                    BtnHomeResultados.Visibility = Visibility.Visible;
                    BtnSalirFiltro.Visibility = Visibility.Visible; // Salir de la TAB
                    break;

                case ViewMode.Detail:
                    pnlDetalle.Visibility = Visibility.Visible;
                    lblTitulo.Text = "APPLICANTS MANAGEMENT - DETAILS";
                    SetFilterControlsEnabled(false); // Deshabilita los filtros
                    BtnActualizarStatus.Visibility = Visibility.Visible;
                    BtnEliminarDetalle.Visibility = Visibility.Visible;
                    BtnBackDetalle.Visibility = Visibility.Visible;
                    // El BtnSalirFiltro no se muestra en el detalle, ya que el [Back] es el botón principal
                    BtnSalirFiltro.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        // Habilita o deshabilita los controles de filtro
        private void SetFilterControlsEnabled(bool enabled)
        {
            cmbDepartment.IsEnabled = enabled;
            cmbState.IsEnabled = enabled;
            cmbCity.IsEnabled = enabled;
            cmbStatusFilter.IsEnabled = enabled;
            txtZipCodeFilter.IsEnabled = enabled;
            txtServiceZipCodesFilter.IsEnabled = enabled;
            txtFullNameFilter.IsEnabled = enabled;
            BtnBuscar.IsEnabled = enabled;
            BtnResetFiltro.IsEnabled = enabled;
        }


        // Carga los ComboBoxes de filtro
        private void CargarCatalogosFiltro()
        {
            // Departamentos
            cmbDepartment.ItemsSource = new ClassDepartment().Listar().DefaultView;
            cmbDepartment.DisplayMemberPath = "Description";
            cmbDepartment.SelectedValuePath = "Id";

            // Estados
            cmbState.ItemsSource = new ClassGeoState().Listar().DefaultView;
            cmbState.DisplayMemberPath = "Description";
            cmbState.SelectedValuePath = "Id";

            // Estados de Aplicante (Status)
            cmbStatusFilter.ItemsSource = ClassApplicants.AllApplicantStatuses;
            cmbStatusFilter.DisplayMemberPath = "Description";
            cmbStatusFilter.SelectedValuePath = "Id";
        }

        // Evento de cambio de selección de Estado para cargar Ciudades
        private void cmbState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbState.SelectedValue != null)
            {
                cmbCity.ItemsSource = new ClassGeoCity().ListarPorEstado((int)cmbState.SelectedValue).DefaultView;
                cmbCity.DisplayMemberPath = "Description";
                cmbCity.SelectedValuePath = "Id";
                cmbCity.IsEnabled = true; // Habilita la ciudad cuando se selecciona un estado
            }
            else
            {
                cmbCity.ItemsSource = null;
                cmbCity.IsEnabled = false; // Deshabilita si no hay estado seleccionado
            }
            cmbCity.SelectedIndex = -1; // Deselecciona cualquier ciudad previa
        }

        // Validación para Zip Code (solo números)
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Validación para Service Zip Codes (números y comas)
        private void ServiceZipCodesValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // --- EVENTOS DE PANTALLA DE FILTRO ---

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            // Validación del filtro obligatorio
            if (cmbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Department is a required filter.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbDepartment.Focus();
                return;
            }

            // Guardar filtros actuales antes de la búsqueda
            SaveCurrentFilters();

            // Realizar la búsqueda y mostrar resultados
            LlenarGridResultadosConFiltrosActuales();
            CtrlForm(ViewMode.Results); // Mostrar resultados
        }

        // Nuevo botón de Reset
        private void BtnResetFiltro_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFiltros();
            // No cambiamos la vista, simplemente limpiamos los filtros en la vista de filtro
        }

        private void BtnSalirFiltro_Click(object sender, RoutedEventArgs e)
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

        // --- EVENTOS DE PANTALLA DE RESULTADOS (y de detalle) ---

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (dgResultados.SelectedItem == null)
            {
                MessageBox.Show("Please select an applicant to view.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgResultados.SelectedItem;
            idAplicanteSeleccionado = (int)row["Id"];

            CargarDetallesAplicante(idAplicanteSeleccionado);
            CtrlForm(ViewMode.Detail); // Mostrar detalle
        }

        private void BtnHomeResultados_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar filtros (ya que volvemos al estado inicial del formulario)
            LimpiarFiltros();
            CtrlForm(ViewMode.Filter); // Volver a la pantalla de filtro
        }

        // Limpia los campos de filtro
        private void LimpiarFiltros()
        {
            cmbDepartment.SelectedValue = null;
            cmbState.SelectedValue = null;
            cmbCity.SelectedValue = null;
            cmbStatusFilter.SelectedValue = null;
            txtZipCodeFilter.Clear();
            txtServiceZipCodesFilter.Clear();
            txtFullNameFilter.Clear();
            dgResultados.ItemsSource = null; // Limpiar resultados anteriores
            // También limpiar los filtros guardados
            currentFilterIdDept = null;
            currentFilterIdState = null;
            currentFilterIdCity = null;
            currentFilterStatus = null;
            currentFilterZipCode = null;
            currentFilterServiceZipCodes = null;
            currentFilterFullName = null;
            // Asegurarse de que los controles de filtro estén habilitados al limpiar
            SetFilterControlsEnabled(true);
        }

        // Guarda los valores actuales de los filtros
        private void SaveCurrentFilters()
        {
            currentFilterIdDept = (int?)cmbDepartment.SelectedValue;
            currentFilterIdState = (int?)cmbState.SelectedValue;
            currentFilterIdCity = (int?)cmbCity.SelectedValue;
            currentFilterStatus = (int?)cmbStatusFilter.SelectedValue;
            currentFilterZipCode = txtZipCodeFilter.Text.Trim();
            currentFilterServiceZipCodes = txtServiceZipCodesFilter.Text.Trim();
            currentFilterFullName = txtFullNameFilter.Text.Trim();
        }

        // Helper para recargar el DataGrid de resultados manteniendo los filtros guardados
        private void LlenarGridResultadosConFiltrosActuales()
        {
            ClassApplicants objApplicants = new ClassApplicants();
            DataTable results = objApplicants.ListarFiltrado(currentFilterIdDept, currentFilterIdState, currentFilterIdCity, currentFilterStatus, currentFilterZipCode, currentFilterServiceZipCodes, currentFilterFullName);
            dgResultados.ItemsSource = results.DefaultView;
        }

        // --- EVENTOS DE PANTALLA DE DETALLE ---

        private void CargarDetallesAplicante(int id)
        {
            ClassApplicants applicant = new ClassApplicants();
            if (applicant.ObtenerPorId(id))
            {
                txtId.Text = applicant.PasswordText;
               
                txtEmail.Text = applicant.Email;
                txtFirstName.Text = applicant.FirstName;
                txtLastName.Text = applicant.LastName;
                // Para los nombres de departamento, estado y ciudad, los obtendremos de los filtros guardados o recargaremos
                txtDepartment.Text = new ClassDepartment().Listar().AsEnumerable()
                                        .FirstOrDefault(r => r.Field<int>("Id") == applicant.IdDepartment)?.Field<string>("Description") ?? "N/A";
                txtState.Text = new ClassGeoState().Listar().AsEnumerable()
                                    .FirstOrDefault(r => r.Field<int>("Id") == applicant.IdGeoState)?.Field<string>("Description") ?? "N/A";
                // Asegúrate de que IdGeoState no sea nulo antes de llamar a ListarPorEstado
                txtCity.Text = new ClassGeoCity().ListarPorEstado(applicant.IdGeoState ?? 0).AsEnumerable()
                                    .FirstOrDefault(r => r.Field<int>("Id") == applicant.IdGeoCity)?.Field<string>("Description") ?? "N/A";

                txtAddress.Text = applicant.Address;
                txtZipCode.Text = applicant.ZipCode;
                txtPhone.Text = applicant.Phone;
                txtDateCreated.Text = applicant.DateCreated.ToString("MM/dd/yyyy HH:mm");
                txtServiceZipCodes.Text = applicant.ServiceZipCodes;
                txtObservations.Text = applicant.Observations;

                // Cargar ComboBox de Status para edición
                cmbStatusEdit.ItemsSource = ClassApplicants.AllApplicantStatuses;
                cmbStatusEdit.DisplayMemberPath = "Description";
                cmbStatusEdit.SelectedValuePath = "Id";
                cmbStatusEdit.SelectedValue = applicant.Status; // Seleccionar el estado actual del aplicante
            }
            else
            {
                MessageBox.Show("Could not load applicant data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CtrlForm(ViewMode.Results); // Volver a los resultados si hay un error
            }
        }

        private void BtnActualizarStatus_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStatusEdit.SelectedValue == null)
            {
                MessageBox.Show("Please select a status to update.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ClassApplicants applicant = new ClassApplicants
            {
                Id = idAplicanteSeleccionado,
                Status = (int)cmbStatusEdit.SelectedValue
            };

            if (applicant.ActualizarStatus())
            {
                MessageBox.Show("Applicant status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Volvemos a la pantalla de resultados y refrescamos para ver el cambio
                CtrlForm(ViewMode.Results);
                LlenarGridResultadosConFiltrosActuales();
            }
            else
            {
                // El mensaje de error ya debería haberse mostrado desde la capa de datos
            }
        }

        private void BtnEliminarDetalle_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete this applicant (ID: {idAplicanteSeleccionado})?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClassApplicants applicant = new ClassApplicants();
                if (applicant.Eliminar(idAplicanteSeleccionado))
                {
                    MessageBox.Show("Applicant deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFiltros(); // Limpiar filtros y regresar a la pantalla de filtro
                    CtrlForm(ViewMode.Filter);
                }
                else
                {
                    // El mensaje de error ya debería haberse mostrado desde la capa de datos
                }
            }
        }

        private void BtnBackDetalle_Click(object sender, RoutedEventArgs e)
        {
            CtrlForm(ViewMode.Results); // Regresar a la pantalla de resultados
            LlenarGridResultadosConFiltrosActuales(); // Asegurar que el grid esté actualizado
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality for Applicants is not yet implemented.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}