# Ajuste MVP — Melhorar Qualidade e Dificuldade das Questões

Com base na auditoria, implemente primeiro apenas melhorias no prompt e validação básica, sem criar novos tipos de questão ainda.

## Objetivo

Melhorar a qualidade das questões atuais mantendo os tipos já usados:

- multiple_choice
- complete_sentence

Não implementar image_choice, listening_choice ou outros tipos neste momento.

---

# Problemas a corrigir

Atualmente as questões estão fáceis porque:

- todos os distratores são fracos;
- quase todas as perguntas são tradução direta;
- todas saem como easy;
- não existe progressão entre blocos;
- o transcript quase não é usado;
- as alternativas erradas são muito óbvias.

---

# Implementar no MVP

## 1. Melhorar o prompt de geração

Atualizar `BuildQuestionGenerationPrompt()` para exigir:

- distratores semanticamente próximos;
- alternativas da mesma categoria;
- perguntas com contexto simples;
- progressão de dificuldade por bloco;
- uso real do transcript;
- variedade entre tradução, completar frase e situação prática.

---

## 2. Nova regra de dificuldade

Gerar 10 questões por lição com esta distribuição:

- questões 1–3: easy
- questões 4–7: medium
- questões 8–10: hard

### Easy

Pode usar tradução simples, mas com distratores próximos.

Exemplo:

Pergunta:
Como se diz "passport" em português?

Opções:
- passaporte
- passagem
- mala
- documento

### Medium

Usar contexto simples.

Exemplo:

Pergunta:
Você está no aeroporto e precisa mostrar seu documento. Qual palavra combina melhor?

Opções:
- passport
- boarding
- suitcase
- ticket

### Hard

Usar frases, intenção ou erro comum.

Exemplo:

Pergunta:
Qual frase está mais correta para perguntar se alguém fala inglês?

Opções:
- Do you speak English?
- You speak English?
- Are you speak English?
- Do you speaks English?

---

## 3. Regras para distratores

As alternativas erradas devem ser plausíveis.

Não aceitar alternativas absurdas.

Exemplo ruim:

Book:
- book
- chair
- pizza
- car

Exemplo bom:

Book:
- book
- notebook
- page
- library

Os distratores devem ser:

- da mesma categoria;
- parecidos semanticamente;
- possíveis erros de aluno brasileiro;
- frases com pequenas diferenças gramaticais;
- nunca palavras aleatórias.

---

## 4. Usar transcript real

As questões devem usar palavras/frases presentes no transcript sempre que possível.

Priorizar:

- palavras repetidas no trecho;
- frases curtas do vídeo;
- estruturas selecionadas pelo agente;
- vocabulário diretamente relacionado ao contexto do vídeo.

---

## 5. Adicionar validação pós-geração simples

Depois que a IA gerar as questões, validar:

- se `correctAnswer` está dentro de `options`;
- se existem 4 opções;
- se as opções não são idênticas;
- se os distratores não são absurdamente curtos ou aleatórios;
- se existe pelo menos uma questão medium e uma hard;
- se todas as questões não ficaram como easy.

Se falhar, logar o motivo e tentar regenerar uma vez.

---

## 6. Não alterar ainda

Não implementar neste momento:

- novos tipos de questão;
- image_choice;
- listening_choice;
- match_pairs;
- adaptive learning;
- spaced repetition;
- mudanças grandes no frontend.

---

# Resultado esperado

Depois da implementação, as questões devem ficar assim:

- menos óbvias;
- com alternativas mais parecidas;
- com progressão real;
- mais ligadas ao vídeo;
- com contexto simples;
- ainda adequadas para A1/A2.

---

# Entregáveis

Ao final, explique:

1. quais arquivos foram alterados;
2. como o prompt ficou;
3. como a difficulty agora é definida;
4. quais validações foram adicionadas;
5. exemplos de questões antes/depois;
6. como testar no banco e no frontend.

Não implemente além deste MVP.
