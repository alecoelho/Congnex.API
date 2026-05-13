# Congnex API — Documentação dos Endpoints

**Base URL:** `http://localhost:5190/api`  
**Swagger UI:** `http://localhost:5190/swagger`

Todos os endpoints retornam o formato padrão:

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

Em caso de erro:

```json
{
  "success": false,
  "data": null,
  "error": "Mensagem de erro"
}
```

---

## 🔐 Autenticação (`/api/auth`)

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/auth/register` | ❌ | Criar nova conta |
| POST | `/api/auth/login` | ❌ | Login com email/senha |
| POST | `/api/auth/google` | ❌ | Login com Google (ID Token) |
| POST | `/api/auth/google-code` | ❌ | Login com Google (Authorization Code) |
| POST | `/api/auth/google-token` | ❌ | Login com Google (Access Token) |
| POST | `/api/auth/apple` | ❌ | Login com Apple |
| POST | `/api/auth/refresh` | ❌ | Renovar access token |
| POST | `/api/auth/logout` | ✅ | Encerrar sessão |

---

### POST `/api/auth/register`

Cria uma nova conta de usuário.

**Request Body:**
```json
{
  "firstName": "jose",
  "lastName": "Silva",
  "email": "jose@email.com",
  "password": "Minhasenha01@@"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "abc123...",
    "expiresIn": 3600,
    "userId": "guid",
    "firstName": "João",
    "lastName": "Silva",
    "email": "joao@email.com",
    "plan": "free"
  }
}
```

**Erros:**
- `409 Conflict` — Email já cadastrado

---

### POST `/api/auth/login`

Login com email e senha.

**Request Body:**
```json
{
  "email": "joao@email.com",
  "password": "minhasenha123"
}
```

**Response (200):** Mesmo formato do register (AuthResult)

**Erros:**
- `401 Unauthorized` — Credenciais inválidas

---

### POST `/api/auth/google`

Autenticação via Google usando ID Token (obtido do Google Sign-In no mobile).

**Request Body:**
```json
{
  "idToken": "eyJhbGciOi..."
}
```

**Response (200):** AuthResult

**Erros:**
- `401 Unauthorized` — Token inválido

---

### POST `/api/auth/google-code`

Autenticação via Google usando Authorization Code (fluxo web).

**Request Body:**
```json
{
  "code": "4/0AX4XfWh...",
  "redirectUri": "http://localhost:3000/callback"
}
```

**Response (200):** AuthResult

**Erros:**
- `401 Unauthorized` — Code inválido

---

### POST `/api/auth/google-token`

Autenticação via Google usando Access Token.

**Request Body:**
```json
{
  "accessToken": "ya29.a0AfH6SM..."
}
```

**Response (200):** AuthResult

**Erros:**
- `401 Unauthorized` — Token inválido

---

### POST `/api/auth/apple`

Autenticação via Apple Sign-In.

**Request Body:**
```json
{
  "idToken": "eyJhbGciOi...",
  "fullName": "João Silva"
}
```

> `fullName` é opcional — Apple só envia o nome na primeira autenticação.

**Response (200):** AuthResult

**Erros:**
- `401 Unauthorized` — Token inválido

---

### POST `/api/auth/refresh`

Renova o access token usando um refresh token válido.

**Request Body:**
```json
{
  "refreshToken": "abc123..."
}
```

**Response (200):** AuthResult (com novos tokens)

**Erros:**
- `401 Unauthorized` — Refresh token expirado ou inválido

---

### POST `/api/auth/logout`

Encerra a sessão do usuário (invalida o refresh token).

**Headers:** `Authorization: Bearer <access_token>`

**Response (200):**
```json
{
  "success": true,
  "error": null
}
```

---

## 📚 Lições (`/api/lessons`)

> Todos os endpoints requerem autenticação (`Authorization: Bearer <token>`)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/lessons/units` | Listar unidades e lições |
| GET | `/api/lessons/{lessonId}/questions` | Obter questões de uma lição |
| POST | `/api/lessons/{lessonId}/complete` | Completar uma lição |

---

### GET `/api/lessons/units`

