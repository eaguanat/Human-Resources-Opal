using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Human_Resources.Data;


namespace Human_Resources.Forms
{
    public partial class frmDocsRequired : Page
    {
        int idSeleccionado = 0;
        public frmDocsRequired()
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
                    cmbFiltroDpto.ItemsSource = dt.DefaultView;
                    cmbFiltroDpto.DisplayMemberPath = "Description";
                    cmbFiltroDpto.SelectedValuePath = "Id";
                }

                // Sections
                // Creamos una lista de objetos anónimos con el Texto y el Valor
                var secciones = new[] 
                {
                    new { Texto = "SECTION 2", Valor = 2 },
                    new { Texto = "SECTION 3", Valor = 3 },
                    new { Texto = "SECTION 4", Valor = 4 }
                };
                cmbSection.ItemsSource = secciones;
                cmbSection.DisplayMemberPath = "Texto";  // Lo que el usuario ve
                cmbSection.SelectedValuePath = "Valor";  // El ID que guardas en la BD
            }
            catch (Exception ex) { MessageBox.Show("Error loading States: " + ex.Message); }
        }

        private void LlenarGrid()
        {
            if (cmbFiltroDpto.SelectedValue != null)
            {
                int idFindA = Convert.ToInt32(cmbFiltroDpto.SelectedValue);
                int idFindB = Convert.ToInt32(cmbSection.SelectedValue);
                ClassDocsRequired objeto = new ClassDocsRequired();
                DataTable dt = objeto.Listar(idFindA,idFindB);
                dgDocuments.ItemsSource = dt != null ? dt.DefaultView : null;

                bool tieneRegistros = (dt != null && dt.Rows.Count > 0);
                ActualizarEstadoBotones(true, tieneRegistros);
            }
        }

        private void ActualizarEstadoBotones(bool estadoSeleccionado, bool tieneRegistros)
        {
            BtnAdd.IsEnabled = estadoSeleccionado;
            BtnModify.IsEnabled = tieneRegistros;
            BtnDelete.IsEnabled = tieneRegistros;
            dgDocuments.IsEnabled = tieneRegistros;
            if (btnPrint != null)
                btnPrint.Visibility = tieneRegistros ? Visibility.Visible : Visibility.Collapsed;
        }

        private void cmbFiltroDpto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSection.SelectedValue != null)  LlenarGrid();
        }

        private void cmbSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSection.SelectedValue != null) LlenarGrid();
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            idSeleccionado = 0;
            lblTitulo.Text = "New Document";
            
            lblFindA.Text = cmbFiltroDpto.Text + " - " + cmbSection.Text;
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            if (btnPrint != null) btnPrint.Visibility = Visibility.Collapsed;
            txtDescription.Clear();
            CbRequired.IsChecked = false;
            txtDescription.Focus();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgDocuments.SelectedItem == null)
            {
                MessageBox.Show("Please select a document.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DataRowView fila = (DataRowView)dgDocuments.SelectedItem;
            idSeleccionado = Convert.ToInt32(fila["Id"]);
            txtDescription.Text = fila["Description"].ToString();
            CbRequired.IsChecked = fila["Required"] != DBNull.Value ? Convert.ToBoolean(fila["Required"]) : false;
            lblFindA.Text = cmbFiltroDpto.Text + " - " + cmbSection.Text;
            lblTitulo.Text = "Update Description";
            if (btnPrint != null) btnPrint.Visibility = Visibility.Collapsed;
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtDescription.Focus();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please enter the description name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return;
            }

            ClassDocsRequired objeto = new ClassDocsRequired();
            objeto.Id = idSeleccionado;
            objeto.Description = txtDescription.Text.Trim();
            objeto.IdDepartment = Convert.ToInt32(cmbFiltroDpto.SelectedValue);
            objeto.IdSection = Convert.ToInt32(cmbSection.SelectedValue);
            objeto.Required = CbRequired.IsChecked ?? false;

            bool success = (idSeleccionado == 0) ? objeto.Insertar() : objeto.Actualizar();
            if (success)
            {
                MessageBox.Show("Saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnVolverInicio_Click(null, null);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please enter the description name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return;
            }


            DataRowView fila = (DataRowView)dgDocuments.SelectedItem;
            int idEliminar = Convert.ToInt32(fila["Id"]);
            if (MessageBox.Show($"Delete Document: {fila["Description"]}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ClassDocsRequired objeto = new ClassDocsRequired();
                if (objeto.EstaEnUso(idEliminar)) MessageBox.Show("Cannot delete: Document is in use.");
                else if (objeto.Eliminar(idEliminar)) LlenarGrid();
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "Definition of Documents";
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
            // 1. VALIDACIÓN: Aseguramos que ambos combos tengan selección
            if (cmbFiltroDpto.SelectedValue == null || cmbSection.SelectedValue == null)
            {
                MessageBox.Show("Please select Department and Section before printing.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Capturamos los textos de los combos para el título
            string nombreDepartamento = cmbFiltroDpto.Text;
            string nombreSeccion = cmbSection.Text; // <--- Nueva variable para la Sección

            // 2. Obtener datos
            int idFindA = Convert.ToInt32(cmbFiltroDpto.SelectedValue);
            int idFindB = Convert.ToInt32(cmbSection.SelectedValue);
            Data.ClassDocsRequired objData = new Data.ClassDocsRequired();
            DataTable dt = objData.Listar(idFindA, idFindB);

            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No documents found for this selection.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 3. Crear el FlowDocument
            FlowDocument doc = new FlowDocument();
            doc.PagePadding = new Thickness(50);
            doc.FontFamily = new FontFamily("Segoe UI");

            // --- ENCABEZADO: Empresa y Fecha (Se mantiene igual) ---
            Table tableHeader = new Table();
            tableHeader.Columns.Add(new TableColumn());
            tableHeader.Columns.Add(new TableColumn());
            TableRowGroup rowGroupHeader = new TableRowGroup();
            TableRow headerRow = new TableRow();
            string companyName = Window.GetWindow(this).Title;
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(companyName)) { FontWeight = FontWeights.Bold, FontSize = 12 }));
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

            // 4. TÍTULO DINÁMICO (Departamento + Sección)
            // Combinamos ambos textos en una sola línea o puedes usar un LineBreak
            Paragraph tituloCentral = new Paragraph();
            tituloCentral.Inlines.Add(new Run(nombreDepartamento.ToUpper()));
            tituloCentral.Inlines.Add(new LineBreak()); // Salto de línea para que se vea elegante
            tituloCentral.Inlines.Add(new Run(nombreSeccion.ToUpper()) { FontSize = 16 }); // Sección un poco más pequeña

            tituloCentral.FontSize = 20;
            tituloCentral.FontWeight = FontWeights.Bold;
            tituloCentral.TextAlignment = TextAlignment.Center;
            tituloCentral.Margin = new Thickness(0, 20, 0, 5);
            doc.Blocks.Add(tituloCentral);

            // 5. SUBTÍTULO
            Paragraph subTitulo = new Paragraph(new Run("( R )  Required Description"));
            subTitulo.FontSize = 12;
            subTitulo.FontStyle = FontStyles.Italic;
            subTitulo.TextAlignment = TextAlignment.Left;
            subTitulo.Margin = new Thickness(0, 0, 0, 10);
            doc.Blocks.Add(subTitulo);

            // Línea divisoria
            doc.Blocks.Add(new BlockUIContainer(new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 2), Margin = new Thickness(0, 0, 0, 15) }));

            // 6. LISTADO DE DOCUMENTOS (Con la corrección de 'Required')
            foreach (DataRow row in dt.Rows)
            {
                bool isRequired = row["Required"] != DBNull.Value ? Convert.ToBoolean(row["Required"]) : false;
                string descripcion = row["Description"].ToString();

                Paragraph pDoc = new Paragraph();
                pDoc.Margin = new Thickness(10, 0, 0, 8);

                string checkChar = isRequired ? "  ☑  " : "  ☐  ";

                Run runCheck = new Run(checkChar)
                {
                    FontFamily = new FontFamily("Segoe UI Symbol"),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };

                pDoc.Inlines.Add(runCheck);
                pDoc.Inlines.Add(new Run(" " + descripcion) { FontSize = 12 });
                doc.Blocks.Add(pDoc);
            }

            // 7. DIÁLOGO DE IMPRESIÓN (Se mantiene igual)
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                doc.PageHeight = pd.PrintableAreaHeight;
                doc.PageWidth = pd.PrintableAreaWidth;
                doc.ColumnWidth = pd.PrintableAreaWidth;
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Department Documents Report");
            }
        }
    }
}