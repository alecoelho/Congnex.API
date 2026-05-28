# Ajuste na Estratégia de Seleção e Exibição de Vídeos

## Problema Atual

O agente está selecionando vídeos muito longos (ex: 1 hora inteira), porém o aluno normalmente precisa estudar apenas uma estrutura específica presente dentro desse conteúdo.

Exibir o vídeo completo torna a experiência:

* cansativa,
* desproporcional,
* menos eficiente para microlearning,
* e prejudica o foco do aluno.

---

# Nova Estratégia Proposta

Em vez de exibir o vídeo inteiro, devemos trabalhar apenas com o trecho relevante.

## Fluxo Ideal

1. Buscar o vídeo normalmente.
2. Extrair a transcrição completa do vídeo.
3. Identificar o trecho relacionado à estrutura selecionada pelo agente.
4. Selecionar apenas esse intervalo.
5. Exibir somente o trecho relevante no player.

---

# Exemplo

## Cenário Atual

* Vídeo encontrado: 1 hora.
* Estrutura selecionada:

  * greetings básicos,
  * “How are you?”,
  * “Nice to meet you”.

Hoje:

* o sistema exibe o vídeo completo.

Problema:

* o aluno precisa procurar manualmente a parte útil.

---

## Novo Comportamento Esperado

A transcrição identifica:

* greetings entre 05:20 e 10:15.

O app então:

* exibe apenas esse trecho,
* inicia automaticamente no timestamp correto,
* e limita a reprodução ao intervalo relevante.

Resultado:

* estudo mais rápido,
* foco maior,
* experiência mais dinâmica.

---

# Regras Sugeridas

## Duração Ideal

Os trechos devem ter duração proporcional ao conteúdo estudado.

Sugestão:

* mínimo: 3 minutos
* ideal: 5–10 minutos
* máximo recomendado: 15 minutos

---

## Nunca Exibir Vídeos Longos Completos Automaticamente

Se o vídeo tiver:

* 30 minutos,
* 1 hora,
* 2 horas,

o sistema NÃO deve reproduzir o vídeo inteiro automaticamente.

---

# Estratégia de Seleção de Trecho

Se a estrutura aparecer várias vezes:

* selecionar o trecho:

  * mais claro,
  * mais objetivo,
  * com melhor contexto,
  * mais adequado ao nível do aluno.

---

# Possibilidades Técnicas

## 1. Transcript Timestamping

Utilizar timestamps da transcrição do YouTube.

---

## 2. Transcript Chunking

Dividir a transcrição em blocos:

* 30 segundos,
* 1 minuto,
* ou janelas semânticas.

---

## 3. Semantic Matching

Comparar:

* target_structures,
* learning goals,
* keywords,
* CEFR level,
* student motivations,
  com os chunks da transcrição.

---

## 4. Segment Scoring

Calcular score para cada trecho baseado em:

* densidade da estrutura,
* clareza,
* repetição útil,
* velocidade da fala,
* simplicidade do vocabulário,
* adequação ao nível A1/A2,
* relevância semântica.

---

## 5. Best Segment Selection

Selecionar:

* startTime,
* endTime,
* clipDuration,
* confidenceScore.

---

## 6. Smart Context Expansion

Se o trecho encontrado for muito curto:

* expandir alguns segundos/minutos antes e depois.

Exemplo:

* trecho encontrado: 40 segundos.
* sistema expande:

  * +1 minuto antes,
  * +1 minuto depois.

Isso evita cortes bruscos e melhora o contexto.

---

# Arquitetura Proposta

Video Search
→ Transcript Extraction
→ Transcript Chunking
→ Semantic Scoring
→ Best Segment Selection
→ Clip Playback

---

# Melhorias no Ranking de Vídeos

O ranking NÃO deve considerar apenas o vídeo inteiro.

Agora o score deve considerar:

* qualidade do trecho útil,
* quantidade de conteúdo relevante por minuto,
* clareza da fala,
* repetição educacional,
* velocidade da fala,
* adequação CEFR,
* densidade de estruturas úteis.

---

# Decisão Importante

Precisamos decidir entre duas abordagens:

## Opção 1 — Timestamp Playback

Mais simples:

* apenas iniciar o player no timestamp correto.

Exemplo:
youtube.com/watch?v=XXX&t=320s

Vantagens:

* simples,
* barato,
* escalável,
* sem processamento pesado.

Desvantagens:

* usuário ainda pode navegar no vídeo inteiro.

---

## Opção 2 — Real Clip Generation

Gerar clips reais do vídeo.

Vantagens:

* experiência mais limpa,
* foco total,
* controle completo.

Desvantagens:

* maior custo,
* processamento,
* armazenamento,
* possível complexidade legal.

---

# Recomendações

## MVP

Começar com:

* transcript chunking,
* semantic scoring,
* timestamp playback.

Porque:

* é barato,
* rápido,
* altamente escalável,
* e já resolve 80% do problema.

---

## Futuro

Adicionar:

* clip generation,
* cache de segmentos,
* ranking baseado em retenção,
* analytics por trecho,
* qualidade pedagógica do segmento.

---

# Perguntas em Aberto

Precisamos definir:

1. Qual duração mínima aceitável de um trecho?
2. O sistema pode juntar múltiplos trechos do mesmo vídeo?
3. Devemos priorizar Shorts primeiro?
4. O aluno pode expandir para assistir o vídeo completo?
5. Devemos salvar segmentos já analisados em cache?
6. Como lidar com vídeos sem transcript?
7. Vamos usar embeddings semânticos ou apenas keywords inicialmente?

---

# Objetivo Final

Transformar vídeos longos em experiências curtas, objetivas e altamente relevantes para microlearning.
