using System;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WinForms = System.Windows.Forms;


namespace Human_Resources.Forms
{
    public partial class frmFindStaff : Page
    {
        private Data.ClassFindStaff objData = new Data.ClassFindStaff();

        public frmFindStaff()
        {
            InitializeComponent();
            LoadInitialData();
            EstadoFormulario(0); // PASO 1 al arrancar
        }

        /// <summary>
        /// TORRE DE CONTROL - Gestión de los 3 Pasos
        /// </summary>
        private void EstadoFormulario(int estado)
        {
            switch (estado)
            {
                case 0: // PASO 1: FILTROS
                    gridFiltros.Visibility = Visibility.Visible;
                    gridFiltros.IsEnabled = true;
                    txtSelectedStaff.Visibility = Visibility.Collapsed;
                    dgListado.Visibility = Visibility.Collapsed;
                    dgDocuments.Visibility = Visibility.Collapsed;

                    btnSearch.IsEnabled = true;
                    btnExit.IsEnabled = true;
                    btnViewDetail.IsEnabled = false;
                    btnPrint.IsEnabled = false; // Nada que imprimir aún
                    btnGoBack.IsEnabled = false;
                    break;

                case 2: // PASO 2: LISTADO DE ENFERMEROS
                    gridFiltros.Visibility = Visibility.Visible;
                    gridFiltros.IsEnabled = false;



                    txtSelectedStaff.Visibility = Visibility.Collapsed;
                    dgListado.Visibility = Visibility.Visible;
                    dgDocuments.Visibility = Visibility.Collapsed;

                    btnSearch.IsEnabled = true;
                    btnExit.IsEnabled = true;
                    btnGoBack.IsEnabled = true;
                    btnViewDetail.IsEnabled = true;
                    btnPrint.IsEnabled = true; // Imprimir listado general
                    btnGoBack.Content = "Back to Filter";
                    break;

                case 3: // PASO 3: DETALLE DE DOCUMENTOS
                    gridFiltros.Visibility = Visibility.Collapsed;
                    txtSelectedStaff.Visibility = Visibility.Visible;
                    dgListado.Visibility = Visibility.Collapsed;
                    dgDocuments.Visibility = Visibility.Visible;

                    btnSearch.IsEnabled = false;
                    btnExit.IsEnabled = false;
                    btnViewDetail.IsEnabled = false;
                    btnPrint.IsEnabled = true; // Imprimir detalle individual
                    btnGoBack.Content = "Home";
                    break;
            }
        }

        private void LoadInitialData()
        {
            try
            {
                // Department:
                cbDepartment.ItemsSource = objData.ObtenerDepartementos().DefaultView;
                cbDepartment.DisplayMemberPath = "Description";
                cbDepartment.SelectedValuePath = "id";

                // License
                cbLicence.IsEnabled = false;
                cbLicence.ItemsSource = null;


                // Región:
                cbRegion.ItemsSource = objData.ObtenerRegiones().DefaultView;
                cbRegion.DisplayMemberPath = "Description";
                cbRegion.SelectedValuePath = "id";

                // Estados
                cbState.ItemsSource = objData.ObtenerState().DefaultView;
                cbState.DisplayMemberPath = "Description";
                cbState.SelectedValuePath = "id";

                cbCity.IsEnabled = false;
                cbCity.ItemsSource = null;

                // Documentos de Imigracion 
                cbImmigration.ItemsSource = objData.ObtenerDocsInmigration().DefaultView;
                cbImmigration.DisplayMemberPath = "Description";
                cbImmigration.SelectedValuePath = "id";


                // Bancos
                cbBank.ItemsSource = objData.ObtenerBancos().DefaultView;
                cbBank.DisplayMemberPath = "Description"; // Ajusta al nombre de tu columna
                cbBank.SelectedValuePath = "id";

                // Payrool Type
                cbPayment.ItemsSource = objData.ObtenerDocsPayroll().DefaultView;
                cbPayment.DisplayMemberPath = "Description"; // Ajusta al nombre de tu columna
                cbPayment.SelectedValuePath = "id";


                // Payroll Method
                cbMethod.ItemsSource = objData.ObtenerBanksMethod().DefaultView;
                cbMethod.DisplayMemberPath = "Description"; // Ajusta al nombre de tu columna
                cbMethod.SelectedValuePath = "id";



            }
            catch (Exception ex)
            {
                // Es vital atrapar errores aquí por si falla la DB
                Console.WriteLine("Error cargando combos: " + ex.Message);
            }
        }


