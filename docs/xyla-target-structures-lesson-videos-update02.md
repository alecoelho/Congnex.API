# Evolução do Fluxo da Xyla: Estruturas-Alvo para Busca de Vídeos

## Objetivo

Precisamos evoluir o fluxo de geração de conteúdo da Xyla.

Antes de buscar vídeos no YouTube, a agente Xyla deve primeiro definir as estruturas em inglês mais importantes para o aluno estudar naquela unidade.

Essas estruturas serão utilizadas para:

- Buscar vídeos relevantes
- Gerar exercícios
- Criar diálogos
- Gerar atividades de listening
- Criar roleplays
- Reforçar repetição contextual

---

## Banco de Dados

Adicionar um novo campo na tabela `lesson_videos` para armazenar até 5 estruturas principais da unidade.

### Sugestão de campo

```sql
target_structures JSON NULL
```

### Exemplo de conteúdo

```json
[
  "I need to change the oil.",
  "The battery is dead.",
  "Can you open the hood?",
  "The engine is making noise.",
  "Your brakes are worn."
]
```

---

## Fluxo Correto da Xyla

### 1. Analisar o perfil do aluno

A Xyla deve considerar:

- Nível de inglês
- Profissão
- Objetivos
- Interesses
- Rotina
- Contexto do dia a dia

### Exemplo

Aluno:

- Profissão: mecânico
- Nível: A1
- Objetivo: aprender frases úteis para falar com clientes e entender situações simples na oficina

---

### 2. Definir as estruturas ideais

A Xyla deve escolher de 3 a 5 estruturas principais por unidade.

As estruturas devem ser:

- Úteis no mundo real
- Simples para iniciantes
- Fáceis de visualizar
- Altamente repetíveis
- Contextualizadas com a vida do aluno
- Apropriadas para o nível de inglês do aluno

### Importante

Para alunos A1, não focar em gramática complexa.

O foco deve ser:

- Comunicação
- Compreensão
- Vocabulário útil
- Repetição natural
- Confiança para falar

---

## Regra Pedagógica

Para alunos A1:

- Usar apenas 3 a 5 estruturas por unidade
- Repetir as mesmas estruturas nos 5 blocos
- Evitar excesso de frases novas
- Priorizar profundidade ao invés de quantidade

A unidade deve trabalhar as mesmas estruturas em:

- Vídeo
- Exercícios
- Diálogos
- Shadowing
- Roleplay
- Revisões futuras

---

## 3. Salvar as estruturas

Após definir as estruturas, salvar no campo:

```sql
lesson_videos.target_structures
```

### Exemplo

```json
[
  "I need to change the oil.",
  "The battery is dead.",
  "Can you open the hood?",
  "The engine is making noise."
]
```

---

## 4. Buscar vídeos no YouTube

Somente depois de definir as estruturas, a Xyla deve buscar vídeos no YouTube.

A busca deve priorizar vídeos que contenham:

- As frases exatas
- Ou frases semanticamente semelhantes
- Contexto visual forte
- Fala clara
- Situações reais
- Linguagem simples
- Trechos curtos de 10 a 30 segundos

---

## Critérios para Seleção de Vídeo

O vídeo ideal deve ter:

- Fala clara
- Contexto visual fácil de entender
- Vocabulário relacionado ao objetivo do aluno
- Trechos curtos
- Situação real ou semi-real
- Frases compatíveis com as estruturas-alvo

### Para aluno mecânico A1

Exemplo de estruturas:

```json
[
  "I need to change the oil.",
  "The battery is dead.",
  "Can you open the hood?",
  "The engine is making noise.",
  "Your brakes are worn."
]
```

---

## Estrutura da Unidade

Cada unidade possui:

- 1 tema principal
- 5 blocos
- 10 questões por bloco

### Exemplo de unidade

Tema:

```text
At the Auto Repair Shop
```

### Blocos

1. Listening
2. Vocabulary
3. Conversation
4. Pronunciation
5. Real Life Mission

---

## Distribuição dos Blocos

### Bloco 1 — Listening

Objetivo:

- O aluno escuta o trecho do vídeo
- Identifica palavras e frases principais
- Reconhece as estruturas-alvo

Tipos de questões:

- Multiple choice
- Complete the sentence
- Listen and choose
- True or false

---

### Bloco 2 — Vocabulary

Objetivo:

- Ensinar palavras relacionadas às estruturas

Exemplo:

- oil
- battery
- hood
- engine
- brakes
- noise

Tipos de questões:

- Match word and image
- Translate
- Choose the correct word
- Fill in the blank

---

### Bloco 3 — Conversation

Objetivo:

- Criar mini diálogos usando as estruturas-alvo

Exemplo:

```text
Customer: The engine is making noise.
Mechanic: Can you open the hood?
```

Tipos de questões:

- Organize the dialogue
- Choose the best answer
- Complete the conversation

---

### Bloco 4 — Pronunciation

Objetivo:

- Repetição
- Shadowing
- Treino de fala

Tipos de atividades:

- Listen and repeat
- Record your voice
- Repeat slowly
- Compare pronunciation

---

### Bloco 5 — Real Life Mission

Objetivo:

- Simulação prática com IA

Exemplo:

A Xyla faz o papel de cliente, e o aluno responde como mecânico.

Situação:

```text
The customer says: "The engine is making noise."
The student should answer: "Can you open the hood?"
```

---

## Resultado Esperado

A Xyla deve se comportar como uma professora particular inteligente.

Ela deve:

- Entender o perfil do aluno
- Escolher estruturas úteis
- Salvar essas estruturas no banco
- Buscar vídeos compatíveis
- Criar atividades baseadas nas estruturas
- Reforçar o aprendizado com repetição contextual

---

## Princípio Principal

A Xyla não deve buscar vídeos antes de saber exatamente o que o aluno precisa estudar.

O fluxo correto é:

```text
Perfil do aluno
→ Definir estruturas-alvo
→ Salvar estruturas
→ Buscar vídeo
→ Criar exercícios
→ Reforçar com roleplay
```

---

## Observação Final

O objetivo não é ensinar muitas frases.

O objetivo é fazer o aluno dominar poucas estruturas, mas conseguir usá-las com confiança em situações reais.
