using Human_Resources.Data; // Agregado: Necesario para ClassCfgUsers y AccessModule
using Human_Resources.Forms;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;


namespace Human_Resources
{
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = true;

        // Propiedades para almacenar la información del usuario loggeado
        public int CurrentLoggedInUserId { get; private set; }
        private List<int> CurrentUserAccesses { get; set; }

        // CAMBIO IMPORTANTE: Constructor que recibe el ID del usuario y sus accesos
        // Este es el constructor que se llamará desde frmLogin.
        public MainWindow(int userId, List<int> userAccesses)
        {
            InitializeComponent();
            CurrentLoggedInUserId = userId;
            CurrentUserAccesses = userAccesses;
            ApplyUserPermissions(); // Aplica los permisos al menú al inicio
        }

        // El constructor vacío original se mantiene, pero es crucial que NO se use para iniciar la app
        // ya que no tendría información del usuario y, por lo tanto, no aplicaría permisos.
        // Si no se usa programáticamente (solo para diseño en XAML), puede ser viable.
        // Si la aplicación se inicia SIEMPRE desde el login, este constructor ya no debería ser usado en producción.
        public MainWindow()
        {
            InitializeComponent();
            // Si por alguna razón se llama a este constructor, se establecerían valores por defecto
            // o se lanzaría una excepción si se requiere un login.
            // Para fines de diseño y testing, puede ser útil.
            CurrentLoggedInUserId = -1; // O algún valor que indique no loggeado
            CurrentUserAccesses = new List<int>(); // Sin accesos
            ApplyUserPermissions(); // Aplicar permisos (vacíos si userId es -1)
        }


        // ============================================================
        // LÓGICA DE NAVEGACIÓN POR PESTAÑAS (TABCONTROL)
        // ============================================================

        private void AbrirFormulario(string titulo, Page pagina)
        {
            // 1. Verificamos si la pestaña ya existe para no abrirla dos veces
            foreach (TabItem item in tcPrincipal.Items)
            {
                if (item.Header.ToString() == titulo)
                {
                    tcPrincipal.SelectedItem = item; // Si existe, la seleccionamos
                    return;
                }
            }

            // 2. Si no existe, creamos una nueva pestaña
            TabItem nuevaTab = new TabItem();
            nuevaTab.Header = titulo;

            // Creamos un Frame interno para cada pestaña para alojar la Page
            Frame frameInterno = new Frame();
            frameInterno.Navigate(pagina);
            nuevaTab.Content = frameInterno;

            tcPrincipal.Items.Add(nuevaTab);
            tcPrincipal.SelectedItem = nuevaTab; // Enfocamos la nueva pestaña
        }

