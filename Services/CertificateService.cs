using Microsoft.EntityFrameworkCore;
using RajFabAPI.Data;
using RajFabAPI.DTOs;
using RajFabAPI.Models;
using RajFabAPI.Services.Interface;

public class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _db;

    public CertificateService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Certificate> GenerateCertificateAsync(GenerateCertificateDto dto, Guid issuerUserId)
    {
        // Get last version
        var lastVersion = await _db.Certificates
            .Where(c => c.RegistrationNumber == dto.RegistrationNumber)
            .MaxAsync(c => (int?)c.CertificateVersion) ?? 0;

        var version = lastVersion + 1;

        var certificate = new Certificate
        {
            RegistrationNumber = dto.RegistrationNumber,
            StartDate = dto.StartDate ?? DateTime.Now,
            EndDate = dto.EndDate ?? DateTime.Now.AddYears(1),
            IssuedByUserId = issuerUserId,
            IssuedAt = dto.IssuedAt ?? DateTime.Now,
            Status = dto.Status,
            CertificateVersion = version,
            ModuleId = dto.ModuleId,
            Remarks = dto.Remarks,
            CertificateUrl = dto.CertificateUrl
        };

        _db.Certificates.Add(certificate);
        await _db.SaveChangesAsync();

        return certificate;
    }

    public async Task<List<Certificate>> GetCertificatesByRegistrationNumberAsync(string registrationNumber)
    {
        return await _db.Certificates
            .Where(c => c.RegistrationNumber == registrationNumber)
            .OrderByDescending(c => c.CertificateVersion)
            .ToListAsync();
    }

    public async Task<Certificate?> GetLatestCertificateAsync(string registrationNumber)
    {
        return await _db.Certificates
            .Where(c => c.RegistrationNumber == registrationNumber)
            .OrderByDescending(c => c.CertificateVersion)
            .FirstOrDefaultAsync();
    }
}
