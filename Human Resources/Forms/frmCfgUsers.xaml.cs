using System;
using System.Collections.Generic;
using System.Configuration; // Para ConfigurationErrorsException
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Human_Resources.Data; // Para acceder a ClassCfgUsers y AccessModule

namespace Human_Resources.Forms
{
    public partial class frmCfgUsers : Page
    {
        private int idSeleccionado = 0;

        public frmCfgUsers()
        {
            InitializeComponent();
            LlenarGrid();
            CtrlForm(true); // Mostrar GridListado al inicio
        }

        private void LlenarGrid()
        {
            ClassCfgUsers obj = new ClassCfgUsers();
            dgListado.ItemsSource = obj.Listar(txtSearch.Text).DefaultView;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Solo permite caracteres del 0 al 9
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Controlar estado del formulario (vista de lista vs. vista de edición)
        private void CtrlForm(bool showList)
        {
            GridListado.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            pnlSearch.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnAdd.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnModify.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnDelete.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;
            BtnExit.Visibility = showList ? Visibility.Visible : Visibility.Collapsed;

            GridEdicion.Visibility = showList ? Visibility.Collapsed : Visibility.Visible;
        }

           
        private void CtrlEdition() // El TabControl
        {
            tcUser.SelectedIndex = 0; // Selecciona la primera pestaña ("User Details")
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Input,
                new Action(() => { txtFullName.Focus(); }));
        }



        // Limpiar campos para un nuevo registro
        private void LimpiarCampos()
        {
            txtFullName.Clear();
            txtName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtDescriptionJob.Clear();
            pbPassword.Clear(); // MUY IMPORTANTE: NUNCA mostrar la contraseña guardada
            pbConfirmPassword.Clear();
            chkActive.IsChecked = true;

            // Reiniciar la lista de módulos de acceso
            var allModules = ClassCfgUsers.AllAccessModules.Select(m => new AccessModule { Id = m.Id, Description = m.Description, IsChecked = false }).ToList();
            lstAccessModules.ItemsSource = allModules;
        }

        // --- EVENTOS DE BOTONES ---

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
            idSeleccionado = 0; // Indica que es un nuevo registro
            lblTitulo.Text = "NEW USER RECORD";
            CtrlForm(false); // Mostrar vista de edición
            CtrlEdition();   // El TabControl
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to modify.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgListado.SelectedItem;
            idSeleccionado = (int)row["id"];

            CargarDatosParaEditar(idSeleccionado);

            lblTitulo.Text = "UPDATE USER";
            CtrlForm(false); // Mostrar vista de edición
        }

        private void CargarDatosParaEditar(int id)
        {
            ClassCfgUsers user = new ClassCfgUsers();
            if (user.ObtenerPorId(id))
            {
                txtFullName.Text = user.FullName;
                txtName.Text = user.Name;
                txtEmail.Text = user.Email;
                txtPhone.Text = user.Phone;
                txtDescriptionJob.Text = user.DescriptionJob;
                chkActive.IsChecked = user.Active;
                pbPassword.Clear();        // Nunca precargar contraseñas
                pbConfirmPassword.Clear(); // Nunca precargar contraseñas

                // Cargar accesos del usuario
                List<int> userAccessIds = user.ObtenerAccesosPorUsuario(id);
                var allModules = ClassCfgUsers.AllAccessModules.Select(m => new AccessModule { Id = m.Id, Description = m.Description, IsChecked = userAccessIds.Contains(m.Id) }).ToList();
                lstAccessModules.ItemsSource = allModules;
                lstAccessModules.Items.Refresh(); // Asegurarse de que el ListBox se redibuje
                CtrlEdition();
            }
            else
            {
                MessageBox.Show("Could not load user data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnVolverInicio_Click(null, null); // Volver a la lista si hay un error
            }
        }


        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. Validaciones ---
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { MessageBox.Show("Full Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtFullName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("User Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtFullName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Email is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtEmail.Focus(); return; }
            if (!IsValidEmail(txtEmail.Text)) { MessageBox.Show("Please enter a valid email address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); txtEmail.Focus(); return; }

            string newPassword = pbPassword.Password;
            string confirmPassword = pbConfirmPassword.Password;

            if (idSeleccionado == 0) // Nuevo usuario
            {
                if (string.IsNullOrWhiteSpace(newPassword)) { MessageBox.Show("Password is required for new users.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); pbPassword.Focus(); return; }
            }

            if (!string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(confirmPassword)) // Si se intentó cambiar la contraseña
            {
                if (newPassword != confirmPassword) { MessageBox.Show("Password and Confirm Password do not match.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); pbConfirmPassword.Focus(); return; }
                if (newPassword.Length < 6) { MessageBox.Show("Password must be at least 6 characters long.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning); pbPassword.Focus(); return; }
            }

            // --- 2. Preparar objeto de usuario ---
            ClassCfgUsers user = new ClassCfgUsers
            {
                Id = idSeleccionado,
                FullName = txtFullName.Text.Trim(),
                Name = txtName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                DescriptionJob = txtDescriptionJob.Text.Trim(),
                Active = chkActive.IsChecked ?? true // Si es null, asumimos true
            };

            // Solo asignar PasswordText si se va a actualizar/insertar la contraseña
            if (idSeleccionado == 0 || !string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordText = newPassword;
            }
            // Si es un update y no se ingresó nueva contraseña, PasswordText se queda null/vacío,
            // y el método Actualizar() de ClassCfgUsers lo manejará (no cambiará la clave)

            int savedId = 0;
            if (idSeleccionado == 0)
            {
                savedId = user.Insertar();
            }
            else
            {
                if (user.Actualizar())
                {
                    savedId = idSeleccionado;
                }
            }

            if (savedId > 0)
            {
                // --- 3. Guardar Accesos ---
                var selectedModules = (List<AccessModule>)lstAccessModules.ItemsSource;
                if (selectedModules != null)
                {
                    user.GuardarAccesos(savedId, selectedModules);
                }

                MessageBox.Show("User data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BtnVolverInicio_Click(null, null); // Regresar a la lista
                LlenarGrid(); // Refrescar la lista de usuarios
            }
            else
            {
                MessageBox.Show("Failed to save user data. See previous errors.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            // Regex para validación básica de email
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                // Permite email locales (ej. "user@localhost") también.
                // Para una validación estricta de dominios reales, se necesitaría un regex más complejo o una librería.
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }


        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgListado.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgListado.SelectedItem;
            int idToDelete = (int)row["id"];
            string userName = row["FullName"].ToString();

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete user '{userName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClassCfgUsers user = new ClassCfgUsers();
                if (user.Eliminar(idToDelete))
                {
                    MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LlenarGrid(); // Refrescar la lista
                }
                else
                {
                    MessageBox.Show("Failed to delete user. See previous errors.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            lblTitulo.Text = "USER MANAGEMENT";
            CtrlForm(true); // Volver a la vista de lista
            LlenarGrid(); // Refrescar por si se hizo un cambio
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para cerrar la pestaña, similar a otros formularios
            var principal = Window.GetWindow(this) as MainWindow;
            if (principal != null)
            {
                TabItem tabToClose = null;
                foreach (TabItem item in principal.tcPrincipal.Items)
                {
                    if (item.Content is Frame f && f.Content == this)
                    {
                        tabToClose = item;
                        break;
                    }
                }
                if (tabToClose != null)
                {
                    principal.tcPrincipal.Items.Remove(tabToClose);
                }
            }
        }

       
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LlenarGrid(); // Refrescar el grid al escribir en el buscador
        }
    }
}