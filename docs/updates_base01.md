# Refatoração da Estrutura de Vídeos, Questões e Respostas

## Objetivo

Refatorar completamente a estrutura atual do banco relacionada a:

- vídeos
- roteiros/transcrições
- questões
- alternativas
- respostas do aluno
- revisão
- extração de conteúdo por IA

A estrutura antiga baseada em JSON excessivo deve ser removida.

---

# Problemas da Estrutura Atual

A estrutura atual possui os seguintes problemas:

- alternativas armazenadas em JSON
- respostas corretas dentro de JSON
- campos genéricos como `options`
- campo `correct_answers` sem utilidade real
- `media_url` sendo usado como label
- difícil consulta para:
  - erros do aluno
  - estatísticas
  - revisão
  - IA
  - relatórios

---

# Novo Fluxo Ideal

```text
YouTube video
→ extrai roteiro/transcrição
→ salva roteiro em JSON
→ IA analisa roteiro
→ extrai objetos, palavras, frases e expressões
→ gera questões
→ salva questões em tabelas normais
→ aluno responde
→ sistema salva resposta individual
→ erros alimentam revisão/flashcards
```

---

# Regra Principal

## JSON deve existir apenas para:

- roteiro bruto/transcrição do vídeo

## Todo o restante deve usar tabelas relacionais normais.

---

# Estrutura Nova

---

# 1. lesson_videos

Tabela responsável pelos vídeos usados nas aulas.

## Campos

```sql
lesson_videos
- id
- lesson_id
- youtube_video_id
- youtube_url
- title
- transcript_json
- language
- duration_seconds
- created_at
- updated_at
```

## Regras

- transcript_json deve armazenar o roteiro bruto do vídeo.
- Cada item do roteiro deve possuir:
  - start
  - end
  - speaker
  - text

## Exemplo transcript_json

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

---

# 2. video_learning_items

Tabela responsável pelos conteúdos extraídos pela IA a partir da transcrição.

## Campos

```sql
video_learning_items
- id
- video_id
- item_type
- text_en
- text_pt
- category
- difficulty
- timestamp_start
- timestamp_end
- created_at
- updated_at
```

## item_type

```text
object
word
phrase
expression
grammar
```

## Exemplos

```text
object  | milk   | leite
word    | coffee | café
phrase  | Can I have some milk, please?
```

---

# 3. questions

Tabela principal de perguntas.

## Campos

```sql
questions
- id
- lesson_id
- video_id
- learning_item_id
- type
- label
- prompt
- instruction
- question_text
- correct_answer
- audio_text
- image_url
- order_index
- difficulty
- created_at
- updated_at
```

## Tipos possíveis

```text
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

## Regras

- correct_answer deve ficar em coluna própria.
- NÃO salvar resposta correta em JSON.
- NÃO salvar alternativas dentro da tabela questions.
- A tabela deve ser simples e facilmente consultável.

---

# 4. question_options

Tabela de alternativas das questões.

## Campos

```sql
question_options
- id
- question_id
- option_text
- option_image_url
- option_audio_url
- is_correct
- order_index
- created_at
- updated_at
```

## Regras

- Cada alternativa deve ser uma linha.
- A alternativa correta deve usar:

```text
is_correct = true
```

- NÃO utilizar JSON.

---

# 5. question_pairs

Tabela usada para exercícios de associação.

## Campos

```sql
question_pairs
- id
- question_id
- left_text
- right_text
- left_audio_url
- right_audio_url
- left_image_url
- right_image_url
- order_index
- created_at
- updated_at
```

---

# 6. user_question_answers

Tabela responsável por armazenar cada resposta individual do aluno.

## Campos

```sql
user_question_answers
- id
- user_id
- lesson_id
- question_id
- selected_option_id
- text_answer
- audio_url
- is_correct
- time_spent_seconds
- answered_at
- created_at
```

## Regras

### Múltipla escolha

Usar:

```text
selected_option_id
```

### Tradução / escrita

Usar:

```text
text_answer
```

### Pronúncia

Usar:

```text
audio_url
```

## Objetivo

Essa tabela será usada para:

- revisão automática
- flashcards
- análise de erros
- progresso
- IA personalizada
- relatórios
- métricas

---

# 7. user_progress

A tabela user_progress deve continuar existindo.

Porém:

## NÃO deve armazenar respostas individuais.

Ela deve armazenar apenas:

```sql
user_progress
- user_id
- unit_id
- block_id
- lesson_id
- status
- score
- correct_answers
- total_questions
- xp_earned
- completed_at
```

## Responsabilidade

- progresso da lição
- XP
- conclusão
- score geral

---

# Índices Obrigatórios

Criar índices para:

```sql
lesson_videos.lesson_id
video_learning_items.video_id
questions.lesson_id
questions.video_id
questions.learning_item_id
question_options.question_id
question_pairs.question_id
user_question_answers.user_id
user_question_answers.lesson_id
user_question_answers.question_id
```

---

# Foreign Keys

## Relacionamentos

```text
lessons → lesson_videos
lesson_videos → video_learning_items
lesson_videos → questions
video_learning_items → questions
questions → question_options
questions → question_pairs
users → user_question_answers
lessons → user_question_answers
questions → user_question_answers
question_options → user_question_answers
```

---

# Objetivo Final da Arquitetura

A nova estrutura precisa permitir:

- salvar vídeos do YouTube por aula
- salvar transcrição completa em JSON
- extrair palavras, objetos e frases do vídeo
- gerar perguntas automaticamente
- salvar alternativas fora do JSON
- salvar respostas do aluno individualmente
- saber exatamente o que o aluno errou
- gerar revisão automática
- criar flashcards
- alimentar IA personalizada
- montar relatórios
- medir dificuldade
- escalar o sistema futuramente

---

# Regra Final Importante

## JSON deve existir apenas para:

```text
transcript_json
```

## Questões, alternativas e respostas devem SEMPRE usar tabelas relacionais normais.