Retorna todas as unidades com suas lições e o progresso do usuário.

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "title": "Fundamentos",
      "description": "Aprenda os conceitos básicos",
      "orderIndex": 1,
      "lessons": [
        {
          "id": "guid",
          "title": "Introdução",
          "orderIndex": 1,
          "xpReward": 10,
          "status": "completed",
          "score": 95,
          "completedAt": "2025-01-15T10:30:00Z"
        },
        {
          "id": "guid",
          "title": "Variáveis",
          "orderIndex": 2,
          "xpReward": 15,
          "status": "current",
          "score": 0,
          "completedAt": null
        }
      ]
    }
  ]
}
```

> `status` pode ser: `"locked"`, `"current"` ou `"completed"`

---

### GET `/api/lessons/{lessonId}/questions`

Retorna as questões de uma lição específica.

**Parâmetros:**
- `lessonId` (GUID) — ID da lição na URL

**Response (200):**
```json
{
  "success": true,
  "data": [ ... ]
}
```

**Erros:**
- `404 Not Found` — Lição não encontrada

---

### POST `/api/lessons/{lessonId}/complete`

Registra a conclusão de uma lição com as respostas do usuário.

**Parâmetros:**
- `lessonId` (GUID) — ID da lição na URL

**Request Body:**
```json
{
  "score": 85,
  "answers": [
    {
      "questionId": "guid",
      "givenAnswers": ["resposta1"],
      "isCorrect": true
    },
    {
      "questionId": "guid",
      "givenAnswers": ["resposta_errada"],
      "isCorrect": false
    }
  ]
}
```

**Response (200):** Resultado da conclusão (XP ganho, progresso atualizado)

**Erros:**
- `404 Not Found` — Lição não encontrada

---

## 🔔 Notificações (`/api/notifications`)

> Todos os endpoints requerem autenticação

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/notifications/device` | Registrar dispositivo para push |
| DELETE | `/api/notifications/device/{registrationId}` | Remover dispositivo |

---

### POST `/api/notifications/device`

Registra um dispositivo para receber notificações push (via Azure Notification Hubs).

**Request Body:**
```json
{
  "token": "fcm_or_apns_device_token",
  "platform": "iOS"
}
```

> `platform` aceita: `"iOS"` ou `"Android"`

**Response (200):**
```json
{
  "success": true,
  "data": {
    "registrationId": "abc123..."
  }
}
```

**Erros:**
- `400 Bad Request` — Plataforma inválida

---

### DELETE `/api/notifications/device/{registrationId}`

Remove o registro de um dispositivo (para de receber push).

**Parâmetros:**
- `registrationId` (string) — ID retornado no registro

**Response (200):**
```json
{
  "success": true,
  "error": null
}
```

---

## 💳 Pagamentos (`/api/payments`)

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/payments/checkout` | ✅ | Criar sessão de checkout Stripe |
| POST | `/api/payments/cancel` | ✅ | Cancelar assinatura |
| POST | `/api/payments/reactivate` | ✅ | Reativar assinatura cancelada |
| GET | `/api/payments/subscription` | ✅ | Ver status da assinatura |
| POST | `/api/payments/portal` | ✅ | Abrir portal do cliente Stripe |
| POST | `/api/payments/webhook` | ❌ | Webhook do Stripe |

---

### POST `/api/payments/checkout`

Cria uma sessão de checkout do Stripe para assinar o plano Super.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "checkoutUrl": "https://checkout.stripe.com/c/pay/..."
  }
}
```

**Erros:**
- `409 Conflict` — Usuário já possui assinatura ativa

---

### POST `/api/payments/cancel`

Cancela a assinatura (acesso continua até o fim do período).

**Response (200):**
```json
{
  "success": true,
  "data": {
    "cancelAtPeriodEnd": true,
    "cancelAt": "2025-02-15T00:00:00Z",
    "message": "Subscription will cancel at period end. Access continues until then."
  }
}
```

**Erros:**
- `404 Not Found` — Nenhuma assinatura encontrada
- `409 Conflict` — Assinatura já cancelada

---

### POST `/api/payments/reactivate`

Reativa uma assinatura que foi cancelada mas ainda está no período ativo.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "cancelAtPeriodEnd": false,
    "message": "Subscription reactivated. It will renew automatically."
  }
}
```

**Erros:**
- `404 Not Found` — Nenhuma assinatura encontrada
- `409 Conflict` — Assinatura não está em estado cancelável

---

### GET `/api/payments/subscription`

Retorna o status atual da assinatura do usuário.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "plan": "super",
    "status": "active",
    "cancelAtPeriodEnd": false,
    "cancelAt": null,
    "currentPeriodEnd": "2025-02-15T00:00:00Z",
    "renewsAt": "2025-02-15T00:00:00Z"
  }
}
```

