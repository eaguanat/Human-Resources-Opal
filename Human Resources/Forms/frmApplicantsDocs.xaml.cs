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
using System.Windows.Shapes;
// using System.Windows.Markup; // No es necesario normalmente, pero puede ayudar en algunos casos de referencia.
using Human_Resources.Forms;
using System.Data; // Necesario para el DataTable
using Human_Resources.Data;
using System.Security.Policy; // Para que reconozca tu carpeta y la clase

namespace Human_Resources.Forms
{
    public partial class frmtblApplicantsDocs : Page
    {
        public frmtblApplicantsDocs()
        {
            InitializeComponent();
            // Cargo el Grid al iniciar el formulario
            CargarCombos();
                        // Acutualiza el estado de los botones según si hay datos o no
            ActualizarEstadoBotones(false);
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
            }
            catch (Exception ex) { MessageBox.Show("Error loading States: " + ex.Message); }
        }

        private void ActualizarEstadoBotones(bool rtState)
        {
            dgListado.IsEnabled = rtState;
            BtnAdd.IsEnabled = (cmbFiltroDpto.SelectedValue != null) ? true : false;
            BtnModify.IsEnabled = rtState;
            BtnDelete.IsEnabled = rtState;
            btnPrint.Visibility = rtState ? Visibility.Visible : Visibility.Collapsed;
            btnPrint.IsEnabled = rtState;
        }

