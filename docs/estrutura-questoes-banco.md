# Estrutura das Questões no Banco de Dados

## Tabela: `questions`

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) / GUID | ID único da questão |
| `lesson_id` | CHAR(36) / GUID | FK para a lição |
| `type` | VARCHAR | Tipo da questão (enum) |
| `prompt` | TEXT | Texto da pergunta exibido ao usuário |
| `correct_answers` | JSON | Array de respostas corretas (legado, usar `["_"]`) |
| `options` | JSON/TEXT | **Campo principal**: JSON com o objeto `data` completo |
| `media_url` | VARCHAR | **Usado como `label`** (ex: "PALAVRA NOVA", "ESCUTA") |
| `order_index` | INT | Ordem de exibição na lição (1-10) |
| `created_at` | DATETIME | Data de criação |
| `updated_at` | DATETIME | Data de atualização |

---

## Mapeamento dos Campos

| Campo no Banco | Campo na API | Descrição |
|----------------|--------------|-----------|
| `type` | `type` | Tipo da questão (camelCase) |
| `media_url` | `label` | Label exibido no topo da tela |
| `prompt` | `question` | Texto da pergunta |
| `options` | `data` | JSON com dados específicos do tipo |
| `order_index` | `orderIndex` | Ordem de exibição |

---

## Tipos de Questão (enum `QuestionType`)

```
imageWordChoice
multipleChoice
translate
fillBlank
matchPairs
listenAndTap
listeningWordSelection
videoListening
```

---

## Formato de Inserção por Tipo

### 1. `imageWordChoice`

Escolher imagem/emoji que corresponde à palavra.

**Campos no banco:**
```
type:          imageWordChoice
media_url:     PALAVRA NOVA
prompt:        Qual destas imagens é 'tea'?
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "targetWord": "tea",
  "correctOptionId": 2,
  "options": [
    { "id": 1, "label": "coffee", "emoji": "☕" },
    { "id": 2, "label": "tea", "emoji": "🍵" },
    { "id": 3, "label": "milk", "emoji": "🥛" }
  ]
}
```

---

### 2. `multipleChoice`

Múltipla escolha com áudio.

**Campos no banco:**
```
type:          multipleChoice
media_url:     PALAVRA NOVA
prompt:        Qual é a tradução de 'coffee'?
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "audioText": "coffee",
  "choices": ["café", "chá", "água"],
  "correctAnswer": "café"
}
```

---

### 3. `translate`

Traduzir usando banco de palavras.

**Campos no banco:**
```
type:          translate
media_url:     TRADUÇÃO
prompt:        Escreva em português:
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "audioText": "thank you",
  "wordOptions": ["obrigado", "por favor", "olá", "tchau"],
  "correctAnswer": "obrigado"
}
```

---

### 4. `fillBlank`

Preencher lacuna na frase.

**Campos no banco:**
```
type:          fillBlank
media_url:     COMPLETE A FRASE
prompt:        Complete a frase:
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "sentence": "I ____ coffee every morning.",
  "choices": ["drink", "drinks", "drinking"],
  "correctAnswer": "drink"
}
```

---

### 5. `matchPairs`

Conectar pares (português ↔ inglês).

**Campos no banco:**
```
type:          matchPairs
media_url:     VOCABULÁRIO
prompt:        Combine os pares:
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "leftWords": ["chá", "olá", "água"],
  "rightWords": ["water", "hello", "tea"],
  "correctPairs": {
    "chá": "tea",
    "olá": "hello",
    "água": "water"
  }
}
```

---

### 6. `listenAndTap`

Ouvir e montar a frase com palavras.

**Campos no banco:**
```
type:          listenAndTap
media_url:     ESCUTA
prompt:        Toque no que escutar:
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "audioText": "milk",
  "choices": ["hello", "milk", "water", "thanks"],
  "correctAnswer": "milk"
}
```

---

### 7. `listeningWordSelection`

Ouvir e selecionar a palavra correta.

**Campos no banco:**
```
type:          listeningWordSelection
media_url:     ESCUTA
prompt:        Toque no que escutar
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "audioWord": "tea",
  "options": ["water", "coffee", "sugar", "tea"],
  "correctAnswer": "tea"
}
```

---

### 8. `videoListening`

Assistir vídeo de conversa e identificar estrutura.

**Campos no banco:**
```
type:          videoListening
media_url:     ESCUTA
prompt:        Assista ao vídeo e toque na estrutura que ouviu:
correct_answers: ["_"]
options:       (JSON abaixo)
```

**JSON do campo `options`:**
```json
{
  "videoSource": "conversation_milk",
  "conversationText": "— Hi! Can I have some milk, please?\n— Sure, here you go!",
  "targetPhrase": "milk",
  "options": ["hello", "milk", "water", "thanks"],
  "correctAnswer": "milk"
}
```

---

## Labels Disponíveis (campo `media_url`)

