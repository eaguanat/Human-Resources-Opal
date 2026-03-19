using Human_Resources.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Human_Resources.Forms
{
    public partial class frmStaffDocsRequired : Page
    {
        private int _idStaffSeleccionado = 0;
        private ClassStaffDocsRequired _dataAccess = new ClassStaffDocsRequired();

        public frmStaffDocsRequired()
        {
            InitializeComponent();
            CargarCombos();
        }

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
            catch (Exception ex) { MessageBox.Show("Error loading Departments: " + ex.Message, "CargarCombos"); }
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarGridStaff();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarGridStaff();
        }

        private void ActualizarGridStaff()
        {
            if (cmbDepartment.SelectedValue == null) return;
            int idDept = Convert.ToInt32(cmbDepartment.SelectedValue);
            dgStaff.ItemsSource = _dataAccess.ListarStaff(txtSearch.Text, idDept).DefaultView;
        }

        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (dgStaff.SelectedItem == null || cmbSection.SelectedValue == null)
            {
                MessageBox.Show("Please select a Staff member and a Section.");
                return;
            }

            DataRowView row = (DataRowView)dgStaff.SelectedItem;
            _idStaffSeleccionado = Convert.ToInt32(row["Id"]);
            int idSec = Convert.ToInt32(((ComboBoxItem)cmbSection.SelectedItem).Tag);

            lblSelectedStaff.Text = $"Staff: {row["Name"]} {row["LastName"]} - Section {idSec}";
            CargarListaDocumentos(idSec);

            pnlStep1.Visibility = Visibility.Collapsed;
            pnlStep2.Visibility = Visibility.Visible;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var principal = Window.GetWindow(this) as MainWindow;
            if (principal != null)
            {
                TabItem t = null;
                foreach (TabItem item in principal.tcPrincipal.Items)
                    if (item.Content is Frame f && f.Content == this) { t = item; break; }
                if (t != null) principal.tcPrincipal.Items.Remove(t);
            }
        }

        private void CargarListaDocumentos(int idSec)
        {
            DataTable dt = _dataAccess.ListarDocsPorPersona(_idStaffSeleccionado, idSec);
            List<DocItemVM> lista = new List<DocItemVM>();

            foreach (DataRow r in dt.Rows)
            {
                lista.Add(new DocItemVM
                {
                    IdDocsRequired = Convert.ToInt32(r["IdDocsRequired"]),
                    IdRegistro = r["IdRegistro"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["IdRegistro"]),
                    Description = r["Description"].ToString(),
                    DateDisplay = r["LastDate"] == DBNull.Value ? "---" : Convert.ToDateTime(r["LastDate"]).ToString("MM/dd/yyyy"),
                    NewDate = ""
                });
            }
            listDocs.ItemsSource = lista;
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
                btnBack.Focus();
                tb.Focus();
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

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (pnlStep2.Visibility == Visibility.Visible)
            {
                pnlStep2.Visibility = Visibility.Collapsed;
                pnlStep1.Visibility = Visibility.Visible;
            }
        }

        private void BtnUpdateDate_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DocItemVM;
            if (string.IsNullOrEmpty(item.NewDate) || !DateTime.TryParse(item.NewDate, out DateTime novaData))
            {
                MessageBox.Show("Please enter a valid date (MM/DD/YYYY)");
                return;
            }

            _dataAccess.IdStaff = _idStaffSeleccionado;
            _dataAccess.IdDocsRequired = item.IdDocsRequired;
            _dataAccess.DateDoc = novaData;

            if (_dataAccess.Insertar())
            {
                MessageBox.Show("Document Updated Successfully.");
                int idSec = Convert.ToInt32(((ComboBoxItem)cmbSection.SelectedItem).Tag);
                CargarListaDocumentos(idSec);
            }
        }

        private void BtnModifyDate_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DocItemVM;
            if (!item.IdRegistro.HasValue)
            {
                MessageBox.Show("There is no record to modify.");
                return;
            }

            if (!DateTime.TryParse(item.NewDate, out DateTime novaData))
            {
                MessageBox.Show("Please enter a valid date to correct.");
                return;
            }

            _dataAccess.Id = item.IdRegistro.Value;
            _dataAccess.DateDoc = novaData;

            if (_dataAccess.Actualizar())
            {
                MessageBox.Show("Record Corrected.");
                int idSec = Convert.ToInt32(((ComboBoxItem)cmbSection.SelectedItem).Tag);
                CargarListaDocumentos(idSec);
            }
        }

        private void DateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9/]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_idStaffSeleccionado == 0 || cmbSection.SelectedItem == null)
            {
                MessageBox.Show("Please select a staff member and section.");
                return;
            }

            try
            {
                int idSec = Convert.ToInt32(((ComboBoxItem)cmbSection.SelectedItem).Tag);

                // 1. Obtener Datos del Staff
                ClassStaff objStaff = new ClassStaff();
                DataTable dtStaff = objStaff.Listar("");
                DataRow rowStaff = dtStaff.AsEnumerable().FirstOrDefault(x => x.Field<int>("id") == _idStaffSeleccionado);

                // 2. Obtener Licencias (POSITION) y Documentos (Reporte)
                DataTable dtLicenses = _dataAccess.ObtenerLicenciasStaff(_idStaffSeleccionado);
                DataTable dtDocs = _dataAccess.ObtenerDataReporte(_idStaffSeleccionado, idSec);

                // 3. Configurar el Reporte
                prnStaffDocsRequired reporte = new prnStaffDocsRequired();

                reporte.lblSeccionNum.Text = idSec.ToString();
                reporte.lblCompany.Text = Window.GetWindow(this).Title ?? "N/A"; 
                reporte.lblDepto.Text = cmbDepartment.Text;
                reporte.lblNombre.Text = $"{rowStaff["LastName"]}, {rowStaff["Name"]}";

                // Asignar ItemsSources
                reporte.icLicenses.ItemsSource = dtLicenses.DefaultView; // <-- Nueva sección Position
                reporte.icDocs.ItemsSource = dtDocs.DefaultView;

                // Llenar campos de fechas y puesto
                if (dtStaff.Columns.Contains("JobDes")) reporte.lblJob.Text = rowStaff["JobDes"]?.ToString();

                if (dtStaff.Columns.Contains("AppDay") && rowStaff["AppDay"] != DBNull.Value)
                    reporte.lblAppDate.Text = Convert.ToDateTime(rowStaff["AppDay"]).ToString("MM/dd/yyyy");

                if (dtStaff.Columns.Contains("HiredDay") && rowStaff["HiredDay"] != DBNull.Value)
                    reporte.lblHired.Text = Convert.ToDateTime(rowStaff["HiredDay"]).ToString("MM/dd/yyyy");
                if (dtStaff.Columns.Contains("EndDay") && rowStaff["EndDay"] != DBNull.Value)
                    reporte.lblEnd.Text = Convert.ToDateTime(rowStaff["EndDay"]).ToString("MM/dd/yyyy");

                // 4. Imprimir
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintVisual(reporte.pnlReporte, "Staff Compliance Report");
                    MessageBox.Show("Report sent successfully.", "Print");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing report: " + ex.Message);
            }
        }

        public class DocItemVM
        {
            public int IdDocsRequired { get; set; }
            public int? IdRegistro { get; set; }
            public string Description { get; set; }
            public string DateDisplay { get; set; }
            public string NewDate { get; set; }
        }
    }
}