        private void cmbFiltroDpto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LlenarGrid();
        }
        private void LlenarGrid()
        {

            if (cmbFiltroDpto.SelectedValue != null)
            {
                int idDpt = Convert.ToInt32(cmbFiltroDpto.SelectedValue);
                ClasstblApplicantsDocs objetoNegocio = new ClasstblApplicantsDocs();
                DataTable tablaDatos = objetoNegocio.Listar(idDpt);
                if (tablaDatos != null)
                {
                    dgListado.ItemsSource = tablaDatos.DefaultView;
                }
                bool tieneRegistros = (tablaDatos != null && tablaDatos.Rows.Count > 0);
                ActualizarEstadoBotones(tieneRegistros);
            }
        }

        // BOTONERA PRINCIPAL
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // LlenarGrid();

        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            btnPrint.Visibility = Visibility.Collapsed; // Quito el boton de Impresion
            lblTitulo.Text = "New Required Document";
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            cmbFiltroDpto.Visibility = Visibility.Collapsed;
            TextCombo.Visibility = Visibility.Collapsed;
            txtDescription.Clear();
            txtDescription.Focus();
        }

        int idSeleccionado = 0;
        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar si hay una fila seleccionada
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a record from the list.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Quito el boton de Impresion
            btnPrint.Visibility = Visibility.Collapsed;
            cmbFiltroDpto.Visibility = Visibility.Collapsed;
            TextCombo.Visibility = Visibility.Collapsed;

            // 2. Obtener los datos de la fila seleccionada
            // Convertimos el Item seleccionado a DataRowView (el formato del DataTable en el grid)
            DataRowView fila = (DataRowView)dgListado.SelectedItem;

            // 3. Guardamos el ID en nuestra variable y pasamos el texto al TextBox
            idSeleccionado = Convert.ToInt32(fila["Id"]);
            txtDescription.Text = fila["Description"].ToString();

            // 4. Cambiar de vista (esto ya lo tenías)
            lblTitulo.Text = "Update of applicant documents";
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtDescription.Focus();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // this.NavigationService.Content = null;
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

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validación de campo vacío
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Please enter a state name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return;
            }

            // 2. Preparar el objeto con los datos de la pantalla
            ClasstblApplicantsDocs objeto = new ClasstblApplicantsDocs();
            objeto.Id = idSeleccionado; // Si es 0, SQL lo ignorará en el Insert. Si es > 0, SQL lo usará para el Update.
            objeto.IdDepartment = Convert.ToInt32(cmbFiltroDpto.SelectedValue);
            objeto.Description = txtDescription.Text.Trim();

            bool success = false;
            string successMessage = "";

            // 3. Decidir acción basada en idSeleccionado
            if (idSeleccionado == 0)
            {
                // CASO: NUEVO REGISTRO
                success = objeto.Insertar();
                successMessage = "The new state has been saved successfully.";
            }
            else
            {
                // CASO: MODIFICAR EXISTENTE
                success = objeto.Actualizar();
                successMessage = "The state information has been updated successfully.";
            }

            // 4. Procesar el resultado de la operación
            if (success)
            {
                MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiamos todo para la próxima vez
                idSeleccionado = 0;
                txtDescription.Clear();

                // Regresamos a la lista y refrescamos los datos
                BtnVolverInicio_Click(null, null);
                LlenarGrid();

                // Refrescamos el estado de los botones (Enabled/Disabled)
                ActualizarEstadoBotones((dgListado.Items != null && dgListado.Items.Count > 0));
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar selección
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a record to delete.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Obtener datos de la fila
            DataRowView fila = (DataRowView)dgListado.SelectedItem;
            int idAEliminar = Convert.ToInt32(fila["Id"]);
            string nombreEstado = fila["Description"].ToString();

            // 3. Pedir confirmación (Are you sure?)
            MessageBoxResult answer = MessageBox.Show($"Are you sure you want to delete this record?: {nombreEstado}?",
                                                      "Confirm Delete",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                ClasstblApplicantsDocs objeto = new ClasstblApplicantsDocs();

                // 5. Proceder a eliminar
                if (objeto.Eliminar(idAEliminar))
                {
                    MessageBox.Show("The record was successfully deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 6. Refrescar la pantalla
                    LlenarGrid();
                    ActualizarEstadoBotones((dgListado.Items != null && dgListado.Items.Count > 0));
                }
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "Definition of Required Documents";
            GridListado.Visibility = Visibility.Visible;
            GridEdicion.Visibility = Visibility.Collapsed;
            cmbFiltroDpto.Visibility = Visibility.Visible;
            TextCombo.Visibility = Visibility.Visible;

            // Refrescamos el estado de los botones (Enabled/Disabled)
            ActualizarEstadoBotones((dgListado.Items != null && dgListado.Items.Count > 0));
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen())
                {
                    // --- CONFIGURACIÓN ---
                    Typeface fontBold = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                    Typeface fontRegular = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                    Typeface fontItalic = new Typeface(new FontFamily("Segoe UI"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal);

                    double margin = 50;
                    double yPos = margin;
                    double pageWidth = pd.PrintableAreaWidth;
                    double pageHeight = pd.PrintableAreaHeight;
                    double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                    // 1. CABECERA IZQUIERDA: Empresa
                    string companyName = Window.GetWindow(this).Title;
                    FormattedText textCompany = new FormattedText(companyName, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 12, Brushes.Black, dpi);
                    dc.DrawText(textCompany, new Point(margin, yPos));

                    // 2. CABECERA DERECHA: Fecha y Hora
                    string dateStr = "Date: " + DateTime.Now.ToString("MM/dd/yyyy");
                    string timeStr = "Time: " + DateTime.Now.ToString("hh:mm tt");

                    FormattedText textDate = new FormattedText(dateStr, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 10, Brushes.Black, dpi);
                    FormattedText textTime = new FormattedText(timeStr, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 10, Brushes.Black, dpi);

                    dc.DrawText(textDate, new Point(pageWidth - textDate.Width - margin, yPos));
                    dc.DrawText(textTime, new Point(pageWidth - textTime.Width - margin, yPos + 15));

                    yPos += 60;

                    // 3. TÍTULO CENTRAL
                    FormattedText textTitle = new FormattedText(lblTitulo.Text.ToUpper(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 16, Brushes.Black, dpi);
                    dc.DrawText(textTitle, new Point((pageWidth - textTitle.Width) / 2, yPos));

                    yPos += 45;

                    // 4. MEMBRETE: Descriptions + Línea
                    FormattedText textHeader = new FormattedText("Descriptions", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 12, Brushes.Black, dpi);
                    dc.DrawText(textHeader, new Point(margin, yPos));

                    yPos += 22;
                    dc.DrawLine(new Pen(Brushes.Black, 1.5), new Point(margin, yPos), new Point(pageWidth - margin, yPos));

                    yPos += 15;

                    // 5. LISTADO DE ESTADOS
                    int idDpt = Convert.ToInt32(cmbFiltroDpto.SelectedValue);
                    ClasstblApplicantsDocs obj = new ClasstblApplicantsDocs();
                    DataTable dt = obj.Listar(idDpt);

                    if (dt != null)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            // Verificamos si nos estamos quedando sin espacio (Margen inferior de 70 para el número de página)
                            if (yPos > pageHeight - 80) break;

                            FormattedText rowText = new FormattedText(row["Description"].ToString(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 11, Brushes.Black, dpi);
                            dc.DrawText(rowText, new Point(margin + 10, yPos));
                            yPos += 20;
                        }
                    }

                    // 6. PIE DE PÁGINA: Número de página
                    string pageNumberStr = "Page 1 of 1";
                    FormattedText textPageNum = new FormattedText(pageNumberStr, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontItalic, 9, Brushes.Gray, dpi);
                    dc.DrawText(textPageNum, new Point((pageWidth - textPageNum.Width) / 2, pageHeight - margin));
                }

                // Enviar a la impresora
                pd.PrintVisual(visual, "Required Documents List Report");
            }
        }
    }
}
