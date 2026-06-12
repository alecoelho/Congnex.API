using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Infrastructure.Persistence;

public static class DbSeeder
{
    // Estrutura oficial das 10 unidades do Inglixy
    private static readonly (string Title, string Description, int Order)[] OfficialUnits =
    [
        ("Introductions",          "Aprenda a se apresentar",                          1),
        ("Family & Friends",       "Fale sobre sua família e seus amigos",             2),
        ("School & Daily Life",    "Converse sobre escola e rotina diária",            3),
        ("Food & Drinks",          "Peça comida e fale sobre refeições",               4),
        ("Shopping",               "Faça compras e pergunte preços",                   5),
        ("Transportation",         "Use ônibus, táxi e peça direções",                 6),
        ("Airport & Travel",       "Viaje com confiança em aeroportos",                7),
        ("Hotel & Accommodation",  "Faça check-in e converse em hotéis",              8),
        ("Work & Jobs",            "Fale sobre profissão e ambiente de trabalho",      9),
        ("Health & Emergencies",   "Peça ajuda em situações de saúde e emergência",   10),
    ];

    public static async Task SeedAsync(CongnexDbContext db)
    {
        // Upsert: cria ou atualiza as 10 unidades oficiais
        foreach (var (title, desc, order) in OfficialUnits)
        {
            var existing = await db.Units
                .FirstOrDefaultAsync(u => u.OrderIndex == order && u.LanguageCode == "en");

            if (existing is null)
            {
                db.Units.Add(new Unit
                {
                    LanguageCode = "en",
                    OrderIndex   = order,
                    Title        = title,
                    Description  = desc
                });
            }
            else
            {
                // Atualiza título e descrição se mudarem
                existing.Title       = title;
                existing.Description = desc;
            }
        }

        await db.SaveChangesAsync();
    }
}
