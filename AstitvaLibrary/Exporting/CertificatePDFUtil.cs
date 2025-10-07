using AstitvaLibrary.Models;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.IO;

namespace AstitvaLibrary.Exporting;

internal static class CertificatePDFUtil
{
    // Government certificate color scheme
    internal static readonly Color _governmentBlue = Color.FromArgb(0, 51, 102); // Official government blue
    internal static readonly Color _governmentGold = Color.FromArgb(218, 165, 32); // Official government gold
    internal static readonly Color _sealRed = Color.FromArgb(139, 0, 0); // Official seal red
    internal static readonly Color _lightBlue = Color.FromArgb(230, 240, 250);
    internal static readonly Color _darkBlue = Color.FromArgb(25, 25, 112);
    internal static readonly Color _black = Color.Black;
    internal static readonly Color _darkGray = Color.FromArgb(64, 64, 64);

    // Government certificate fonts - Professional and formal
    internal static readonly PdfStandardFont _titleFont = new(PdfFontFamily.TimesRoman, 20, PdfFontStyle.Bold);
    internal static readonly PdfStandardFont _subTitleFont = new(PdfFontFamily.TimesRoman, 16, PdfFontStyle.Bold);
    internal static readonly PdfStandardFont _headerFont = new(PdfFontFamily.TimesRoman, 14, PdfFontStyle.Bold);
    internal static readonly PdfStandardFont _subHeaderFont = new(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
    internal static readonly PdfStandardFont _normalFont = new(PdfFontFamily.TimesRoman, 11);
    internal static readonly PdfStandardFont _boldFont = new(PdfFontFamily.TimesRoman, 11, PdfFontStyle.Bold);
    internal static readonly PdfStandardFont _smallFont = new(PdfFontFamily.TimesRoman, 9);
    internal static readonly PdfStandardFont _footerFont = new(PdfFontFamily.TimesRoman, 8);

    internal const float _pageMargin = 40f;
    internal const float _sectionSpacing = 20f;

    /// <summary>
    /// Draws the government logo/emblem
    /// </summary>
    internal static void DrawGovernmentLogo(PdfGraphics graphics, SizeF pageSize, float currentY)
    {
        try
        {
            // Try to load the logo from multiple possible paths
            var possibleLogoPaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "images", "logo.jpg"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.jpg"),
                Path.Combine(Directory.GetCurrentDirectory(), "Astitva", "wwwroot", "images", "logo.jpg"),
                Path.Combine(Directory.GetCurrentDirectory(), "Astitva.Web", "wwwroot", "images", "logo.jpg")
            };

            string logoPath = null;
            foreach (var path in possibleLogoPaths)
            {
                if (File.Exists(path))
                {
                    logoPath = path;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(logoPath))
            {
                // Load and draw the actual logo
                using var logoStream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
                var logoImage = new PdfBitmap(logoStream);

                // Draw logo centered with appropriate size
                var logoSize = 70f;
                var logoRect = new RectangleF(pageSize.Width / 2 - logoSize / 2, currentY, logoSize, logoSize);
                graphics.DrawImage(logoImage, logoRect);
            }
            else
            {
                // Fallback to placeholder if logo not found
                DrawLogoPlaceholder(graphics, pageSize, currentY);
            }
        }
        catch
        {
            // Fallback to placeholder if there's any error loading the logo
            DrawLogoPlaceholder(graphics, pageSize, currentY);
        }
    }

    /// <summary>
    /// Draws a placeholder logo when the actual logo cannot be loaded
    /// </summary>
    internal static void DrawLogoPlaceholder(PdfGraphics graphics, SizeF pageSize, float currentY)
    {
        // Draw government emblem area placeholder
        var emblemRect = new RectangleF(pageSize.Width / 2 - 35, currentY, 70, 70);
        graphics.DrawRectangle(new PdfPen(_governmentBlue, 2), emblemRect);

        // Draw emblem placeholder with government seal
        graphics.DrawString("🇮🇳", new PdfStandardFont(PdfFontFamily.TimesRoman, 28),
            new PdfSolidBrush(_governmentBlue),
            new PointF(pageSize.Width / 2 - 18, currentY + 20));

        // Add "GOVT SEAL" text below
        graphics.DrawString("GOVT SEAL", _smallFont, new PdfSolidBrush(_darkGray),
            new PointF(pageSize.Width / 2 - 25, currentY + 50));
    }

    /// <summary>
    /// Creates the government certificate header with emblem and title
    /// </summary>
    internal static float DrawGovernmentHeader(PdfPage page, string certificateType)
    {
        var graphics = page.Graphics;
        var pageSize = page.GetClientSize();
        var currentY = _pageMargin;

        // Draw government emblem with actual logo
        DrawGovernmentLogo(graphics, pageSize, currentY);

        currentY += 80;

        // Government of India header
        var govIndiaText = "GOVERNMENT OF INDIA";
        var govIndiaSize = _titleFont.MeasureString(govIndiaText);
        graphics.DrawString(govIndiaText, _titleFont, new PdfSolidBrush(_governmentBlue),
            new PointF((pageSize.Width - govIndiaSize.Width) / 2, currentY));

        currentY += 30;

        // Certificate type title
        var certTypeSize = _subTitleFont.MeasureString(certificateType);
        graphics.DrawString(certificateType, _subTitleFont, new PdfSolidBrush(_sealRed),
            new PointF((pageSize.Width - certTypeSize.Width) / 2, currentY));

        currentY += 40;

        // Decorative line
        graphics.DrawLine(new PdfPen(_governmentGold, 2),
            new PointF(_pageMargin, currentY),
            new PointF(pageSize.Width - _pageMargin, currentY));

        return currentY + 20;
    }

    /// <summary>
    /// Draws the certificate statement section
    /// </summary>
    internal static float DrawCertificateStatement(PdfPage page, float currentY, string statement)
    {
        var graphics = page.Graphics;
        var pageSize = page.GetClientSize();

        // Certificate statement box
        var statementRect = new RectangleF(_pageMargin, currentY, pageSize.Width - (2 * _pageMargin), 60);
        graphics.DrawRectangle(new PdfPen(_governmentBlue, 1), new PdfSolidBrush(_lightBlue), statementRect);

        // Draw statement text with proper wrapping
        var textRect = new RectangleF(_pageMargin + 15, currentY + 10, pageSize.Width - (2 * _pageMargin) - 30, 40);
        var stringFormat = new PdfStringFormat
        {
            Alignment = PdfTextAlignment.Center,
            LineAlignment = PdfVerticalAlignment.Middle,
            WordWrap = PdfWordWrapType.Word
        };

        graphics.DrawString(statement, _normalFont, new PdfSolidBrush(_darkBlue), textRect, stringFormat);

        return currentY + statementRect.Height + _sectionSpacing;
    }

    /// <summary>
    /// Draws a registration details section
    /// </summary>
    internal static float DrawRegistrationSection(PdfPage page, float currentY, Dictionary<string, string> registrationDetails)
    {
        var graphics = page.Graphics;
        var pageSize = page.GetClientSize();

        // Section header
        graphics.DrawString("REGISTRATION DETAILS", _subHeaderFont, new PdfSolidBrush(_governmentBlue),
            new PointF(_pageMargin, currentY));

        currentY += 25;

        // Registration details box
        var boxHeight = (registrationDetails.Count * 25) + 20;
        var detailsRect = new RectangleF(_pageMargin, currentY, pageSize.Width - (2 * _pageMargin), boxHeight);
        graphics.DrawRectangle(new PdfPen(_darkGray, 1), detailsRect);

        currentY += 15;

        // Draw registration details
        foreach (var detail in registrationDetails)
        {
            if (!string.IsNullOrEmpty(detail.Value))
            {
                graphics.DrawString($"{detail.Key}:", _boldFont, new PdfSolidBrush(_black),
                    new PointF(_pageMargin + 15, currentY));

                graphics.DrawString(detail.Value, _normalFont, new PdfSolidBrush(_darkGray),
                    new PointF(_pageMargin + 200, currentY));

                currentY += 20;
            }
        }

        return currentY + 15;
    }

    /// <summary>
    /// Draws a personal information section with two columns
    /// </summary>
    internal static float DrawPersonalInfoSection(PdfPage page, float currentY, string sectionTitle,
        Dictionary<string, string> leftColumn, Dictionary<string, string> rightColumn = null)
    {
        var graphics = page.Graphics;
        var pageSize = page.GetClientSize();

        // Section header
        graphics.DrawString(sectionTitle, _subHeaderFont, new PdfSolidBrush(_governmentBlue),
            new PointF(_pageMargin, currentY));

        currentY += 25;

        // Calculate box height based on content
        var maxRows = Math.Max(leftColumn.Count, rightColumn?.Count ?? 0);
        var boxHeight = (maxRows * 25) + 20;

        var detailsRect = new RectangleF(_pageMargin, currentY, pageSize.Width - (2 * _pageMargin), boxHeight);
        graphics.DrawRectangle(new PdfPen(_darkGray, 1), detailsRect);

        var leftX = _pageMargin + 15;
        var rightX = pageSize.Width / 2 + 15;
        var startY = currentY + 15;

        // Draw left column
        var leftY = startY;
        foreach (var detail in leftColumn)
        {
            if (!string.IsNullOrEmpty(detail.Value))
            {
                graphics.DrawString($"{detail.Key}:", _boldFont, new PdfSolidBrush(_black),
                    new PointF(leftX, leftY));

                graphics.DrawString(detail.Value, _normalFont, new PdfSolidBrush(_darkGray),
                    new PointF(leftX + 120, leftY));

                leftY += 20;
            }
        }

        // Draw right column if provided
        if (rightColumn != null)
        {
            var rightY = startY;
            foreach (var detail in rightColumn)
            {
                if (!string.IsNullOrEmpty(detail.Value))
                {
                    graphics.DrawString($"{detail.Key}:", _boldFont, new PdfSolidBrush(_black),
                        new PointF(rightX, rightY));

                    graphics.DrawString(detail.Value, _normalFont, new PdfSolidBrush(_darkGray),
                        new PointF(rightX + 120, rightY));

                    rightY += 20;
                }
            }
        }

        return currentY + boxHeight + _sectionSpacing;
    }

    /// <summary>
    /// Draws the official certification section with signature area
    /// </summary>
    internal static float DrawCertificationSection(PdfPage page, float currentY, MunicipalityModel municipality)
    {
        var graphics = page.Graphics;
        var pageSize = page.GetClientSize();

        // Certification statement
        var certificationText = $"I hereby certify that the above particulars are true according to the register maintained in this office.";

        var certRect = new RectangleF(_pageMargin, currentY, pageSize.Width - (2 * _pageMargin), 40);
        var stringFormat = new PdfStringFormat
        {
            Alignment = PdfTextAlignment.Center,
            LineAlignment = PdfVerticalAlignment.Middle,
            WordWrap = PdfWordWrapType.Word
        };

        graphics.DrawString(certificationText, _normalFont, new PdfSolidBrush(_black), certRect, stringFormat);

        currentY += 60;

        // Date and signature section
        var signatureY = currentY;

        // Date
        graphics.DrawString($"Date: {DateTime.Now:dd/MM/yyyy}", _boldFont, new PdfSolidBrush(_black),
            new PointF(_pageMargin, signatureY));

        // Signature area
        var signatureRect = new RectangleF(pageSize.Width - 250, signatureY, 200, 80);
        graphics.DrawRectangle(new PdfPen(_darkGray, 1), signatureRect);

        graphics.DrawString("[DIGITAL SIGNATURE]", _smallFont, new PdfSolidBrush(_sealRed),
            new PointF(signatureRect.X + 50, signatureRect.Y + 20));

        graphics.DrawString("REGISTRAR", _boldFont, new PdfSolidBrush(_black),
            new PointF(signatureRect.X + 60, signatureRect.Y + 40));

        var municipalityName = municipality?.Name ?? "Municipal Authority";
        graphics.DrawString(municipalityName, _normalFont, new PdfSolidBrush(_black),
            new PointF(signatureRect.X + 20, signatureRect.Y + 60));

        return signatureY + 100;
    }

    /// <summary>
    /// Draws the certificate footer with important notes
    /// </summary>
    internal static void DrawCertificateFooter(PdfPage page, float currentY, string certificateId)
    {
        var graphics = page.Graphics;
        var pageSize = page.GetClientSize();

        // Important notes section
        var footerY = Math.Max(currentY + 30, pageSize.Height - 150);

        // Decorative line
        graphics.DrawLine(new PdfPen(_governmentGold, 1),
            new PointF(_pageMargin, footerY),
            new PointF(pageSize.Width - _pageMargin, footerY));

        footerY += 15;

        // Important note
        var noteText = "Important: This is a digitally generated certificate. Any tampering or misuse is punishable under law.";
        var noteRect = new RectangleF(_pageMargin, footerY, pageSize.Width - (2 * _pageMargin), 25);
        var noteFormat = new PdfStringFormat
        {
            Alignment = PdfTextAlignment.Center,
            LineAlignment = PdfVerticalAlignment.Middle,
            WordWrap = PdfWordWrapType.Word
        };

        graphics.DrawString(noteText, _footerFont, new PdfSolidBrush(_sealRed), noteRect, noteFormat);

        footerY += 35;

        // Certificate ID and generation info
        graphics.DrawString($"Certificate ID: {certificateId}", _footerFont, new PdfSolidBrush(_darkGray),
            new PointF(_pageMargin, footerY));

        var generatedText = $"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        var generatedSize = _footerFont.MeasureString(generatedText);
        graphics.DrawString(generatedText, _footerFont, new PdfSolidBrush(_darkGray),
            new PointF(pageSize.Width - _pageMargin - generatedSize.Width, footerY));
    }

    /// <summary>
    /// Creates a professional government certificate PDF document
    /// </summary>
    internal static (PdfDocument document, PdfPage page) CreateCertificateDocument()
    {
        var pdfDocument = new PdfDocument();
        var pdfPage = pdfDocument.Pages.Add();
        pdfDocument.PageSettings.Size = PdfPageSize.A4;
        pdfDocument.PageSettings.Margins.All = 0; // Remove default margins for custom control

        return (pdfDocument, pdfPage);
    }

    /// <summary>
    /// Finalizes PDF document and returns as MemoryStream
    /// </summary>
    internal static MemoryStream FinalizePdfDocument(PdfDocument pdfDocument)
    {
        var stream = new MemoryStream();
        pdfDocument.Save(stream);
        pdfDocument.Close();
        return stream;
    }

    /// <summary>
    /// Generates a safe filename for certificates
    /// </summary>
    internal static string GenerateCertificateFileName(string certificateType, string personName, string registrationNo)
    {
        var safeName = personName?.Replace(" ", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "_")
            .Replace("*", "_")
            .Replace("?", "_")
            .Replace("\"", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace("|", "_") ?? "Certificate";

        var safeRegNo = !string.IsNullOrEmpty(registrationNo) ? $"_{registrationNo}" : "";
        return $"{certificateType}_{safeName}{safeRegNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
    }
}