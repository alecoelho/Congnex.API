# Script para o Backend — Endpoint de Questões por Lição

O frontend precisa de um endpoint que retorne as questões de uma lição específica no formato exato que ele consome.

---

## Endpoint Necessário

```
GET /api/lessons/{lessonId}/questions
Authorization: Bearer <token>
```

---

## Response Esperada pelo Frontend

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "type": "imageWordChoice",
      "label": "PALAVRA NOVA",
      "question": "Qual destas imagens é 'chá'?",
      "data": {
        "targetWord": "tea",
        "correctOptionId": 2,
        "options": [
          { "id": 1, "label": "coffee", "emoji": "☕" },
          { "id": 2, "label": "tea", "emoji": "🍵" },
          { "id": 3, "label": "milk", "emoji": "🥛" }
        ]
      },
      "orderIndex": 1
    },
    {
      "id": "guid",
      "type": "multipleChoice",
      "label": "PALAVRA NOVA",
      "question": "Qual é a tradução de 'coffee'?",
      "data": {
        "audioText": "coffee",
        "choices": ["café", "chá", "água"],
        "correctAnswer": "café"
      },
      "orderIndex": 2
    }
  ]
}
```

---

## Tipos de Questão e seus campos `data`

### 1. `imageWordChoice`
Escolher imagem/emoji que corresponde à palavra.
```json
{
  "type": "imageWordChoice",
  "label": "PALAVRA NOVA",
  "question": "Qual destas imagens é 'chá'?",
  "data": {
    "targetWord": "tea",
    "correctOptionId": 2,
    "options": [
      { "id": 1, "label": "coffee", "emoji": "☕" },
      { "id": 2, "label": "tea", "emoji": "🍵" },
      { "id": 3, "label": "milk", "emoji": "🥛" }
    ]
  }
}
```

### 2. `multipleChoice`
Múltipla escolha com áudio.
```json
{
  "type": "multipleChoice",
  "label": "PALAVRA NOVA",
  "question": "Qual é a tradução de 'coffee'?",
  "data": {
    "audioText": "coffee",
    "choices": ["café", "chá", "água"],
    "correctAnswer": "café"
  }
}
```

### 3. `translate`
Traduzir usando banco de palavras.
```json
{
  "type": "translate",
  "label": "TRADUÇÃO",
  "question": "Escreva em português:",
  "data": {
    "audioText": "thank you",
    "wordOptions": ["obrigado", "por favor", "olá", "tchau"],
    "correctAnswer": "obrigado"
  }
}
```

### 4. `fillBlank`
Preencher lacuna na frase.
```json
{
  "type": "fillBlank",
  "label": "COMPLETE A FRASE",
  "question": "Complete a frase:",
  "data": {
    "sentence": "I ____ coffee every morning.",
    "choices": ["drink", "drinks", "drinking"],
    "correctAnswer": "drink"
  }
}
```

### 5. `matchPairs`
Conectar pares (português ↔ inglês).
```json
{
  "type": "matchPairs",
  "label": "VOCABULÁRIO",
  "question": "Combine os pares:",
  "data": {
    "leftWords": ["chá", "olá", "água"],
    "rightWords": ["water", "hello", "tea"],
    "correctPairs": { "chá": "tea", "olá": "hello", "água": "water" }
  }
}
```

### 6. `listenAndTap`
Ouvir e montar a frase com palavras.
```json
{
  "type": "listenAndTap",
  "label": "ESCUTA",
  "question": "Toque no que escutar:",
  "data": {
    "audioText": "milk",
    "choices": ["hello", "milk", "water", "thanks"],
    "correctAnswer": "milk"
  }
}
```

### 7. `listeningWordSelection`
Ouvir e selecionar a palavra correta.
```json
{
  "type": "listeningWordSelection",
  "label": "ESCUTA",
  "question": "Toque no que escutar",
  "data": {
    "audioWord": "tea",
    "options": ["water", "coffee", "sugar", "tea"],
    "correctAnswer": "tea"
  }
}
```

### 8. `videoListening`
Assistir vídeo de conversa e identificar estrutura.
```json
{
  "type": "videoListening",
  "label": "ESCUTA",
  "question": "Assista ao vídeo e toque na estrutura que ouviu:",
  "data": {
    "videoSource": "conversation_milk",
    "conversationText": "— Hi! Can I have some milk, please?\n— Sure, here you go!",
    "targetPhrase": "milk",
    "options": ["hello", "milk", "water", "thanks"],
    "correctAnswer": "milk"
  }
}
```

---

## Estrutura do Response

```typescript
interface QuestionResponse {
  id: string;           // GUID da questão
  type: string;         // Tipo (ver lista acima)
  label: string;        // Label exibido no topo (ex: "ESCUTA", "TRADUÇÃO")
  question: string;     // Texto da pergunta
  data: object;         // Campos específicos do tipo (JSON)
  orderIndex: number;   // Ordem de exibição (1, 2, 3...)
}
```

---

## Opção Alternativa (mais simples)

Se preferir manter o formato flat que já existe no backend:

```json
{
  "id": "guid",
  "type": "MultipleChoice",
  "prompt": "Qual é a tradução de 'coffee'?",
  "correctAnswers": ["café"],
  "options": "[\"café\", \"chá\", \"água\"]",
  "mediaUrl": null,
  "orderIndex": 1
}
```

**Nesse caso, o frontend fará o mapeamento.** Mas o formato ideal é o primeiro (com `data` tipado), pois cada tipo de questão tem campos diferentes.

---

## Seed de Questões (exemplo para Unidade 1, Lição 1)

Cada lição deve ter **7-9 questões** com variedade de tipos. Exemplo:

| # | Tipo | Pergunta |
|---|------|----------|
| 1 | imageWordChoice | Qual destas imagens é 'chá'? |
| 2 | listeningWordSelection | Toque no que escutar (tea) |
| 3 | multipleChoice | Qual é a tradução de "coffee"? |
| 4 | translate | Escreva em português: "thank you" |
| 5 | fillBlank | I ____ coffee every morning. |
| 6 | matchPairs | Combine: chá↔tea, olá↔hello, água↔water |
| 7 | translate | Escreva em inglês: "olá" |
| 8 | videoListening | Assista e identifique: "milk" |
| 9 | videoListening | Assista e identifique: "good morning" |

---

## Endpoint de Completar Lição

```
POST /api/lessons/{lessonId}/complete
Authorization: Bearer <token>
Content-Type: application/json

{
  "score": 78,
  "answers": [
    { "questionId": "guid", "givenAnswers": ["café"], "isCorrect": true },
    { "questionId": "guid", "givenAnswers": ["chá"], "isCorrect": false }
  ]
}
```

Response:
```json
{
  "success": true,
  "data": {
    "xpEarned": 10,
    "totalXp": 50,
    "newStreak": 3
  }
}
```

---

## Decisão Necessária

**Qual formato o backend vai usar?**

1. **Formato com `data` tipado** (recomendado) — cada tipo tem seus campos específicos dentro de `data`
2. **Formato flat atual** (`prompt`, `correctAnswers`, `options` como JSON string) — frontend faz o parse

Se usar o formato 2 (flat), me avise que eu adapto o frontend para fazer o mapeamento. Se usar o formato 1, o frontend já está pronto para consumir.

---

## Instrução Final

- Cada lição precisa de 7-9 questões no seed
- 10 unidades × 5 lições × ~8 questões = ~400 questões no total
- Pode começar com a Unidade 1 (5 lições, ~40 questões) para testar
- Os tipos mais importantes para o MVP: `multipleChoice`, `translate`, `fillBlank`, `listeningWordSelection`
