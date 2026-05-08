using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(CongnexDbContext db)
    {
        if (await db.Units.AnyAsync()) return;  // already seeded

        var units = new[]
        {
            ("Introdução",     "Frases básicas e saudações",          1),
            ("Comida & Bebida","Peça comida em restaurantes",         2),
            ("Viagem",         "Navegue em aeroportos e hotéis",      3),
            ("Família",        "Fale sobre família e amigos",         4),
            ("Trabalho",       "Vocabulário profissional",            5),
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

            // 6 lessons per unit
            for (int i = 1; i <= 6; i++)
            {
                var lesson = new Lesson
                {
                    UnitId     = unit.Id,
                    OrderIndex = i,
                    Title      = $"Lição {i}",
                    XpReward   = 10
                };

                // 4 questions per lesson
                for (int q = 1; q <= 4; q++)
                {
                    lesson.Questions.Add(new Question
                    {
                        LessonId      = lesson.Id,
                        Type          = QuestionType.MultipleChoice,
                        Prompt        = $"[{title}] Questão {q}",
                        CorrectAnswers = ["A"],
                        Options        = "[\"A\",\"B\",\"C\",\"D\"]",
                        OrderIndex    = q
                    });
                }

                unit.Lessons.Add(lesson);
            }

            db.Units.Add(unit);
        }

        await db.SaveChangesAsync();
    }
}