> `plan` pode ser: `"free"` ou `"super"`  
> `status` pode ser: `"trialing"`, `"active"`, `"past_due"`, `"canceled"`, `"unpaid"`

---

### POST `/api/payments/portal`

Cria uma sessão do Customer Portal do Stripe (gerenciar pagamento, faturas, etc).

**Response (200):**
```json
{
  "success": true,
  "data": {
    "portalUrl": "https://billing.stripe.com/p/session/..."
  }
}
```

**Erros:**
- `404 Not Found` — Nenhuma assinatura/customer encontrado

---

### POST `/api/payments/webhook`

Endpoint chamado pelo Stripe para notificar eventos (pagamento confirmado, assinatura cancelada, etc).

> Este endpoint é público (sem autenticação). A validação é feita via header `Stripe-Signature`.

**Headers:**
- `Stripe-Signature` — Assinatura do webhook

**Request Body:** JSON raw do evento Stripe

**Response:**
- `200 OK` — Evento processado
- `400 Bad Request` — Assinatura inválida

---

## 📖 Revisão Espaçada (`/api/review`)

> Todos os endpoints requerem autenticação. Usa o algoritmo FSRS para repetição espaçada.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/review/due` | Obter itens pendentes de revisão |
| POST | `/api/review/submit` | Submeter resultado de revisão |

---

### GET `/api/review/due`

Retorna os itens que estão pendentes de revisão (due date ≤ agora).

**Query Parameters:**
- `limit` (int, opcional, default: 20) — Quantidade máxima de itens

**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "source": "lesson",
      "prompt": "Como se diz 'hello' em português?",
      "options": "[\"Olá\", \"Tchau\", \"Obrigado\", \"Por favor\"]",
      "correctAnswers": ["Olá"],
      "dueDate": "2025-01-15T10:00:00Z",
      "reps": 3,
      "state": "review"
    }
  ]
}
```

> `state` pode ser: `"new"`, `"learning"`, `"review"`, `"relearning"`

---

### POST `/api/review/submit`

Submete o resultado de uma revisão (o algoritmo FSRS calcula a próxima data).

**Request Body:**
```json
{
  "reviewItemId": "guid",
  "rating": 3
}
```

> `rating` de 1 a 4:
> - `1` = Again (errou completamente)
> - `2` = Hard (difícil, quase errou)
> - `3` = Good (acertou normalmente)
> - `4` = Easy (muito fácil)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "reviewItemId": "guid",
    "nextDueDate": "2025-01-18T10:00:00Z",
    "stability": 3.5,
    "difficulty": 5.2,
    "state": "review",
    "reps": 4,
    "lapses": 0
  }
}
```

**Erros:**
- `404 Not Found` — Item de revisão não encontrado
- `400 Bad Request` — Rating inválido

---

## 👤 Usuário (`/api/users`)

> Todos os endpoints requerem autenticação

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/users/me` | Obter perfil do usuário |
| PUT | `/api/users/me` | Atualizar perfil |

---

### GET `/api/users/me`

Retorna o perfil completo do usuário autenticado.

**Response (200):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "firstName": "João",
    "lastName": "Silva",
    "email": "joao@email.com",
    "plan": "free",
    "xp": 1250,
    "streak": 7,
    "lives": 5,
    "energy": 80,
    "maxEnergy": 100,
    "level": "intermediate",
    "dailyMinutes": 15,
    "motivations": "career"
  }
}
```

**Erros:**
- `404 Not Found` — Usuário não encontrado

---

### PUT `/api/users/me`

Atualiza o perfil do usuário. Todos os campos são opcionais.

**Request Body:**
```json
{
  "firstName": "João",
  "lastName": "Silva",
  "dailyMinutes": 30,
  "motivations": "travel"
}
```

**Response (200):**
```json
{
  "success": true,
  "error": null
}
```

**Erros:**
- `404 Not Found` — Usuário não encontrado

---

## 🏥 Health Check

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/health` | ❌ | Verificar se a API está online |

**Response (200):** `Healthy`

---

## 🔑 Autenticação nos Endpoints Protegidos

Todos os endpoints marcados com ✅ (Auth) requerem o header:

```
Authorization: Bearer <access_token>
```

O `access_token` é obtido via login/register e expira em 60 minutos (dev). Use o endpoint `/api/auth/refresh` para renovar.
