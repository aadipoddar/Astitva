using AstitvaLibrary.Services;
using AstitvaLibrary.Models;
using Microsoft.AspNetCore.Components;

namespace Astitva.Shared.Pages;

public partial class Verification
{
    private string certificateHash = string.Empty;
    private bool isVerifying = false;
    private CertificateVerificationResult? verificationResult;
    private BlockchainStats? blockchainStats;

    protected override async Task OnInitializedAsync()
    {
        await LoadBlockchainStats();
    }

    private async Task VerifyCertificate()
    {
        if (string.IsNullOrWhiteSpace(certificateHash))
            return;

        isVerifying = true;
        verificationResult = null;
        StateHasChanged();

        try
        {
            verificationResult = await BlockchainService.VerifyCertificateByHashAsync(certificateHash.Trim());
            await LoadBlockchainStats(); // Refresh stats
        }
        catch (Exception ex)
        {
            verificationResult = new CertificateVerificationResult(
                false, 
                false, 
                $"Verification error: {ex.Message}"
            );
        }
        finally
        {
            isVerifying = false;
            StateHasChanged();
        }
    }

    private async Task LoadBlockchainStats()
    {
        try
        {
            blockchainStats = await BlockchainService.GetBlockchainStatsAsync();
        }
        catch (Exception)
        {
            // Handle error silently or show a message
            blockchainStats = null;
        }
    }
}