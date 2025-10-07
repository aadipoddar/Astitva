namespace AstitvaLibrary.Models;

public class BirthCertificateModel
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string? MiddleName { get; set; }
	public string? LastName { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string Sex { get; set; }
	public string? FatherName { get; set; }
	public string? MotherName { get; set; }
	public int? RegistrationNo { get; set; }
	public DateOnly RegistrationDate { get; set; }
	public string? BirthPlace { get; set; }
	public string? Address { get; set; }
	public int MunicipalityId { get; set; }
	public int UserId { get; set; }
	public bool Approved { get; set; }
	public bool Status { get; set; }
}

public class BirthCertificateOverviewModel
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string? MiddleName { get; set; }
	public string? LastName { get; set; }
	public DateOnly DateOfBirth { get; set; }
	public string Sex { get; set; }
	public string? FatherName { get; set; }
	public string? MotherName { get; set; }
	public int? RegistrationNo { get; set; }
	public DateOnly RegistrationDate { get; set; }
	public string? BirthPlace { get; set; }
	public string? Address { get; set; }
	public int MunicipalityId { get; set; }
	public string MunicipalityName { get; set; }
	public string MunicipalityCityName { get; set; }
	public int UserId { get; set; }
	public bool Approved { get; set; }
	public bool Status { get; set; }
}