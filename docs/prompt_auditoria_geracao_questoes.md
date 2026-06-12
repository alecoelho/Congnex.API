# Prompt para Auditoria do Sistema de Geração de Questões

Antes de alterar a dificuldade das questões, precisamos entender exatamente como o sistema atual está funcionando.

Analise detalhadamente TODO o pipeline de geração de questões e explique o comportamento atual do sistema.

Quero um diagnóstico técnico completo.

---

## 1. Fluxo Completo

Mostre passo a passo:

- onde as questões são geradas;
- qual serviço/agente é responsável;
- qual prompt está sendo usado;
- quais dados entram na geração;
- quais dados são retornados;
- como as alternativas são criadas;
- como a resposta correta é definida;
- como a dificuldade é escolhida.

---

## 2. Estrutura Atual das Questões

Explique:

- quais tipos de questão existem;
- quais campos cada questão possui;
- como as alternativas são armazenadas;
- como o frontend renderiza cada tipo;
- quais tipos estão realmente sendo usados atualmente.

Exemplos possíveis:

- multiple_choice
- image_choice
- listening
- reorder_sentence
- fill_blank
- etc.

---

## 3. Pipeline de Geração

Explique detalhadamente o fluxo:

```text
Video
→ transcript
→ extraction
→ structures
→ AI generation
→ parsing
→ database
→ frontend
```

Quero entender exatamente:

- em qual etapa a IA gera perguntas;
- onde os distratores são criados;
- onde o nível da questão é definido;
- se existe validação;
- se existe pós-processamento.

---

## 4. Prompt Atual

Mostre:

- o prompt completo usado para gerar questões;
- system prompt;
- user prompt;
- qualquer template utilizado.

Explique:

- quais instruções influenciam dificuldade;
- quais instruções influenciam alternativas;
- quais instruções influenciam CEFR.

---

## 5. Análise de Dificuldade

Analise tecnicamente por que as questões estão fáceis.

Identifique:

- distratores fracos;
- alternativas absurdas;
- perguntas literais;
- baixa diversidade;
- repetição de padrões;
- excesso de tradução direta;
- falta de contexto;
- falta de inferência;
- falta de ambiguidade controlada.

Inclua exemplos reais encontrados no código, prompt ou dados salvos.

---

## 6. Sistema de Alternativas

Explique exatamente como as alternativas erradas são geradas.

Quero saber:

- são aleatórias?
- são geradas pela IA?
- são semanticamente parecidas?
- existe scoring?
- existe validação de qualidade?
- existe proteção contra alternativas absurdas?

---

## 7. CEFR / Difficulty

Explique:

- como o sistema decide easy / medium / hard;
- se existe lógica real ou apenas labels;
- se a dificuldade altera:
  - vocabulário;
  - contexto;
  - distratores;
  - tamanho da frase;
  - velocidade;
  - inferência.

---

## 8. Dados Salvos

Mostre:

- tabelas envolvidas;
- entidades;
- JSONs;
- campos importantes;
- exemplos reais salvos no banco.

---

## 9. Frontend

Explique:

- como as questões são renderizadas;
- quais componentes existem;
- quais limitações o frontend possui;
- se o frontend suporta novos tipos de questões.

---

## 10. Gargalos e Limitações

Liste:

- problemas arquiteturais;
- limitações atuais;
- riscos;
- partes frágeis;
- problemas de escalabilidade;
- problemas de qualidade pedagógica.

---

## 11. Melhorias Recomendadas

Somente após analisar tudo:

- sugira melhorias;
- priorize por impacto;
- diferencie:
  - MVP;
  - médio prazo;
  - avançado.

---

## Importante

Não implemente nada ainda.

Primeiro faça uma auditoria completa do sistema atual para entendermos:

- o comportamento real;
- os gargalos;
- e a origem do problema de dificuldade baixa.

Se necessário, leia:

- prompts;
- services;
- entities;
- repositories;
- DTOs;
- queries;
- handlers;
- frontend components;
- AI agents;
- e qualquer código relacionado à geração de questões.
