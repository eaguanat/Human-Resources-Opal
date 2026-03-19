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
    public partial class frmGeoCity : Page
    {
        int idSeleccionado = 0;

        public frmGeoCity()
        {
            InitializeComponent();
            CargarCombos();
            ActualizarEstadoBotones(false, false);
        }

        private void CargarCombos()
        {
            try
            {
                ClassGeoState objState = new ClassGeoState();
                DataTable dt = objState.Listar();
                if (dt != null)
                {
                    cmbFiltroEstado.ItemsSource = dt.DefaultView;
                    cmbFiltroEstado.DisplayMemberPath = "Description";
                    cmbFiltroEstado.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading States: " + ex.Message); }
        }

        private void LlenarGrid()
        {
            if (cmbFiltroEstado.SelectedValue != null)
            {
                int idEstado = Convert.ToInt32(cmbFiltroEstado.SelectedValue);
                ClassGeoCity objeto = new ClassGeoCity();
                DataTable dt = objeto.ListarPorEstado(idEstado);
                dgCiudades.ItemsSource = dt != null ? dt.DefaultView : null;

                bool tieneRegistros = (dt != null && dt.Rows.Count > 0);
                ActualizarEstadoBotones(true, tieneRegistros);
            }
        }

        private void ActualizarEstadoBotones(bool estadoSeleccionado, bool tieneRegistros)
        {
            BtnAdd.IsEnabled = estadoSeleccionado;
            BtnModify.IsEnabled = tieneRegistros;
            BtnDelete.IsEnabled = tieneRegistros;
            dgCiudades.IsEnabled = tieneRegistros;
            if (btnPrint != null)
                btnPrint.Visibility = tieneRegistros ? Visibility.Visible : Visibility.Collapsed;
        }

        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LlenarGrid();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            idSeleccionado = 0;
            lblTitulo.Text = "New City";
            lblEstadoSeleccionado.Text = cmbFiltroEstado.Text;
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            if (btnPrint != null) btnPrint.Visibility = Visibility.Collapsed;
            txtCiudad.Clear();
            txtCiudad.Focus();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgCiudades.SelectedItem == null)
            {
                MessageBox.Show("Please select a city.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DataRowView fila = (DataRowView)dgCiudades.SelectedItem;
            idSeleccionado = Convert.ToInt32(fila["Id"]);
            txtCiudad.Text = fila["Description"].ToString();
            lblEstadoSeleccionado.Text = cmbFiltroEstado.Text;
            lblTitulo.Text = "Update City";
            if (btnPrint != null) btnPrint.Visibility = Visibility.Collapsed;
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtCiudad.Focus();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCiudad.Text))
            {
                MessageBox.Show("Please enter the city name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCiudad.Focus();
                return;
            }

            ClassGeoCity objeto = new ClassGeoCity();
            objeto.Id = idSeleccionado;
            objeto.Description = txtCiudad.Text.Trim();
            objeto.IdGeoState = Convert.ToInt32(cmbFiltroEstado.SelectedValue);

            bool success = (idSeleccionado == 0) ? objeto.Insertar() : objeto.Actualizar();
            if (success)
            {
                MessageBox.Show("Saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnVolverInicio_Click(null, null);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCiudades.SelectedItem == null) return;
            DataRowView fila = (DataRowView)dgCiudades.SelectedItem;
            int idEliminar = Convert.ToInt32(fila["Id"]);
            if (MessageBox.Show($"Delete city: {fila["Description"]}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ClassGeoCity objeto = new ClassGeoCity();
                if (objeto.EstaEnUso(idEliminar)) MessageBox.Show("Cannot delete: City is in use.");
                else if (objeto.Eliminar(idEliminar)) LlenarGrid();
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "Definition of Cities";
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
            // 1. Obtener los datos desde la clase (Método que creamos con JOIN)
            Data.ClassGeoCity objData = new Data.ClassGeoCity();
            DataTable dt = objData.ListarTodoParaReporte();

            if (dt.Rows.Count == 0) return;

            // 2. Crear el FlowDocument
            FlowDocument doc = new FlowDocument();
            doc.PagePadding = new Thickness(50);
            doc.FontFamily = new FontFamily("Segoe UI");

            // --- CONFIGURACIÓN DE ENCABEZADO (Empresa y Fecha/Hora) ---
            // Usamos una tabla pequeña para posicionar Empresa a la izq y Fecha a la der
            Table tableHeader = new Table();
            tableHeader.Columns.Add(new TableColumn());
            tableHeader.Columns.Add(new TableColumn());
            TableRowGroup rowGroupHeader = new TableRowGroup();
            TableRow headerRow = new TableRow();

            // Empresa (Tope Izquierdo)
            string companyName = Window.GetWindow(this).Title;
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(companyName)) { FontWeight = FontWeights.Bold, FontSize = 12 }));

            // Fecha y Hora (Tope Derecho)
            Paragraph pFechaHora = new Paragraph();
            pFechaHora.Inlines.Add(new Run(DateTime.Now.ToString("MM/dd/yyyy")));
            pFechaHora.Inlines.Add(new LineBreak());
            pFechaHora.Inlines.Add(new Run(DateTime.Now.ToString("hh:mm tt")));
            pFechaHora.TextAlignment = TextAlignment.Right;
            pFechaHora.FontSize = 10;
            headerRow.Cells.Add(new TableCell(pFechaHora));

            rowGroupHeader.Rows.Add(headerRow);
            tableHeader.RowGroups.Add(rowGroupHeader);
            doc.Blocks.Add(tableHeader);

            // 3. TÍTULO CENTRAL
            Paragraph tituloCentral = new Paragraph(new Run("GEOGRAPHIC DIRECTORY"));
            tituloCentral.FontSize = 20;
            tituloCentral.FontWeight = FontWeights.Bold;
            tituloCentral.TextAlignment = TextAlignment.Center;
            tituloCentral.Margin = new Thickness(0, 20, 0, 20);
            doc.Blocks.Add(tituloCentral);

            // 4. CUERPO DEL REPORTE (Agrupado por Estado)
            var listaOrdenada = dt.AsEnumerable()
                .Select(row => new {
                    Estado = row.Field<string>("StateDescription"),
                    Ciudad = row.Field<string>("Description")
                }).ToList();

            string estadoActual = "";

            foreach (var item in listaOrdenada)
            {
                if (item.Estado != estadoActual)
                {
                    estadoActual = item.Estado;

                    // Nombre del Estado
                    Paragraph pEstado = new Paragraph(new Run(estadoActual.ToUpper()));
                    pEstado.FontSize = 14;
                    pEstado.FontWeight = FontWeights.Bold;
                    pEstado.Margin = new Thickness(0, 15, 0, 5);
                    doc.Blocks.Add(pEstado);

                    // Línea divisoria (Descriptions style)
                    Border linea = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 0, 0, 10) };
                    doc.Blocks.Add(new BlockUIContainer(linea));
                }

                // Listado de Ciudades
                Paragraph pCiudad = new Paragraph(new Run("• " + item.Ciudad));
                pCiudad.FontSize = 12;
                pCiudad.Margin = new Thickness(20, 0, 0, 2);
                doc.Blocks.Add(pCiudad);
            }

            // 5. DIÁLOGO DE IMPRESIÓN Y PAGINACIÓN
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                // Ajustamos el tamaño del documento al papel seleccionado
                doc.PageHeight = pd.PrintableAreaHeight;
                doc.PageWidth = pd.PrintableAreaWidth;
                doc.ColumnWidth = pd.PrintableAreaWidth;

                // Para el número de páginas, usamos el Paginator
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Geographic Directory Report");
            }
        }
    }
}