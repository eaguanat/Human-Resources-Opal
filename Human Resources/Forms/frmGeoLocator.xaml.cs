using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Human_Resources.Data;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.Core;

namespace Human_Resources.Forms
{
    public partial class frmGeoLocator : Page
    {
        // --- PROPIEDADES DE ESTADO DEL FORMULARIO ---
        private decimal _centerLat, _centerLng;
        private List<GeoResult> _currentResults;

        // API Key de Google Maps
        private const string GoogleMapsApiKey = "AIzaSyCSYgzcr-KiVxVXJ_jwvxnmap9o6_sfvUk";

        // --- CONSTRUCTOR ---
        public frmGeoLocator()
        {
            InitializeComponent();
            CargarCatalogosFiltro();
            ResetFormState();
            InitializeWebView2();
        }

        // --- INICIALIZACIÓN Y GESTIÓN DE ESTADO ---

        private async void InitializeWebView2()
        {
            try
            {
                // Si el mapa ya tiene su motor encendido, no hacemos nada más
                if (webView.CoreWebView2 == null)
                {
                    // Solo inicializamos si es necesario
                    await webView.EnsureCoreWebView2Async(null);
                }

                // Una vez listo, conectamos el evento para recibir clics del mapa
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // Cargamos el mapa inicial
                await LoadMap(28.00m, -81.50m, new List<GeoResult>());
            }
            catch (Exception ex)
            {
                // Si el error es que ya estaba inicializado, lo ignoramos y seguimos
                if (ex.Message.Contains("already initialized"))
                {
                    await LoadMap(28.00m, -81.50m, new List<GeoResult>());
                }
                else
                {
                    MessageBox.Show($"Error initializing WebView2: {ex.Message}", "WebView2 Error");
                }
            }
        }


        private void CoreWebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // 1. Leemos el mensaje del mapa de forma segura
                string json = e.TryGetWebMessageAsString();
                var data = Newtonsoft.Json.Linq.JObject.Parse(json);
                int id = (int)data["id"];

                // 2. Buscamos a la persona
                var entidad = _currentResults.FirstOrDefault(x => x.Id == id);

