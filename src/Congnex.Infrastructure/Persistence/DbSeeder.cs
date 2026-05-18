using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(CongnexDbContext db)
    {
        if (await db.Units.AnyAsync()) return;  // already seeded

        var units = new[]
        {
            ("Introdução",      "Aprenda frases básicas e saudações",           1),
            ("Comida & Bebida", "Vocabulário de alimentos e restaurantes",      2),
            ("Viagem",          "Frases úteis para viajantes",                  3),
            ("Família",         "Membros da família e relacionamentos",         4),
            ("Trabalho",        "Vocabulário profissional",                     5),
            ("Compras",         "Fazer compras e negociar",                     6),
            ("Saúde",           "Vocabulário médico e bem-estar",               7),
            ("Entretenimento",  "Lazer, filmes e música",                       8),
            ("Cultura",         "Expressões culturais e costumes",              9),
            ("Avançado",        "Tópicos complexos e fluência",                10),
        };

        foreach (var (title, desc, order) in units)
        {
            var unit = new Unit
            {
                LanguageCode = "en",
                OrderIndex   = order,
                Title        = title,
                Description  = desc
            };

            for (int i = 1; i <= 5; i++)
            {
                var lesson = new Lesson
                {
                    UnitId     = unit.Id,
                    OrderIndex = i,
                    Title      = $"Lição {i}",
                    XpReward   = 10
                };

                unit.Lessons.Add(lesson);
            }

            db.Units.Add(unit);
        }

        await db.SaveChangesAsync();
    }
}
