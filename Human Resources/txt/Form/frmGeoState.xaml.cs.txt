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
using Human_Resources.Data; // Para que reconozca tu carpeta y la clase ClassGeoState

namespace Human_Resources.Forms
{
    public partial class frmGeoState : Page
    {
        public frmGeoState()
        {
            InitializeComponent();
            // Cargo el Grid al iniciar el formulario
            LlenarGrid();

            // Acutualiza el estado de los botones según si hay datos o no
            ActualizarEstadoBotones((dgEstados.Items != null && dgEstados.Items.Count > 0));
        }

        private void ActualizarEstadoBotones(bool rtState)
        {
            dgEstados.IsEnabled = rtState;
            BtnModify.IsEnabled = rtState;
            BtnDelete.IsEnabled = rtState;
            btnPrint.Visibility = rtState ? Visibility.Visible : Visibility.Collapsed;
            btnPrint.IsEnabled = rtState;
        }

        private void LlenarGrid()
        {
            // 1. Instanciamos la clase que creamos antes
            ClassGeoState objetoNegocio = new ClassGeoState();

            // 2. Traemos los datos usando el método Listar()
            DataTable tablaDatos = objetoNegocio.Listar();

            // 3. Verificamos que traiga algo (que no sea null/Nothing)
            if (tablaDatos != null)
            {
                // 4. Asignamos al nombre de tu DataGrid: dgEstados
                dgEstados.ItemsSource = tablaDatos.DefaultView;
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
            lblTitulo.Text = "New State";
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtEstado.Clear();
            txtEstado.Focus();
        }

        int idSeleccionado = 0;
        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar si hay una fila seleccionada
            if (dgEstados.SelectedItem == null)
            {
                MessageBox.Show("Please select a record from the list.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Quito el boton de Impresion
            btnPrint.Visibility = Visibility.Collapsed; 

            // 2. Obtener los datos de la fila seleccionada
            // Convertimos el Item seleccionado a DataRowView (el formato del DataTable en el grid)
            DataRowView fila = (DataRowView)dgEstados.SelectedItem;

            // 3. Guardamos el ID en nuestra variable y pasamos el texto al TextBox
            idSeleccionado = Convert.ToInt32(fila["Id"]);
            txtEstado.Text = fila["Description"].ToString();

            // 4. Cambiar de vista (esto ya lo tenías)
            lblTitulo.Text = "Update State";
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtEstado.Focus();
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
            if (string.IsNullOrWhiteSpace(txtEstado.Text))
            {
                MessageBox.Show("Please enter a state name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEstado.Focus();
                return;
            }

            // 2. Preparar el objeto con los datos de la pantalla
            ClassGeoState objeto = new ClassGeoState();
            objeto.Id = idSeleccionado; // Si es 0, SQL lo ignorará en el Insert. Si es > 0, SQL lo usará para el Update.
            objeto.Description = txtEstado.Text.Trim();

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
                txtEstado.Clear();

                // Regresamos a la lista y refrescamos los datos
                BtnVolverInicio_Click(null, null);
                LlenarGrid();

                // Refrescamos el estado de los botones (Enabled/Disabled)
                ActualizarEstadoBotones((dgEstados.Items != null && dgEstados.Items.Count > 0));
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar selección
            if (dgEstados.SelectedItem == null)
            {
                MessageBox.Show("Please select a record to delete.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Obtener datos de la fila
            DataRowView fila = (DataRowView)dgEstados.SelectedItem;
            int idAEliminar = Convert.ToInt32(fila["Id"]);
            string nombreEstado = fila["Description"].ToString();

            // 3. Pedir confirmación (Are you sure?)
            MessageBoxResult answer = MessageBox.Show($"Are you sure you want to delete the state: {nombreEstado}?",
                                                      "Confirm Delete",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                ClassGeoState objeto = new ClassGeoState();

                // 4. Validar si está en uso antes de borrar
                if (objeto.EstaEnUso(idAEliminar))
                {
                    MessageBox.Show("This state cannot be deleted because it is currently assigned to staff members or has cities assigned to it..",
                                    "Integrity Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Stop);
                    return;
                }

                // 5. Proceder a eliminar
                if (objeto.Eliminar(idAEliminar))
                {
                    MessageBox.Show("State deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 6. Refrescar la pantalla
                    LlenarGrid();
                    ActualizarEstadoBotones((dgEstados.Items != null && dgEstados.Items.Count > 0));
                }
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "Definition of States";
            GridListado.Visibility = Visibility.Visible;
            GridEdicion.Visibility = Visibility.Collapsed;

            // Refrescamos el estado de los botones (Enabled/Disabled)
            ActualizarEstadoBotones((dgEstados.Items != null && dgEstados.Items.Count > 0));
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
                    ClassGeoState obj = new ClassGeoState();
                    DataTable dt = obj.Listar();

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
                pd.PrintVisual(visual, "State List Report");
            }
        }
    }
}
