using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Human_Resources.Data;
using System.IO;

namespace Human_Resources.Forms
{
    public partial class frmContractGenerator : Page
    {
        // --- PROPIEDADES DE ESTADO ---
        private int _selectedStaffId = 0;
        private ClassContractGenerator _contractData = new ClassContractGenerator();

        public enum ViewMode
        {
            Search,
            SearchResults,
            ValidationPrint
        }

        // --- CONSTRUCTOR ---
        public frmContractGenerator()
        {
            InitializeComponent();
            ResetFormState();
        }

        // --- GESTIÓN DE ESTADO Y VISIBILIDAD ---
        private void CtrlForm(ViewMode mode)
        {
            // 1. Reset de visibilidad inicial
            GridStaffResults.Visibility = Visibility.Collapsed;
            pnlValidationPrint.Visibility = Visibility.Collapsed;
            pnlSearchButtons.Visibility = Visibility.Collapsed;
            pnlPrintButtons.Visibility = Visibility.Collapsed;

            // 2. Configuración por modo
            switch (mode)
            {
                case ViewMode.Search:
                    lblTitulo.Text = "CONTRACT GENERATOR - SELECT STAFF";
                    txtSearchStaff.IsEnabled = true;
                    BtnSearch.IsEnabled = true;
                    txtSearchStaff.Focus();
                    break;

                case ViewMode.SearchResults:
                    lblTitulo.Text = "CONTRACT GENERATOR - SELECT STAFF";
                    txtSearchStaff.IsEnabled = false; // Bloqueamos para que no filtren mientras seleccionan
                    BtnSearch.IsEnabled = false;
                    GridStaffResults.Visibility = Visibility.Visible;
                    pnlSearchButtons.Visibility = Visibility.Visible;
                    BtnSelectStaff.Visibility = Visibility.Visible;
                    BtnHomeSearch.Visibility = Visibility.Visible;
                    break;

                case ViewMode.ValidationPrint:
                    lblTitulo.Text = $"CONTRACT FOR: {_contractData.StaffData.Name} {_contractData.StaffData.LastName}";
                    pnlValidationPrint.Visibility = Visibility.Visible;
                    pnlPrintButtons.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ResetFormState()
        {
            txtSearchStaff.Clear();
            dgStaffResults.ItemsSource = null;
            _selectedStaffId = 0;
            _contractData = new ClassContractGenerator();
            lstValidationResults.ItemsSource = null;
            BtnPrint.IsEnabled = false;

            CtrlForm(ViewMode.Search);
        }

        // --- INDICADORES DE CARGA ---
        private void ShowProgressIndicator(string message)
        {
            txtProgressMessage.Text = message;
            pnlProgressOverlay.Visibility = Visibility.Visible;
            this.IsEnabled = false;
        }

        private void HideProgressIndicator()
        {
            pnlProgressOverlay.Visibility = Visibility.Collapsed;
            this.IsEnabled = true;
        }

        // --- EVENTOS ---

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string filter = txtSearchStaff.Text.Trim();
            if (string.IsNullOrWhiteSpace(filter))
            {
                MessageBox.Show("Please enter a name or last name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ClassStaff staffManager = new ClassStaff();
            DataTable results = staffManager.ListarParaContratos(filter);

            if (results != null && results.Rows.Count > 0)
            {
                dgStaffResults.ItemsSource = results.DefaultView;
                CtrlForm(ViewMode.SearchResults);
            }
            else
            {
                MessageBox.Show("No staff found.", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnSelectStaff_Click(object sender, RoutedEventArgs e)
        {
            if (dgStaffResults.SelectedItem == null)
            {
                MessageBox.Show("Please select a staff member.", "Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgStaffResults.SelectedItem;
            _selectedStaffId = (int)row["id"];

            ShowProgressIndicator("Loading and validating data...");

            // Usamos Task.Run para no congelar la UI mientras carga de la DB
            bool loadSuccess = await Task.Run(() => _contractData.LoadContractData(_selectedStaffId));

            HideProgressIndicator();

            if (loadSuccess)
            {
                DisplayValidationChecklist();
                CtrlForm(ViewMode.ValidationPrint);
            }
        }

        private void DisplayValidationChecklist()
        {
            var pdfFieldValues = _contractData.MapDataToPdfFields();
            var validations = _contractData.ValidatePdfFields(pdfFieldValues);

            lstValidationResults.ItemsSource = validations;
            BtnPrint.IsEnabled = validations.All(v => v.IsPresent);

            if (!BtnPrint.IsEnabled)
            {
                MessageBox.Show("Required fields are missing in the staff profile.", "Data Incomplete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var pdfFieldValues = _contractData.MapDataToPdfFields();

            ShowProgressIndicator("Printing the Contract... Please check printer.");

            bool printSuccess = await Task.Run(() => _contractData.PrintPdfContract(pdfFieldValues));

            HideProgressIndicator();

            if (printSuccess)
            {
                MessageBox.Show("Successfully sent to printer.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetFormState();
            }
        }

        private void BtnBackToSearch_Click(object sender, RoutedEventArgs e)
        {
            CtrlForm(ViewMode.SearchResults);
        }

        private void BtnHomeSearch_Click(object sender, RoutedEventArgs e)
        {
            ResetFormState();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var principal = Window.GetWindow(this) as MainWindow;
            if (principal?.tcPrincipal != null)
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
                if (tabToClose != null) principal.tcPrincipal.Items.Remove(tabToClose);
            }
        }

        private void txtSearchStaff_TextChanged(object sender, TextChangedEventArgs e) { }
    }
}