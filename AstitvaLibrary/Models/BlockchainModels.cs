using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AstitvaLibrary.Models;

/// <summary>
/// Represents a simple block in the blockchain containing certificate data
/// </summary>
public class CertificateBlock
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public string CertificateHash { get; set; }
    public string CertificateType { get; set; } // "Birth" or "Death"
    public int CertificateId { get; set; }
    public string PreviousHash { get; set; }
    public string Hash { get; set; }

    public CertificateBlock(int index, string certificateHash, string certificateType, int certificateId, string previousHash)
    {
        Index = index;
        Timestamp = DateTime.UtcNow;
        CertificateHash = certificateHash;
        CertificateType = certificateType;
        CertificateId = certificateId;
        PreviousHash = previousHash;
        Hash = CalculateHash();
    }

    /// <summary>
    /// Calculates the hash of this block using SHA256
    /// </summary>
    public string CalculateHash()
    {
        var blockData = $"{Index}{Timestamp:O}{CertificateHash}{CertificateType}{CertificateId}{PreviousHash}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// Simple blockchain to store certificate hashes
/// </summary>
public class CertificateBlockchain
{
    public List<CertificateBlock> Chain { get; set; }

    public CertificateBlockchain()
    {
        Chain = new List<CertificateBlock>();
        CreateGenesisBlock();
    }

    /// <summary>
    /// Creates the first block in the blockchain
    /// </summary>
    private void CreateGenesisBlock()
    {
        var genesisBlock = new CertificateBlock(0, "GENESIS", "Genesis", 0, "0");
        Chain.Add(genesisBlock);
    }

    /// <summary>
    /// Gets the latest block in the chain
    /// </summary>
    public CertificateBlock GetLatestBlock()
    {
        return Chain[^1];
    }

    /// <summary>
    /// Adds a new certificate to the blockchain
    /// </summary>
    public void AddCertificate(string certificateHash, string certificateType, int certificateId)
    {
        var previousBlock = GetLatestBlock();
        var newBlock = new CertificateBlock(
            Chain.Count,
            certificateHash,
            certificateType,
            certificateId,
            previousBlock.Hash
        );
        Chain.Add(newBlock);
    }

    /// <summary>
    /// Validates the integrity of the blockchain
    /// </summary>
    public bool IsChainValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            var currentBlock = Chain[i];
            var previousBlock = Chain[i - 1];

            // Check if current block's hash is correct
            if (currentBlock.Hash != currentBlock.CalculateHash())
                return false;

            // Check if current block points to previous block
            if (currentBlock.PreviousHash != previousBlock.Hash)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Finds a certificate in the blockchain by its hash
    /// </summary>
    public CertificateBlock? FindCertificate(string certificateHash)
    {
        return Chain.FirstOrDefault(block => block.CertificateHash == certificateHash);
    }

    /// <summary>
    /// Gets all certificates of a specific type
    /// </summary>
    public List<CertificateBlock> GetCertificatesByType(string certificateType)
    {
        return Chain.Where(block => block.CertificateType == certificateType).ToList();
    }
}

/// <summary>
/// Model for certificate verification results
/// </summary>
public class CertificateVerificationResult
{
    public bool IsValid { get; set; }
    public bool ExistsOnBlockchain { get; set; }
    public DateTime? BlockchainTimestamp { get; set; }
    public string Message { get; set; }
    public CertificateBlock? Block { get; set; }

    public CertificateVerificationResult(bool isValid, bool existsOnBlockchain, string message, CertificateBlock? block = null)
    {
        IsValid = isValid;
        ExistsOnBlockchain = existsOnBlockchain;
        Message = message;
        Block = block;
        BlockchainTimestamp = block?.Timestamp;
    }
}