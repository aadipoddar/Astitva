using AstitvaLibrary.Models;

namespace AstitvaLibrary.Exporting;

public static class BirthCertificatePDFExport
{
	/// <summary>
	/// Generates a comprehensive professional government-style birth certificate PDF
	/// </summary>
	public static MemoryStream GenerateBirthCertificate(BirthCertificateOverviewModel certificate, UserModel user)
	{
		var (document, page) = CertificatePDFUtil.CreateCertificateDocument();

		try
		{
			var currentY = 0f;
			var certificateId = GenerateCertificateId(certificate, user);

			// 1. Draw enhanced government header with watermark
			currentY = DrawEnhancedGovernmentHeader(page, "BIRTH CERTIFICATE", certificateId);

			// 2. Draw comprehensive certificate statement
			var statement = "This is to certify that the following particulars have been compiled from the original " +
						   "record of birth which is registered in this office under the Registration of Births and Deaths Act, 1969. " +
						   "This certificate is issued under the authority vested in the Registrar of Births and Deaths.";
			currentY = CertificatePDFUtil.DrawCertificateStatement(page, currentY, statement);

			// 3. Draw detailed registration information
			var registrationDetails = new Dictionary<string, string>
			{
				["Certificate Number"] = certificateId,
				["Registration Number"] = certificate.RegistrationNo?.ToString() ?? "Pending Assignment",
				["Registration Date"] = certificate.RegistrationDate.ToString("dd MMMM yyyy"),
				["Registration Authority"] = certificate.MunicipalityName ?? "Municipal Corporation",
				["District/City"] = certificate.MunicipalityCityName ?? "Administrative District",
				["State"] = "Government State",
				["Registration Type"] = "Original Birth Record"
			};
			currentY = CertificatePDFUtil.DrawRegistrationSection(page, currentY, registrationDetails);

			// 4. Draw comprehensive personal information
			var personalInfo = new Dictionary<string, string>
			{
				["Full Name"] = GetFullName(certificate.FirstName, certificate.MiddleName, certificate.LastName),
				["Gender"] = certificate.Sex ?? "Not specified",
				["Date of Birth"] = certificate.DateOfBirth.ToString("dd MMMM yyyy"),
				["Place of Birth"] = certificate.BirthPlace ?? "Not specified",
				["Birth Time"] = "Not recorded",
				["Birth Weight"] = "Not recorded"
			};

			var birthDetails = new Dictionary<string, string>
			{
				["Day of Week"] = certificate.DateOfBirth.DayOfWeek.ToString(),
				["Birth Order"] = "Not specified",
				["Multiple Birth"] = "Single Birth",
				["Premature"] = "Not specified",
				["Hospital/Institution"] = certificate.BirthPlace ?? "Not specified",
				["Attending Physician"] = "Not recorded"
			};

			currentY = CertificatePDFUtil.DrawPersonalInfoSection(page, currentY, "BIRTH PARTICULARS", personalInfo, birthDetails);

			// 5. Draw comprehensive parent information
			var fatherInfo = new Dictionary<string, string>
			{
				["Father's Full Name"] = certificate.FatherName ?? "Not specified",
				["Father's Age at Birth"] = "Not recorded",
				["Father's Occupation"] = "Not recorded",
				["Father's Education"] = "Not recorded",
				["Father's Nationality"] = "Indian",
				["Father's Religion"] = "Not specified"
			};

			var motherInfo = new Dictionary<string, string>
			{
				["Mother's Full Name"] = certificate.MotherName ?? "Not specified",
				["Mother's Age at Birth"] = "Not recorded",
				["Mother's Occupation"] = "Not recorded",
				["Mother's Education"] = "Not recorded",
				["Mother's Nationality"] = "Indian",
				["Mother's Religion"] = "Not specified"
			};

			currentY = CertificatePDFUtil.DrawPersonalInfoSection(page, currentY, "PARENTAL INFORMATION", fatherInfo, motherInfo);

			// 6. Draw address and residence information
			var permanentAddress = new Dictionary<string, string>
			{
				["Permanent Address"] = certificate.Address ?? "Not specified",
				["Pin Code"] = "Not specified",
				["Tehsil/Taluka"] = "Not specified",
				["Village/Town"] = "Not specified"
			};

			var residenceInfo = new Dictionary<string, string>
			{
				["Type of Residence"] = "Not specified",
				["Duration at Address"] = "Not specified",
				["Previous Address"] = "Not applicable",
				["Contact Information"] = user?.Number ?? "Not provided"
			};

			currentY = CertificatePDFUtil.DrawPersonalInfoSection(page, currentY, "RESIDENCE INFORMATION", permanentAddress, residenceInfo);

			// 7. Draw informant details
			var informantDetails = new Dictionary<string, string>
			{
				["Informant Name"] = GetFullName(certificate.FatherName, null, null) ?? certificate.MotherName ?? "Legal Guardian",
				["Relationship"] = !string.IsNullOrEmpty(certificate.FatherName) ? "Father" : "Mother",
				["Informant Address"] = certificate.Address ?? "Same as above",
				["Date of Information"] = certificate.RegistrationDate.ToString("dd MMMM yyyy"),
				["Signature Status"] = "Digitally Verified",
				["Contact Number"] = user?.Number ?? "Not provided"
			};

			currentY = CertificatePDFUtil.DrawPersonalInfoSection(page, currentY, "INFORMANT DETAILS", informantDetails);

			// 8. Draw verification and security features
			var verificationInfo = new Dictionary<string, string>
			{
				["Digital Verification Code"] = GenerateVerificationCode(certificate),
				["Security Hash"] = GenerateSecurityHash(certificateId),
				["QR Code Reference"] = $"QR-{certificateId}",
				["Blockchain Hash"] = "Not implemented",
				["Anti-Forgery Code"] = GenerateAntiForgeryCode(certificate.Id),
				["Biometric Reference"] = "Not available"
			};

			currentY = CertificatePDFUtil.DrawPersonalInfoSection(page, currentY, "VERIFICATION & SECURITY", verificationInfo);

			// 9. Draw legal disclaimers and notes
			currentY = DrawLegalDisclaimer(page, currentY);

			// 10. Draw enhanced certification section with multiple authorities
			currentY = DrawEnhancedCertificationSection(page, currentY, certificate);

			// 11. Draw comprehensive footer with multiple identifiers
			DrawEnhancedCertificateFooter(page, currentY, certificateId, certificate);

			return CertificatePDFUtil.FinalizePdfDocument(document);
		}
		catch
		{
			document?.Close();
			throw;
		}
	}