        private void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Verificamos que haya algo seleccionado
            if (cbDepartment.SelectedValue != null)
            {
                try
                {
                    int idDepartment = Convert.ToInt32(cbDepartment.SelectedValue);

                    // 2. Cargamos las ciudades filtradas
                    DataTable dtLicense = objData.ObtenerLicense(idDepartment);
                    cbLicence.ItemsSource = dtLicense.DefaultView;
                    cbLicence.DisplayMemberPath = "Description";
                    cbLicence.SelectedValuePath = "Id";

                    if (cbLicence.Items.Count > 0)
                    {
                        cbLicence.IsEnabled = true;
                    }
                    else
                    {
                        cbLicence.IsEnabled = false;
                        cbLicence.ItemsSource = null;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Department: " + ex.Message);
                }
            }
            else
            {
                // Si no hay estado seleccionado, reseteamos y bloqueamos ciudades
                cbLicence.ItemsSource = null;
                cbLicence.IsEnabled = false;
            }
        }


        private void cbState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Verificamos que haya algo seleccionado
            if (cbState.SelectedValue != null)
            {
                try
                {
                    int idEstado = Convert.ToInt32(cbState.SelectedValue);

                    // 2. Cargamos las ciudades filtradas
                    DataTable dtCiudades = objData.ObtenerCity(idEstado);
                    cbCity.ItemsSource = dtCiudades.DefaultView;
                    cbCity.DisplayMemberPath = "Description";
                    cbCity.SelectedValuePath = "Id";

                    if (cbCity.Items.Count > 0)
                    {
                        cbCity.IsEnabled = true;
                    }
                    else
                    {
                        cbCity.IsEnabled = false;
                        cbCity.ItemsSource = null;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading cities: " + ex.Message);
                }
            }
            else
            {
                // Si no hay estado seleccionado, reseteamos y bloqueamos ciudades
                cbCity.ItemsSource = null;
                cbCity.IsEnabled = false;
            }
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Solo permite caracteres del 0 al 9
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #region EVENTOS

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verifico que tenga el Departamento Seleccionado
                if (cbDepartment.SelectedValue == null)
                {
                    MessageBox.Show("You must select a department.",
                                    "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Detiene la ejecución
                }
                // 1. VALIDACIONES DE FECHAS
                if (dtFrom.SelectedDate == null || dtUntil.SelectedDate == null)
                {
                    MessageBox.Show("Both 'From' and 'Until' dates are required to search for expired documents.",
                                    "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Detiene la ejecución
                }

                if (dtFrom.SelectedDate > dtUntil.SelectedDate)
                {
                    MessageBox.Show("The 'From' date cannot be later than the 'Until' date.",
                                    "Date Range Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Detiene la ejecución
                }

                // 2. CAPTURA DE DATOS (Una vez validados)
                string name = txtSearch.Text;
                bool active = chkActive.IsChecked ?? false;
                DateTime from = dtFrom.SelectedDate.Value;
                DateTime until = dtUntil.SelectedDate.Value;

                // Capturamos los IDs de los combos (si no hay selección, enviamos 0)
                int idDepartment = cbDepartment.SelectedValue != null ? Convert.ToInt32(cbDepartment.SelectedValue) : 0;
                int idRegion = cbRegion.SelectedValue != null ? Convert.ToInt32(cbRegion.SelectedValue) : 0;
                int idState = cbState.SelectedValue != null ? Convert.ToInt32(cbState.SelectedValue) : 0;
                int idCity = cbCity.SelectedValue != null ? Convert.ToInt32(cbCity.SelectedValue) : 0;
                int idInmigration = cbImmigration.SelectedValue != null ? Convert.ToInt32(cbImmigration.SelectedValue) : 0;
                int idBank = cbBank.SelectedValue != null ? Convert.ToInt32(cbBank.SelectedValue) : 0;
                int idPayroll = cbPayment.SelectedValue != null ? Convert.ToInt32(cbPayment.SelectedValue) : 0;
                int idMethod = cbMethod.SelectedValue != null ? Convert.ToInt32(cbMethod.SelectedValue) : 0;
                int idLicense = cbLicence.SelectedValue != null ? Convert.ToInt32(cbLicence.SelectedValue) : 0;



                // 3. LLAMADA A LA BASE DE DATOS
                // Ajusté los parámetros según lo que hemos ido agregando
                DataTable dt = objData.ObtenerStaffConVencimientos(idDepartment, name, idRegion, active, from, until, idState, idCity, txtZip.Text, idLicense, idPayroll, idBank, idMethod, idInmigration);

                dgListado.ItemsSource = dt.DefaultView;

                if (dt.Rows.Count > 0)
                {
                    EstadoFormulario(2);
                }
                else
                {
                    MessageBox.Show("No staff members found with expired documents in the selected range.",
                                    "btenerStaffConVencimientos", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("You must select a record.", // Mensaje
                "Validation Error",                          // Título de la ventana
                MessageBoxButton.OK,                         // Botones (OK, OKCancel, YesNo)
                MessageBoxImage.Warning);                    // Icono (Error, Warning, Information)
                return;
            }

            try
            {
                DataRowView row = (DataRowView)dgListado.SelectedItem;
                int idStaff = Convert.ToInt32(row["id"]);
                string fullName = row["FullName"].ToString();
                int idDepartment = cbDepartment.SelectedValue != null ? Convert.ToInt32(cbDepartment.SelectedValue) : 0;

                // Recuperamos las fechas del filtro para que el detalle sea exacto
                DateTime desde = dtFrom.SelectedDate.Value;
                DateTime hasta = dtUntil.SelectedDate.Value;

                txtSelectedStaff.Text = "Staff: " + fullName;

                // Enviamos las fechas al nuevo método
                dgDocuments.ItemsSource = objData.ObtenerDetalleVencidosPorStaff(idStaff, idDepartment, desde, hasta).DefaultView;

                EstadoFormulario(3);
            }
            catch (Exception ex) { MessageBox.Show("Detail Error: " + ex.Message, "btnViewDetail_Click"); }
        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            // Lógica de navegación inversa
            if (dgDocuments.Visibility == Visibility.Visible) // Si estoy en el 3
            {
                EstadoFormulario(2); // Home total
            }
            else if (dgListado.Visibility == Visibility.Visible) // Si estoy en el 2
            {
                EstadoFormulario(0); // Volver al filtro
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                foreach (TabItem item in mainWindow.tcPrincipal.Items)
                {
                    if (item.Content is Frame f && f.Content == this)
                    {
                        mainWindow.tcPrincipal.Items.Remove(item);
                        break;
                    }
                }
            }
        }


        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            // Reset de campos
            txtSearch.Clear();
            dtFrom.SelectedDate = null;
            dtUntil.SelectedDate = null;
            dgListado.ItemsSource = null;
            // Limpiando Combos
            cbDepartment.SelectedValue = null;
            cbRegion.SelectedValue = null;
            cbState.SelectedValue = null;
            cbCity.SelectedValue = null;
            cbLicence.SelectedValue = null;
            cbImmigration.SelectedValue = null;
            cbBank.SelectedValue = null;
            cbPayment.SelectedValue = null;
            cbMethod.SelectedValue = null;
            cbImmigration.SelectedValue = null;
            txtZip.Text = "";

            // Limpieza de fuentes de datos
            dgListado.ItemsSource = null;
            dgDocuments.ItemsSource = null;
            EstadoFormulario(0);
        }

        // MÉTODO DE IMPRESIÓN
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new WinForms.FolderBrowserDialog();
                dialog.Description = "Select the folder to save the reports";

                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;

                    if (dgDocuments.Visibility == Visibility.Visible)
                    {
                        // --- CASO 1: INDIVIDUAL ---
                        if (dgDocuments.ItemsSource == null || dgListado.SelectedItem == null) return;

                        DataRowView selectedStaff = (DataRowView)dgListado.SelectedItem;

                        int idStaff = 0;
                        if (selectedStaff.Row.Table.Columns.Contains("id")) idStaff = Convert.ToInt32(selectedStaff["id"]);
                        else if (selectedStaff.Row.Table.Columns.Contains("ID")) idStaff = Convert.ToInt32(selectedStaff["ID"]);

                        string nombreStaff = "Unknown";
                        if (selectedStaff.Row.Table.Columns.Contains("Full Name")) nombreStaff = selectedStaff["Full Name"].ToString();
                        else if (selectedStaff.Row.Table.Columns.Contains("FullName")) nombreStaff = selectedStaff["FullName"].ToString();

                        DataTable dtParaReporte = ((DataView)dgDocuments.ItemsSource).ToTable();
                        ProcesarYGuardarPDF(dtParaReporte, folderPath, nombreStaff, idStaff);
                    }
                    else
                    {
                        // --- CASO 2: MASIVO ---
                        DataTable dtLote = objData.ObtenerDetalleVencidosMasivo(
                            cbDepartment.SelectedValue != null ? Convert.ToInt32(cbDepartment.SelectedValue) : 0,
                            txtSearch.Text,
                            cbRegion.SelectedValue != null ? Convert.ToInt32(cbRegion.SelectedValue) : 0,
                            chkActive.IsChecked ?? false,
                            dtFrom.SelectedDate.Value,
                            dtUntil.SelectedDate.Value,
                            cbState.SelectedValue != null ? Convert.ToInt32(cbState.SelectedValue) : 0,
                            cbCity.SelectedValue != null ? Convert.ToInt32(cbCity.SelectedValue) : 0,
                            txtZip.Text,
                            cbLicence.SelectedValue != null ? Convert.ToInt32(cbLicence.SelectedValue) : 0,
                            cbPayment.SelectedValue != null ? Convert.ToInt32(cbPayment.SelectedValue) : 0,
                            cbBank.SelectedValue != null ? Convert.ToInt32(cbBank.SelectedValue) : 0,
                            cbMethod.SelectedValue != null ? Convert.ToInt32(cbMethod.SelectedValue) : 0,
                            cbImmigration.SelectedValue != null ? Convert.ToInt32(cbImmigration.SelectedValue) : 0
                        );

                        if (dtLote == null || dtLote.Rows.Count == 0) return;

                        string colNombreReal = dtLote.Columns.Contains("Full Name") ? "Full Name" : "FullName";

                        var staffGroups = dtLote.AsEnumerable().GroupBy(r => r[colNombreReal].ToString());

                        foreach (var group in staffGroups)
                        {
                            string nombreEnfermero = group.Key;
                            DataTable dtStaff = group.CopyToDataTable();

                            // --- BÚSQUEDA DEL ID REAL ---
                            int idReal = 0;
                            // Buscamos en el DataGrid de la izquierda (dgListado) al enfermero que coincida con este nombre
                            if (dgListado.ItemsSource != null)
                            {
                                foreach (DataRowView item in dgListado.ItemsSource)
                                {
                                    string nombreEnLista = item.Row.Table.Columns.Contains("FullName") ? item["FullName"].ToString() : item["Full Name"].ToString();
                                    if (nombreEnLista == nombreEnfermero)
                                    {
                                        idReal = item.Row.Table.Columns.Contains("id") ? Convert.ToInt32(item["id"]) : (item.Row.Table.Columns.Contains("ID") ? Convert.ToInt32(item["ID"]) : 0);
                                        break;
                                    }
                                }
                            }

                            ProcesarYGuardarPDF(dtStaff, folderPath, nombreEnfermero, idReal);
                        }
                    }

                    MessageBox.Show("Reports generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Printing Error");
            }
        }



        private void ProcesarYGuardarPDF(DataTable dtStaff, string folderPath, string staffName, int idStaff)
        {
            try
            {
                // 1. Limpiamos el nombre para el archivo físico
                string nombreLimpio = staffName.Replace(" ", "_").Replace(",", "").Replace(".", "");
                string fileName = $"{idStaff}_DR_{nombreLimpio}.xps";
                string fullPath = System.IO.Path.Combine(folderPath, fileName);

                FlowDocument doc = new FlowDocument();

                // CONFIGURACIÓN DE PÁGINA (Tamaño Carta)
                doc.PageHeight = 1056;
                doc.PageWidth = 816;
                doc.PagePadding = new Thickness(50);
                doc.ColumnWidth = 816;
                doc.FontFamily = new FontFamily("Verdana");
                doc.FontSize = 10;

                // --- 1. ENCABEZADO ---
                Grid headerGrid = new Grid() { Width = 716 };
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                var principal = Window.GetWindow(this) as MainWindow;
                string nombreCompania = principal != null ? principal.Title.ToString() : "COMPANY NAME";

                TextBlock txtEmpresa = new TextBlock(new Bold(new Run(nombreCompania))) { FontSize = 10, TextAlignment = TextAlignment.Left };
                TextBlock txtFecha = new TextBlock(new Run("Date: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm"))) { FontSize = 9, TextAlignment = TextAlignment.Right };

                headerGrid.Children.Add(txtEmpresa);
                Grid.SetColumn(txtEmpresa, 0);
                headerGrid.Children.Add(txtFecha);
                Grid.SetColumn(txtFecha, 1);
                doc.Blocks.Add(new BlockUIContainer(headerGrid));

                // --- 2. TÍTULOS ---
                doc.Blocks.Add(new Paragraph(new Run("EXPIRED DOCUMENTS"))
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 30, 0, 5)
                });

