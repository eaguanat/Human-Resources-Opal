using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows; // Para MessageBox, aunque idealmente se lanza excepción
using iText.Kernel.Pdf;
using iText.Forms; // <--- Esta es la más importante para AcroForm
using iText.Forms.Fields;
using System.Drawing.Printing; // Para la impresión (puede que no sea estrictamente necesario con Process.Start)
using System.Diagnostics; // Para Process.Start para imprimir
using System.Data; // Necesario para DataRow y Field<T>

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


        /// <summary>
        /// Genera y rellena el PDF, y lo envía a la impresora predeterminada.
        /// </summary>
        /// <param name="pdfFields">Diccionario de campos a rellenar.</param>
        /// <returns>True si la impresión fue exitosa.</returns>
        public bool PrintPdfContract(Dictionary<string, string> pdfFields)
        {
            try
            {
                // 1. VALIDACIÓN PREVIA DE OBJETOS
                if (CompanyData == null) { MessageBox.Show("Company data is not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                if (StaffData == null) { MessageBox.Show("Staff data is not selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                if (DepartmentData == null) { MessageBox.Show("Department data is missing for this staff.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return false; }

                // 2. OBTENER RUTAS
                string templatesPath = CompanyData.PathTemplates ?? "";
                string contractsPath = CompanyData.PathContracts ?? "";
                string templateFileName = DepartmentData.ContractTemplateName ?? "";

                // 3. VALIDACIÓN DE RUTAS Y ARCHIVOS
                if (string.IsNullOrWhiteSpace(templatesPath) || !Directory.Exists(templatesPath))
                {
                    MessageBox.Show($"Templates path is invalid or inaccessible:\n{templatesPath}", "Path Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(contractsPath) || !Directory.Exists(contractsPath))
                {
                    MessageBox.Show($"Output contracts path does not exist:\n{contractsPath}", "Path Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(templateFileName))
                {
                    MessageBox.Show("The department has no PDF template assigned.", "Template Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Full Path - Usando tu lógica de concatenación directa
                string fullTemplatePath = $"{templatesPath}{contractsPath}{templateFileName}";

                if (!File.Exists(fullTemplatePath))
                {
                    MessageBox.Show($"PDF File not found at:\n{fullTemplatePath}", "File Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;
                }

                // 4. GENERACIÓN EN MEMORIA
                string outputFileName = $"Contract_{StaffData.LastName}_{StaffData.Name}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                // Importante: usamos la misma lógica de "Full Path" para el archivo de salida
                string fullOutputPath = $"{templatesPath}{contractsPath}{outputFileName}";

                using (var originalPdfStream = new FileStream(fullTemplatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var outputMemoryStream = new MemoryStream())
                {
                    using (PdfReader pdfReader = new PdfReader(originalPdfStream))
                    using (PdfWriter pdfWriter = new PdfWriter(outputMemoryStream))
                    using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
                    {
                        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDocument, true);
                        if (form == null)
                        {
                            MessageBox.Show("The PDF does not have interactive fields (AcroForms).", "PDF Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        foreach (var field in pdfFields)
                        {
                            PdfFormField pdfField = form.GetField(field.Key);
                            if (pdfField != null)
                            {
                                pdfField.SetValue(field.Value ?? "");
                            }
                        }

                        form.FlattenFields();
                        pdfDocument.Close();
                    }
                    File.WriteAllBytes(fullOutputPath, outputMemoryStream.ToArray());
                }

                // 5. VISTA PREVIA (Para elegir impresora y ahorrar papel/tinta)
                // Eliminamos el verbo "print" para que abra el visor predeterminado
                ProcessStartInfo previewInfo = new ProcessStartInfo
                {
                    FileName = fullOutputPath,
                    UseShellExecute = true
                };

                Process.Start(previewInfo);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error during printing process:\n{ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}