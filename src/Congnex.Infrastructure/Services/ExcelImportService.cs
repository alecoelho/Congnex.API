using ClosedXML.Excel;
using Congnex.Application.Admin.Commands;

namespace Congnex.Infrastructure.Services;

/// <summary>
/// Lê planilhas Excel e converte em DTOs de importação.
/// Uma planilha por vídeo: linha 1 = cabeçalho, linha 2 = dados do vídeo + 1ª linha de transcrição,
/// linhas seguintes (com title/videoUrl vazios) = continuação da transcrição.
/// </summary>
public static class ExcelImportService
{
    // ── Vídeo ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Colunas esperadas: title | description | unitTitle | videoUrl | xpReward | level | transcript
    /// </summary>
    public static ImportVideoLessonRequest ReadVideoLesson(Stream stream)
    {
        using var wb  = new XLWorkbook(stream);
        var ws         = wb.Worksheet(1);
        var rows       = ws.RangeUsed()?.RowsUsed().Skip(1).ToList()
                         ?? throw new InvalidDataException("Planilha vazia ou sem cabeçalho.");

        if (!rows.Any())
            throw new InvalidDataException("Nenhuma linha de dados encontrada.");

        // Primeira linha: dados do vídeo
        var first = rows[0];
        var title       = Cell(first, 1);
        var description = Cell(first, 2);
        var unitTitle   = Cell(first, 3);
        var videoUrl    = Cell(first, 4);
        var xpReward    = int.TryParse(Cell(first, 5), out var xp) ? xp : 10;
        var level       = Cell(first, 6); // A1, A2, B1, B2, C1, C2

        if (string.IsNullOrWhiteSpace(title))    throw new InvalidDataException("Coluna 'title' obrigatória.");
        if (string.IsNullOrWhiteSpace(unitTitle)) throw new InvalidDataException("Coluna 'unitTitle' obrigatória.");
        if (string.IsNullOrWhiteSpace(videoUrl)) throw new InvalidDataException("Coluna 'videoUrl' obrigatória.");

        // Todas as linhas contribuem com transcrição (coluna 7)
        var transcript = new List<TranscriptLineDto>();
        foreach (var row in rows)
        {
            var transcriptCell = Cell(row, 7);
            if (string.IsNullOrWhiteSpace(transcriptCell)) continue;

            // Cada linha pode ter múltiplos segmentos separados por quebra de linha dentro da célula
            var segments = transcriptCell
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var segment in segments)
            {
                // Extrai timestamp do início da linha: "0:07 texto..." ou "2. 0:07 texto..."
                var tsMatch = System.Text.RegularExpressions.Regex.Match(
                    segment, @"^(?:\d+\.\s*)?(\d+:\d+(?::\d+)?)\s*(.*)");

                if (tsMatch.Success)
                {
                    transcript.Add(new TranscriptLineDto(
                        StartTime: tsMatch.Groups[1].Value,
                        Text:      tsMatch.Groups[2].Value.Trim()));
                }
                else
                {
                    // Sem timestamp — adiciona como texto puro com timestamp "0:00"
                    transcript.Add(new TranscriptLineDto("0:00", segment));
                }
            }
        }

