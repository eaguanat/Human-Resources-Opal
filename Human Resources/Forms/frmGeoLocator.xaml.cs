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
                string json = e.WebMessageAsJson;
                // El mapa nos envía el ID del marcador que se tocó
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                int id = data.id;

                // Buscamos la entidad en nuestra lista actual de resultados
                var entidad = _currentResults.FirstOrDefault(x => x.Id == id);

                if (entidad != null)
                {
                    string info = $"NAME: {entidad.Name}\n" +
                                  $"ADDRESS: {entidad.Address}\n" +
                                  $"PHONE: {entidad.Phone}\n" +
                                  $"EMAIL: {entidad.Email}\n" +
                                  $"DISTANCE: {entidad.DistanceMiles:N2} miles";

                    MessageBox.Show(info, $"Details - {entidad.EntityType}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
        }

        // 1. Agregamos "async" aquí
        private async void ResetFormState()
        {
            cmbOrigin.SelectedIndex = -1;
            cmbDepartment.SelectedIndex = -1;
            txtAddress.Clear();
            cmbTopX.SelectedValue = 10;
            dgResultados.ItemsSource = null;
            _currentResults = null;
            _centerLat = 0;
            _centerLng = 0;

            // 2. Agregamos "await" aquí
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
            if (cmbOrigin.SelectedValue == null) { MessageBox.Show("Origin is required.", "Validation"); return; }
            if (cmbDepartment.SelectedValue == null) { MessageBox.Show("Department is required.", "Validation"); return; }
            if (string.IsNullOrWhiteSpace(txtAddress.Text)) { MessageBox.Show("Address is required.", "Validation"); return; }

            string entityType = cmbOrigin.SelectedValue.ToString();
            int idDepartment = (int)cmbDepartment.SelectedValue;
            string address = txtAddress.Text.Trim();
            int topX = (int)cmbTopX.SelectedValue;

            ClassGeocoding geocodingService = new ClassGeocoding();
            var coordinates = await geocodingService.GetCoordinatesAsync(address);

            if (!coordinates.HasValue)
            {
                MessageBox.Show("Could not find coordinates.", "Error");
                return;
            }

            _centerLat = coordinates.Value.lat;
            _centerLng = coordinates.Value.lng;

            ClassGeoLocator geoLocator = new ClassGeoLocator();
            _currentResults = geoLocator.GetNearestEntities(entityType, idDepartment, _centerLat, _centerLng, topX);

            dgResultados.ItemsSource = _currentResults;

            // CORRECCIÓN: Agregamos 'await' aquí
            await LoadMap(_centerLat, _centerLng, _currentResults);

            CtrlForm(ViewMode.Results);
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
            // Verificamos que haya algo seleccionado
            if (dgResultados.SelectedItem is GeoResult entidad)
            {
                // En lugar de abrir formularios, usamos la misma lógica del mensaje emergente
                string info = $"NAME: {entidad.Name}\n" +
                              $"ADDRESS: {entidad.Address}\n" +
                              $"PHONE: {entidad.Phone}\n" +
                              $"EMAIL: {entidad.Email}\n" +
                              $"DISTANCE: {entidad.DistanceMiles:N2} miles";

                MessageBox.Show(info, $"Details - {entidad.EntityType}", MessageBoxButton.OK, MessageBoxImage.Information);

                // Opcional: Deseleccionar para que el usuario pueda volver a hacer clic
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