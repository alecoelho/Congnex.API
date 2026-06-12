# Script para o Kiro Frontend — Endpoint de Flashcards

## Endpoint Disponível

```
GET /api/lessons/{lessonId}/flashcards
Authorization: Bearer <token>
```

## Response JSON

```json
{
  "success": true,
  "data": [
    {
      "word": "hello",
      "translation": "olá",
      "emoji": "👋"
    },
    {
      "word": "goodbye",
      "translation": "tchau",
      "emoji": null
    },
    {
      "word": "good morning",
      "translation": "bom dia",
      "emoji": null
    },
    {
      "word": "coffee",
      "translation": "café",
      "emoji": null
    },
    {
      "word": "thank you",
      "translation": "obrigado",
      "emoji": "🙏"
    }
  ]
}
```

## TypeScript Types

```typescript
interface Flashcard {
  word: string;        // Palavra em inglês
  translation: string; // Tradução em português
  emoji: string | null; // Emoji representativo (pode ser null)
}
```

## Como Funciona

O backend extrai automaticamente as palavras/traduções das questões da lição:
- De `imageWordChoice` → extrai `targetWord` + emoji
- De `multipleChoice` → extrai `audioText` (inglês) + `correctAnswer` (português)
- De `translate` → extrai `audioText` + `correctAnswer`
- De `listeningWordSelection` / `listenAndTap` → extrai `audioWord`/`audioText`

Palavras duplicadas são removidas automaticamente. Retorna ~5-8 flashcards por lição.

## Exemplo de Chamada

```typescript
// No lessonsService.ts, adicionar:
export async function getFlashcards(lessonId: string): Promise<Flashcard[]> {
  const response = await api.get(`/api/lessons/${lessonId}/flashcards`);
  return response.data.data;
}
```

## Integração na LessonReview

```typescript
// Hook useFlashcards.ts
import { useState, useEffect } from 'react';
import { getFlashcards } from '../services/lessonsService';

interface Flashcard {
  word: string;
  translation: string;
  emoji: string | null;
}

export function useFlashcards(lessonId: string) {
  const [flashcards, setFlashcards] = useState<Flashcard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetch() {
      try {
        setLoading(true);
        const data = await getFlashcards(lessonId);
        if (!cancelled) setFlashcards(data);
      } catch (err: any) {
        if (!cancelled) setError(err.message);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetch();
    return () => { cancelled = true; };
  }, [lessonId]);

  return { flashcards, loading, error };
}
```

## Fluxo na Tela

```
LessonScreen (questões)
  ↓ completa todas as questões
  ↓ POST /api/lessons/{lessonId}/complete
  ↓
LessonReview (flashcards)
  ↓ GET /api/lessons/{lessonId}/flashcards
  ↓ Mostra cards com word/translation/emoji
  ↓ Usuário swipa ou toca para avançar
  ↓
HomeScreen (refetch units para atualizar progresso)
```

## Componente Flashcard (sugestão visual)

```
┌─────────────────────────┐
│                         │
│         👋              │  ← emoji (se disponível)
│                         │
│       hello             │  ← word (frente)
│                         │
│       ─────             │
│                         │
│        olá              │  ← translation (verso)
│                         │
└─────────────────────────┘
        [1/5]                ← indicador de progresso
```

## Erros Possíveis

- `404 Not Found` — Lição não existe
- `401 Unauthorized` — Token inválido/expirado

## Notas

- O endpoint requer autenticação (Bearer token)
- Retorna entre 5-8 flashcards por lição (sem duplicatas)
- Se a lição não tiver questões com palavras extraíveis, retorna array vazio
- O `lessonId` é o mesmo GUID usado em `GET /api/lessons/{lessonId}/questions`