                if (entidad != null)
                {
                    // --- TRUCO SENSEI ---
                    // NO usamos: dgResultados.SelectedItem = entidad; (esto dispararía su MessageBox)

                    // USAMOS esto: Solo mueve la lista para que la persona sea visible
                    // Así usted ve quién es en la tabla sin que salte el cuadro de texto.
                    dgResultados.ScrollIntoView(entidad);
                }
            }
            catch (Exception ex)
            {
                // Esto evita que el programa se cierre si hay un error de comunicación
                System.Diagnostics.Debug.WriteLine("Error de comunicación con el mapa: " + ex.Message);
            }
        }

        private enum ViewMode
        {
            Filter,
            Results
        }

        private void CtrlForm(ViewMode mode)
        {
            txtInitialMessage.Visibility = (mode == ViewMode.Filter) ? Visibility.Visible : Visibility.Collapsed;
            pnlResultados.Visibility = (mode == ViewMode.Results) ? Visibility.Visible : Visibility.Collapsed;

            BtnBuscar.Visibility = Visibility.Collapsed;
            BtnResetFiltro.Visibility = Visibility.Collapsed;
            BtnExit.Visibility = Visibility.Collapsed;
            BtnBack.Visibility = Visibility.Collapsed;

            switch (mode)
            {
                case ViewMode.Filter:
                    lblTitulo.Text = "GEOLOCATOR - SEARCH";
                    SetFilterControlsEnabled(true);
                    BtnBuscar.Visibility = Visibility.Visible;
                    BtnResetFiltro.Visibility = Visibility.Visible;
                    BtnExit.Visibility = Visibility.Visible;
                    break;

                case ViewMode.Results:
                    lblTitulo.Text = "GEOLOCATOR - RESULTS";
                    SetFilterControlsEnabled(false);
                    BtnBack.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetFilterControlsEnabled(bool enabled)
        {
            cmbOrigin.IsEnabled = enabled;
            cmbDepartment.IsEnabled = enabled;
            txtAddress.IsEnabled = enabled;
            cmbTopX.IsEnabled = enabled;
            BtnBuscar.IsEnabled = enabled;
            BtnResetFiltro.IsEnabled = enabled;

            // Bloqueamos o habilitamos el CheckBox según el modo
            chkActive.IsEnabled = enabled;

            // Opcional: Un toque visual para que se note el bloqueo (opacidad)
            chkActive.Opacity = enabled ? 1.0 : 0.7;
        }

        // 1. Agregamos "async" aquí
        private async void ResetFormState()
        {
            cmbOrigin.SelectedIndex = -1;
            cmbDepartment.SelectedIndex = -1;
            txtAddress.Clear();
            cmbTopX.SelectedValue = 10;

            // Reset del CheckBox
            chkActive.IsChecked = true;
            chkActive.IsEnabled = true;

            dgResultados.ItemsSource = null;
            _currentResults = null;
            _centerLat = 0;
            _centerLng = 0;

            await LoadMap(28.00m, -81.50m, new List<GeoResult>());

            CtrlForm(ViewMode.Filter);
        }

        private void CargarCatalogosFiltro()
        {
            cmbOrigin.ItemsSource = new List<dynamic>
            {
                new { Id = "Staff", Description = "STAFF" },
                new { Id = "Applicant", Description = "APPLICANT" }
            };
            cmbOrigin.DisplayMemberPath = "Description";
            cmbOrigin.SelectedValuePath = "Id";

            cmbDepartment.ItemsSource = new ClassDepartment().Listar().DefaultView;
            cmbDepartment.DisplayMemberPath = "Description";
            cmbDepartment.SelectedValuePath = "Id";

            cmbTopX.ItemsSource = new List<dynamic>
            {
                new { Id = 10, Description = "Top 10" },
                new { Id = 20, Description = "Top 20" },
                new { Id = 50, Description = "Top 50" },
                new { Id = 100, Description = "Top 100" }
            };
            cmbTopX.DisplayMemberPath = "Description";
            cmbTopX.SelectedValuePath = "Id";
            cmbTopX.SelectedValue = 10;
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones de entrada
            if (cmbOrigin.SelectedValue == null) { MessageBox.Show("Origin is required.", "Validation"); return; }
            if (cmbDepartment.SelectedValue == null) { MessageBox.Show("Department is required.", "Validation"); return; }
            if (string.IsNullOrWhiteSpace(txtAddress.Text)) { MessageBox.Show("Address is required.", "Validation"); return; }

            string entityType = cmbOrigin.SelectedValue.ToString();
            int idDepartment = (int)cmbDepartment.SelectedValue;
            string address = txtAddress.Text.Trim();
            int topX = (int)cmbTopX.SelectedValue;
            bool soloActivos = chkActive.IsChecked ?? true;

            // 2. Obtener Coordenadas
            ClassGeocoding geocodingService = new ClassGeocoding();
            var coordinates = await geocodingService.GetCoordinatesAsync(address);

            if (!coordinates.HasValue)
            {
                MessageBox.Show("Could not find coordinates for the provided address.", "Error");
                return;
            }

            _centerLat = coordinates.Value.lat;
            _centerLng = coordinates.Value.lng;

            // 3. Consultar la Base de Datos
            ClassGeoLocator geoLocator = new ClassGeoLocator();
            _currentResults = geoLocator.GetNearestEntities(entityType, idDepartment, _centerLat, _centerLng, topX, soloActivos);

            // --- EL FIX SENSEI: Validación de resultados ---
            if (_currentResults == null || _currentResults.Count == 0)
            {
                MessageBox.Show("No results found for the selected search filters.",
                                "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return; // Salimos aquí para que el formulario se quede en modo Filtro
            }

            // 4. Si llegamos aquí, sí hay datos: Actualizamos UI y navegamos
            dgResultados.ItemsSource = _currentResults;

            await LoadMap(_centerLat, _centerLng, _currentResults);

            CtrlForm(ViewMode.Results);
        }

        private void cmbOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verificamos que el control esté cargado y tenga selección
            if (cmbOrigin.SelectedValue != null)
            {
                string selected = cmbOrigin.SelectedValue.ToString();

                if (selected == "Staff")
                {
                    // Para Staff: habilitamos y marcamos por defecto
                    chkActive.IsEnabled = true;
                    chkActive.IsChecked = true;
                    chkActive.Opacity = 1.0; // Que se vea clarito
                }
                else if (selected == "Applicant")
                {
                    // Para Applicant: lo deshabilitamos porque no aplica el filtro
                    chkActive.IsEnabled = false;
                    chkActive.IsChecked = true; // Lo dejamos marcado pero bloqueado
                    chkActive.Opacity = 0.5; // Lo ponemos un poco gris para indicar que no aplica
                }
            }
        }

        private void BtnResetFiltro_Click(object sender, RoutedEventArgs e) => ResetFormState();

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
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
                if (tabToClose != null) principal.tcPrincipal.Items.Remove(tabToClose);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => ResetFormState();

        private void dgResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgResultados.SelectedItem is GeoResult entidad)
            {
                // 1. Obtenemos las descripciones reales usando tus clases
                string nombreCiudad = "";
                string nombreEstado = "";

                try
                {
                    // Buscamos la ciudad (usamos el ID que viene en la entidad)
                    // Nota: Asegúrate de que GeoResult tenga las propiedades IdGeoCity e IdGeoState cargadas
                    var objCiudad = new ClassGeoCity();
                    DataTable dtCiudad = objCiudad.ListarPorEstado(entidad.IdGeoState); // Filtramos por su estado

                    // Buscamos la fila específica de la ciudad
                    DataRow[] filaCiudad = dtCiudad.Select($"Id = {entidad.IdGeoCity}");
                    if (filaCiudad.Length > 0) nombreCiudad = filaCiudad[0]["Description"].ToString();

                    // Buscamos el estado
                    var objEstado = new ClassGeoState();
                    DataTable dtEstado = objEstado.Listar();
                    DataRow[] filaEstado = dtEstado.Select($"Id = {entidad.IdGeoState}");
                    if (filaEstado.Length > 0) nombreEstado = filaEstado[0]["Description"].ToString();
                }
                catch { /* Manejo de error silencioso o log */ }

                // 2. Construimos la línea de Ciudad, Estado y Zip
                string ciudadEstadoZip = "";
                if (!string.IsNullOrWhiteSpace(nombreCiudad)) ciudadEstadoZip += nombreCiudad;
                if (!string.IsNullOrWhiteSpace(nombreEstado)) ciudadEstadoZip += (ciudadEstadoZip.Length > 0 ? ", " : "") + nombreEstado;

                // Usamos el campo ZipCode que ya tiene la entidad
                if (!string.IsNullOrWhiteSpace(entidad.ZipCode)) ciudadEstadoZip += (ciudadEstadoZip.Length > 0 ? " " : "") + entidad.ZipCode;

                // 3. Construimos la dirección completa
                string fullAddress = entidad.Address;
                if (!string.IsNullOrWhiteSpace(ciudadEstadoZip))
                {
                    fullAddress += "\n" + ciudadEstadoZip;
                }

                // 4. Armamos y mostramos el mensaje final
                string info = $"NAME: {entidad.Name}\n" +
                              $"ADDRESS: {fullAddress}\n" +
                              $"PHONE: {entidad.Phone}\n" +
                              $"EMAIL: {entidad.Email}\n" +
                              $"DISTANCE: {entidad.DistanceMiles:N2} miles";

                MessageBox.Show(info, $"Details - {entidad.EntityType}", MessageBoxButton.OK, MessageBoxImage.Information);

                dgResultados.SelectedItem = null;
            }
        }

        private void dgResultados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgResultados.SelectedItem is GeoResult selectedResult) OpenDetailForm(selectedResult);
        }

        // --- INTEGRACIÓN CON GOOGLE MAPS ---

        // CORRECCIÓN 2: Cambiado a 'async Task' para evitar error de await
        private async Task LoadMap(decimal centerLat, decimal centerLng, List<GeoResult> markers)
        {
            if (webView.CoreWebView2 == null) await webView.EnsureCoreWebView2Async(null);

            string staffColor = "red";
            string applicantColor = "blue";
            // string centerColor = "green";

            var jsMarkers = markers.Select(m => new
            {
                id = m.Id,
                lat = m.Latitude,
                lng = m.Longitude,
                title = $"{m.EntityType}: {m.Name} ({m.DistanceMiles:N2} mi)",
                color = (m.EntityType == "Staff") ? staffColor : applicantColor,
                entityType = m.EntityType
            }).ToList();

            string markersJson = JsonConvert.SerializeObject(jsMarkers);

            // CORRECCIÓN 3: LLAVES DOBLES {{ }} para que C# no se confunda
            string htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    html, body {{ height: 100%; margin: 0; padding: 0; }}
                    #map {{ height: 100%; }}
                </style>
            </head>
            <body>
                <div id='map'></div>
                <script>
                    var map;
                    var googleMarkers = []; 
                    var infoWindow;

                    function initMap() {{
                        map = new google.maps.Map(document.getElementById('map'), {{
                            zoom: 12,
                            center: {{ lat: {centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, lng: {centerLng.ToString(System.Globalization.CultureInfo.InvariantCulture)} }}
                        }});
                        infoWindow = new google.maps.InfoWindow();

                        var centerMarker = new google.maps.Marker({{
                            position: {{ lat: {centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, lng: {centerLng.ToString(System.Globalization.CultureInfo.InvariantCulture)} }},
                            map: map,
                            icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
                        }});

                        var results = {markersJson};
                        results.forEach(function(result) {{
                            var marker = new google.maps.Marker({{
                                position: {{ lat: result.lat, lng: result.lng }},
                                map: map,
                                title: result.title,
                                icon: 'http://maps.google.com/mapfiles/ms/icons/' + result.color + '-dot.png'
                            }});

                            marker.addListener('click', function() {{
                                infoWindow.setContent('<b>' + result.title + '</b>');
                                infoWindow.open(map, marker);
                                window.chrome.webview.postMessage(JSON.stringify({{ type: 'markerClick', id: result.id, entityType: result.entityType }}));
                            }});
                            googleMarkers.push(marker);
                        }});
                    }}
                    function highlightMarker(id, entityType) {{
                        // Lógica de resaltado simple
                    }}
                </script>
                <script async defer src='https://maps.googleapis.com/maps/api/js?key={GoogleMapsApiKey}&callback=initMap'></script>
            </body>
            </html>";

            webView.CoreWebView2.NavigateToString(htmlContent);
        }

        private async void HighlightMarkerOnMap(int id, string entityType)
        {
            if (webView.CoreWebView2 != null)
            {
                string jsFunctionCall = $"highlightMarker({id}, '{entityType}');";
                await webView.CoreWebView2.ExecuteScriptAsync(jsFunctionCall);
            }
        }

        private void OpenDetailForm(GeoResult result)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            if (result.EntityType == "Staff")
            {
                var staffPage = new frmStaff();
                mainWindow.AbrirFormulario($"Staff - {result.Name}", staffPage);
            }
            else
            {
                var applicantPage = new frmApplicants();
                mainWindow.AbrirFormulario($"Applicant - {result.Name}", applicantPage);
            }
        }
    }
}