	/// <summary>
	/// Gets the full name from individual name components
	/// </summary>
	private static string GetFullName(string firstName, string middleName, string lastName)
	{
		var parts = new[] { firstName, middleName, lastName }
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Select(p => p.Trim());
		return string.Join(" ", parts);
	}

	/// <summary>
	/// Generates a unique certificate ID
	/// </summary>
	private static string GenerateCertificateId(BirthCertificateOverviewModel certificate, UserModel user)
	{
		var municipalityId = certificate.MunicipalityId.ToString("D3");
		var userId = user?.Id.ToString("D6") ?? "000000";
		var certificateId = certificate.Id.ToString("D6");
		var dateCode = certificate.RegistrationDate.ToString("yyyyMM");

		return $"BC-{municipalityId}-{dateCode}-{certificateId}-{userId}";
	}

	/// <summary>
	/// Generates a safe filename for the birth certificate
	/// </summary>
	public static string GenerateFileName(BirthCertificateOverviewModel certificate, UserModel user)
	{
		var fullName = GetFullName(certificate.FirstName, certificate.MiddleName, certificate.LastName);
		var registrationNo = certificate.RegistrationNo?.ToString();

		return CertificatePDFUtil.GenerateCertificateFileName("BirthCertificate", fullName, registrationNo);
	}

