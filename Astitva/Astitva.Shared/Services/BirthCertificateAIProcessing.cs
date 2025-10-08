using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models;

namespace Astitva.Shared.Services;

internal static class BirthCertificateAIProcessing
{
	private static async Task<string> GetAIResponse(string prompt)
	{
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("https://api.aimlapi.com/v1/");
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Secrets.OpenAPI);

		var requestBody = JsonSerializer.Serialize(new
		{
			model = "google/gemma-3n-e4b-it",
			messages = new[]
			{
				new
				{
					role = "user",
					content = prompt
				}
			},
			temperature = 0.7,
			top_p = 0.7,
			frequency_penalty = 1,
			max_output_tokens = 512,
			top_k = 50
		});

		var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
		var response = await httpClient.PostAsync("chat/completions", content);
		response.EnsureSuccessStatusCode();

		if (!response.IsSuccessStatusCode)
			throw new HttpRequestException($"AI API request failed with status code {response.StatusCode}");

		string responseContent = await response.Content.ReadAsStringAsync();

		using JsonDocument outputAsJson = JsonDocument.Parse(responseContent);
		string messageContent = outputAsJson.RootElement
			.GetProperty("choices")[0]
			.GetProperty("message")
			.GetProperty("content")
			.GetString() ?? string.Empty;

		int jsonStart = messageContent.IndexOf('{');
		int jsonEnd = messageContent.LastIndexOf('}') + 1;

		if (jsonStart >= 0 && jsonEnd > jsonStart)
		{
			string jsonContent = messageContent[jsonStart..jsonEnd];
			return jsonContent;
		}

		throw new InvalidOperationException("AI response did not contain valid JSON");
	}

	internal static async Task<BirthCertificateResponseModel> ExtractBirthCertificateDetailsFromTranscript(string transcript)
	{
		if (string.IsNullOrEmpty(transcript))
			return new BirthCertificateResponseModel();

		BirthCertificateResponseModel extractedInfo = new();

		try
		{
			string prompt = @"Extract the following information from the provided text for a birth certificate form: 
1. First name
2. Middle name (optional)
3. Last name (optional)
4. Sex/Gender (Male, Female, or Other)
5. Date of birth (in dd/MM/yyyy format)
6. Birth place (optional)
7. Father's name (optional)
8. Mother's name (optional)
9. Address/residence (optional)
10. Registration number (optional, numeric)

Return ONLY a JSON object with these keys: 'firstName', 'middleName', 'lastName', 'sex', 'dateOfBirth', 'birthPlace', 'fatherName', 'motherName', 'address', 'registrationNo'.
If information is not provided, use empty string for text fields and null for numeric fields.
Text: " + transcript;

			string aiJSONResponse = await GetAIResponse(prompt);

			extractedInfo = JsonSerializer.Deserialize<BirthCertificateResponseModel>(aiJSONResponse);

			if (extractedInfo is null)
				return new BirthCertificateResponseModel();

			// Validate and clean the extracted data
			extractedInfo.firstName = CleanTextInput(extractedInfo.firstName);
			extractedInfo.middleName = CleanTextInput(extractedInfo.middleName);
			extractedInfo.lastName = CleanTextInput(extractedInfo.lastName);
			extractedInfo.fatherName = CleanTextInput(extractedInfo.fatherName);
			extractedInfo.motherName = CleanTextInput(extractedInfo.motherName);
			extractedInfo.birthPlace = CleanTextInput(extractedInfo.birthPlace);
			extractedInfo.address = CleanTextInput(extractedInfo.address);

			// Validate sex field
			if (!string.IsNullOrEmpty(extractedInfo.sex))
			{
				extractedInfo.sex = extractedInfo.sex.ToLowerInvariant() switch
				{
					"male" or "m" or "boy" => "Male",
					"female" or "f" or "girl" => "Female",
					"other" or "o" => "Other",
					_ => ""
				};
			}

			// Validate and parse date of birth
			if (!string.IsNullOrEmpty(extractedInfo.dateOfBirth))
			{
				if (DateTime.TryParse(extractedInfo.dateOfBirth, out DateTime parsedDate))
				{
					if (parsedDate > DateTime.Now)
						extractedInfo.dateOfBirth = "";
					else if (parsedDate < DateTime.Now.AddYears(-150))
						extractedInfo.dateOfBirth = "";
					else
						extractedInfo.dateOfBirth = parsedDate.ToString("dd/MM/yyyy");
				}
				else
				{
					extractedInfo.dateOfBirth = "";
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error processing transcript with AI: {ex.Message}");
		}

		return extractedInfo;
	}

	private static string CleanTextInput(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return "";

		return input.Trim();
	}
}

public class BirthCertificateResponseModel
{
	public string firstName { get; set; } = "";
	public string middleName { get; set; } = "";
	public string lastName { get; set; } = "";
	public string sex { get; set; } = "";
	public string dateOfBirth { get; set; } = "";
	public string birthPlace { get; set; } = "";
	public string fatherName { get; set; } = "";
	public string motherName { get; set; } = "";
	public string address { get; set; } = "";
	public int? registrationNo { get; set; }
}