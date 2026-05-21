
# Melhorias no Fluxo de Nivelamento da Xyla

## Objetivo

Precisamos tornar o início da experiência da Xyla:
- mais simples
- mais natural
- menos intimidador
- mais preciso
- mais agradável para alunos iniciantes

Hoje o fluxo funciona tecnicamente, mas ainda parece um “teste”.

O ideal é que o aluno sinta que:
- está conversando com uma professora amigável
- não está sendo avaliado
- pode errar sem pressão
- consegue começar mesmo sem saber inglês

O foco principal deve ser:

> satisfação + confiança + continuidade no app

---

# Problema Atual

Hoje a Xyla:
- tenta detectar o nível rápido demais
- depende muito de respostas livres
- pede inglês cedo demais para alunos A1
- não possui validação progressiva
- pode gerar ansiedade no iniciante

---

# Nova Estratégia Recomendada

A Xyla NÃO deve começar tentando avaliar.

Ela deve começar:
- criando conforto
- criando conexão
- entendendo o aluno
- incentivando pequenas vitórias

O nivelamento deve acontecer de forma invisível durante a conversa.

---

# Fluxo Ideal da Entrevista

## Etapa 1 — Boas-vindas Humanizada

Objetivo:
- diminuir ansiedade
- gerar conforto
- explicar que não precisa saber inglês

### Exemplo

> “Oi João 😊
> Eu sou a Xyla e vou estudar inglês com você no seu ritmo.
> Você pode responder em português se quiser, tudo bem?”

---

## Etapa 2 — Descobrir Objetivo

Perguntas simples:
- trabalho
- viagem
- morar fora
- filmes
- jogos
- faculdade

### Exemplo

> “Por que você quer aprender inglês?”

---

## Etapa 3 — Descobrir Contexto

### Exemplo

> “Com o que você trabalha?”

ou

> “O que você gosta de fazer?”

---

## Etapa 4 — Micro Teste Invisível

O aluno NÃO deve perceber que está sendo testado.

---

# Estratégia Correta para Descobrir Nível

## NÃO usar:
- prova
- gramática
- perguntas difíceis
- “qual seu nível?”
- textos longos

## USAR:
- micro interações
- reconhecimento simples
- escolha
- áudio curto
- frases visuais

---

# Fluxo Recomendado de Nivelamento

## Nível 1 — Reconhecimento

```text
How are you?
```

Pergunta:

> “Você já viu essa frase antes?”

Botões:
- Sim
- Não
- Mais ou menos

---

## Nível 2 — Escolha Simples

```text
What does "Good morning" mean?
```

Opções:
- Boa noite
- Bom dia
- Obrigado

---

## Nível 3 — Vocabulário Visual

Imagem de carro:

```text
What is this?
```

Opções:
- engine
- banana
- chair

---

## Nível 4 — Mini Produção

Somente se o aluno estiver confortável.

```text
Como você diria "Meu nome é João" em inglês?
```

---

# Melhor Estratégia para A1

Para alunos iniciantes:
- evitar perguntas abertas em inglês logo no início
- permitir português sempre
- usar opções
- usar imagens
- usar reconhecimento antes de produção

---

# Melhorias Técnicas Recomendadas

## Adicionar campo em users

```sql
english_level VARCHAR(10)
```

## Adicionar confidence_score

```sql
level_confidence FLOAT
```

---

# Resultado Esperado

Após o novo fluxo:
- o aluno sente conforto
- a IA detecta o nível com mais precisão
- o aluno não trava
- o conteúdo fica mais personalizado
- a retenção melhora

---

# Resumo Final

A Xyla deve agir menos como:
- uma prova de inglês

E mais como:
- uma professora particular paciente e amigável.

O nivelamento ideal deve ser:
- invisível
- leve
- progressivo
- baseado em pequenas interações
- focado em confiança e experiência do usuário