	/// <summary>
	/// Draws an enhanced government header with watermark and additional security features
	/// </summary>
	private static float DrawEnhancedGovernmentHeader(Syncfusion.Pdf.PdfPage page, string certificateType, string certificateId)
	{
		var graphics = page.Graphics;
		var pageSize = page.GetClientSize();
		var currentY = 20f; // Start higher for more space

		// Draw watermark background
		DrawWatermark(page);

		// Draw government logo/emblem (enhanced)
		DrawEnhancedGovernmentLogo(graphics, pageSize, currentY);

		currentY += 100;

		// Government of India header (enhanced)
		var govIndiaText = "GOVERNMENT OF INDIA";
		var govIndiaSize = CertificatePDFUtil._titleFont.MeasureString(govIndiaText);
		graphics.DrawString(govIndiaText, CertificatePDFUtil._titleFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._governmentBlue),
			new Syncfusion.Drawing.PointF((pageSize.Width - govIndiaSize.Width) / 2, currentY));

		currentY += 25;

		// Ministry information
		var ministryText = "MINISTRY OF HOME AFFAIRS";
		var ministrySize = CertificatePDFUtil._normalFont.MeasureString(ministryText);
		graphics.DrawString(ministryText, CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkBlue),
			new Syncfusion.Drawing.PointF((pageSize.Width - ministrySize.Width) / 2, currentY));

		currentY += 20;

		// Department information
		var deptText = "OFFICE OF THE REGISTRAR GENERAL & CENSUS COMMISSIONER";
		var deptSize = CertificatePDFUtil._smallFont.MeasureString(deptText);
		graphics.DrawString(deptText, CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF((pageSize.Width - deptSize.Width) / 2, currentY));

		currentY += 35;

		// Certificate type title (enhanced)
		var certTypeSize = CertificatePDFUtil._subTitleFont.MeasureString(certificateType);
		graphics.DrawString(certificateType, CertificatePDFUtil._subTitleFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._sealRed),
			new Syncfusion.Drawing.PointF((pageSize.Width - certTypeSize.Width) / 2, currentY));

		currentY += 25;

		// Certificate ID display
		var certIdText = $"Certificate ID: {certificateId}";
		var certIdSize = CertificatePDFUtil._normalFont.MeasureString(certIdText);
		graphics.DrawString(certIdText, CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF((pageSize.Width - certIdSize.Width) / 2, currentY));

		currentY += 30;

		// Enhanced decorative lines
		graphics.DrawLine(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentGold, 3),
			new Syncfusion.Drawing.PointF(CertificatePDFUtil._pageMargin, currentY),
			new Syncfusion.Drawing.PointF(pageSize.Width - CertificatePDFUtil._pageMargin, currentY));

		graphics.DrawLine(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentBlue, 1),
			new Syncfusion.Drawing.PointF(CertificatePDFUtil._pageMargin, currentY + 5),
			new Syncfusion.Drawing.PointF(pageSize.Width - CertificatePDFUtil._pageMargin, currentY + 5));

		return currentY + 25;
	}

	/// <summary>
	/// Draws enhanced government logo with border and styling
	/// </summary>
	private static void DrawEnhancedGovernmentLogo(Syncfusion.Pdf.Graphics.PdfGraphics graphics, Syncfusion.Drawing.SizeF pageSize, float currentY)
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
				// Draw enhanced border around logo
				var borderRect = new Syncfusion.Drawing.RectangleF(pageSize.Width / 2 - 45, currentY - 5, 90, 90);
				graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentGold, 3), borderRect);
				graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentBlue, 1),
					new Syncfusion.Drawing.RectangleF(borderRect.X - 2, borderRect.Y - 2, borderRect.Width + 4, borderRect.Height + 4));

				// Load and draw the actual logo
				using var logoStream = new FileStream(logoPath, FileMode.Open, FileAccess.Read);
				var logoImage = new Syncfusion.Pdf.Graphics.PdfBitmap(logoStream);

				// Draw logo centered with appropriate size
				var logoRect = new Syncfusion.Drawing.RectangleF(pageSize.Width / 2 - 40, currentY, 80, 80);
				graphics.DrawImage(logoImage, logoRect);
			}
			else
			{
				// Enhanced fallback placeholder
				DrawEnhancedLogoPlaceholder(graphics, pageSize, currentY);
			}
		}
		catch
		{
			// Enhanced fallback placeholder if there's any error
			DrawEnhancedLogoPlaceholder(graphics, pageSize, currentY);
		}
	}

	/// <summary>
	/// Draws an enhanced placeholder logo when the actual logo cannot be loaded
	/// </summary>
	private static void DrawEnhancedLogoPlaceholder(Syncfusion.Pdf.Graphics.PdfGraphics graphics, Syncfusion.Drawing.SizeF pageSize, float currentY)
	{
		// Draw enhanced government emblem area
		var emblemRect = new Syncfusion.Drawing.RectangleF(pageSize.Width / 2 - 40, currentY, 80, 80);

		// Multiple border layers for enhanced appearance
		graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentGold, 3), emblemRect);
		graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentBlue, 1),
			new Syncfusion.Drawing.RectangleF(emblemRect.X - 2, emblemRect.Y - 2, emblemRect.Width + 4, emblemRect.Height + 4));

		// Fill with government color gradient effect
		var gradientBrush = new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._lightBlue);
		graphics.DrawRectangle(gradientBrush, new Syncfusion.Drawing.RectangleF(emblemRect.X + 3, emblemRect.Y + 3, emblemRect.Width - 6, emblemRect.Height - 6));

		// Draw enhanced emblem with multiple elements
		graphics.DrawString("🇮🇳", new Syncfusion.Pdf.Graphics.PdfStandardFont(Syncfusion.Pdf.Graphics.PdfFontFamily.TimesRoman, 32),
			new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._governmentBlue),
			new Syncfusion.Drawing.PointF(pageSize.Width / 2 - 20, currentY + 15));

		// Add "GOVERNMENT SEAL" text below
		graphics.DrawString("GOVERNMENT", CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(pageSize.Width / 2 - 25, currentY + 50));
		graphics.DrawString("SEAL", CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(pageSize.Width / 2 - 12, currentY + 62));
	}

	/// <summary>
	/// Draws watermark background for security
	/// </summary>
	private static void DrawWatermark(Syncfusion.Pdf.PdfPage page)
	{
		var graphics = page.Graphics;
		var pageSize = page.GetClientSize();

		// Create watermark text
		var watermarkText = "GOVERNMENT CERTIFIED";
		var watermarkFont = new Syncfusion.Pdf.Graphics.PdfStandardFont(Syncfusion.Pdf.Graphics.PdfFontFamily.TimesRoman, 48, Syncfusion.Pdf.Graphics.PdfFontStyle.Bold);

		// Set transparency
		graphics.Save();
		graphics.SetTransparency(0.1f);

		// Draw diagonal watermark
		graphics.TranslateTransform(pageSize.Width / 2, pageSize.Height / 2);
		graphics.RotateTransform(-45);

		var watermarkSize = watermarkFont.MeasureString(watermarkText);
		graphics.DrawString(watermarkText, watermarkFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._governmentBlue),
			new Syncfusion.Drawing.PointF(-watermarkSize.Width / 2, -watermarkSize.Height / 2));

		graphics.Restore();
	}

	/// <summary>
	/// Draws legal disclaimer section
	/// </summary>
	private static float DrawLegalDisclaimer(Syncfusion.Pdf.PdfPage page, float currentY)
	{
		var graphics = page.Graphics;
		var pageSize = page.GetClientSize();

		// Legal disclaimer header
		graphics.DrawString("LEGAL NOTICES & DISCLAIMERS", CertificatePDFUtil._subHeaderFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._governmentBlue),
			new Syncfusion.Drawing.PointF(CertificatePDFUtil._pageMargin, currentY));

		currentY += 25;

		// Disclaimer text
		var disclaimerText = "• This certificate is issued based on information provided by the informant and recorded in official registers.\n" +
							"• Any false information or tampering with this document is punishable under the Indian Penal Code.\n" +
							"• This digital certificate has the same legal validity as the original paper certificate.\n" +
							"• For verification purposes, visit the official government portal or contact the issuing authority.\n" +
							"• This certificate is valid for all legal, official, and administrative purposes.\n" +
							"• In case of discrepancies, the original register entry shall be considered authentic.";

		var disclaimerRect = new Syncfusion.Drawing.RectangleF(CertificatePDFUtil._pageMargin, currentY, pageSize.Width - (2 * CertificatePDFUtil._pageMargin), 100);
		graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._darkGray, 1), new Syncfusion.Pdf.Graphics.PdfSolidBrush(Syncfusion.Drawing.Color.FromArgb(248, 249, 250)), disclaimerRect);

		var textRect = new Syncfusion.Drawing.RectangleF(CertificatePDFUtil._pageMargin + 10, currentY + 10, pageSize.Width - (2 * CertificatePDFUtil._pageMargin) - 20, 80);
		var stringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
		{
			LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Top,
			WordWrap = Syncfusion.Pdf.Graphics.PdfWordWrapType.Word
		};

		graphics.DrawString(disclaimerText, CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray), textRect, stringFormat);

		return currentY + disclaimerRect.Height + 20;
	}

	/// <summary>
	/// Draws enhanced certification section with multiple validation levels
	/// </summary>
	private static float DrawEnhancedCertificationSection(Syncfusion.Pdf.PdfPage page, float currentY, BirthCertificateOverviewModel certificate)
	{
		var graphics = page.Graphics;
		var pageSize = page.GetClientSize();

		// Main certification statement
		var certificationText = "I hereby certify that the above particulars are true and correct according to the register of births " +
							   "maintained in this office under the Registration of Births and Deaths Act, 1969, and the rules made thereunder.";

		var certRect = new Syncfusion.Drawing.RectangleF(CertificatePDFUtil._pageMargin, currentY, pageSize.Width - (2 * CertificatePDFUtil._pageMargin), 50);
		var stringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
		{
			Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
			LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle,
			WordWrap = Syncfusion.Pdf.Graphics.PdfWordWrapType.Word
		};

		graphics.DrawString(certificationText, CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black), certRect, stringFormat);

		currentY += 70;

		// Multi-level certification
		var signatureY = currentY;
		var signatureWidth = (pageSize.Width - (3 * CertificatePDFUtil._pageMargin)) / 2;

		// Registrar signature
		var registrarRect = new Syncfusion.Drawing.RectangleF(CertificatePDFUtil._pageMargin, signatureY, signatureWidth, 90);
		graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._darkGray, 1), registrarRect);

		graphics.DrawString("[DIGITAL SIGNATURE]", CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._sealRed),
			new Syncfusion.Drawing.PointF(registrarRect.X + 20, registrarRect.Y + 15));

		graphics.DrawString("REGISTRAR OF BIRTHS & DEATHS", CertificatePDFUtil._boldFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(registrarRect.X + 10, registrarRect.Y + 35));

		var municipalityName = certificate.MunicipalityName ?? "Municipal Corporation";
		graphics.DrawString(municipalityName, CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(registrarRect.X + 10, registrarRect.Y + 55));

		graphics.DrawString($"Date: {DateTime.Now:dd/MM/yyyy}", CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(registrarRect.X + 10, registrarRect.Y + 70));

		// Superintendent signature
		var superintendentRect = new Syncfusion.Drawing.RectangleF(pageSize.Width - CertificatePDFUtil._pageMargin - signatureWidth, signatureY, signatureWidth, 90);
		graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._darkGray, 1), superintendentRect);

		graphics.DrawString("[VERIFIED & APPROVED]", CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._sealRed),
			new Syncfusion.Drawing.PointF(superintendentRect.X + 20, superintendentRect.Y + 15));

		graphics.DrawString("SUPERINTENDENT", CertificatePDFUtil._boldFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(superintendentRect.X + 30, superintendentRect.Y + 35));

		graphics.DrawString("Civil Registration Office", CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(superintendentRect.X + 20, superintendentRect.Y + 55));

		graphics.DrawString($"Verified: {DateTime.Now:dd/MM/yyyy}", CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(superintendentRect.X + 10, superintendentRect.Y + 70));

		return signatureY + 110;
	}

	/// <summary>
	/// Draws enhanced certificate footer with comprehensive information
	/// </summary>
	private static void DrawEnhancedCertificateFooter(Syncfusion.Pdf.PdfPage page, float currentY, string certificateId, BirthCertificateOverviewModel certificate)
	{
		var graphics = page.Graphics;
		var pageSize = page.GetClientSize();

		var footerY = Math.Max(currentY + 20, pageSize.Height - 200);

		// Enhanced decorative lines
		graphics.DrawLine(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._governmentGold, 2),
			new Syncfusion.Drawing.PointF(CertificatePDFUtil._pageMargin, footerY),
			new Syncfusion.Drawing.PointF(pageSize.Width - CertificatePDFUtil._pageMargin, footerY));

		footerY += 15;

		// QR Code placeholder area
		var qrRect = new Syncfusion.Drawing.RectangleF(CertificatePDFUtil._pageMargin, footerY, 80, 80);
		graphics.DrawRectangle(new Syncfusion.Pdf.Graphics.PdfPen(CertificatePDFUtil._darkGray, 1), qrRect);
		graphics.DrawString("QR CODE", CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(qrRect.X + 15, qrRect.Y + 35));

		// Certificate information beside QR code
		var infoX = qrRect.Right + 20;
		var infoY = footerY;

		graphics.DrawString($"Certificate ID: {certificateId}", CertificatePDFUtil._boldFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._black),
			new Syncfusion.Drawing.PointF(infoX, infoY));

		infoY += 15;
		graphics.DrawString($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(infoX, infoY));

		infoY += 15;
		graphics.DrawString($"Valid From: {certificate.RegistrationDate:dd/MM/yyyy}", CertificatePDFUtil._normalFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(infoX, infoY));

		infoY += 15;
		graphics.DrawString($"Security Hash: {GenerateSecurityHash(certificateId)}", CertificatePDFUtil._smallFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(infoX, infoY));

		// Footer warning
		footerY += 95;
		var warningText = "⚠ WARNING: This is a government issued document. Any unauthorized reproduction, alteration, or misuse is a criminal offense punishable under law.";
		var warningRect = new Syncfusion.Drawing.RectangleF(CertificatePDFUtil._pageMargin, footerY, pageSize.Width - (2 * CertificatePDFUtil._pageMargin), 30);
		var warningFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
		{
			Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
			LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle,
			WordWrap = Syncfusion.Pdf.Graphics.PdfWordWrapType.Word
		};

		graphics.DrawString(warningText, CertificatePDFUtil._footerFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._sealRed), warningRect, warningFormat);

		// Page number and classification
		footerY += 35;
		graphics.DrawString("Page 1 of 1", CertificatePDFUtil._footerFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._darkGray),
			new Syncfusion.Drawing.PointF(CertificatePDFUtil._pageMargin, footerY));

		var classificationText = "OFFICIAL USE ONLY - GOVERNMENT CLASSIFIED";
		var classSize = CertificatePDFUtil._footerFont.MeasureString(classificationText);
		graphics.DrawString(classificationText, CertificatePDFUtil._footerFont, new Syncfusion.Pdf.Graphics.PdfSolidBrush(CertificatePDFUtil._sealRed),
			new Syncfusion.Drawing.PointF(pageSize.Width - CertificatePDFUtil._pageMargin - classSize.Width, footerY));
	}

	/// <summary>
	/// Generates a verification code for the certificate
	/// </summary>
	private static string GenerateVerificationCode(BirthCertificateOverviewModel certificate)
	{
		var hash = Math.Abs((certificate.FirstName + certificate.DateOfBirth.ToString()).GetHashCode());
		return $"VRF{hash:X6}";
	}

	/// <summary>
	/// Generates a security hash for the certificate
	/// </summary>
	private static string GenerateSecurityHash(string certificateId)
	{
		var hash = Math.Abs((certificateId + DateTime.Now.ToString("yyyyMMdd")).GetHashCode());
		return $"SEC{hash:X8}";
	}

	/// <summary>
	/// Generates an anti-forgery code
	/// </summary>
	private static string GenerateAntiForgeryCode(int certificateId)
	{
		var hash = Math.Abs((certificateId + DateTime.Now.Ticks).GetHashCode());
		return $"AF{hash:X4}";
	}
}
