using Human_Resources.Data;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Human_Resources.Forms
{
    public partial class frmStaffDocsAdditional : Page
    {
        private int idStaffSeleccionado = 0;
        private DataTable dtDocumentosCargados;
        private ClassStaffDocsAdditional _dataAccess = new ClassStaffDocsAdditional();

        public frmStaffDocsAdditional()
        {
            InitializeComponent();
            CargarCombos();
        }

        #region LÓGICA DE BÚSQUEDA (ESTADO 1)

        private void CargarCombos()
        {
            try
            {
                ClassDepartment objState = new ClassDepartment();
                DataTable dt = objState.Listar();
                if (dt != null)
                {
                    cmbDepartment.ItemsSource = dt.DefaultView;
                    cmbDepartment.DisplayMemberPath = "Description";
                    cmbDepartment.SelectedValuePath = "Id";
                }

            }
            catch (Exception ex) { MessageBox.Show("Error loading States: " + ex.Message, "CargarCombos"); }
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartment.SelectedValue == null) return;
            int idDept = Convert.ToInt32(cmbDepartment.SelectedValue);
            LlenarGridStaff();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cmbDepartment.SelectedValue == null) return;
            int idDept = Convert.ToInt32(cmbDepartment.SelectedValue);
            LlenarGridStaff();
        }


        private void LlenarGridStaff()
        {
            Human_Resources.Data.ClassStaff obj = new Human_Resources.Data.ClassStaff();
            int idDept = Convert.ToInt32(cmbDepartment.SelectedValue);
            dgListado.ItemsSource = obj.Listar(txtSearch.Text).DefaultView;
            dgListado.ItemsSource = _dataAccess.ListarStaff(txtSearch.Text, idDept).DefaultView;

        }
        
        #endregion

        #region LÓGICA DE CARGA Y GUARDADO (ESTADO 2)

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EjecutarGuardado())
            {
                MessageBox.Show("All documentation has been saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                btnHome_Click(null, null);
            }
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            idStaffSeleccionado = 0;
            txtSearch.Clear();
            GridDocumentos.Visibility = Visibility.Collapsed;
            GridSeleccion.Visibility = Visibility.Visible;
            LlenarGridStaff();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (idStaffSeleccionado == 0 || dtDocumentosCargados == null)
            {
                MessageBox.Show("Please select a staff member first.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EjecutarGuardado())
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    DrawingVisual visual = new DrawingVisual();
                    using (DrawingContext dc = visual.RenderOpen())
                    {
                        Typeface fontBold = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                        Typeface fontRegular = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                        double margin = 50;
                        double yPos = margin;
                        double pageWidth = pd.PrintableAreaWidth;
                        double dpi = 96.0;

                        string company = Window.GetWindow(this).Title;
                        dc.DrawText(new FormattedText(company, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 12, Brushes.Black, dpi), new Point(margin, yPos));

                        string dateStr = "Print Date: " + DateTime.Now.ToString("MM/dd/yyyy");
                        dc.DrawText(new FormattedText(dateStr, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 10, Brushes.Black, dpi), new Point(pageWidth - margin - 110, yPos));

                        yPos += 40;
                        dc.DrawText(new FormattedText("STAFF DOCUMENTATION ADDITIONAL REPORT", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 16, Brushes.Black, dpi), new Point(margin, yPos));

                        yPos += 25;
                        dc.DrawText(new FormattedText(lblStaffSelected.Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 12, Brushes.DarkBlue, dpi), new Point(margin, yPos));

                        yPos += 40;

                        dc.DrawText(new FormattedText("Description", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 11, Brushes.Black, dpi), new Point(margin + 5, yPos));
                        dc.DrawText(new FormattedText("Reg Date", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontBold, 11, Brushes.Black, dpi), new Point(pageWidth - margin - 60, yPos));

                        yPos += 22;
                        dc.DrawLine(new Pen(Brushes.Black, 1.5), new Point(margin, yPos), new Point(pageWidth - margin, yPos));
                        yPos += 10;

                        foreach (DataRow row in dtDocumentosCargados.Rows)
                        {
                            if (yPos > pd.PrintableAreaHeight - 60) break;


                            dc.DrawText(new FormattedText(row["Description"].ToString(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 10, Brushes.Black, dpi), new Point(margin + 5, yPos));

                            if (row["DateDoc"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["DateDoc"].ToString()))
                            {
                                DateTime f = Convert.ToDateTime(row["DateDoc"]);
                                dc.DrawText(new FormattedText(f.ToString("MM/dd/yyyy"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontRegular, 10, Brushes.Black, dpi), new Point(pageWidth - margin - 60, yPos));
                            }
                            yPos += 22;
                            dc.DrawLine(new Pen(Brushes.LightGray, 0.5), new Point(margin, yPos), new Point(pageWidth - margin, yPos));
                            yPos += 10;

                        }
                    }
                    pd.PrintVisual(visual, "DocsAddReport");
                    btnHome_Click(null, null);
                }
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var principal = Window.GetWindow(this) as MainWindow;
            if (principal != null)
            {
                TabItem t = null;
                foreach (TabItem item in principal.tcPrincipal.Items)
                {
                    if (item.Content is Frame f && f.Content == this) { t = item; break; }
                }
                if (t != null) principal.tcPrincipal.Items.Remove(t);
            }
        }

        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null) return;

            DataRowView row = (DataRowView)dgListado.SelectedItem;
            idStaffSeleccionado = Convert.ToInt32(row["id"]);
            lblStaffSelected.Text = $"Recording documents for: {row["Name"]} {row["LastName"]}";

            Human_Resources.Data.ClassStaffDocsAdditional objDocs = new Human_Resources.Data.ClassStaffDocsAdditional();
            dtDocumentosCargados = objDocs.ListarDocumentosPorStaff(idStaffSeleccionado);
            icDocuments.ItemsSource = dtDocumentosCargados.DefaultView;

            GridSeleccion.Visibility = Visibility.Collapsed;
            GridDocumentos.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var container = icDocuments.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                if (container != null)
                {
                    var firstTextBox = FindVisualChild<TextBox>(container);
                    firstTextBox?.Focus();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t) return t;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        private bool EjecutarGuardado()
        {
            try
            {
                btnSave.Focus();
                foreach (DataRow row in dtDocumentosCargados.Rows)
                {
                    if (row["DateDoc"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["DateDoc"].ToString()))
                    {
                        if (!DateTime.TryParse(row["DateDoc"].ToString(), out _))
                        {
                            MessageBox.Show($"The date for '{row["Description"]}' is not valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }

                Human_Resources.Data.ClassStaffDocsAdditional objDocs = new Human_Resources.Data.ClassStaffDocsAdditional();
                return objDocs.GuardarMasivo(idStaffSeleccionado, dtDocumentosCargados);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Technical Error: " + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region INTERACCIÓN DE DOCUMENTOS

        private void RowBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.FindName("PART_TextBox") is TextBox txt)
            {
                txt.Focus();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb && !tb.IsFocused)
            {
                e.Handled = true;
                tb.Focus();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "/";
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            if (e.Key == Key.Back && tb.Text.EndsWith("/"))
            {
                tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
                tb.CaretIndex = tb.Text.Length;
                return;
            }

            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                if (tb.Text.Length == 2 || tb.Text.Length == 5)
                {
                    tb.Text += "/";
                    tb.CaretIndex = tb.Text.Length;
                }
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (!string.IsNullOrWhiteSpace(tb.Text) && !DateTime.TryParse(tb.Text, out _))
                {
                    MessageBox.Show("Invalid date: MM/DD/YYYY", "Date Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tb.SelectAll();
                    return;
                }
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                if (!tb.MoveFocus(request)) btnSave.Focus();
            }
        }

        #endregion
    }
}