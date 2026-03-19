using System;
using System.ComponentModel;
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
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;
using WinForms = System.Windows.Forms;


namespace Human_Resources.Forms
{
    public partial class frmFindStaffAdditional : Page
    {
        private Data.ClassFindStaffAdditional objData = new Data.ClassFindStaffAdditional();
        public frmFindStaffAdditional()
        {
            InitializeComponent();
            LoadInitialData();
            EstadoFormulario(1); // PASO 1 al arrancar
        }

        /// <summary>
        /// TORRE DE CONTROL - Gestión de los 3 Pasos
        /// </summary>
        private void EstadoFormulario(int estado)
        {
            switch (estado)
            {
                case 1: // PASO 1: FILTROS
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
                // En este formulario las fechas ya no son obligatorias para buscar faltantes
                DataTable dt = objData.ObtenerStaffConFaltantes(
                    cbDepartment.SelectedValue != null ? Convert.ToInt32(cbDepartment.SelectedValue) : 0,
                    txtSearch.Text,
                    cbRegion.SelectedValue != null ? Convert.ToInt32(cbRegion.SelectedValue) : 0,
                    chkActive.IsChecked ?? false,
                    cbState.SelectedValue != null ? Convert.ToInt32(cbState.SelectedValue) : 0,
                    cbCity.SelectedValue != null ? Convert.ToInt32(cbCity.SelectedValue) : 0,
                    txtZip.Text,
                    cbLicence.SelectedValue != null ? Convert.ToInt32(cbLicence.SelectedValue) : 0,
                    cbPayment.SelectedValue != null ? Convert.ToInt32(cbPayment.SelectedValue) : 0,
                    cbBank.SelectedValue != null ? Convert.ToInt32(cbBank.SelectedValue) : 0,
                    cbMethod.SelectedValue != null ? Convert.ToInt32(cbMethod.SelectedValue) : 0,
                    cbImmigration.SelectedValue != null ? Convert.ToInt32(cbImmigration.SelectedValue) : 0

                );

                if (dt == null || dt.Rows.Count == 0)
                {
                    // Limpiamos el DataGrid para que no muestre datos viejos
                    dgListado.ItemsSource = null;

                    MessageBox.Show("No information exists with the entered filter. Please try other criteria.",
                                    "No Results Found",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return; // Salimos para no ejecutar el cambio de estado del formulario
                }

                dgListado.ItemsSource = dt.DefaultView;
                EstadoFormulario(2);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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

            if (dgListado.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgListado.SelectedItem;
            int idStaff = Convert.ToInt32(row["id"]);

            DataTable dtDocs = objData.ObtenerDetalleFaltantes(idStaff);
            dgDocuments.ItemsSource = dtDocs.DefaultView;

            txtSelectedStaff.Text = $"Pending Additional Docs: {row["FullName"]}";
            txtSelectedStaff.Visibility = Visibility.Visible;
            EstadoFormulario(3);
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
                EstadoFormulario(1); // Volver al filtro
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
            EstadoFormulario(1);
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
                        // --- CASO 1: INDIVIDUAL (Paso 3) ---
                        if (dgDocuments.ItemsSource == null || dgListado.SelectedItem == null) return;

                        DataRowView selectedStaff = (DataRowView)dgListado.SelectedItem;

                        // Intento obtener ID (flexible a id o ID)
                        int idStaff = selectedStaff.Row.Table.Columns.Contains("id")
                            ? Convert.ToInt32(selectedStaff["id"])
                            : (selectedStaff.Row.Table.Columns.Contains("ID") ? Convert.ToInt32(selectedStaff["ID"]) : 0);

                        // Intento obtener Nombre (flexible a Full Name o FullName)
                        string nombreStaff = selectedStaff.Row.Table.Columns.Contains("Full Name")
                            ? selectedStaff["Full Name"].ToString()
                            : (selectedStaff.Row.Table.Columns.Contains("FullName") ? selectedStaff["FullName"].ToString() : "Unknown");

                        DataTable dtParaReporte = ((DataView)dgDocuments.ItemsSource).ToTable();
                        ProcesarYGuardarPDF(dtParaReporte, folderPath, nombreStaff, idStaff);
                    }
                    else
                    {
                        // --- CASO 2: MASIVO (Desde el Paso 2) ---
                        // IMPORTANTE: Cambié el nombre del método para que coincida con tu nueva lógica de "Faltantes"
                        DataTable dtLote = objData.ObtenerDetalleFaltantesMasivo(
                            cbDepartment.SelectedValue != null ? Convert.ToInt32(cbDepartment.SelectedValue) : 0,
                            txtSearch.Text,
                            cbRegion.SelectedValue != null ? Convert.ToInt32(cbRegion.SelectedValue) : 0,
                            chkActive.IsChecked ?? false,
                            cbState.SelectedValue != null ? Convert.ToInt32(cbState.SelectedValue) : 0,
                            cbCity.SelectedValue != null ? Convert.ToInt32(cbCity.SelectedValue) : 0,
                            txtZip.Text,
                            cbLicence.SelectedValue != null ? Convert.ToInt32(cbLicence.SelectedValue) : 0,
                            cbPayment.SelectedValue != null ? Convert.ToInt32(cbPayment.SelectedValue) : 0,
                            cbBank.SelectedValue != null ? Convert.ToInt32(cbBank.SelectedValue) : 0,
                            cbMethod.SelectedValue != null ? Convert.ToInt32(cbMethod.SelectedValue) : 0,
                            cbImmigration.SelectedValue != null ? Convert.ToInt32(cbImmigration.SelectedValue) : 0
                        );

                        if (dtLote == null || dtLote.Rows.Count == 0)
                        {
                            MessageBox.Show("No missing documents found for the selected filters.", "Information");
                            return;
                        }

                        string colNombreReal = dtLote.Columns.Contains("Full Name") ? "Full Name" : "FullName";

                        // Agrupamos por nombre de Staff
                        var staffGroups = dtLote.AsEnumerable().GroupBy(r => r[colNombreReal].ToString());

                        foreach (var group in staffGroups)
                        {
                            string nombreEnfermero = group.Key;
                            DataTable dtStaff = group.CopyToDataTable();

                            // --- BÚSQUEDA DEL ID REAL (Relacionando con el listado de la izquierda) ---
                            int idReal = 0;
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

                            // Llamamos a la impresión (el método ProcesarYGuardarPDF ya debe tener el prefijo _DA_)
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



        private void ProcesarYGuardarPDF(DataTable dtStaff, string folderPath, string nombreStaff, int idStaff)
        {
            try
            {
                string nombreLimpio = Regex.Replace(nombreStaff, @"[\\/:*?""<>|]", "").Replace(" ", "_");
                string fileName = $"{idStaff}_DA_{nombreLimpio}.xps";
                string fullPath = System.IO.Path.Combine(folderPath, fileName);

                // 1. Configuración Tamaño Carta (Letter: 8.5 x 11 pulgadas)
                // 96 pixels por pulgada es el estándar de WPF
                FlowDocument doc = new FlowDocument
                {
                    PageHeight = 11 * 96,
                    PageWidth = 8.5 * 96,
                    PagePadding = new Thickness(72), // 1 pulgada de margen
                    ColumnWidth = 8.5 * 96, // Evita que el texto se divida en dos columnas

                    // 2. Tipografía Redondeada y Bonita
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 13,
                    TextAlignment = TextAlignment.Left
                };

                // Cabecera del Reporte - Un poco más elegante
                Paragraph titulo = new Paragraph(new Run("ADDITIONAL DOCUMENTS PENDING"))
                {
                    FontSize = 22,
                    FontWeight = FontWeights.Light, // El peso ligero lo hace ver más moderno
                    Foreground = Brushes.DarkSlateGray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(titulo);

                doc.Blocks.Add(new Paragraph(new Run($"Staff: {nombreStaff}"))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center
                });

                doc.Blocks.Add(new Paragraph(new Run($"Date Generated: {DateTime.Now:MM/dd/yyyy}"))
                {
                    FontSize = 11,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 30)
                });

                // Configuración de Tabla
                Table table = new Table { CellSpacing = 0, BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1) };
                // Ajustamos la columna casi al ancho total de la página carta menos márgenes
                table.Columns.Add(new TableColumn { Width = new GridLength(600) });

                TableRowGroup group = new TableRowGroup();

                // Header de la tabla estilizado
                TableRow header = new TableRow { Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)), FontWeight = FontWeights.Bold };
                TableCell headerCell = new TableCell(new Paragraph(new Run("DOCUMENT DESCRIPTION")) { Margin = new Thickness(10) });
                header.Cells.Add(headerCell);
                group.Rows.Add(header);

                foreach (DataRow row in dtStaff.Rows)
                {
                    TableRow r = new TableRow();

                    // Esta línea detecta si la columna se llama "Description" o "DocName" dinámicamente
                    string colNombre = dtStaff.Columns.Contains("Description") ? "Description" : "DocName";

                    Paragraph p = new Paragraph(new Run("• " + row[colNombre].ToString())) { Margin = new Thickness(10, 5, 10, 5) }; r.Cells.Add(new TableCell(p));
                    group.Rows.Add(r);
                }

                table.RowGroups.Add(group);
                doc.Blocks.Add(table);

                // Pie de página simple
                doc.Blocks.Add(new Paragraph(new Run("Please submit these documents to HR as soon as possible."))
                {
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 40, 0, 0),
                    TextAlignment = TextAlignment.Center
                });

                objData.ImprimirDocSilencioso(doc, fullPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in {nombreStaff}: {ex.Message}");
            }
        }        
        #endregion
    }
}