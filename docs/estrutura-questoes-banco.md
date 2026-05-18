# Estrutura de Questões no Banco de Dados (v2)

> **Atualizado em:** Maio 2026
> **Versão:** 2.0 — Estrutura relacional (sem JSON para questões/alternativas/respostas)

---

## Visão Geral da Arquitetura

```text
YouTube video
→ extrai roteiro/transcrição
→ salva roteiro em JSON (único uso de JSON)
→ IA analisa roteiro
→ extrai objetos, palavras, frases e expressões
→ gera questões
→ salva questões em tabelas relacionais
→ aluno responde
→ sistema salva resposta individual
→ erros alimentam revisão/flashcards
```

---

## Regra Principal

- **JSON** existe apenas para `transcript_json` (roteiro bruto do vídeo)
- **Questões, alternativas e respostas** usam tabelas relacionais normais

---

## Tabelas

---

### 1. `lesson_videos`

Vídeos do YouTube vinculados a aulas.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `lesson_id` | CHAR(36) | FK → lessons |
| `youtube_video_id` | VARCHAR(50) | ID do vídeo no YouTube |
| `youtube_url` | VARCHAR(500) | URL completa |
| `title` | VARCHAR(200) | Título do vídeo |
| `transcript_json` | LONGTEXT | Roteiro/transcrição em JSON |
| `language` | VARCHAR(10) | Idioma (default: "en") |
| `duration_seconds` | INT | Duração em segundos |
| `created_at` | DATETIME | Criação |
| `updated_at` | DATETIME | Atualização |

**Formato do `transcript_json`:**
```json
[
  {
    "start": 0.5,
    "end": 3.2,
    "speaker": "person_1",
    "text": "Hi! Can I have some milk, please?"
  }
]
```

**Índice:** `lesson_id`

---

### 2. `video_learning_items`

Conteúdos extraídos pela IA a partir da transcrição.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `video_id` | CHAR(36) | FK → lesson_videos |
| `item_type` | VARCHAR(50) | Tipo do item |
| `text_en` | VARCHAR(500) | Texto em inglês |
| `text_pt` | VARCHAR(500) | Tradução em português |
| `category` | VARCHAR(100) | Categoria (food, greetings, etc.) |
| `difficulty` | VARCHAR(20) | easy, medium, hard |
| `timestamp_start` | DOUBLE | Início no vídeo (segundos) |
| `timestamp_end` | DOUBLE | Fim no vídeo (segundos) |
| `created_at` | DATETIME | Criação |
| `updated_at` | DATETIME | Atualização |

**Valores de `item_type`:**
```
object | word | phrase | expression | grammar
```

**Exemplos:**
| item_type | text_en | text_pt |
|-----------|---------|---------|
| object | milk | leite |
| word | coffee | café |
| phrase | Can I have some milk, please? | Posso ter um pouco de leite, por favor? |
| expression | here you go | aqui está |

**Índice:** `video_id`

---

### 3. `questions`

Tabela principal de perguntas. Sem JSON.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `lesson_id` | CHAR(36) | FK → lessons |
| `video_id` | CHAR(36) | FK → lesson_videos (opcional) |
| `learning_item_id` | CHAR(36) | FK → video_learning_items (opcional) |
| `type` | VARCHAR(50) | Tipo da questão |
| `label` | VARCHAR(100) | Label visual (ex: "PALAVRA NOVA") |
| `prompt` | VARCHAR(2000) | Instrução curta |
| `instruction` | VARCHAR(500) | Instrução adicional |
| `question_text` | VARCHAR(2000) | Texto principal da pergunta |
| `correct_answer` | VARCHAR(500) | Resposta correta (texto) |
| `audio_text` | VARCHAR(500) | Texto para TTS/áudio |
| `image_url` | VARCHAR(500) | URL da imagem (se aplicável) |
| `order_index` | INT | Ordem na lição (1-10) |
| `difficulty` | VARCHAR(20) | easy, medium, hard |
| `created_at` | DATETIME | Criação |
| `updated_at` | DATETIME | Atualização |

**Tipos de questão (`type`):**
```
multiple_choice
image_choice
listening_choice
translation_pt
translation_en
complete_sentence
match_pairs
video_listening
pronunciation
```

**Labels disponíveis (`label`):**
| Label | Quando usar |
|-------|-------------|
| PALAVRA NOVA | Introduzindo vocabulário novo |
| ESCUTA | Questões de áudio/listening |
| TRADUÇÃO | Questões de tradução |
| COMPLETE A FRASE | Fill in the blank |
| VOCABULÁRIO | Match pairs / revisão |
| REVISÃO | Revisão de conteúdo anterior |
| PRONÚNCIA | Exercícios de pronúncia |

**Índices:** `lesson_id`, `video_id`, `learning_item_id`

---

### 4. `question_options`

Alternativas das questões. Cada alternativa é uma linha.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `question_id` | CHAR(36) | FK → questions |
| `option_text` | VARCHAR(500) | Texto da alternativa |
| `option_image_url` | VARCHAR(500) | Imagem (para image_choice) |
| `option_audio_url` | VARCHAR(500) | Áudio (para listening) |
| `is_correct` | BOOLEAN | Se é a resposta correta |
| `order_index` | INT | Ordem de exibição |
| `created_at` | DATETIME | Criação |
| `updated_at` | DATETIME | Atualização |

