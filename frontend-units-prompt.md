# Prompt para o Kiro Frontend — Consumir Endpoint de Unidades e Lições

## Endpoint Disponível

```
GET /api/lessons/units
Authorization: Bearer <token>
```

## Response JSON (exemplo real do backend)

```json
{
  "success": true,
  "data": [
    {
      "id": "guid-da-unidade",
      "title": "Introdução",
      "description": "Aprenda frases básicas e saudações",
      "orderIndex": 1,
      "lessons": [
        {
          "id": "guid-da-licao",
          "title": "Lição 1",
          "orderIndex": 1,
          "xpReward": 10,
          "status": "current",
          "score": 0,
          "completedAt": null
        },
        {
          "id": "guid-da-licao-2",
          "title": "Lição 2",
          "orderIndex": 2,
          "xpReward": 10,
          "status": "locked",
          "score": 0,
          "completedAt": null
        },
        {
          "id": "guid-da-licao-3",
          "title": "Lição 3",
          "orderIndex": 3,
          "xpReward": 10,
          "status": "locked",
          "score": 0,
          "completedAt": null
        },
        {
          "id": "guid-da-licao-4",
          "title": "Lição 4",
          "orderIndex": 4,
          "xpReward": 10,
          "status": "locked",
          "score": 0,
          "completedAt": null
        },
        {
          "id": "guid-da-licao-5",
          "title": "Lição 5",
          "orderIndex": 5,
          "xpReward": 10,
          "status": "locked",
          "score": 0,
          "completedAt": null
        }
      ]
    },
    {
      "id": "guid-unidade-2",
      "title": "Comida & Bebida",
      "description": "Vocabulário de alimentos e restaurantes",
      "orderIndex": 2,
      "lessons": [...]
    }
  ]
}
```

## Campos Importantes

### Unit (Unidade)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | string (GUID) | ID único da unidade |
| `title` | string | Nome da unidade (ex: "Introdução", "Comida & Bebida") |
| `description` | string | Descrição curta da unidade |
| `orderIndex` | number | Ordem de exibição (1, 2, 3...) |
| `lessons` | array | Lista de lições dentro da unidade |

### Lesson (Lição)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | string (GUID) | ID único da lição |
| `title` | string | Nome da lição (ex: "Lição 1") |
| `orderIndex` | number | Ordem dentro da unidade |
| `xpReward` | number | XP ganho ao completar |
| `status` | string | `"current"` / `"locked"` / `"completed"` |
| `score` | number | Pontuação (0-100), 0 se não completou |
| `completedAt` | string? | ISO date ou null |

## Lógica de Status das Lições

- `"completed"` — Lição já finalizada pelo usuário (tem score e completedAt)
- `"current"` — Próxima lição disponível para o usuário (apenas UMA por vez)
- `"locked"` — Lição bloqueada (precisa completar as anteriores)

## Dados Reais no Banco (10 unidades, 5-6 lições cada)

O backend já tem seed com estas unidades:
1. **Introdução** — Aprenda frases básicas e saudações
2. **Comida & Bebida** — Vocabulário de alimentos e restaurantes
3. **Viagem** — Frases úteis para viajantes
4. **Família** — Membros da família e relacionamentos
5. **Trabalho** — Vocabulário profissional
6. **Compras** — Fazer compras e negociar
7. **Saúde** — Vocabulário médico e bem-estar
8. **Entretenimento** — Lazer, filmes, música
9. **Cultura** — Expressões culturais e costumes
10. **Avançado** — Tópicos complexos e fluência

Cada unidade tem **5 ou 6 lições** com `xpReward = 10`.

## Mapeamento para a Tela (baseado no screenshot)

```
┌─────────────────────────────────────────┐
│ UNIDADE {orderIndex}                    │
│ {title}                                 │
│ {description}                           │
│ ████████░░░░░░░░░░░░ {completedCount}/{totalLessons} │
└─────────────────────────────────────────┘

┌─────────────┐  ┌─────────────┐
│ {orderIndex}│  │ {orderIndex}│ 🔒
│ {title}     │  │ {title}     │
│ {completed}/{total} │  │ {completed}/{total} │
└─────────────┘  └─────────────┘
```

### Como calcular o progresso:
```typescript
// Progresso da unidade
const completedLessons = unit.lessons.filter(l => l.status === "completed").length;
const totalLessons = unit.lessons.length;
const progress = `${completedLessons}/${totalLessons}`;

// Unidade atual do usuário
const currentUnit = units.find(u => 
  u.lessons.some(l => l.status === "current")
);

// Unidades restantes
const remainingUnits = units.length - (currentUnit?.orderIndex ?? 1);
```

### Lógica de bloqueio visual:
- Unidade com pelo menos 1 lição `"current"` ou `"completed"` → **desbloqueada**
- Unidade onde TODAS as lições são `"locked"` → **bloqueada** (mostrar 🔒)

## Exemplo de Chamada (React Native / Expo)

```typescript
const response = await api.get('/api/lessons/units', {
  headers: { Authorization: `Bearer ${token}` }
});

const units: Unit[] = response.data.data;
```

## TypeScript Types

```typescript
interface Unit {
  id: string;
  title: string;
  description: string;
  orderIndex: number;
  lessons: Lesson[];
}

interface Lesson {
  id: string;
  title: string;
  orderIndex: number;
  xpReward: number;
  status: 'current' | 'locked' | 'completed';
  score: number;
  completedAt: string | null;
}
```

## Notas
- O endpoint requer autenticação (Bearer token)
- As unidades já vêm ordenadas por `orderIndex`
- As lições dentro de cada unidade já vêm ordenadas
- O backend calcula automaticamente qual lição é "current" baseado no progresso do usuário
- Não precisa de paginação — são apenas 10 unidades
