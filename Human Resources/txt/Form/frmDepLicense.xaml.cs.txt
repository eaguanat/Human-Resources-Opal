using Human_Resources.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Human_Resources.Forms
{
    public partial class frmDepLicense : Page
    {
        int idSeleccionado = 0;

        public frmDepLicense()
        {
            InitializeComponent();
            CargarCombos();
            ActualizarEstadoBotones(false, false);
        }

        private void CargarCombos()
        {
            try
            {
                ClassDepartment objState = new ClassDepartment();
                DataTable dt = objState.Listar();
                if (dt != null)
                {
                    cmbFiltro.ItemsSource = dt.DefaultView;
                    cmbFiltro.DisplayMemberPath = "Description";
                    cmbFiltro.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading States: " + ex.Message); }
        }

        private void LlenarGrid()
        {
            if (cmbFiltro.SelectedValue != null)
            {
                int idFind = Convert.ToInt32(cmbFiltro.SelectedValue);
                ClassDepLicense objeto = new ClassDepLicense();
                DataTable dt = objeto.Listar(idFind);
                dgListado.ItemsSource = dt != null ? dt.DefaultView : null;

                bool tieneRegistros = (dt != null && dt.Rows.Count > 0);
                ActualizarEstadoBotones(true, tieneRegistros);
            }
        }

        private void ActualizarEstadoBotones(bool estadoSeleccionado, bool tieneRegistros)
        {
            BtnAdd.IsEnabled = estadoSeleccionado;
            BtnModify.IsEnabled = tieneRegistros;
            BtnDelete.IsEnabled = tieneRegistros;
            dgListado.IsEnabled = tieneRegistros;
            if (btnPrint != null)
                btnPrint.Visibility = tieneRegistros ? Visibility.Visible : Visibility.Collapsed;
        }

        private void cmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LlenarGrid();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            idSeleccionado = 0;
            lblTitulo.Text = "New License";
            lblSeleccionado.Text = cmbFiltro.Text;
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            if (btnPrint != null) btnPrint.Visibility = Visibility.Collapsed;
            txtDescription.Clear();
            txtDescription.Focus();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a record.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DataRowView fila = (DataRowView)dgListado.SelectedItem;
            idSeleccionado = Convert.ToInt32(fila["Id"]);
            txtDescription.Text = fila["Description"].ToString();
            lblSeleccionado.Text = cmbFiltro.Text;
            lblTitulo.Text = "Update License";
            if (btnPrint != null) btnPrint.Visibility = Visibility.Collapsed;
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtDescription.Focus();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please enter the city name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return;
            }

            ClassDepLicense objeto = new ClassDepLicense();
            objeto.Id = idSeleccionado;
            objeto.Description = txtDescription.Text.Trim();
            objeto.IdDepartment = Convert.ToInt32(cmbFiltro.SelectedValue);

            bool success = (idSeleccionado == 0) ? objeto.Insertar() : objeto.Actualizar();
            if (success)
            {
                MessageBox.Show("Saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnVolverInicio_Click(null, null);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null) return;
            DataRowView fila = (DataRowView)dgListado.SelectedItem;
            int idEliminar = Convert.ToInt32(fila["Id"]);
            if (MessageBox.Show($"Delete record: {fila["Description"]}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ClassDepLicense objeto = new ClassDepLicense();
                if (objeto.EstaEnUso(idEliminar)) MessageBox.Show("Cannot delete: City is in use.");
                else if (objeto.Eliminar(idEliminar)) LlenarGrid();
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "Definition of Licenses";
            GridListado.Visibility = Visibility.Visible;
            GridEdicion.Visibility = Visibility.Collapsed;
            LlenarGrid();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // Buscamos la ventana principal (MainWindow)
            var principal = Window.GetWindow(this) as MainWindow;

            if (principal != null)
            {
                // Buscamos la pestaña que contiene este formulario
                TabItem tabACerrar = null;
                foreach (TabItem item in principal.tcPrincipal.Items)
                {
                    if (item.Content is Frame f && f.Content == this)
                    {
                        tabACerrar = item;
                        break;
                    }
                }

                // Si la encontramos, le pedimos a la MainWindow que la elimine
                if (tabACerrar != null)
                {
                    principal.tcPrincipal.Items.Remove(tabACerrar);
                }
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            // 1. OBTENEMOS LOS PADRES (Departamentos)
            Data.ClassDepartment objDept = new Data.ClassDepartment();
            DataTable dtDept = objDept.Listar();

            if (dtDept == null || dtDept.Rows.Count == 0) return;

            // 2. CONFIGURACIÓN DEL DOCUMENTO
            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Segoe UI"),
                PageWidth = 8.5 * 96, // Tamaño Carta
                ColumnWidth = 8.5 * 96
            };

            // --- ENCABEZADO (Empresa Izq / Fecha-Hora Der) ---
            Table headerTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 30) };
            headerTable.Columns.Add(new TableColumn());
            headerTable.Columns.Add(new TableColumn());
            TableRowGroup hGroup = new TableRowGroup();
            TableRow hRow = new TableRow();

            // Empresa
            string companyName = Window.GetWindow(this).Title.ToUpper();
            hRow.Cells.Add(new TableCell(new Paragraph(new Run(companyName)) { FontSize = 10, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray }));

            // Fecha y Hora
            Paragraph pDateTime = new Paragraph();
            pDateTime.Inlines.Add(new Run(DateTime.Now.ToString("MMMM dd, yyyy")));
            pDateTime.Inlines.Add(new LineBreak());
            pDateTime.Inlines.Add(new Run(DateTime.Now.ToString("hh:mm tt")));
            pDateTime.TextAlignment = TextAlignment.Right;
            pDateTime.FontSize = 9;
            pDateTime.Foreground = Brushes.Gray;
            hRow.Cells.Add(new TableCell(pDateTime));

            hGroup.Rows.Add(hRow);
            headerTable.RowGroups.Add(hGroup);
            doc.Blocks.Add(headerTable);

            // --- TÍTULO CENTRAL ---
            doc.Blocks.Add(new Paragraph(new Run("REGISTERED LICENSES BY DEPARTMENT"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Light,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 40)
            });

            // 3. RECORRIDO MAESTRO-DETALLE
            foreach (DataRow rowDept in dtDept.Rows)
            {
                int idDepto = Convert.ToInt32(rowDept["id"]);
                string nombreDepto = rowDept["Description"].ToString();

                // Buscamos las licencias de este departamento
                Data.ClassDepLicense objLic = new Data.ClassDepLicense();
                DataTable dtLicencias = objLic.Listar(idDepto);

                if (dtLicencias != null && dtLicencias.Rows.Count > 0)
                {
                    // EL PADRE: Departamento (Franja Gris)
                    Border bandaGris = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(242, 242, 242)),
                        BorderThickness = new Thickness(0, 1, 0, 1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                        Padding = new Thickness(15, 8, 15, 8),
                        Margin = new Thickness(0, 15, 0, 10)
                    };
                    bandaGris.Child = new TextBlock { Text = nombreDepto.ToUpper(), FontWeight = FontWeights.Bold, FontSize = 13 };
                    doc.Blocks.Add(new BlockUIContainer(bandaGris));

                    // EL HIJO: Detalle de Licencias (Identado)
                    foreach (DataRow rowLic in dtLicencias.Rows)
                    {
                        Paragraph pLic = new Paragraph(new Run("•  " + rowLic["Description"].ToString()))
                        {
                            Margin = new Thickness(50, 3, 0, 3),
                            FontSize = 12
                        };
                        doc.Blocks.Add(pLic);
                    }
                }
            }

            // 4. DIÁLOGO DE IMPRESIÓN CON NUMERACIÓN DE PÁGINAS
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                // Ajustar el documento al área de impresión de la impresora seleccionada
                doc.PageHeight = pd.PrintableAreaHeight;
                doc.PageWidth = pd.PrintableAreaWidth;
                doc.ColumnWidth = pd.PrintableAreaWidth;

                // El paginador de WPF gestiona automáticamente el "Página X de Y" si se configura el DocumentPaginator
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "License_Report_Final");
            }
        }

    }
}