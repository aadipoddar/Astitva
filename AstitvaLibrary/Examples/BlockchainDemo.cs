using AstitvaLibrary.Models;
using AstitvaLibrary.Services;

namespace AstitvaLibrary.Examples;

/// <summary>
/// Simple examples showing how the blockchain integration works
/// This demonstrates the key features without external APIs
/// </summary>
public static class BlockchainDemo
{
    /// <summary>
    /// Example: Create a birth certificate and add it to blockchain
    /// </summary>
    public static async Task<string> CreateAndRegisterBirthCertificate()
    {
        // 1. Create a sample birth certificate
        var birthCertificate = new BirthCertificateModel
        {
            Id = 1,
            FirstName = "John",
            MiddleName = "William",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 5, 15),
            Sex = "Male",
            FatherName = "Robert Doe",
            MotherName = "Mary Doe",
            RegistrationNo = 12345,
            RegistrationDate = new DateOnly(1990, 6, 1),
            BirthPlace = "City Hospital",
            Address = "123 Main Street",
            MunicipalityId = 1,
            UserId = 1,
            Approved = true,
            Status = true
        };

        // 2. Add to blockchain (this happens automatically when certificate is saved)
        var certificateHash = await BlockchainService.AddBirthCertificateToBlockchainAsync(birthCertificate);

        Console.WriteLine($"✅ Birth Certificate added to blockchain!");
        Console.WriteLine($"🔑 Certificate Hash: {certificateHash}");
        Console.WriteLine($"📅 Registration Date: {birthCertificate.RegistrationDate}");

        return certificateHash;
    }

    /// <summary>
    /// Example: Verify a certificate using its hash
    /// </summary>
    public static async Task<bool> VerifyCertificateExample(string certificateHash)
    {
        Console.WriteLine($"\n🔍 Verifying certificate with hash: {certificateHash}");

        // Verify the certificate
        var result = await BlockchainService.VerifyCertificateByHashAsync(certificateHash);

        Console.WriteLine($"📋 Verification Result: {result.Message}");

        if (result.Block != null)
        {
            Console.WriteLine($"⛓️  Block Index: {result.Block.Index}");
            Console.WriteLine($"📅 Timestamp: {result.Block.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"📝 Certificate Type: {result.Block.CertificateType}");
        }

        return result.IsValid;
    }

    /// <summary>
    /// Example: Show blockchain statistics
    /// </summary>
    public static async Task ShowBlockchainStats()
    {
        var stats = await BlockchainService.GetBlockchainStatsAsync();

        Console.WriteLine("\n📊 Blockchain Statistics:");
        Console.WriteLine($"   Total Blocks: {stats.TotalBlocks}");
        Console.WriteLine($"   Birth Certificates: {stats.BirthCertificates}");
        Console.WriteLine($"   Death Certificates: {stats.DeathCertificates}");
        Console.WriteLine($"   Blockchain Valid: {(stats.IsValid ? "✅ Yes" : "❌ No")}");
        Console.WriteLine($"   Last Block: {stats.LastBlockTimestamp:yyyy-MM-dd HH:mm:ss} UTC");
    }

    /// <summary>
    /// Complete demo showing the full blockchain workflow
    /// </summary>
    public static async Task RunCompleteDemo()
    {
        Console.WriteLine("🔗 Astitva Blockchain Demo");
        Console.WriteLine("=" + new string('=', 50));

        try
        {
            // 1. Show initial blockchain state
            Console.WriteLine("\n1️⃣  Initial Blockchain State:");
            await ShowBlockchainStats();

            // 2. Create and register a birth certificate
            Console.WriteLine("\n2️⃣  Creating Birth Certificate:");
            var birthHash = await CreateAndRegisterBirthCertificate();

            // 3. Create and register a death certificate
            Console.WriteLine("\n3️⃣  Creating Death Certificate:");
            var deathHash = await CreateAndRegisterDeathCertificate();

            // 4. Show updated blockchain state
            Console.WriteLine("\n4️⃣  Updated Blockchain State:");
            await ShowBlockchainStats();

            // 5. Verify both certificates
            Console.WriteLine("\n5️⃣  Certificate Verification:");
            await VerifyCertificateExample(birthHash);
            await VerifyCertificateExample(deathHash);

            // 6. Try to verify an invalid hash
            Console.WriteLine("\n6️⃣  Testing Invalid Certificate:");
            await VerifyCertificateExample("INVALID_HASH_123");

            Console.WriteLine("\n✅ Demo completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Demo failed: {ex.Message}");
        }
    }

    private static async Task<string> CreateAndRegisterDeathCertificate()
    {
        var deathCertificate = new DeathCertificateModel
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfDeath = new DateOnly(2023, 3, 20),
            Sex = "Female",
            FatherName = "William Smith",
            MotherName = "Elizabeth Smith",
            RegistrationNo = 67890,
            RegistrationDate = new DateOnly(2023, 3, 25),
            DeathPlace = "Memorial Hospital",
            Address = "456 Oak Avenue",
            MunicipalityId = 2,
            UserId = 2,
            Approved = true,
            Status = true
        };

        var certificateHash = await BlockchainService.AddDeathCertificateToBlockchainAsync(deathCertificate);

        Console.WriteLine($"✅ Death Certificate added to blockchain!");
        Console.WriteLine($"🔑 Certificate Hash: {certificateHash}");
        Console.WriteLine($"📅 Registration Date: {deathCertificate.RegistrationDate}");

        return certificateHash;
    }
}