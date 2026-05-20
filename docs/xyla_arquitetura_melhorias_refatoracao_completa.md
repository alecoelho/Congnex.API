# Refatoração Completa da Arquitetura do Agente Xyla

# Objetivo

Refatorar profundamente a arquitetura atual do agente Xyla.

A análise técnica identificou diversos problemas estruturais, gargalos de performance e limitações de escalabilidade.

O objetivo é transformar o Xyla em:

- arquitetura modular
- pipeline de aprendizado
- IA adaptativa
- sistema escalável
- plataforma preparada para múltiplos agentes
- sistema baseado em contexto real
- aprendizado personalizado

---

# Problemas Identificados

A implementação atual possui:

- streaming falso
- IMemoryCache volátil
- prompts hardcoded
- busca de vídeos frágil
- motivations sempre null
- agente fazendo responsabilidades demais
- ausência de pipeline de conteúdo
- ausência de memória real de aprendizado
- ausência de persistência da conversa
- crescimento desorganizado do contexto
- alto acoplamento
- difícil escalabilidade

---

# 1. Implementar Streaming Real

# Problema Atual

Hoje o backend:

- espera a resposta completa da IA
- divide em palavras manualmente
- envia depois ao frontend

Isso gera:

- alta latência
- falsa sensação de streaming
- experiência ruim

---

# Nova Implementação Obrigatória

Implementar streaming token-by-token.

Fluxo esperado:

```text
Azure/OpenAI
↓
token recebido
↓
enviar imediatamente
↓
frontend renderiza instantaneamente
```

---

# Regras

- NÃO esperar resposta completa
- NÃO dividir texto manualmente
- enviar tokens conforme chegam
- manter typing animation
- implementar cancelamento seguro
- implementar timeout seguro

---

# 2. Substituir IMemoryCache por Redis

# Problema Atual

Hoje as sessões usam:

```text
IMemoryCache
```

Problemas:

- perde sessões ao reiniciar
- não escala horizontalmente
- não funciona com múltiplas instâncias

---

# Nova Implementação Obrigatória

Migrar sessões para Redis.

---

# Redis Deve Armazenar

- ChatHistory
- estado da entrevista
- contexto do usuário
- nível detectado
- vídeo escolhido
- learning items temporários
- progresso da entrevista

---

# Regras

- TTL configurável
- serialização segura
- limpeza automática
- preparado para múltiplos servidores

---

# 3. Separar Prompts em Arquivos

# Problema Atual

Prompts hardcoded no código.

Exemplo:

```text
BuildAgentInstructions()
```

---

# Nova Estrutura Obrigatória

```text
/prompts
  xyla-system.md
  xyla-interview.md
  xyla-video-selection.md
  xyla-learning-items.md
  xyla-question-generation.md
  xyla-review-generation.md
```

---

# Benefícios

- edição sem deploy
- versionamento
- A/B testing
- prompts menores
- prompts modulares
- manutenção simples

---

# 4. Corrigir motivations

# Problema Atual

O campo:

```text
motivations
```

está sempre null.

Isso quebra a personalização.

---

# Implementação Obrigatória

Garantir:

- persistência correta
- leitura correta
- envio correto ao prompt
- uso correto na seleção do vídeo

---

# 5. Criar Pipeline Modular

# Problema Atual

Hoje o Xyla faz:

- conversa
- detecta nível
- busca vídeo
- gera plano
- salva banco
- processa streaming
- chama APIs externas
- gera questões

Tudo junto.

---

# Nova Arquitetura Obrigatória

```text
Xyla Interview Agent
↓
Profile Analyzer
↓
Video Recommendation Service
↓
Transcript Processor
↓
Learning Item Extractor
↓
Question Generator
↓
Review Generator
↓
Flashcard Generator
```

---

# Regras

Cada módulo deve possuir:

- responsabilidade única
- interfaces claras
- baixa dependência
- fácil substituição
- fácil manutenção
- desacoplamento

---

# 6. Remover Brave Search como Fonte Principal

# Problema Atual

Hoje:

```text
Brave Search
→ primeiro vídeo encontrado
```

Problemas:

- vídeos ruins
- vídeos deletados
- vídeos irrelevantes
- conteúdo inadequado
- baixa previsibilidade

---

# Nova Estratégia Obrigatória

Criar tabela:

```text
curated_videos
```

---

# Estrutura Sugerida

```sql
curated_videos
- id
- youtube_video_id
- youtube_url
- title
- level
- category
- motivation
- min_age
- max_age
- speech_speed
- duration_seconds
- transcript_available
- is_active
- created_at
```

---

# Novo Fluxo

```text
Xyla
↓
gerar intenção/contexto
↓
VideoRecommendationService
↓
busca vídeos curados
↓
escolhe melhor vídeo
```

---

# Benefícios

- controle de qualidade
- vídeos confiáveis
- menor custo
- menor latência
- previsibilidade
- melhor experiência

---

# 7. Implementar Pipeline de Conteúdo

Fluxo obrigatório:

```text
Vídeo
↓
transcrição
↓
transcript_json
↓
IA analisa
↓
extração de learning items
↓
video_learning_items
↓
geração de questões
↓
questions
↓
respostas do aluno
↓
user_question_answers
↓
análise de erros
↓
review
↓
flashcards
```

---

# 8. Implementar Memória de Aprendizado

O Xyla deve começar a utilizar:

```text
user_question_answers
```

para personalização.

---

# A IA Deve Saber

- palavras que o aluno erra
- temas com dificuldade
- tipos de exercício com dificuldade
- retenção
- velocidade de aprendizado
- progresso

---

# Objetivo

Transformar o sistema em:

```text
IA adaptativa real
```

---

# 9. Persistir Conversas

Criar:

```text
xyla_conversations
xyla_messages
```

---

# Objetivo

Permitir:

- analytics
- retomada de conversa
- métricas
- histórico
- debugging
- treinamento futuro

---

# 10. Implementar Cache de Dados do Usuário

# Problema Atual

Banco consultado a cada mensagem.

---

# Nova Implementação Obrigatória

Buscar dados apenas:

- no início da sessão

Depois:

- armazenar no Redis
- reutilizar durante toda a sessão

---

# 11. Implementar Validação de Vídeos

Antes de utilizar qualquer vídeo:

Validar:

- disponibilidade
- duração
- idioma
- presença de diálogo
- transcript disponível
- qualidade mínima

---

# 12. Implementar Observabilidade

Adicionar métricas para:

- tempo médio de resposta
- tempo médio da entrevista
- taxa de conclusão
- vídeos mais usados
- questões mais erradas
- custo médio por conversa
- latência
- falhas da IA

---

# 13. Objetivo Final da Arquitetura

A nova arquitetura deve permitir:

- múltiplos agentes
- IA adaptativa
- vídeos personalizados
- revisão inteligente
- flashcards automáticos
- pipeline de conteúdo
- escalabilidade horizontal
- baixa latência
- prompts modulares
- aprendizado contextual
- forte personalização

---

# Regras Finais

- NÃO implementar soluções temporárias
- NÃO manter responsabilidades acopladas
- NÃO criar lógica hardcoded

Toda a arquitetura deve ser:

- modular
- desacoplada
- escalável
- preparada para crescimento
- orientada a pipeline
- fácil de manter
- preparada para IA adaptativa