                // Aquí incluimos el ID en el título visual para mayor claridad
                doc.Blocks.Add(new Paragraph(new Run($"Staff : {staffName}"))
                {
                    TextAlignment = TextAlignment.Left,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                });

                string fechaFiltro = (dtFrom.SelectedDate.HasValue && dtUntil.SelectedDate.HasValue)
                    ? $"{dtFrom.SelectedDate.Value:MM/dd/yyyy} to {dtUntil.SelectedDate.Value:MM/dd/yyyy}"
                    : "All dates";

                doc.Blocks.Add(new Paragraph(new Run($"Filters: {fechaFiltro}"))
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 9,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 20)
                });

                // --- 3. TABLA ---
                Table table = new Table() { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 0, 1) };
                table.Columns.Add(new TableColumn() { Width = new GridLength(600) });
                table.Columns.Add(new TableColumn() { Width = new GridLength(120) });

                TableRowGroup group = new TableRowGroup();
                TableRow headerRow = new TableRow() { Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)), FontWeight = FontWeights.Bold };
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))) { Padding = new Thickness(5) });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Exp. Date"))) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });
                group.Rows.Add(headerRow);

                int i = 0;
                foreach (DataRow row in dtStaff.Rows)
                {
                    TableRow r = new TableRow();
                    if (i % 2 != 0) r.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));

                    // PROTECCIÓN: Verificamos si la columna existe antes de usarla
                    string desc = dtStaff.Columns.Contains("Description") ? row["Description"].ToString() : "No Description";
                    r.Cells.Add(new TableCell(new Paragraph(new Run(desc)) { Margin = new Thickness(0) }) { Padding = new Thickness(5) });

                    string dateStr = "N/A";
                    if (dtStaff.Columns.Contains("DateDoc"))
                    {
                        dateStr = row["DateDoc"] == DBNull.Value ? "MISSING" : Convert.ToDateTime(row["DateDoc"]).ToString("MM/dd/yyyy");
                    }

                    r.Cells.Add(new TableCell(new Paragraph(new Run(dateStr)) { Margin = new Thickness(0) }) { Padding = new Thickness(5), TextAlignment = TextAlignment.Right });

                    group.Rows.Add(r);
                    i++;
                }

                table.RowGroups.Add(group);
                doc.Blocks.Add(table);

                // --- 4. FOOTER ---
                doc.Blocks.Add(new Paragraph(new Run("Page 1 of 1")) { TextAlignment = TextAlignment.Right, FontSize = 8, Foreground = Brushes.Gray, Margin = new Thickness(0, 50, 0, 0) });

                // LLAMADA FINAL AL DATA
                objData.ImprimirDocSilencioso(doc, fullPath);
            }
            catch (Exception ex)
            {
                // Si falla un PDF, que nos diga cuál fue
                throw new Exception($"Error processing PDF for {staffName}: {ex.Message}");
            }
        }
        #endregion
    }
}