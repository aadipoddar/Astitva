using AstitvaLibrary.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AstitvaLibrary.Services;

/// <summary>
/// Simple blockchain service for certificate management
/// No external APIs needed - everything runs locally
/// </summary>
public static class BlockchainService
{
    private static CertificateBlockchain? _blockchain;
    private static readonly string BlockchainFileName = "certificate_blockchain.json";

    /// <summary>
    /// Initialize or load the blockchain
    /// </summary>
    public static async Task<CertificateBlockchain> GetBlockchainAsync()
    {
        if (_blockchain != null)
            return _blockchain;

        try
        {
            // Try to load existing blockchain from file
            if (File.Exists(BlockchainFileName))
            {
                var json = await File.ReadAllTextAsync(BlockchainFileName);
                var loadedBlockchain = JsonSerializer.Deserialize<CertificateBlockchain>(json);
                
                // Validate the loaded blockchain
                if (loadedBlockchain != null && loadedBlockchain.IsChainValid())
                {
                    _blockchain = loadedBlockchain;
                    return _blockchain;
                }
            }
        }
        catch (Exception)
        {
            // If loading fails, create new blockchain
        }

        // Create new blockchain if loading failed or file doesn't exist
        _blockchain = new CertificateBlockchain();
        await SaveBlockchainAsync();
        return _blockchain;
    }

    /// <summary>
    /// Save blockchain to local file
    /// </summary>
    private static async Task SaveBlockchainAsync()
    {
        if (_blockchain == null) return;

        try
        {
            var json = JsonSerializer.Serialize(_blockchain, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(BlockchainFileName, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save blockchain: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate a unique hash for a birth certificate
    /// </summary>
    public static string GenerateBirthCertificateHash(BirthCertificateModel certificate)
    {
        var certificateData = $"{certificate.FirstName}{certificate.MiddleName}{certificate.LastName}" +
                            $"{certificate.DateOfBirth:yyyy-MM-dd}{certificate.Sex}{certificate.FatherName}" +
                            $"{certificate.MotherName}{certificate.RegistrationNo}{certificate.RegistrationDate:yyyy-MM-dd}" +
                            $"{certificate.BirthPlace}{certificate.MunicipalityId}";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(certificateData));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Generate a unique hash for a death certificate
    /// </summary>
    public static string GenerateDeathCertificateHash(DeathCertificateModel certificate)
    {
        var certificateData = $"{certificate.FirstName}{certificate.MiddleName}{certificate.LastName}" +
                            $"{certificate.DateOfDeath:yyyy-MM-dd}{certificate.Sex}{certificate.FatherName}" +
                            $"{certificate.MotherName}{certificate.RegistrationNo}{certificate.RegistrationDate:yyyy-MM-dd}" +
                            $"{certificate.DeathPlace}{certificate.MunicipalityId}";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(certificateData));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Add a birth certificate to the blockchain
    /// </summary>
    public static async Task<string> AddBirthCertificateToBlockchainAsync(BirthCertificateModel certificate)
    {
        var blockchain = await GetBlockchainAsync();
        var certificateHash = GenerateBirthCertificateHash(certificate);

        // Check if certificate already exists
        var existingBlock = blockchain.FindCertificate(certificateHash);
        if (existingBlock != null)
        {
            return certificateHash; // Already exists, return existing hash
        }

        // Add to blockchain
        blockchain.AddCertificate(certificateHash, "Birth", certificate.Id);
        await SaveBlockchainAsync();

        return certificateHash;
    }

    /// <summary>
    /// Add a death certificate to the blockchain
    /// </summary>
    public static async Task<string> AddDeathCertificateToBlockchainAsync(DeathCertificateModel certificate)
    {
        var blockchain = await GetBlockchainAsync();
        var certificateHash = GenerateDeathCertificateHash(certificate);

        // Check if certificate already exists
        var existingBlock = blockchain.FindCertificate(certificateHash);
        if (existingBlock != null)
        {
            return certificateHash; // Already exists, return existing hash
        }

        // Add to blockchain
        blockchain.AddCertificate(certificateHash, "Death", certificate.Id);
        await SaveBlockchainAsync();

        return certificateHash;
    }

    /// <summary>
    /// Verify a birth certificate against the blockchain
    /// </summary>
    public static async Task<CertificateVerificationResult> VerifyBirthCertificateAsync(BirthCertificateModel certificate)
    {
        var blockchain = await GetBlockchainAsync();
        var certificateHash = GenerateBirthCertificateHash(certificate);

        // Check if blockchain is valid
        if (!blockchain.IsChainValid())
        {
            return new CertificateVerificationResult(false, false, "Blockchain integrity compromised!");
        }

        // Find certificate in blockchain
        var block = blockchain.FindCertificate(certificateHash);
        if (block == null)
        {
            return new CertificateVerificationResult(false, false, "Certificate not found on blockchain - may be fraudulent!");
        }

        return new CertificateVerificationResult(true, true, "Certificate is authentic and verified on blockchain!", block);
    }

    /// <summary>
    /// Verify a death certificate against the blockchain
    /// </summary>
    public static async Task<CertificateVerificationResult> VerifyDeathCertificateAsync(DeathCertificateModel certificate)
    {
        var blockchain = await GetBlockchainAsync();
        var certificateHash = GenerateDeathCertificateHash(certificate);

        // Check if blockchain is valid
        if (!blockchain.IsChainValid())
        {
            return new CertificateVerificationResult(false, false, "Blockchain integrity compromised!");
        }

        // Find certificate in blockchain
        var block = blockchain.FindCertificate(certificateHash);
        if (block == null)
        {
            return new CertificateVerificationResult(false, false, "Certificate not found on blockchain - may be fraudulent!");
        }

        return new CertificateVerificationResult(true, true, "Certificate is authentic and verified on blockchain!", block);
    }

    /// <summary>
    /// Get blockchain statistics
    /// </summary>
    public static async Task<BlockchainStats> GetBlockchainStatsAsync()
    {
        var blockchain = await GetBlockchainAsync();
        var birthCertificates = blockchain.GetCertificatesByType("Birth").Count;
        var deathCertificates = blockchain.GetCertificatesByType("Death").Count;

        return new BlockchainStats
        {
            TotalBlocks = blockchain.Chain.Count,
            BirthCertificates = birthCertificates,
            DeathCertificates = deathCertificates,
            IsValid = blockchain.IsChainValid(),
            LastBlockTimestamp = blockchain.GetLatestBlock().Timestamp
        };
    }

    /// <summary>
    /// Verify a certificate by its hash
    /// </summary>
    public static async Task<CertificateVerificationResult> VerifyCertificateByHashAsync(string certificateHash)
    {
        var blockchain = await GetBlockchainAsync();

        // Check if blockchain is valid
        if (!blockchain.IsChainValid())
        {
            return new CertificateVerificationResult(false, false, "Blockchain integrity compromised!");
        }

        // Find certificate in blockchain
        var block = blockchain.FindCertificate(certificateHash);
        if (block == null)
        {
            return new CertificateVerificationResult(false, false, "Certificate hash not found on blockchain!");
        }

        return new CertificateVerificationResult(true, true, $"Certificate is authentic! Registered on {block.Timestamp:yyyy-MM-dd HH:mm:ss} UTC", block);
    }
}

/// <summary>
/// Blockchain statistics model
/// </summary>
public class BlockchainStats
{
    public int TotalBlocks { get; set; }
    public int BirthCertificates { get; set; }
    public int DeathCertificates { get; set; }
    public bool IsValid { get; set; }
    public DateTime LastBlockTimestamp { get; set; }
}