        return new ImportVideoLessonRequest(title, description, unitTitle, videoUrl, xpReward, level, transcript);
    }

    // ── Multiple Choice / Image Choice ────────────────────────────────────────

    /// <summary>
    /// Colunas: questionText | option1 | option2 | option3 | option4 | correctAnswer | difficulty | imageUrl | instruction | label
    /// </summary>
    public static List<MultipleChoiceRowDto> ReadMultipleChoice(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws        = wb.Worksheet(1);
        return ws.RangeUsed()?.RowsUsed().Skip(1)
            .Select(row => new MultipleChoiceRowDto(
                QuestionText:  Cell(row, 1),
                Option1:       Cell(row, 2),
                Option2:       Cell(row, 3),
                Option3:       Cell(row, 4),
                Option4:       Cell(row, 5),
                CorrectAnswer: Cell(row, 6),
                Difficulty:    Cell(row, 7),
                ImageUrl:      NullIfEmpty(Cell(row, 8)),
                Instruction:   NullIfEmpty(Cell(row, 9)),
                Label:         NullIfEmpty(Cell(row, 10))))
            .ToList() ?? [];
    }

    // ── Translation ───────────────────────────────────────────────────────────

    /// <summary>
    /// Colunas: questionText | correctAnswer | difficulty | wrongOption1 | wrongOption2 | wrongOption3 | instruction* | label*
    /// </summary>
    public static List<TranslationRowDto> ReadTranslation(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws        = wb.Worksheet(1);
        return ws.RangeUsed()?.RowsUsed().Skip(1)
            .Select(row => new TranslationRowDto(
                QuestionText:  Cell(row, 1),
                CorrectAnswer: Cell(row, 2),
                Difficulty:    Cell(row, 3),
                WrongOption1:  NullIfEmpty(Cell(row, 4)),
                WrongOption2:  NullIfEmpty(Cell(row, 5)),
                WrongOption3:  NullIfEmpty(Cell(row, 6)),
                Instruction:   NullIfEmpty(Cell(row, 7)),
                Label:         NullIfEmpty(Cell(row, 8))))
            .ToList() ?? [];
    }

    // ── Complete Sentence ─────────────────────────────────────────────────────

    /// <summary>
    /// Colunas: questionText | correctAnswer | difficulty | wrongOption1 | wrongOption2 | wrongOption3 | instruction* | label*
    /// (questionText deve conter ___ no lugar do espaço)
    /// </summary>
    public static List<CompleteSentenceRowDto> ReadCompleteSentence(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws        = wb.Worksheet(1);
        return ws.RangeUsed()?.RowsUsed().Skip(1)
            .Select(row => new CompleteSentenceRowDto(
                QuestionText:  Cell(row, 1),
                CorrectAnswer: Cell(row, 2),
                Difficulty:    Cell(row, 3),
                WrongOption1:  NullIfEmpty(Cell(row, 4)),
                WrongOption2:  NullIfEmpty(Cell(row, 5)),
                WrongOption3:  NullIfEmpty(Cell(row, 6)),
                Instruction:   NullIfEmpty(Cell(row, 7)),
                Label:         NullIfEmpty(Cell(row, 8))))
            .ToList() ?? [];
    }

    // ── Match Pairs ───────────────────────────────────────────────────────────

    /// <summary>
    /// Colunas: groupLabel | leftText | rightText | difficulty
    /// Várias linhas com mesmo groupLabel = um Question com múltiplos pares
    /// </summary>
    public static List<MatchPairsRowDto> ReadMatchPairs(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws        = wb.Worksheet(1);
        return ws.RangeUsed()?.RowsUsed().Skip(1)
            .Select(row => new MatchPairsRowDto(
                GroupLabel: Cell(row, 1),
                LeftText:   Cell(row, 2),
                RightText:  Cell(row, 3),
                Difficulty: Cell(row, 4)))
            .ToList() ?? [];
    }

    // ── Listening ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Colunas: questionText | audioText | option1 | option2 | option3 | option4 | correctAnswer | difficulty | instruction | label
    /// </summary>
    public static List<ListeningRowDto> ReadListening(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws        = wb.Worksheet(1);
        return ws.RangeUsed()?.RowsUsed().Skip(1)
            .Select(row => new ListeningRowDto(
                QuestionText:  Cell(row, 1),
                AudioText:     Cell(row, 2),
                Option1:       Cell(row, 3),
                Option2:       Cell(row, 4),
                Option3:       Cell(row, 5),
                Option4:       Cell(row, 6),
                CorrectAnswer: Cell(row, 7),
                Difficulty:    Cell(row, 8),
                Instruction:   NullIfEmpty(Cell(row, 9)),
                Label:         NullIfEmpty(Cell(row, 10))))
            .ToList() ?? [];
    }

    // ── Pronunciation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Colunas: questionText | correctAnswer | difficulty | instruction | label
    /// </summary>
    public static List<PronunciationRowDto> ReadPronunciation(Stream stream)
    {
        using var wb = new XLWorkbook(stream);
        var ws        = wb.Worksheet(1);
        return ws.RangeUsed()?.RowsUsed().Skip(1)
            .Select(row => new PronunciationRowDto(
                QuestionText:  Cell(row, 1),
                CorrectAnswer: Cell(row, 2),
                Difficulty:    Cell(row, 3),
                Instruction:   NullIfEmpty(Cell(row, 4)),
                Label:         NullIfEmpty(Cell(row, 5))))
            .ToList() ?? [];
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Cell(IXLRangeRow row, int col) =>
        row.Cell(col).GetValue<string>().Trim();

    private static string? NullIfEmpty(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
