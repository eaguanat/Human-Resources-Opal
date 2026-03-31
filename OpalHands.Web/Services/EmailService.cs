using System.Net;
using System.Net.Mail;
using System.Text;
using OpalHands.Web.Data;
using OpalHands.Web.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpalHands.Web.Services
{
    public class EmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmailService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task SendWelcomeEmailWithPdfAsync(string applicantEmail, string applicantName, int departmentId)
        {
            // 1. OBTENCIÓN DE DATOS CON BLINDAJE
            var company = await _context.tblCompany.FirstOrDefaultAsync();
            if (company == null) return;

            // Buscamos nombres con valores por defecto (?? "") para evitar el error verde
            var cityName = await _context.tblGeoCity
                .Where(c => c.Id == company.idGeoCity)
                .Select(c => c.Description)
                .FirstOrDefaultAsync() ?? "";

            var stateName = await _context.tblGeoState
                .Where(s => s.Id == company.idGeoState)
                .Select(s => s.Description)
                .FirstOrDefaultAsync() ?? "";

            var dept = await _context.tblDepartment.FindAsync(departmentId);
            var docs = await _context.tblApplicantsDocs
                                     .Where(d => d.idDepartment == departmentId)
                                     .ToListAsync();

            // 2. GENERAR EL PDF
            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            var logoPath = Path.Combine(_env.WebRootPath, "images", "Opal Hands.jpg");
                            if (File.Exists(logoPath))
                            {
                                col.Item().Width(100).Image(logoPath);
                            }
                            else
                            {
                                col.Item().Text("Opal Hands").FontSize(24).Bold().FontColor("#00BCD4");
                            }
                            col.Item().Text(company.SenderName ?? "Opal Hands HR").FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(company.Address ?? "").FontSize(9).FontColor(Colors.Grey.Medium);

                            // Si cityName y stateName están vacíos, no romperá el diseño
                            col.Item().Text($"{cityName}, {stateName} {company.ZipCode}".Trim().Trim(',')).FontSize(9).FontColor(Colors.Grey.Medium);

                            col.Item().PaddingTop(5).Text(company.Phone ?? "").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(company.Email ?? "").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().AlignCenter().Text("REQUIRED DOCUMENTS / DOCUMENTOS REQUERIDOS").FontSize(14).Bold().FontColor(Colors.Black);
                        col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text($"Applicant / Aplicante: {applicantName}").Bold();
                        col.Item().Text($"Position  / Posición : {dept?.Description ?? "N/A"}");
                        col.Item().Text($"Date      / Fecha    : {DateTime.Now:MM/dd/yyyy HH:mm}");

                        col.Item().PaddingTop(20).Text("Please use this checklist to gather your documents:");
                        col.Item().Text("Por favor use esta lista para reunir sus documentos:");

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                            });

                            foreach (var doc in docs)
                            {
                                table.Cell().PaddingVertical(4).AlignCenter().Text("⏹").FontSize(14).FontColor(Colors.Black);
                                table.Cell().PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Text(doc.Description ?? "");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page / Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf();

            // 3. ENVÍO (Mantenemos tu lógica de Gmail)
            var smtpClient = new SmtpClient(company.MailServer)
            {
                Port = company.MailPort ?? 587,
                Credentials = new NetworkCredential(company.MailUsername, "asqseojmfybdvtwv"),
                EnableSsl = company.EnableSSL ?? true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(company.MailUsername ?? "", company.SenderName ?? "Opal Hands HR"),
                Subject = "Required Documents - " + applicantName,
                Body = "Dear applicant, please find attached the list of required documents.\n\nEstimado aplicante, adjunto encontrará la lista de documentos requeridos.",
                IsBodyHtml = false
            };
            mailMessage.To.Add(applicantEmail);
            mailMessage.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), "Opal_Hands_Documents.pdf"));

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}