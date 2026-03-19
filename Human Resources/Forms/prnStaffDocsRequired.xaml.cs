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

namespace Human_Resources.Forms
{
    /// <summary>
    /// Lógica de interacción para prnStaffDocsRequired.xaml
    /// </summary>
    public partial class prnStaffDocsRequired : Page
    {
        public prnStaffDocsRequired()
        {
            InitializeComponent();
        }
        private void BtnActualPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();

            if (pd.ShowDialog() == true)
            {
                // Guardamos el escalado original
                double originalWidth = pnlReporte.ActualWidth;

                // 3. Imprimir el contenedor principal
                pd.PrintVisual(pnlReporte, "Staff Sections Report");

                MessageBox.Show("Report sent to printer.");
            }
        }
    }

}
