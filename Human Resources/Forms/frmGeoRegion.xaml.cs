using Human_Resources.Data;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Human_Resources.Forms
{
    public partial class frmGeoRegion : Page
    {
        Data.ClassGeoRegion obj = new Data.ClassGeoRegion();
        bool isNuevo = false;

        public frmGeoRegion()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            dgRegiones.ItemsSource = obj.Listar().DefaultView;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            isNuevo = true;
            txtRegion.Clear();
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
            txtRegion.Focus();
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegiones.SelectedItem == null)
            {
                MessageBox.Show("Please select a record from the list.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            isNuevo = false;
            DataRowView row = (DataRowView)dgRegiones.SelectedItem;
            obj.Id = Convert.ToInt32(row["Id"]);
            txtRegion.Text = row["Description"].ToString();
            GridListado.Visibility = Visibility.Collapsed;
            GridEdicion.Visibility = Visibility.Visible;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRegion.Text)) return;
            obj.Description = txtRegion.Text.Trim();

            bool exito = isNuevo ? obj.Insertar() : obj.Actualizar();

            if (exito)
            {
                CargarDatos();
                BtnVolverInicio_Click(null, null);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgRegiones.SelectedItem == null)
            {
                MessageBox.Show("Please select a region to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgRegiones.SelectedItem;
            int idSeleccionado = Convert.ToInt32(row["Id"]);
            string nombreRegion = row["Description"].ToString();

            // 1. Validar si la región está siendo usada por empleados
            if (obj.EstaEnUso(idSeleccionado))
            {
                MessageBox.Show($"The region '{nombreRegion}' cannot be deleted because it is currently assigned to staff members.",
                                "Data Integrity", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // 2. Si no está en uso, pedir confirmación
            if (MessageBox.Show($"Are you sure you want to delete the region: {nombreRegion}?",
                                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (obj.Eliminar(idSeleccionado))
                {
                    CargarDatos();
                    MessageBox.Show("Region deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            GridEdicion.Visibility = Visibility.Collapsed;
            GridListado.Visibility = Visibility.Visible;
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
            

        // REPORTE ESTANDARIZADO "OPAL HANDS"
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
                    ClassGeoRegion obj = new ClassGeoRegion();
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
                pd.PrintVisual(visual, "Regions List Report");
            }
        }
    }
}