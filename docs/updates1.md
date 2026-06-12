# Script para o Backend — Endpoint de Revisão de Flashcards (Spaced Repetition)

## Contexto

O frontend agora tem botões ✅ (Lembro) e ❌ (Não lembro) na tela de flashcards. Quando o aluno clica em um deles, o frontend envia o resultado para o backend armazenar e usar para decidir quando mostrar a palavra novamente (spaced repetition).

---

## Endpoint Necessário

```
POST /api/flashcards/review
Authorization: Bearer <token>
Content-Type: application/json
```

### Request Body

```json
{
  "lessonId": "guid-da-licao",
  "word": "hello",
  "remembered": true
}
```

| Campo | Tipo | Descrição |
|-------|------|-----------|
| lessonId | string (GUID) | ID da lição de onde veio o flashcard |
| word | string | A palavra em inglês que foi revisada |
| remembered | boolean | `true` = lembrou, `false` = não lembrou |

### Response

```json
{
  "success": true,
  "data": null
}
```

---

## Lógica de Spaced Repetition (sugestão)

### Tabela: `FlashcardReviews` (ou `UserWordProgress`)

```sql
CREATE TABLE FlashcardReviews (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    LessonId UNIQUEIDENTIFIER NOT NULL,
    Word NVARCHAR(100) NOT NULL,
    Remembered BIT NOT NULL,
    ReviewCount INT NOT NULL DEFAULT 1,
    CorrectCount INT NOT NULL DEFAULT 0,
    LastReviewedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    NextReviewAt DATETIME2 NOT NULL,
    IntervalDays INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_FlashcardReviews_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_FlashcardReviews_Lesson FOREIGN KEY (LessonId) REFERENCES Lessons(Id)
);

CREATE UNIQUE INDEX IX_FlashcardReviews_UserWord ON FlashcardReviews(UserId, Word);
```

### Algoritmo de Intervalo (simplificado)

```csharp
// Quando o aluno LEMBRA (remembered = true):
review.CorrectCount++;
review.IntervalDays = review.IntervalDays * 2; // Dobra o intervalo
review.NextReviewAt = DateTime.UtcNow.AddDays(review.IntervalDays);

// Quando o aluno NÃO LEMBRA (remembered = false):
review.IntervalDays = 1; // Reseta para 1 dia
review.NextReviewAt = DateTime.UtcNow.AddDays(1);

// Sempre:
review.ReviewCount++;
review.LastReviewedAt = DateTime.UtcNow;
```

### Intervalos progressivos:
- 1ª vez lembrou → próxima em 1 dia
- 2ª vez lembrou → próxima em 2 dias
- 3ª vez lembrou → próxima em 4 dias
- 4ª vez lembrou → próxima em 8 dias
- Não lembrou em qualquer momento → volta para 1 dia

---

## Endpoint Opcional: Flashcards para Revisar Hoje

```
GET /api/flashcards/due
Authorization: Bearer <token>
```

Retorna flashcards que precisam ser revisados (onde `NextReviewAt <= DateTime.UtcNow`):

```json
{
  "success": true,
  "data": [
    {
      "word": "hello",
      "translation": "olá",
      "emoji": "👋",
      "lessonId": "guid",
      "reviewCount": 3,
      "correctCount": 2,
      "lastReviewedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

Isso permite que no futuro o frontend mostre "Você tem 5 palavras para revisar hoje!" na HomeScreen.

---

## Fluxo Completo

```
Aluno abre tab Flashcards
  ↓
GET /api/lessons/{lessonId}/flashcards (palavras da lição atual)
  ↓
Aluno vê card → vira → clica ✅ ou ❌
  ↓
POST /api/flashcards/review { word, lessonId, remembered }
  ↓
Backend salva/atualiza FlashcardReviews
  ↓
Calcula próximo intervalo de revisão
```

---

## Prioridade

1. **Obrigatório agora**: `POST /api/flashcards/review` — salvar o resultado
2. **Pode ser depois**: `GET /api/flashcards/due` — listar palavras para revisar
3. **Futuro**: Notificação push "Hora de revisar 5 palavras!"

---

## Notas

- Se o `word` já existe para o usuário, atualizar (upsert)
- Se não existe, criar novo registro
- O `lessonId` ajuda a rastrear de qual lição veio a palavra
- Não precisa retornar dados no response (apenas `success: true`)
- O frontend faz fire-and-forget (não espera a resposta para avançar o card)
