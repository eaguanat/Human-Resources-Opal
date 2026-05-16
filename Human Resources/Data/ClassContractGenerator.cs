using iText.Forms; // <--- Esta es la más importante para AcroForm
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Data; // Necesario para DataRow y Field<T>
using System.Diagnostics; // Para Process.Start para imprimir
using System.Drawing.Printing; // Para la impresión (puede que no sea estrictamente necesario con Process.Start)
using System.IO;
using System.Linq;
using System.Windows; // Para MessageBox, aunque idealmente se lanza excepción
using System.Threading.Tasks;


namespace Human_Resources.Data
{
    // Modelo para un campo de PDF con su estado de validación
    public class PdfFieldValidation
    {
        public string PdfFieldName { get; set; } // Nombre del campo en el PDF (para referencia interna)
        public string Description { get; set; } // Descripción amigable en la UI
        public string Value { get; set; }       // Valor que se intentará inyectar
        public bool IsPresent { get; set; }     // Si el valor está presente o es nulo/vacío
        public string StatusIcon { get; set; }  // "✅" o "❌"
    }

    public class ClassContractGenerator
    {
        // --- PROPIEDADES ---
        public ClassCompany CompanyData { get; private set; }
        public ClassStaff StaffData { get; private set; }
        public ClassStaff.LicenseEntry StaffLicenseData { get; private set; } // La primera licencia encontrada
        public ClassGeoState StaffStateData { get; private set; }
        public ClassGeoCity StaffCityData { get; private set; }
        public ClassDepartment DepartmentData { get; private set; } // Datos del departamento del staff

        public ClassGeoState CompanyStateData { get; private set; }
        public ClassGeoCity CompanyCityData { get; private set; }


        public ClassContractGenerator()
        {
            // Inicializar las propiedades para evitar NREs
            CompanyData = new ClassCompany();
            StaffData = new ClassStaff();
            StaffLicenseData = new ClassStaff.LicenseEntry(); // Usamos la clase anidada en ClassStaff
            StaffStateData = new ClassGeoState();
            StaffCityData = new ClassGeoCity();
            DepartmentData = new ClassDepartment();
            CompanyStateData = new ClassGeoState();
            CompanyCityData = new ClassGeoCity();
        }

