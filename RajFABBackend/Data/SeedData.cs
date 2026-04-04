using Microsoft.EntityFrameworkCore;
using RajFabAPI.Models;

namespace RajFabAPI.Data
{
    public static class SeedData
    {
        public static async Task SeedFactoryTypesAsync(ApplicationDbContext context)
        {
            // Only seed if no factory types exist
            if (await context.FactoryTypes_Old.AnyAsync())
                return;

            var factoryTypes = new List<FactoryTypeOld>
            {
                // Non Hazardous Factories (workers up to 50) 
                new FactoryTypeOld
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Non Hazardous Factories (workers up to 50)",
                    Description = "Factory with non-hazardous processes employing up to 50 workers",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },

                // Non Hazardous Factories (workers more than 50)
                new FactoryTypeOld
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Non Hazardous Factories (workers more than 50)",
                    Description = "Factory with non-hazardous processes employing more than 50 workers",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },

                // Factories Carrying out Hazardous Process
                new FactoryTypeOld
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Factories Carrying out Hazardous Process",
                    Description = "Factory carrying out hazardous manufacturing processes",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },

                // Factories Carrying out Dangerous Operations
                new FactoryTypeOld
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Factories Carrying out Dangerous Operations",
                    Description = "Factory carrying out dangerous manufacturing operations",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },

                // Factories (MAH) Covered Under RCIMAH Rules, 1991
                new FactoryTypeOld
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Factories (MAH) Covered Under RCIMAH Rules, 1991",
                    Description = "Major Accident Hazard factories covered under RCIMAH Rules",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.FactoryTypes_Old.AddRange(factoryTypes);
            await context.SaveChangesAsync();

            // Now seed document types for Factory module
            await SeedFactoryDocumentTypesAsync(context, factoryTypes);
        }

        private static async Task SeedFactoryDocumentTypesAsync(ApplicationDbContext context, List<FactoryTypeOld> factoryTypes)
        {
            // Check if factory document types already exist
            if (await context.DocumentTypes.AnyAsync(dt => dt.Module == "Factory"))
                return;

            var documentTypes = new List<DocumentType>
            {
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Form No.1 - Occupier/Factory Manager Declaration",
                    Description = "Form No.1 signed by the occupier/Factory Manager",
                    FileTypes = ".pdf,.doc,.docx",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Form No.1A - Affidavit",
                    Description = "Affidavit on Non Judicial Stamp Paper of Rs.10/- signed by the occupier",
                    FileTypes = ".pdf,.doc,.docx",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Land Ownership Documents",
                    Description = "Lease deed/rent deed/sale deed (In case of non RIICO land, also submit land use conversion order)",
                    FileTypes = ".pdf,.doc,.docx,.jpg,.jpeg,.png",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Approved Land Plan",
                    Description = "RIICO site plan/khasara plan",
                    FileTypes = ".pdf,.dwg,.jpg,.jpeg,.png",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Manufacturing Process Description",
                    Description = "Brief description of the Manufacturing Process in its Various Stages",
                    FileTypes = ".pdf,.doc,.docx,.txt",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "List of Hazardous Processes",
                    Description = "List of Hazardous Processes as per the First Schedule as defined in Section 2(cb)",
                    FileTypes = ".pdf,.doc,.docx,.txt",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new DocumentType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Plan/Maps",
                    Description = "Plan/MAPS drawn to scale as per rule 3A and duly signed",
                    FileTypes = ".pdf,.dwg,.jpg,.jpeg,.png",
                    MaxSizeMB = 25,
                    Module = "Factory",
                    ServiceType = "Registration",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            context.DocumentTypes.AddRange(documentTypes);
            await context.SaveChangesAsync();

            // Create associations between factory types and document types
            await CreateFactoryDocumentAssociationsAsync(context, factoryTypes, documentTypes);
        }

        private static async Task CreateFactoryDocumentAssociationsAsync(
            ApplicationDbContext context, 
            List<FactoryTypeOld> factoryTypes, 
            List<DocumentType> documentTypes)
        {
            var associations = new List<FactoryTypeDocument>();

            // Get document types by name for easier reference
            var formNo1 = documentTypes.First(dt => dt.Name.Contains("Form No.1 - Occupier"));
            var affidavit = documentTypes.First(dt => dt.Name.Contains("Form No.1A"));
            var landOwnership = documentTypes.First(dt => dt.Name.Contains("Land Ownership"));
            var landPlan = documentTypes.First(dt => dt.Name.Contains("Approved Land Plan"));
            var processDescription = documentTypes.First(dt => dt.Name.Contains("Manufacturing Process Description"));
            var hazardousProcesses = documentTypes.First(dt => dt.Name.Contains("List of Hazardous Processes"));
            var planMaps = documentTypes.First(dt => dt.Name.Contains("Plan/Maps"));

            foreach (var factoryType in factoryTypes)
            {
                // All factory types require these basic documents
                associations.AddRange(new[]
                {
                    new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = formNo1.Id,
                        IsRequired = true,
                        Order = 1
                    },
                    new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = affidavit.Id,
                        IsRequired = true,
                        Order = 2
                    },
                    new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = landOwnership.Id,
                        IsRequired = true,
                        Order = 3
                    },
                    new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = landPlan.Id,
                        IsRequired = true,
                        Order = 4
                    },
                    new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = processDescription.Id,
                        IsRequired = true,
                        Order = 5
                    }
                });

                // Additional documents for hazardous and dangerous factory types
                if (factoryType.Name.Contains("Hazardous") || factoryType.Name.Contains("Dangerous") || factoryType.Name.Contains("MAH"))
                {
                    associations.Add(new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = hazardousProcesses.Id,
                        IsRequired = true,
                        Order = 6
                    });
                }

                // Plan/Maps for dangerous operations and MAH factories
                if (factoryType.Name.Contains("Dangerous") || factoryType.Name.Contains("MAH"))
                {
                    associations.Add(new FactoryTypeDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        FactoryTypeId = factoryType.Id,
                        DocumentTypeId = planMaps.Id,
                        IsRequired = true,
                        Order = 7
                    });
                }
            }

            context.FactoryTypeDocuments.AddRange(associations);
            await context.SaveChangesAsync();
        }
    }
}