| Label | Quando usar |
|-------|-------------|
| `PALAVRA NOVA` | Introduzindo vocabulário novo |
| `ESCUTA` | Questões de áudio/listening |
| `TRADUÇÃO` | Questões de tradução |
| `COMPLETE A FRASE` | Fill in the blank |
| `VOCABULÁRIO` | Match pairs / revisão de vocabulário |
| `REVISÃO` | Questões de revisão de conteúdo anterior |
| `QUESTÃO` | Label genérico (fallback) |

---

## Exemplo SQL de Inserção

```sql
INSERT INTO questions (id, lesson_id, type, prompt, correct_answers, options, media_url, order_index, created_at, updated_at)
VALUES (
  UUID(),
  'GUID-DA-LICAO',
  'multipleChoice',
  'Qual é a tradução de ''coffee''?',
  '["_"]',
  '{"audioText":"coffee","choices":["café","chá","água"],"correctAnswer":"café"}',
  'PALAVRA NOVA',
  1,
  NOW(),
  NOW()
);
```

---

## Exemplo Completo — Inserir uma Lição com 10 Questões

```sql
-- Supondo que a lição já existe com ID = 'abc123...'
SET @lesson_id = 'abc123-guid-da-licao';

INSERT INTO questions (id, lesson_id, type, prompt, correct_answers, options, media_url, order_index, created_at, updated_at) VALUES
(UUID(), @lesson_id, 'imageWordChoice', 'Qual destas imagens é ''hello''?', '["_"]', '{"targetWord":"hello","correctOptionId":1,"options":[{"id":1,"label":"hello","emoji":"👋"},{"id":2,"label":"goodbye","emoji":"🙋"},{"id":3,"label":"thanks","emoji":"🙏"}]}', 'PALAVRA NOVA', 1, NOW(), NOW()),
(UUID(), @lesson_id, 'multipleChoice', 'Qual é a tradução de ''hello''?', '["_"]', '{"audioText":"hello","choices":["olá","tchau","obrigado"],"correctAnswer":"olá"}', 'PALAVRA NOVA', 2, NOW(), NOW()),
(UUID(), @lesson_id, 'listeningWordSelection', 'Toque no que escutar', '["_"]', '{"audioWord":"hello","options":["goodbye","hello","thanks","please"],"correctAnswer":"hello"}', 'ESCUTA', 3, NOW(), NOW()),
(UUID(), @lesson_id, 'translate', 'Escreva em português:', '["_"]', '{"audioText":"good morning","wordOptions":["bom dia","boa noite","boa tarde","olá"],"correctAnswer":"bom dia"}', 'TRADUÇÃO', 4, NOW(), NOW()),
(UUID(), @lesson_id, 'fillBlank', 'Complete a frase:', '["_"]', '{"sentence":"____, how are you?","choices":["Hello","Goodbye","Thanks"],"correctAnswer":"Hello"}', 'COMPLETE A FRASE', 5, NOW(), NOW()),
(UUID(), @lesson_id, 'multipleChoice', 'Qual é a tradução de ''goodbye''?', '["_"]', '{"audioText":"goodbye","choices":["olá","tchau","por favor"],"correctAnswer":"tchau"}', 'PALAVRA NOVA', 6, NOW(), NOW()),
(UUID(), @lesson_id, 'matchPairs', 'Combine os pares:', '["_"]', '{"leftWords":["olá","tchau","obrigado"],"rightWords":["thanks","hello","goodbye"],"correctPairs":{"olá":"hello","tchau":"goodbye","obrigado":"thanks"}}', 'VOCABULÁRIO', 7, NOW(), NOW()),
(UUID(), @lesson_id, 'translate', 'Escreva em inglês:', '["_"]', '{"audioText":"tchau","wordOptions":["hello","goodbye","thanks","please"],"correctAnswer":"goodbye"}', 'TRADUÇÃO', 8, NOW(), NOW()),
(UUID(), @lesson_id, 'listeningWordSelection', 'Toque no que escutar', '["_"]', '{"audioWord":"good morning","options":["good night","good morning","good afternoon","goodbye"],"correctAnswer":"good morning"}', 'ESCUTA', 9, NOW(), NOW()),
(UUID(), @lesson_id, 'multipleChoice', 'Qual é a tradução de ''thank you''?', '["_"]', '{"audioText":"thank you","choices":["obrigado","por favor","desculpa"],"correctAnswer":"obrigado"}', 'REVISÃO', 10, NOW(), NOW());
```

---

## Regras de Negócio

1. Cada lição deve ter **exatamente 10 questões**
2. Variar os tipos (não repetir o mesmo tipo mais de 3x por lição)
3. O `order_index` vai de 1 a 10
4. O campo `correct_answers` é legado — sempre usar `["_"]`
5. Toda a lógica de resposta correta está dentro do JSON do campo `options`
6. O `media_url` é usado como label visual — não é uma URL real

---

## Estrutura Geral do Banco

```
10 unidades (units)
  └── 5 lições cada (lessons)
       └── 10 questões cada (questions)

Total: 10 × 5 × 10 = 500 questões
```