        private void BtnCerrarTab_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para cerrar la pestaña desde la "X" del diseño
            Button btn = (Button)sender;
            TabItem tabItem = FindParent<TabItem>(btn);
            if (tabItem != null)
            {
                tcPrincipal.Items.Remove(tabItem);
            }
        }

        // Helper para encontrar el TabItem padre del botón de cierre
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        // ============================================================
        // EVENTOS DE CLIC EN MENÚS Y SUBMENÚS
        // ============================================================

        private void BtnFrmGeoState_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("States", new Forms.frmGeoState());
        }

        private void BtnFrmGeoCity_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Cities", new Forms.frmGeoCity());
        }
        private void BtnFrmGeoRegion_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Regions", new Forms.frmGeoRegion());
        }

        private void BtnFrmDocsInmigration_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Inmigrations", new Forms.frmDocsInmigration());
        }
        private void BtnFrmLanguage_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Language", new Forms.frmLanguage());
        }

        private void BtnFrmDocsPayroll_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Payroll", new Forms.frmDocsPayroll());
        }
        private void BtnFrmBanks_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Banks", new Forms.frmBanks());
        }
        private void BtnFrmBanksMethod_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Payments", new Forms.frmBanksMethod());
        }
        private void BtnFrmDepartment_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Department", new Forms.frmDepartment());
        }
        private void BtnFrmDepLicense_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Licenses", new Forms.frmDepLicense());
        }



        // DOCUMENTS
        private void BtnFrmDocsRequired_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Documents Required", new Forms.frmDocsRequired());
        }
        private void BtnFrmDocsAdditional_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Docs Additional", new Forms.frmDocsAdditional());
        }


        // STAFF
        private void BtnFrmStaff_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Company Staff", new Forms.frmStaff());
        }
        private void BtnFrmStaffDocsRequired_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Staff Documents Required", new Forms.frmStaffDocsRequired());
        }
        private void BtnFrmStaffDocsAditional_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Staff Documents Additional", new Forms.frmStaffDocsAdditional());
        }


        // FIND STAFF
        private void BtnFrmFindStaff_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Documents Required", new Forms.frmFindStaff());
        }
        private void BtnFrmFindStaffAdditional_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Documents Additional Required", new Forms.frmFindStaffAdditional());
        }


        // PROPECTS
        private void BtnFrmtblApplicants_Click(object sender, RoutedEventArgs e)
        {
            // Este abrirá la lista de gente que aplicó desde la Web
//            AbrirFormulario("tblApplicants", new Forms.frmtblApplicants());
        }

        private void BtnFrmProspectsDocs_Click(object sender, RoutedEventArgs e)
        {
            // Este es para que usted configure la lista de documentos (tblDocsRequired)
            AbrirFormulario("Prospects Documents", new Forms.frmtblApplicantsDocs());
        }


        // SETUP

        private void BtnFrmCompany_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Company", new Forms.frmCompany());
        }
        // Agregado el evento para el formulario de usuarios
        private void BtnFrmCfgUsers_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("User Management", new Forms.frmCfgUsers());
        }

        private void BtnFrmAuthorizedDevices_Click(object sender, RoutedEventArgs e)
        {
            AbrirFormulario("Authorized Devices", new Forms.frmCfgAuthorizedDevices());
        }


        private void SubMenu_Click(object sender, RoutedEventArgs e)
        {
            // Este método evita el error que tenías en el XAML
            // Aquí puedes centralizar lógica si lo deseas en el futuro
        }

        // ============================================================
        // LÓGICA DE ANIMACIÓN Y CONTROL DEL MENÚ LATERAL
        // ============================================================

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            GridLength startWidth = isMenuOpen ? new GridLength(235) : new GridLength(60);
            GridLength endWidth = isMenuOpen ? new GridLength(60) : new GridLength(235);

            var animacion = new GridLengthAnimation
            {
                From = startWidth,
                To = endWidth,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            if (isMenuOpen) OcultarTextos();

            animacion.Completed += (s, ev) =>
            {
                if (!isMenuOpen) MostrarTextos();
                isMenuOpen = !isMenuOpen;
            };

            GridMenu.BeginAnimation(ColumnDefinition.WidthProperty, animacion);
        }

        // Método para aplicar los permisos al menú
        // Se llama una vez al cargar la ventana
        private void ApplyUserPermissions()
        {
            // Lista de todos los contenedores principales de los menús
            var menuContainers = new List<StackPanel>
            {
                pnlDefinitionsContainer,
                pnlDocumentContainer,
                pnlStaffContainer,
                pnlStaffPost,
                pnlFindContainer,
                pnlSetup
            };

            // Oculta todos los contenedores por defecto
            foreach (var panel in menuContainers)
            {
                panel.Visibility = Visibility.Collapsed;
            }

            // Habilita los contenedores a los que el usuario tiene acceso
            foreach (int accessId in CurrentUserAccesses)
            {
                switch (accessId)
                {
                    case 1: pnlDefinitionsContainer.Visibility = Visibility.Visible; break; // Definitions
                    case 2: pnlDocumentContainer.Visibility = Visibility.Visible; break;   // Documents
                    case 3: pnlStaffContainer.Visibility = Visibility.Visible; break;      // Company Staff
                    case 4: pnlStaffPost.Visibility = Visibility.Visible; break;           // Post Documents
                    case 5: pnlFindContainer.Visibility = Visibility.Visible; break;        // Expired Docs
                    case 6: pnlSetup.Visibility = Visibility.Visible; break;               // Setup
                                                                                           // Agrega más casos si tienes otros módulos
                }
            }

            // Además, aseguramos que los submenús estén colapsados al inicio
            pnlSubMenuDefinitions.Visibility = Visibility.Collapsed;
            pnlSubMenuDocument.Visibility = Visibility.Collapsed;
            pnlSubMenuStaff.Visibility = Visibility.Collapsed;
            pnlSubMenuStaffPost.Visibility = Visibility.Collapsed;
            pnlSubMenuFind.Visibility = Visibility.Collapsed;
            pnlSubMenuSetup.Visibility = Visibility.Collapsed;

            // Y las flechas apunten hacia abajo
            if (txtFlecha != null) txtFlecha.Text = "⌵";
            if (txtFlecha5 != null) txtFlecha5.Text = "⌵";
            if (txtFlecha4 != null) txtFlecha4.Text = "⌵";
            if (txtFlecha2 != null) txtFlecha2.Text = "⌵";
            if (txtFlecha6 != null) txtFlecha6.Text = "⌵";
            if (txtFlecha3 != null) txtFlecha3.Text = "⌵";
        }


        // Ajustado para usar los TextBlocks correctos y las flechas
        private void OcultarTextos()
        {
            txtDefinitions.Visibility = Visibility.Collapsed;
            txtDocument.Visibility = Visibility.Collapsed;
            txtStaff.Visibility = Visibility.Collapsed;
            txtPost.Visibility = Visibility.Collapsed;
            txtFind.Visibility = Visibility.Collapsed;
            txtSetup.Visibility = Visibility.Collapsed;

            pnlSubMenuDefinitions.Visibility = Visibility.Collapsed;
            pnlSubMenuDocument.Visibility = Visibility.Collapsed;
            pnlSubMenuFind.Visibility = Visibility.Collapsed;
            pnlSubMenuStaff.Visibility = Visibility.Collapsed;
            pnlSubMenuStaffPost.Visibility = Visibility.Collapsed;
            pnlSubMenuSetup.Visibility = Visibility.Collapsed;

            if (txtFlecha != null) txtFlecha.Text = "⌵";
            if (txtFlecha5 != null) txtFlecha5.Text = "⌵";
            if (txtFlecha4 != null) txtFlecha4.Text = "⌵";
            if (txtFlecha2 != null) txtFlecha2.Text = "⌵";
            if (txtFlecha6 != null) txtFlecha6.Text = "⌵";
            if (txtFlecha3 != null) txtFlecha3.Text = "⌵";
        }

        // Ajustado para usar los TextBlocks correctos
        private void MostrarTextos()
        {
            txtDefinitions.Visibility = Visibility.Visible;
            txtDocument.Visibility = Visibility.Visible;
            txtStaff.Visibility = Visibility.Visible;
            txtPost.Visibility = Visibility.Visible;
            txtFind.Visibility = Visibility.Visible;
            txtSetup.Visibility = Visibility.Visible;
        }

        private void BtnDefinitions_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);

            // Solo interactúa si el contenedor principal de "Definitions" está visible (por permisos)
            if (pnlDefinitionsContainer.Visibility == Visibility.Visible)
            {
                if (pnlSubMenuDefinitions.Visibility == Visibility.Visible)
                {
                    pnlSubMenuDefinitions.Visibility = Visibility.Collapsed;
                    txtFlecha.Text = "⌵";
                }
                else
                {
                    pnlSubMenuDefinitions.Visibility = Visibility.Visible;
                    txtFlecha.Text = "⌃";
                }
            }
        }


        private void BtnDocument_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);
            // Solo interactúa si el contenedor principal de "Documents" está visible (por permisos)
            if (pnlDocumentContainer.Visibility == Visibility.Visible)
            {
                pnlSubMenuDocument.Visibility = (pnlSubMenuDocument.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                if (txtFlecha5 != null) txtFlecha5.Text = (pnlSubMenuDocument.Visibility == Visibility.Visible) ? "⌃" : "⌵";
            }
        }

        // STAFF
        private void BtnStaff_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);
            // Solo interactúa si el contenedor principal de "Company Staff" está visible (por permisos)
            if (pnlStaffContainer.Visibility == Visibility.Visible)
            {
                pnlSubMenuStaff.Visibility = (pnlSubMenuStaff.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                if (txtFlecha4 != null) txtFlecha4.Text = (pnlSubMenuStaff.Visibility == Visibility.Visible) ? "⌃" : "⌵";
            }
        }

        // POSTEO DE DOCUMENTOS
        private void BtnStaffPost_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);
            // Solo interactúa si el contenedor principal de "Post Documents" está visible (por permisos)
            if (pnlStaffPost.Visibility == Visibility.Visible)
            {
                pnlSubMenuStaffPost.Visibility = (pnlSubMenuStaffPost.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                if (txtFlecha2 != null) txtFlecha2.Text = (pnlSubMenuStaffPost.Visibility == Visibility.Visible) ? "⌃" : "⌵";
            }
        }


        // FIND (Ahora es Expired Docs)
        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);
            // Solo interactúa si el contenedor principal de "Expired Docs" está visible (por permisos)
            if (pnlFindContainer.Visibility == Visibility.Visible)
            {
                pnlSubMenuFind.Visibility = (pnlSubMenuFind.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                if (txtFlecha6 != null) txtFlecha6.Text = (pnlSubMenuFind.Visibility == Visibility.Visible) ? "⌃" : "⌵";
            }
        }

        // PROSPECTS
        private void BtnProspects_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);

            if (pnlProspectsContainer.Visibility == Visibility.Visible)
            {
                pnlSubMenuProspects.Visibility = (pnlSubMenuProspects.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                if (txtFlechaProspects != null) txtFlechaProspects.Text = (pnlSubMenuProspects.Visibility == Visibility.Visible) ? "⌃" : "⌵";
            }
        }


        // SETUP
        private void BtnSetup_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuOpen) BtnToggle_Click(null, null);
            // Solo interactúa si el contenedor principal de "Setup" está visible (por permisos)
            if (pnlSetup.Visibility == Visibility.Visible)
            {
                pnlSubMenuSetup.Visibility = (pnlSubMenuSetup.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                // Corregido: Si usas txtFlecha3 para Setup, usa txtFlecha3 aquí.
                if (txtFlecha3 != null) txtFlecha3.Text = (pnlSubMenuSetup.Visibility == Visibility.Visible) ? "⌃" : "⌵";
            }
        }


    }

    // ============================================================
    // CLASE AUXILIAR PARA ANIMACIÓN DE GRID
    // ============================================================
    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);
        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));
        public GridLength From { get => (GridLength)GetValue(FromProperty); set => SetValue(FromProperty, value); }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));
        public GridLength To { get => (GridLength)GetValue(ToProperty); set => SetValue(ToProperty, value); }

        public IEasingFunction EasingFunction { get; set; }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = From.Value;
            double toVal = To.Value;

            double progress = animationClock.CurrentProgress.Value;
            if (EasingFunction != null) progress = EasingFunction.Ease(progress);

            if (fromVal > toVal)
                return new GridLength((1 - progress) * (fromVal - toVal) + toVal);

            return new GridLength(progress * (toVal - fromVal) + fromVal);
        }
    }
}