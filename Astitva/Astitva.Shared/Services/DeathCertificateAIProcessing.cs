using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models;

namespace Astitva.Shared.Services;

internal static class DeathCertificateAIProcessing
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

	internal static async Task<DeathCertificateResponseModel> ExtractDeathCertificateDetailsFromTranscript(string transcript)
	{
		if (string.IsNullOrEmpty(transcript))
			return new DeathCertificateResponseModel();

		DeathCertificateResponseModel extractedInfo = new();

		try
		{
			string prompt = @"Extract the following information from the provided text for a death certificate form: 
1. First name of the deceased
2. Middle name of the deceased (optional)
3. Last name of the deceased (optional)
4. Sex/Gender of the deceased (Male, Female, or Other)
5. Date of death (in dd/MM/yyyy format)
6. Place of death (optional)
7. Father's name of the deceased (optional)
8. Mother's name of the deceased (optional)
9. Address/residence of the deceased (optional)
10. Cause of death (optional)
11. Age at death (optional, numeric)

Return ONLY a JSON object with these keys: 'firstName', 'middleName', 'lastName', 'sex', 'dateOfDeath', 'deathPlace', 'fatherName', 'motherName', 'address', 'causeOfDeath', 'age'.
If information is not provided, use empty string for text fields and null for numeric fields.
Text: " + transcript;

			string aiJSONResponse = await GetAIResponse(prompt);

			extractedInfo = JsonSerializer.Deserialize<DeathCertificateResponseModel>(aiJSONResponse);

			if (extractedInfo is null)
				return new DeathCertificateResponseModel();

			// Validate and clean the extracted data
			extractedInfo.firstName = CleanTextInput(extractedInfo.firstName);
			extractedInfo.middleName = CleanTextInput(extractedInfo.middleName);
			extractedInfo.lastName = CleanTextInput(extractedInfo.lastName);
			extractedInfo.fatherName = CleanTextInput(extractedInfo.fatherName);
			extractedInfo.motherName = CleanTextInput(extractedInfo.motherName);
			extractedInfo.deathPlace = CleanTextInput(extractedInfo.deathPlace);
			extractedInfo.address = CleanTextInput(extractedInfo.address);
			extractedInfo.causeOfDeath = CleanTextInput(extractedInfo.causeOfDeath);

			// Validate sex field
			if (!string.IsNullOrEmpty(extractedInfo.sex))
			{
				extractedInfo.sex = extractedInfo.sex.ToLowerInvariant() switch
				{
					"male" or "m" or "man" => "Male",
					"female" or "f" or "woman" => "Female",
					"other" or "o" => "Other",
					_ => ""
				};
			}

			// Validate and parse date of death
			if (!string.IsNullOrEmpty(extractedInfo.dateOfDeath))
			{
				if (DateTime.TryParse(extractedInfo.dateOfDeath, out DateTime parsedDate))
				{
					if (parsedDate > DateTime.Now)
						extractedInfo.dateOfDeath = "";
					else if (parsedDate < DateTime.Now.AddYears(-150))
						extractedInfo.dateOfDeath = "";
					else
						extractedInfo.dateOfDeath = parsedDate.ToString("dd/MM/yyyy");
				}
				else
				{
					extractedInfo.dateOfDeath = "";
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

public class DeathCertificateResponseModel
{
	public string firstName { get; set; } = "";
	public string middleName { get; set; } = "";
	public string lastName { get; set; } = "";
	public string sex { get; set; } = "";
	public string dateOfDeath { get; set; } = "";
	public string deathPlace { get; set; } = "";
	public string fatherName { get; set; } = "";
	public string motherName { get; set; } = "";
	public string address { get; set; } = "";
	public string causeOfDeath { get; set; } = "";
	public int? age { get; set; }
}