**Regras:**
- A alternativa correta tem `is_correct = true`
- Nunca usar JSON para alternativas

**Índice:** `question_id`

---

### 5. `question_pairs`

Pares para exercícios de associação (match_pairs).

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `question_id` | CHAR(36) | FK → questions |
| `left_text` | VARCHAR(500) | Texto esquerdo (português) |
| `right_text` | VARCHAR(500) | Texto direito (inglês) |
| `left_audio_url` | VARCHAR(500) | Áudio esquerdo |
| `right_audio_url` | VARCHAR(500) | Áudio direito |
| `left_image_url` | VARCHAR(500) | Imagem esquerda |
| `right_image_url` | VARCHAR(500) | Imagem direita |
| `order_index` | INT | Ordem |
| `created_at` | DATETIME | Criação |
| `updated_at` | DATETIME | Atualização |

**Índice:** `question_id`

---

### 6. `user_question_answers`

Respostas individuais do aluno.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `user_id` | CHAR(36) | FK → users |
| `lesson_id` | CHAR(36) | FK → lessons |
| `question_id` | CHAR(36) | FK → questions |
| `selected_option_id` | CHAR(36) | FK → question_options (para múltipla escolha) |
| `text_answer` | VARCHAR(1000) | Resposta escrita (tradução) |
| `audio_url` | VARCHAR(500) | Áudio gravado (pronúncia) |
| `is_correct` | BOOLEAN | Se acertou |
| `time_spent_seconds` | INT | Tempo gasto na questão |
| `answered_at` | DATETIME | Quando respondeu |
| `created_at` | DATETIME | Criação |

**Uso por tipo de questão:**
| Tipo | Campo usado |
|------|-------------|
| multiple_choice, image_choice, listening_choice | `selected_option_id` |
| translation_pt, translation_en, complete_sentence | `text_answer` |
| pronunciation | `audio_url` |
| match_pairs | `is_correct` (validado no backend) |

**Índices:** `user_id`, `lesson_id`, `question_id`

---

### 7. `user_progress`

Progresso geral do aluno por lição.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `id` | CHAR(36) | PK |
| `user_id` | CHAR(36) | FK → users |
| `unit_id` | CHAR(36) | FK → units (opcional) |
| `block_id` | CHAR(36) | Bloco (opcional) |
| `lesson_id` | CHAR(36) | FK → lessons |
| `status` | VARCHAR(50) | locked, available, in_progress, completed |
| `score` | INT | Pontuação (0-100) |
| `correct_answers` | INT | Quantidade de acertos |
| `total_questions` | INT | Total de questões |
| `xp_earned` | INT | XP ganho |
| `completed_at` | DATETIME | Quando completou |
| `created_at` | DATETIME | Criação |
| `updated_at` | DATETIME | Atualização |

**Responsabilidade:** Apenas progresso geral. Respostas individuais ficam em `user_question_answers`.

---

## Relacionamentos (Foreign Keys)

```
lessons ──────────→ lesson_videos
lesson_videos ────→ video_learning_items
lesson_videos ────→ questions
video_learning_items → questions
questions ────────→ question_options
questions ────────→ question_pairs
users ────────────→ user_question_answers
lessons ──────────→ user_question_answers
questions ────────→ user_question_answers
question_options ─→ user_question_answers
```

---

## Exemplos de Inserção por Tipo

### multiple_choice

```sql
-- Questão
INSERT INTO questions (id, lesson_id, type, label, question_text, correct_answer, audio_text, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, 'multiple_choice', 'PALAVRA NOVA', 'Qual é a tradução de "coffee"?', 'café', 'coffee', 1, 'easy', NOW(), NOW());

-- Alternativas
INSERT INTO question_options (id, question_id, option_text, is_correct, order_index, created_at, updated_at) VALUES
(UUID(), @question_id, 'café', true, 1, NOW(), NOW()),
(UUID(), @question_id, 'chá', false, 2, NOW(), NOW()),
(UUID(), @question_id, 'água', false, 3, NOW(), NOW());
```

### image_choice

```sql
-- Questão
INSERT INTO questions (id, lesson_id, type, label, question_text, correct_answer, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, 'image_choice', 'PALAVRA NOVA', 'Qual destas imagens é "tea"?', 'tea', 2, 'easy', NOW(), NOW());

-- Alternativas com imagem/emoji
INSERT INTO question_options (id, question_id, option_text, option_image_url, is_correct, order_index, created_at, updated_at) VALUES
(UUID(), @question_id, 'coffee', '☕', false, 1, NOW(), NOW()),
(UUID(), @question_id, 'tea', '🍵', true, 2, NOW(), NOW()),
(UUID(), @question_id, 'milk', '🥛', false, 3, NOW(), NOW());
```

### translation_pt (traduzir para português)

