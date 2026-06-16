namespace Congnex.Application.Interfaces;

/// <summary>
/// Seleciona questões do catálogo (question_bank) relacionadas ao assunto de um vídeo
/// e copia para a tabela `questions` da lição (consumo do aluno).
/// Estratégia híbrida: filtra por nível CEFR + domínio e rankeia por relevância da
/// transcrição via FULLTEXT.
/// </summary>
public interface IQuestionMatchingService
{
    /// <summary>
    /// Busca questões relacionadas e as copia para a lição informada.
    /// </summary>
    /// <param name="lessonId">Lição que receberá as questões.</param>
    /// <param name="cefrLevel">Nível CEFR do vídeo (A1..C2).</param>
    /// <param name="domain">Domínio/assunto (um dos 12) ou null para ignorar.</param>
    /// <param name="transcript">Texto da transcrição para o ranking por relevância.</param>
    /// <param name="limit">Quantidade máxima de questões a copiar.</param>
    /// <returns>Quantidade de questões efetivamente copiadas.</returns>
    Task<int> MatchAndCopyAsync(
        Guid    lessonId,
        string  cefrLevel,
        string? domain,
        string  transcript,
        int     limit = 60,
        CancellationToken ct = default);
}