        /// <summary>
        /// Carga todos los datos necesarios para generar el contrato de un Staff dado.
        /// </summary>
        /// <param name="idStaff">ID del Staff seleccionado.</param>
        /// <returns>True si todos los datos principales (Company y Staff) se cargaron correctamente.</returns>
        public bool LoadContractData(int idStaff)
        {
            // 1. Cargar datos de la Compañía (ID=1)
            if (!CompanyData.Obtener())
            {
                MessageBox.Show("Could not load company data. Please configure company details in Setup > Company.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            // Cargar datos de estado/ciudad de la compañía
            if (CompanyData.IdGeoState.HasValue)
            {
                var dtState = new ClassGeoState().Listar().AsEnumerable()
                                .FirstOrDefault(r => r.Field<int>("Id") == CompanyData.IdGeoState);
                if (dtState != null) CompanyStateData.Description = dtState.Field<string>("Description");

                if (CompanyData.IdGeoCity.HasValue)
                {
                    var dtCity = new ClassGeoCity().ListarPorEstado(CompanyData.IdGeoState.Value).AsEnumerable()
                                    .FirstOrDefault(r => r.Field<int>("Id") == CompanyData.IdGeoCity);
                    if (dtCity != null) CompanyCityData.Description = dtCity.Field<string>("Description");
                }
            }


            // 2. Cargar datos del Staff
            if (!StaffData.ObtenerPorId(idStaff))
            {
                MessageBox.Show($"Could not load staff data for ID: {idStaff}.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            // Cargar datos de departamento del Staff
            if (StaffData.IdDepartment.HasValue)
            {
                ClassDepartment deptTemp = new ClassDepartment();
                DataTable dtDept = deptTemp.Listar(); // Listamos todos los departamentos
                var deptRow = dtDept.AsEnumerable()
                                .FirstOrDefault(r => r.Field<int>("Id") == StaffData.IdDepartment);
                if (deptRow != null)
                {
                    DepartmentData.Id = StaffData.IdDepartment.Value;
                    DepartmentData.Description = deptRow.Field<string>("Description");
                    DepartmentData.ContractTemplateName = deptRow.Field<string>("ContractTemplateName"); // ¡Cargar nombre de plantilla!
                }
            }
            // Cargar datos de estado/ciudad del Staff
            if (StaffData.IdGeoState.HasValue)
            {
                var dtState = new ClassGeoState().Listar().AsEnumerable()
                                .FirstOrDefault(r => r.Field<int>("Id") == StaffData.IdGeoState);
                if (dtState != null) StaffStateData.Description = dtState.Field<string>("Description");

                if (StaffData.IdGeoCity.HasValue)
                {
                    var dtCity = new ClassGeoCity().ListarPorEstado(StaffData.IdGeoState.Value).AsEnumerable()
                                    .FirstOrDefault(r => r.Field<int>("Id") == StaffData.IdGeoCity);
                    if (dtCity != null) StaffCityData.Description = dtCity.Field<string>("Description");
                }
            }

            // 3. Cargar datos de Licencia del Staff
            if (StaffData.IdDepartment.HasValue) // La licencia está relacionada con el departamento
            {
                var licenses = new ClassStaff().ObtenerLicenciasPorEmpleado(idStaff, StaffData.IdDepartment);
                // Tomar la primera licencia con LicenseNumber si existe
                StaffLicenseData = licenses.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.LicenseNumber));
                if (StaffLicenseData == null)
                {
                    StaffLicenseData = new ClassStaff.LicenseEntry(); // Asegurar que no sea null
                }
            }
            else // Si no hay departamento, no puede haber licencia específica de departamento
            {
                StaffLicenseData = new ClassStaff.LicenseEntry();
            }


            return true;
        }

        /// <summary>
        /// Mapea los datos cargados a un diccionario de campos de PDF.
        /// </summary>
        public Dictionary<string, string> MapDataToPdfFields()
        {
            var pdfFields = new Dictionary<string, string>();

            // --- CAMPOS DEL STAFF ---
            string staffFullName = $"{StaffData.Name} {StaffData.LastName}".Trim();
            string staffCity = StaffCityData.Description ?? "";
            string staffState = StaffStateData.Description ?? "";
            string staffZipCode = StaffData.ZipCod ?? ""; // Usa ZipCod como está en tblStaff
            string staffCityStateZipCode = $"{staffCity}, {staffState}, {staffZipCode}".Trim(',', ' ').Trim();
            string staffFullAddress = $"{StaffData.Address}, {staffCityStateZipCode}".Trim(',', ' ').Trim();

            pdfFields["txt_FullName"] = staffFullName;
            pdfFields["txt_FirstName"] = StaffData.Name ?? "";
            pdfFields["txt_LastName"] = StaffData.LastName ?? "";
            pdfFields["txt_Address"] = StaffData.Address ?? "";
            pdfFields["txt_City"] = staffCity;
            pdfFields["txt_State"] = staffState;
            pdfFields["txt_ZipCode"] = staffZipCode;
            pdfFields["txt_City_State_ZipCode"] = staffCityStateZipCode;
            pdfFields["txt_FullAddress"] = staffFullAddress;
            pdfFields["txt_Phone"] = StaffData.Phone ?? "";
            pdfFields["txt_Social"] = StaffData.Social ?? "";
            pdfFields["txt_email"] = StaffData.Email ?? "";
            pdfFields["txt_BirthDay"] = StaffData.Birthday?.ToString("MM/dd/yyyy") ?? "";

            pdfFields["txt_Title"] = StaffLicenseData.Description ?? ""; // Descripción de la licencia
            pdfFields["txt_LicenseNro"] = StaffLicenseData.LicenseNumber ?? ""; // Número de licencia

            // --- CAMPOS DE LA COMPAÑÍA ---
            string companyCity = CompanyCityData.Description ?? "";
            string companyState = CompanyStateData.Description ?? "";
            string companyZipCode = CompanyData.ZipCode ?? "";
            string companyCityStateZipCode = $"{companyCity}, {companyState}, {companyZipCode}".Trim(',', ' ').Trim();
            string companyFullAddress = $"{CompanyData.Address}, {companyCityStateZipCode}".Trim(',', ' ').Trim();

            pdfFields["txt_CIA_Name"] = CompanyData.Name ?? "";
            pdfFields["txt_CIA_Address"] = CompanyData.Address ?? "";
            pdfFields["txt_CIA_City"] = companyCity;
            pdfFields["txt_CIA_State"] = companyState;
            pdfFields["txt_CIA_ZipCode"] = companyZipCode;
            pdfFields["txt_CIA_City_State_ZipCode"] = companyCityStateZipCode;
            pdfFields["txt_CIA_FullAddress"] = companyFullAddress;

            return pdfFields;
        }

        /// <summary>
        /// Valida si todos los campos requeridos para el contrato están presentes.
        /// </summary>
        /// <param name="pdfFields">Diccionario de campos PDF con sus valores ya mapeados.</param>
        /// <returns>Lista de objetos de validación para mostrar en la UI.</returns>
        public List<PdfFieldValidation> ValidatePdfFields(Dictionary<string, string> pdfFields)
        {
            var validationResults = new List<PdfFieldValidation>();

            // Definir campos requeridos y su descripción amigable
            // Solo los campos del STAFF son obligatorios para esta validación que bloquea la impresión
            var requiredStaffFields = new Dictionary<string, string>
            {
                {"txt_FullName", "Staff Full Name"},
                {"txt_FirstName", "Staff First Name"},
                {"txt_LastName", "Staff Last Name"},
                {"txt_Address", "Staff Address"},
                {"txt_City", "Staff City"},
                {"txt_State", "Staff State"},
                {"txt_ZipCode", "Staff Zip Code"},
                {"txt_Phone", "Staff Phone"},
                {"txt_Social", "Staff Social Security"},
                {"txt_email", "Staff Email"},
                {"txt_BirthDay", "Staff Birthday"},
            };

            foreach (var entry in requiredStaffFields)
            {
                string fieldValue = pdfFields.ContainsKey(entry.Key) ? pdfFields[entry.Key] : null;
                bool isPresent = !string.IsNullOrWhiteSpace(fieldValue);

                validationResults.Add(new PdfFieldValidation
                {
                    PdfFieldName = entry.Key,
                    Description = entry.Value,
                    Value = fieldValue,
                    IsPresent = isPresent,
                    StatusIcon = isPresent ? "✅" : "❌"
                });
            }

            // Validación especial para Licencia: es obligatorio el número si el título existe
            // Si el departamento no tiene plantilla de contrato, la licencia no es un bloqueador.
            if (!string.IsNullOrWhiteSpace(DepartmentData.ContractTemplateName))
            {
                bool licenseTitlePresent = pdfFields.ContainsKey("txt_Title") && !string.IsNullOrWhiteSpace(pdfFields["txt_Title"]);
                bool licenseNumberPresent = pdfFields.ContainsKey("txt_LicenseNro") && !string.IsNullOrWhiteSpace(pdfFields["txt_LicenseNro"]);

                if (licenseTitlePresent && !licenseNumberPresent) // Si hay título pero no número
                {
                    validationResults.Add(new PdfFieldValidation
                    {
                        PdfFieldName = "txt_LicenseNro",
                        Description = "Staff License Number",
                        Value = pdfFields.ContainsKey("txt_LicenseNro") ? pdfFields["txt_LicenseNro"] : null,
                        IsPresent = false,
                        StatusIcon = "❌"
                    });
                }
                else // Si no hay título, o si hay título y número, se considera OK para el checklist
                {
                    validationResults.Add(new PdfFieldValidation
                    {
                        PdfFieldName = "txt_Title",
                        Description = "Staff License Title",
                        Value = pdfFields.ContainsKey("txt_Title") ? pdfFields["txt_Title"] : null,
                        IsPresent = true, // No bloquea si no hay título o si todo está bien
                        StatusIcon = "✅"
                    });
                    if (licenseTitlePresent && licenseNumberPresent) // Si ya hay número y título
                    {
                        validationResults.Add(new PdfFieldValidation
                        {
                            PdfFieldName = "txt_LicenseNro",
                            Description = "Staff License Number",
                            Value = pdfFields.ContainsKey("txt_LicenseNro") ? pdfFields["txt_LicenseNro"] : null,
                            IsPresent = true,
                            StatusIcon = "✅"
                        });
                    }
                }
            }


            return validationResults;
        }








        public bool PrintPdfContract(Dictionary<string, string> pdfFields)
        {
            // 1. VALIDACIÓN
            if (CompanyData == null || StaffData == null || DepartmentData == null) return false;

            // 2. RUTAS
            string templatesPath = CompanyData.PathTemplates ?? "";
            string contractsPath = CompanyData.PathContracts ?? "";
            string templateFileName = DepartmentData.ContractTemplateName ?? "";
            string fullTemplatePath = Path.Combine(templatesPath, contractsPath, templateFileName);

            string outputFileName = $"Temp_{StaffData.LastName}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string fullOutputPath = Path.Combine(templatesPath, contractsPath, outputFileName);

            if (!File.Exists(fullTemplatePath)) return false;

            try
            {
                // --- A. GENERACIÓN (iText 7) ---
                using (PdfReader reader = new PdfReader(fullTemplatePath))
                using (PdfWriter writer = new PdfWriter(fullOutputPath))
                {
                    // SetSmartMode ayuda a que el PDF de 53 páginas no se corrompa
                    writer.SetSmartMode(true);

                    using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
                    {
                        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                        if (form != null)
                        {
                            var fields = form.GetAllFormFields();
                            foreach (var item in pdfFields)
                            {
                                if (fields.ContainsKey(item.Key))
                                {
                                    fields[item.Key].SetValue(item.Value);
                                }
                            }

                            // APLANADO: Vital para convertir el PDF en "foto" 
                            // y evitar que Adobe pida guardar cambios o corte páginas.
                            form.FlattenFields();
                        }
                        pdfDoc.Close();
                    }
                    writer.Close(); // Forzamos el cierre físico del archivo
                }

                // B. PAUSA DE SEGURIDAD (5 segundos para que el disco termine la escritura física)
                System.Threading.Thread.Sleep(5000);

                // --- C. IMPRESIÓN DIRECTA ---
                ProcessStartInfo printInfo = new ProcessStartInfo();
                printInfo.FileName = fullOutputPath;
                printInfo.Verb = "print";
                printInfo.CreateNoWindow = true;
                printInfo.WindowStyle = ProcessWindowStyle.Hidden;
                printInfo.UseShellExecute = true;

                Process.Start(printInfo);

                // --- D. BORRADO SEGURO (En segundo plano) ---
                // Esperamos 45 segundos para dar tiempo a que la cola de impresión termine de leer
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(45000);
                        if (File.Exists(fullOutputPath))
                        {
                            File.Delete(fullOutputPath);
                        }
                    }
                    catch
                    {
                        // Si falla porque el archivo sigue bloqueado, se ignora silenciosamente
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                // Si falla la impresión, intentamos abrir el archivo para impresión manual
                try { Process.Start(new ProcessStartInfo(fullOutputPath) { UseShellExecute = true }); } catch { }
                MessageBox.Show($"Error en proceso: {ex.Message}");
                return false;
            }
        }


    }
}