```sql
INSERT INTO questions (id, lesson_id, type, label, question_text, correct_answer, audio_text, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, 'translation_pt', 'TRADUÇÃO', 'Escreva em português:', 'obrigado', 'thank you', 3, 'easy', NOW(), NOW());
```

### complete_sentence

```sql
INSERT INTO questions (id, lesson_id, type, label, question_text, correct_answer, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, 'complete_sentence', 'COMPLETE A FRASE', 'I ____ coffee every morning.', 'drink', 4, 'medium', NOW(), NOW());

-- Alternativas
INSERT INTO question_options (id, question_id, option_text, is_correct, order_index, created_at, updated_at) VALUES
(UUID(), @question_id, 'drink', true, 1, NOW(), NOW()),
(UUID(), @question_id, 'drinks', false, 2, NOW(), NOW()),
(UUID(), @question_id, 'drinking', false, 3, NOW(), NOW());
```

### match_pairs

```sql
INSERT INTO questions (id, lesson_id, type, label, question_text, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, 'match_pairs', 'VOCABULÁRIO', 'Combine os pares:', 5, 'easy', NOW(), NOW());

-- Pares
INSERT INTO question_pairs (id, question_id, left_text, right_text, order_index, created_at, updated_at) VALUES
(UUID(), @question_id, 'chá', 'tea', 1, NOW(), NOW()),
(UUID(), @question_id, 'olá', 'hello', 2, NOW(), NOW()),
(UUID(), @question_id, 'água', 'water', 3, NOW(), NOW());
```

### listening_choice

```sql
INSERT INTO questions (id, lesson_id, type, label, question_text, correct_answer, audio_text, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, 'listening_choice', 'ESCUTA', 'Toque no que escutar:', 'milk', 'milk', 6, 'easy', NOW(), NOW());

-- Alternativas
INSERT INTO question_options (id, question_id, option_text, is_correct, order_index, created_at, updated_at) VALUES
(UUID(), @question_id, 'hello', false, 1, NOW(), NOW()),
(UUID(), @question_id, 'milk', true, 2, NOW(), NOW()),
(UUID(), @question_id, 'water', false, 3, NOW(), NOW()),
(UUID(), @question_id, 'thanks', false, 4, NOW(), NOW());
```

### video_listening

```sql
INSERT INTO questions (id, lesson_id, video_id, type, label, question_text, correct_answer, audio_text, order_index, difficulty, created_at, updated_at)
VALUES (UUID(), @lesson_id, @video_id, 'video_listening', 'ESCUTA', 'Assista ao vídeo e toque na estrutura que ouviu:', 'milk', 'Can I have some milk please', 7, 'medium', NOW(), NOW());

-- Alternativas
INSERT INTO question_options (id, question_id, option_text, is_correct, order_index, created_at, updated_at) VALUES
(UUID(), @question_id, 'hello', false, 1, NOW(), NOW()),
(UUID(), @question_id, 'milk', true, 2, NOW(), NOW()),
(UUID(), @question_id, 'water', false, 3, NOW(), NOW()),
(UUID(), @question_id, 'thanks', false, 4, NOW(), NOW());
```

---

## Regras de Negócio

1. Cada lição deve ter **10 questões**
2. Variar os tipos (não repetir o mesmo tipo mais de 3x por lição)
3. O `order_index` vai de 1 a 10
4. A resposta correta fica em `correct_answer` (questão) e `is_correct = true` (opção)
5. Alternativas são linhas individuais em `question_options`
6. Pares são linhas individuais em `question_pairs`
7. Respostas do aluno são linhas individuais em `user_question_answers`
8. **Nunca usar JSON** para questões, alternativas ou respostas

---

## Estrutura Geral

```
units (10)
  └── lessons (5 por unidade)
       └── lesson_videos (1+ por lição)
       │    └── video_learning_items (N por vídeo)
       └── questions (10 por lição)
            ├── question_options (3-4 por questão)
            └── question_pairs (3-5 por questão match_pairs)

Total estimado: 10 × 5 × 10 = 500 questões
```

---

## Resumo

| O que mudou | Antes | Agora |
|-------------|-------|-------|
| Alternativas | JSON dentro de `options` | Tabela `question_options` (1 linha por alternativa) |
| Resposta correta | JSON em `correct_answers` | Coluna `correct_answer` + `is_correct` na opção |
| Pares (match) | JSON em `options` | Tabela `question_pairs` (1 linha por par) |
| Respostas do aluno | JSON em `given_answers` | Tabela `user_question_answers` (1 linha por resposta) |
| Tipo da questão | Enum C# | String no banco (`multiple_choice`, etc.) |
| Label | Campo `media_url` | Campo `label` |
| Vídeos | Não existia | Tabela `lesson_videos` com transcrição |
| Conteúdo extraído | Não existia | Tabela `video_learning_items` |
| Progresso | Básico | Inclui `correct_answers`, `total_questions`, `xp_earned` |

**Benefícios:**
- Consultas SQL simples para erros, estatísticas e relatórios
- Revisão automática baseada em respostas individuais
- Flashcards gerados a partir de erros reais
- IA personalizada com dados granulares
- Escalável para novos tipos de questão sem alterar schema
