using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Human_Resources.Data; // Para acceder a las clases de datos
using System.IO; // Para File.Exists y Path.Combine
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;

namespace Human_Resources.Forms
{
    public partial class frmContractGenerator : Page
    {
        // --- PROPIEDADES DE ESTADO DEL FORMULARIO ---
        private int _selectedStaffId = 0;
        private ClassContractGenerator _contractData = new ClassContractGenerator();

        // --- CONSTRUCTOR ---
        public frmContractGenerator()
        {
            InitializeComponent();
            ResetFormState(); // Inicializa el formulario en estado de búsqueda
        }

        // --- INICIALIZACIÓN Y GESTIÓN DE ESTADO ---

        // Definimos los modos de vista para el formulario
        private enum ViewMode
        {
            Search,         // Mostrando el área de búsqueda de Staff y botones de búsqueda
            SearchResults,  // Mostrando el DataGrid de resultados de Staff y botones de selección (filtro deshabilitado)
            ValidationPrint // Mostrando la lista de validación y botones de impresión (filtro deshabilitado)
        }

        // Controlar la visibilidad de los paneles y botones según el modo de vista
        private void CtrlForm(ViewMode mode)
        {
            pnlSearchStaff.Visibility = Visibility.Collapsed;
            GridStaffResults.Visibility = Visibility.Collapsed;
            pnlStaffSelectionButtons.Visibility = Visibility.Collapsed;
            pnlValidationPrint.Visibility = Visibility.Collapsed;

            // Siempre se ven los botones de Search y Exit
            BtnSearch.Visibility = Visibility.Visible;
            BtnExitSearch.Visibility = Visibility.Visible;

            // Deshabilitar/Habilitar controles de búsqueda
            txtSearchStaff.IsEnabled = true;
            BtnSearch.IsEnabled = true;

            switch (mode)
            {
                case ViewMode.Search:
                    lblTitulo.Text = "CONTRACT GENERATOR - SELECT STAFF";
                    pnlSearchStaff.Visibility = Visibility.Visible; // Controles de búsqueda visibles y activos
                    txtSearchStaff.Focus();
                    break;

                case ViewMode.SearchResults:
                    lblTitulo.Text = "CONTRACT GENERATOR - SELECT STAFF";
                    pnlSearchStaff.Visibility = Visibility.Visible; // Controles de búsqueda visibles pero deshabilitados
                    txtSearchStaff.IsEnabled = false;
                    BtnSearch.IsEnabled = false;
                    GridStaffResults.Visibility = Visibility.Visible; // DataGrid visible
                    pnlStaffSelectionButtons.Visibility = Visibility.Visible; // Botones "Select" y "Home" visibles
                    break;

                case ViewMode.ValidationPrint:
                    lblTitulo.Text = $"CONTRACT FOR: {_contractData.StaffData.Name} {_contractData.StaffData.LastName}";
                    pnlValidationPrint.Visibility = Visibility.Visible; // Panel de validación visible
                    // Ocultamos los botones de búsqueda y selección, mostramos los de impresión/atrás
                    BtnSearch.Visibility = Visibility.Collapsed;
                    BtnExitSearch.Visibility = Visibility.Collapsed; // Exit propio del form de impresión
                    break;
            }
        }

        private void ResetFormState()
        {
            txtSearchStaff.Clear();
            dgStaffResults.ItemsSource = null;
            _selectedStaffId = 0;
            _contractData = new ClassContractGenerator(); // Reinicializar datos del contrato
            lstValidationResults.ItemsSource = null;
            BtnPrint.IsEnabled = false; // Deshabilitar impresión
            CtrlForm(ViewMode.Search);
        }

        // --- EVENTOS DE PANTALLA DE BÚSQUEDA DE STAFF ---

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string filter = txtSearchStaff.Text.Trim();
            if (string.IsNullOrWhiteSpace(filter))
            {
                MessageBox.Show("Please enter a name or last name to search for staff.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSearchStaff.Focus();
                return;
            }

            ClassStaff staffManager = new ClassStaff();
            // Modificamos Listar en ClassStaff para incluir DepartmentName si es necesario o un JOIN
            // Por ahora, asumimos que Listar ya trae el DepartmentName si lo requiere el DataGrid
            DataTable results = staffManager.Listar(filter);

            if (results != null && results.Rows.Count > 0)
            {
                dgStaffResults.ItemsSource = results.DefaultView;
                CtrlForm(ViewMode.SearchResults); // Mostrar resultados en DataGrid
            }
            else
            {
                MessageBox.Show("No staff found matching your search criteria.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                dgStaffResults.ItemsSource = null;
                CtrlForm(ViewMode.Search); // Quedarse en la vista de búsqueda si no hay resultados
            }
        }

        private void txtSearchStaff_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Opcional: Podrías hacer una búsqueda en tiempo real aquí si la lista no es muy grande.
            // Por ahora, solo se activa con el botón Search.
        }

        private async void BtnSelectStaff_Click(object sender, RoutedEventArgs e)
        {
            if (dgStaffResults.SelectedItem == null)
            {
                MessageBox.Show("Please select a staff member from the list.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgStaffResults.SelectedItem;
            _selectedStaffId = (int)row["id"];

            // Cargar todos los datos necesarios para el contrato
            // Esto es asíncrono si ClassGeocoding.GetCoordinatesAsync se llama internamente,
            // pero para esta funcionalidad, LoadContractData es síncrono.
            if (_contractData.LoadContractData(_selectedStaffId))
            {
                DisplayValidationChecklist(); // Mostrar checklist de validación
                CtrlForm(ViewMode.ValidationPrint);
            }
            // Si LoadContractData falló, ya mostró un MessageBox, no hace falta más aquí.
        }

        private void BtnHomeSearch_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar búsqueda y volver a la pantalla de búsqueda inicial
            ResetFormState();
        }

        // --- LÓGICA DE VALIDACIÓN Y PANTALLA DE IMPRESIÓN ---

        private void DisplayValidationChecklist()
        {
            Dictionary<string, string> pdfFieldValues = _contractData.MapDataToPdfFields();
            List<PdfFieldValidation> validations = _contractData.ValidatePdfFields(pdfFieldValues);

            lstValidationResults.ItemsSource = validations;

            // Habilitar o deshabilitar el botón de imprimir
            bool allFieldsPresent = validations.All(v => v.IsPresent);
            BtnPrint.IsEnabled = allFieldsPresent;

            if (!allFieldsPresent)
            {
                MessageBox.Show("Some required data fields are missing. Please update the staff member's profile before printing.", "Data Incomplete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!BtnPrint.IsEnabled) // Doble verificación por si acaso
            {
                MessageBox.Show("Cannot print: some required data fields are missing or not valid.", "Print Blocked", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Mapear datos a campos PDF (de nuevo, para asegurar los últimos valores)
            Dictionary<string, string> pdfFieldValues = _contractData.MapDataToPdfFields();

            // Intentar imprimir el PDF
            if (_contractData.PrintPdfContract(pdfFieldValues))
            {
                MessageBox.Show("Contract sent to printer successfully.", "Print Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetFormState(); // Volver a la pantalla de búsqueda inicial y limpiar
            }
            // Si PrintPdfContract falla, ya muestra un MessageBox.
        }

        private void BtnBackToSearch_Click(object sender, RoutedEventArgs e)
        {
            CtrlForm(ViewMode.SearchResults); // Regresar a los resultados de búsqueda de Staff
            // No es necesario recargar el DataGrid, ya tiene los resultados previos.
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