using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Congnex.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(CongnexDbContext db)
    {
        if (await db.Units.AnyAsync()) return;  // already seeded

        var units = new[]
        {
            ("Introdução",      "Aprenda frases básicas e saudações",           1),
            ("Comida & Bebida", "Vocabulário de alimentos e restaurantes",      2),
            ("Viagem",          "Frases úteis para viajantes",                  3),
            ("Família",         "Membros da família e relacionamentos",         4),
            ("Trabalho",        "Vocabulário profissional",                     5),
            ("Compras",         "Fazer compras e negociar",                     6),
            ("Saúde",           "Vocabulário médico e bem-estar",               7),
            ("Entretenimento",  "Lazer, filmes e música",                       8),
            ("Cultura",         "Expressões culturais e costumes",              9),
            ("Avançado",        "Tópicos complexos e fluência",                10),
        };

        foreach (var (title, desc, order) in units)
        {
            var unit = new Unit
            {
                LanguageCode = "en",
                OrderIndex   = order,
                Title        = title,
                Description  = desc
            };

            var lessonQuestions = GetQuestionsForUnit(order);

            for (int i = 1; i <= 5; i++)
            {
                var lesson = new Lesson
                {
                    UnitId     = unit.Id,
                    OrderIndex = i,
                    Title      = $"Lição {i}",
                    XpReward   = 10
                };

                var questions = lessonQuestions[i - 1];
                foreach (var q in questions)
                {
                    q.LessonId = lesson.Id;
                    lesson.Questions.Add(q);
                }

                unit.Lessons.Add(lesson);
            }

            db.Units.Add(unit);
        }

        await db.SaveChangesAsync();
    }

    private static List<Question>[] GetQuestionsForUnit(int unitOrder)
    {
        return unitOrder switch
        {
            1 => GetUnit1Questions(),
            2 => GetUnit2Questions(),
            3 => GetUnit3Questions(),
            4 => GetUnit4Questions(),
            5 => GetUnit5Questions(),
            _ => GetGenericQuestions(unitOrder)
        };
    }

    // ── Unit 1: Introdução ──────────────────────────────────────────────────
    private static List<Question>[] GetUnit1Questions()
    {
        return
        [
            // Lição 1: Saudações
            [
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual destas imagens é 'hello'?", 1,
                    """{"targetWord":"hello","correctOptionId":1,"options":[{"id":1,"label":"hello","emoji":"👋"},{"id":2,"label":"goodbye","emoji":"🙋"},{"id":3,"label":"thanks","emoji":"🙏"}]}"""),
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "Qual é a tradução de 'hello'?", 2,
                    """{"audioText":"hello","choices":["olá","tchau","obrigado"],"correctAnswer":"olá"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 3,
                    """{"audioWord":"hello","options":["goodbye","hello","thanks","please"],"correctAnswer":"hello"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", 4,
                    """{"audioText":"good morning","wordOptions":["bom dia","boa noite","boa tarde","olá"],"correctAnswer":"bom dia"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 5,
                    """{"sentence":"____, how are you?","choices":["Hello","Goodbye","Thanks"],"correctAnswer":"Hello"}"""),
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "Qual é a tradução de 'goodbye'?", 6,
                    """{"audioText":"goodbye","choices":["olá","tchau","por favor"],"correctAnswer":"tchau"}"""),
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual destas imagens é 'good night'?", 7,
                    """{"targetWord":"good night","correctOptionId":3,"options":[{"id":1,"label":"good morning","emoji":"🌅"},{"id":2,"label":"good afternoon","emoji":"☀️"},{"id":3,"label":"good night","emoji":"🌙"}]}"""),
                MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", 8,
                    """{"leftWords":["olá","tchau","obrigado"],"rightWords":["thanks","hello","goodbye"],"correctPairs":{"olá":"hello","tchau":"goodbye","obrigado":"thanks"}}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", 9,
                    """{"audioText":"tchau","wordOptions":["hello","goodbye","thanks","please"],"correctAnswer":"goodbye"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 10,
                    """{"audioWord":"good morning","options":["good night","good morning","good afternoon","goodbye"],"correctAnswer":"good morning"}"""),
            ],
            // Lição 2: Apresentações
            [
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "Qual é a tradução de 'my name is'?", 1,
                    """{"audioText":"my name is","choices":["meu nome é","eu sou","eu tenho"],"correctAnswer":"meu nome é"}"""),
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual imagem representa 'nice to meet you'?", 2,
                    """{"targetWord":"nice to meet you","correctOptionId":2,"options":[{"id":1,"label":"goodbye","emoji":"👋"},{"id":2,"label":"nice to meet you","emoji":"🤝"},{"id":3,"label":"sorry","emoji":"😔"}]}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 3,
                    """{"sentence":"My ____ is Maria.","choices":["name","age","house"],"correctAnswer":"name"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", 4,
                    """{"audioText":"nice to meet you","wordOptions":["prazer em conhecê-lo","como vai","até logo","com licença"],"correctAnswer":"prazer em conhecê-lo"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 5,
                    """{"audioWord":"name","options":["game","name","same","fame"],"correctAnswer":"name"}"""),
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "O que significa 'How are you?'?", 6,
                    """{"audioText":"How are you?","choices":["Como você está?","Onde você está?","Quem é você?"],"correctAnswer":"Como você está?"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 7,
                    """{"sentence":"How ____ you?","choices":["are","is","am"],"correctAnswer":"are"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", 8,
                    """{"audioText":"eu estou bem","wordOptions":["I am fine","I am bad","I am tired","I am hungry"],"correctAnswer":"I am fine"}"""),
                MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", 9,
                    """{"leftWords":["nome","prazer","como vai"],"rightWords":["how are you","name","nice to meet you"],"correctPairs":{"nome":"name","prazer":"nice to meet you","como vai":"how are you"}}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "Qual é a tradução de 'I am fine'?", 10,
                    """{"audioText":"I am fine","choices":["eu estou bem","eu estou mal","eu estou cansado"],"correctAnswer":"eu estou bem"}"""),
            ],
            // Lição 3: Números
            [
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual número é 'three'?", 1,
                    """{"targetWord":"three","correctOptionId":2,"options":[{"id":1,"label":"two","emoji":"2️⃣"},{"id":2,"label":"three","emoji":"3️⃣"},{"id":3,"label":"five","emoji":"5️⃣"}]}"""),
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "Qual é a tradução de 'seven'?", 2,
                    """{"audioText":"seven","choices":["sete","seis","oito"],"correctAnswer":"sete"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 3,
                    """{"audioWord":"five","options":["four","five","nine","six"],"correctAnswer":"five"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", 4,
                    """{"audioText":"dez","wordOptions":["ten","two","twelve","twenty"],"correctAnswer":"ten"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 5,
                    """{"sentence":"I have ____ apples.","choices":["three","tree","free"],"correctAnswer":"three"}"""),
                MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", 6,
                    """{"leftWords":["um","dois","três"],"rightWords":["three","one","two"],"correctPairs":{"um":"one","dois":"two","três":"three"}}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "Quanto é 'eight'?", 7,
                    """{"audioText":"eight","choices":["oito","sete","nove"],"correctAnswer":"oito"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", 8,
                    """{"audioText":"four","wordOptions":["quatro","cinco","seis","três"],"correctAnswer":"quatro"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 9,
                    """{"audioWord":"nine","options":["nine","five","mine","fine"],"correctAnswer":"nine"}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "Qual é 'six' em português?", 10,
                    """{"audioText":"six","choices":["seis","cinco","sete"],"correctAnswer":"seis"}"""),
            ],
            // Lição 4: Cores
            [
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual cor é 'red'?", 1,
                    """{"targetWord":"red","correctOptionId":1,"options":[{"id":1,"label":"red","emoji":"🔴"},{"id":2,"label":"blue","emoji":"🔵"},{"id":3,"label":"green","emoji":"🟢"}]}"""),
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "Qual é a tradução de 'blue'?", 2,
                    """{"audioText":"blue","choices":["azul","verde","amarelo"],"correctAnswer":"azul"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 3,
                    """{"audioWord":"green","options":["red","green","blue","yellow"],"correctAnswer":"green"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", 4,
                    """{"audioText":"amarelo","wordOptions":["yellow","red","green","blue"],"correctAnswer":"yellow"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 5,
                    """{"sentence":"The sky is ____.","choices":["blue","red","green"],"correctAnswer":"blue"}"""),
                MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", 6,
                    """{"leftWords":["vermelho","azul","verde"],"rightWords":["green","red","blue"],"correctPairs":{"vermelho":"red","azul":"blue","verde":"green"}}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "O que é 'white'?", 7,
                    """{"audioText":"white","choices":["branco","preto","cinza"],"correctAnswer":"branco"}"""),
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual cor é 'black'?", 8,
                    """{"targetWord":"black","correctOptionId":3,"options":[{"id":1,"label":"white","emoji":"⚪"},{"id":2,"label":"yellow","emoji":"🟡"},{"id":3,"label":"black","emoji":"⚫"}]}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", 9,
                    """{"audioText":"orange","wordOptions":["laranja","limão","uva","maçã"],"correctAnswer":"laranja"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 10,
                    """{"audioWord":"yellow","options":["yellow","green","red","blue"],"correctAnswer":"yellow"}"""),
            ],
            // Lição 5: Expressões do dia a dia
            [
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "O que significa 'please'?", 1,
                    """{"audioText":"please","choices":["por favor","obrigado","desculpa"],"correctAnswer":"por favor"}"""),
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual imagem representa 'thank you'?", 2,
                    """{"targetWord":"thank you","correctOptionId":2,"options":[{"id":1,"label":"sorry","emoji":"😔"},{"id":2,"label":"thank you","emoji":"🙏"},{"id":3,"label":"hello","emoji":"👋"}]}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", 3,
                    """{"audioText":"desculpa","wordOptions":["sorry","please","thanks","hello"],"correctAnswer":"sorry"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 4,
                    """{"sentence":"____ you very much!","choices":["Thank","Please","Sorry"],"correctAnswer":"Thank"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 5,
                    """{"audioWord":"please","options":["please","thanks","sorry","hello"],"correctAnswer":"please"}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "O que é 'you're welcome'?", 6,
                    """{"audioText":"you're welcome","choices":["de nada","obrigado","por favor"],"correctAnswer":"de nada"}"""),
                MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", 7,
                    """{"leftWords":["por favor","obrigado","desculpa"],"rightWords":["sorry","please","thank you"],"correctPairs":{"por favor":"please","obrigado":"thank you","desculpa":"sorry"}}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", 8,
                    """{"audioText":"excuse me","wordOptions":["com licença","desculpa","por favor","obrigado"],"correctAnswer":"com licença"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 9,
                    """{"sentence":"____ me, where is the bathroom?","choices":["Excuse","Sorry","Please"],"correctAnswer":"Excuse"}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "Qual é a tradução de 'sorry'?", 10,
                    """{"audioText":"sorry","choices":["desculpa","obrigado","por favor"],"correctAnswer":"desculpa"}"""),
            ],
        ];
    }

    // ── Unit 2: Comida & Bebida ─────────────────────────────────────────────
    private static List<Question>[] GetUnit2Questions()
    {
        return
        [
            // Lição 1: Bebidas
            [
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual destas imagens é 'tea'?", 1,
                    """{"targetWord":"tea","correctOptionId":2,"options":[{"id":1,"label":"coffee","emoji":"☕"},{"id":2,"label":"tea","emoji":"🍵"},{"id":3,"label":"milk","emoji":"🥛"}]}"""),
                MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", "Qual é a tradução de 'coffee'?", 2,
                    """{"audioText":"coffee","choices":["café","chá","suco"],"correctAnswer":"café"}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 3,
                    """{"audioWord":"water","options":["water","milk","juice","tea"],"correctAnswer":"water"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", 4,
                    """{"audioText":"suco","wordOptions":["juice","water","milk","tea"],"correctAnswer":"juice"}"""),
                MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", 5,
                    """{"sentence":"I want a cup of ____.","choices":["coffee","table","chair"],"correctAnswer":"coffee"}"""),
                MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", 6,
                    """{"leftWords":["café","chá","leite"],"rightWords":["milk","coffee","tea"],"correctPairs":{"café":"coffee","chá":"tea","leite":"milk"}}"""),
                MakeQuestion(QuestionType.multipleChoice, "REVISÃO", "O que é 'juice'?", 7,
                    """{"audioText":"juice","choices":["suco","leite","água"],"correctAnswer":"suco"}"""),
                MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", 8,
                    """{"audioText":"milk","wordOptions":["leite","café","chá","água"],"correctAnswer":"leite"}"""),
                MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", "Qual imagem é 'water'?", 9,
                    """{"targetWord":"water","correctOptionId":1,"options":[{"id":1,"label":"water","emoji":"💧"},{"id":2,"label":"coffee","emoji":"☕"},{"id":3,"label":"juice","emoji":"🧃"}]}"""),
                MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", 10,
                    """{"audioWord":"tea","options":["coffee","tea","milk","juice"],"correctAnswer":"tea"}"""),
            ],
            // Lição 2-5: Simplified for brevity
            GenerateSimpleLesson("Comida & Bebida", 2, ["rice","bread","chicken","fish","egg"], ["arroz","pão","frango","peixe","ovo"]),
            GenerateSimpleLesson("Comida & Bebida", 3, ["apple","banana","orange","grape","strawberry"], ["maçã","banana","laranja","uva","morango"]),
            GenerateSimpleLesson("Comida & Bebida", 4, ["restaurant","menu","waiter","bill","tip"], ["restaurante","cardápio","garçom","conta","gorjeta"]),
            GenerateSimpleLesson("Comida & Bebida", 5, ["breakfast","lunch","dinner","snack","dessert"], ["café da manhã","almoço","jantar","lanche","sobremesa"]),
        ];
    }

    // ── Unit 3: Viagem ──────────────────────────────────────────────────────
    private static List<Question>[] GetUnit3Questions()
    {
        return
        [
            GenerateSimpleLesson("Viagem", 1, ["airport","hotel","ticket","passport","luggage"], ["aeroporto","hotel","passagem","passaporte","bagagem"]),
            GenerateSimpleLesson("Viagem", 2, ["taxi","bus","train","subway","car"], ["táxi","ônibus","trem","metrô","carro"]),
            GenerateSimpleLesson("Viagem", 3, ["map","street","left","right","straight"], ["mapa","rua","esquerda","direita","reto"]),
            GenerateSimpleLesson("Viagem", 4, ["beach","mountain","museum","park","church"], ["praia","montanha","museu","parque","igreja"]),
            GenerateSimpleLesson("Viagem", 5, ["reservation","check-in","room","key","elevator"], ["reserva","check-in","quarto","chave","elevador"]),
        ];
    }

    // ── Unit 4: Família ─────────────────────────────────────────────────────
    private static List<Question>[] GetUnit4Questions()
    {
        return
        [
            GenerateSimpleLesson("Família", 1, ["mother","father","brother","sister","baby"], ["mãe","pai","irmão","irmã","bebê"]),
            GenerateSimpleLesson("Família", 2, ["grandmother","grandfather","uncle","aunt","cousin"], ["avó","avô","tio","tia","primo"]),
            GenerateSimpleLesson("Família", 3, ["husband","wife","son","daughter","family"], ["marido","esposa","filho","filha","família"]),
            GenerateSimpleLesson("Família", 4, ["friend","neighbor","pet","dog","cat"], ["amigo","vizinho","animal de estimação","cachorro","gato"]),
            GenerateSimpleLesson("Família", 5, ["birthday","party","gift","cake","candle"], ["aniversário","festa","presente","bolo","vela"]),
        ];
    }

    // ── Unit 5: Trabalho ────────────────────────────────────────────────────
    private static List<Question>[] GetUnit5Questions()
    {
        return
        [
            GenerateSimpleLesson("Trabalho", 1, ["office","computer","meeting","email","phone"], ["escritório","computador","reunião","e-mail","telefone"]),
            GenerateSimpleLesson("Trabalho", 2, ["boss","colleague","team","project","deadline"], ["chefe","colega","equipe","projeto","prazo"]),
            GenerateSimpleLesson("Trabalho", 3, ["salary","interview","resume","job","career"], ["salário","entrevista","currículo","emprego","carreira"]),
            GenerateSimpleLesson("Trabalho", 4, ["schedule","break","lunch","overtime","vacation"], ["horário","intervalo","almoço","hora extra","férias"]),
            GenerateSimpleLesson("Trabalho", 5, ["report","presentation","client","contract","budget"], ["relatório","apresentação","cliente","contrato","orçamento"]),
        ];
    }

    // ── Units 6-10: Generic ─────────────────────────────────────────────────
    private static List<Question>[] GetGenericQuestions(int unitOrder)
    {
        var themes = unitOrder switch
        {
            6 => (new[] { "shop","price","cheap","expensive","discount" }, new[] { "loja","preço","barato","caro","desconto" },
                  new[] { "money","card","cash","receipt","bag" }, new[] { "dinheiro","cartão","dinheiro vivo","recibo","sacola" },
                  new[] { "size","color","try on","fit","return" }, new[] { "tamanho","cor","experimentar","servir","devolver" },
                  new[] { "market","fruit","vegetable","meat","fish" }, new[] { "mercado","fruta","vegetal","carne","peixe" },
                  new[] { "online","delivery","order","cart","payment" }, new[] { "online","entrega","pedido","carrinho","pagamento" }),
            7 => (new[] { "doctor","hospital","medicine","pain","fever" }, new[] { "médico","hospital","remédio","dor","febre" },
                  new[] { "headache","stomach","cold","cough","allergy" }, new[] { "dor de cabeça","estômago","resfriado","tosse","alergia" },
                  new[] { "pharmacy","prescription","pill","injection","bandage" }, new[] { "farmácia","receita","comprimido","injeção","curativo" },
                  new[] { "exercise","diet","sleep","stress","relax" }, new[] { "exercício","dieta","sono","estresse","relaxar" },
                  new[] { "emergency","ambulance","nurse","surgery","recovery" }, new[] { "emergência","ambulância","enfermeiro","cirurgia","recuperação" }),
            8 => (new[] { "movie","music","game","book","show" }, new[] { "filme","música","jogo","livro","show" },
                  new[] { "theater","concert","festival","dance","sing" }, new[] { "teatro","concerto","festival","dançar","cantar" },
                  new[] { "sport","soccer","basketball","swimming","running" }, new[] { "esporte","futebol","basquete","natação","corrida" },
                  new[] { "hobby","painting","cooking","reading","gardening" }, new[] { "hobby","pintura","cozinhar","leitura","jardinagem" },
                  new[] { "weekend","vacation","trip","adventure","fun" }, new[] { "fim de semana","férias","viagem","aventura","diversão" }),
            9 => (new[] { "tradition","holiday","celebration","custom","ritual" }, new[] { "tradição","feriado","celebração","costume","ritual" },
                  new[] { "art","history","language","religion","food" }, new[] { "arte","história","idioma","religião","comida" },
                  new[] { "greeting","gesture","respect","polite","rude" }, new[] { "cumprimento","gesto","respeito","educado","rude" },
                  new[] { "festival","costume","music","dance","parade" }, new[] { "festival","fantasia","música","dança","desfile" },
                  new[] { "country","flag","anthem","symbol","heritage" }, new[] { "país","bandeira","hino","símbolo","patrimônio" }),
            _ => (new[] { "although","however","therefore","meanwhile","furthermore" }, new[] { "embora","porém","portanto","enquanto isso","além disso" },
                  new[] { "accomplish","determine","establish","investigate","contribute" }, new[] { "realizar","determinar","estabelecer","investigar","contribuir" },
                  new[] { "perspective","consequence","opportunity","responsibility","environment" }, new[] { "perspectiva","consequência","oportunidade","responsabilidade","ambiente" },
                  new[] { "negotiate","compromise","persuade","convince","debate" }, new[] { "negociar","comprometer","persuadir","convencer","debater" },
                  new[] { "hypothesis","analysis","conclusion","evidence","theory" }, new[] { "hipótese","análise","conclusão","evidência","teoria" }),
        };

        return
        [
            GenerateSimpleLesson($"Unit {unitOrder}", 1, themes.Item1, themes.Item2),
            GenerateSimpleLesson($"Unit {unitOrder}", 2, themes.Item3, themes.Item4),
            GenerateSimpleLesson($"Unit {unitOrder}", 3, themes.Item5, themes.Item6),
            GenerateSimpleLesson($"Unit {unitOrder}", 4, themes.Item7, themes.Item8),
            GenerateSimpleLesson($"Unit {unitOrder}", 5, themes.Item9, themes.Item10),
        ];
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static List<Question> GenerateSimpleLesson(string unit, int lessonNum, string[] enWords, string[] ptWords)
    {
        var questions = new List<Question>();
        int order = 1;

        // Q1: imageWordChoice
        questions.Add(MakeQuestion(QuestionType.imageWordChoice, "PALAVRA NOVA", $"Qual destas é '{enWords[0]}'?", order++,
            $$"""{"targetWord":"{{enWords[0]}}","correctOptionId":1,"options":[{"id":1,"label":"{{enWords[0]}}","emoji":"✅"},{"id":2,"label":"{{enWords[1]}}","emoji":"❌"},{"id":3,"label":"{{enWords[2]}}","emoji":"❌"}]}"""));

        // Q2: multipleChoice
        questions.Add(MakeQuestion(QuestionType.multipleChoice, "PALAVRA NOVA", $"Qual é a tradução de '{enWords[1]}'?", order++,
            $$"""{"audioText":"{{enWords[1]}}","choices":["{{ptWords[1]}}","{{ptWords[2]}}","{{ptWords[3]}}"],"correctAnswer":"{{ptWords[1]}}"}"""));

        // Q3: listeningWordSelection
        questions.Add(MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", order++,
            $$"""{"audioWord":"{{enWords[2]}}","options":["{{enWords[0]}}","{{enWords[2]}}","{{enWords[3]}}","{{enWords[4]}}"],"correctAnswer":"{{enWords[2]}}"}"""));

        // Q4: translate
        questions.Add(MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em inglês:", order++,
            $$"""{"audioText":"{{ptWords[3]}}","wordOptions":["{{enWords[3]}}","{{enWords[0]}}","{{enWords[1]}}","{{enWords[4]}}"],"correctAnswer":"{{enWords[3]}}"}"""));

        // Q5: fillBlank
        questions.Add(MakeQuestion(QuestionType.fillBlank, "COMPLETE A FRASE", "Complete a frase:", order++,
            $$"""{"sentence":"I need ____.","choices":["{{enWords[4]}}","{{enWords[0]}}","{{enWords[1]}}"],"correctAnswer":"{{enWords[4]}}"}"""));

        // Q6: matchPairs
        var matchJson = "{\"leftWords\":[\"" + ptWords[0] + "\",\"" + ptWords[1] + "\",\"" + ptWords[2] + "\"],\"rightWords\":[\"" + enWords[2] + "\",\"" + enWords[0] + "\",\"" + enWords[1] + "\"],\"correctPairs\":{\"" + ptWords[0] + "\":\"" + enWords[0] + "\",\"" + ptWords[1] + "\":\"" + enWords[1] + "\",\"" + ptWords[2] + "\":\"" + enWords[2] + "\"}}";
        questions.Add(MakeQuestion(QuestionType.matchPairs, "VOCABULÁRIO", "Combine os pares:", order++, matchJson));

        // Q7: multipleChoice
        questions.Add(MakeQuestion(QuestionType.multipleChoice, "REVISÃO", $"O que é '{enWords[3]}'?", order++,
            $$"""{"audioText":"{{enWords[3]}}","choices":["{{ptWords[3]}}","{{ptWords[0]}}","{{ptWords[4]}}"],"correctAnswer":"{{ptWords[3]}}"}"""));

        // Q8: translate
        questions.Add(MakeQuestion(QuestionType.translate, "TRADUÇÃO", "Escreva em português:", order++,
            $$"""{"audioText":"{{enWords[4]}}","wordOptions":["{{ptWords[4]}}","{{ptWords[1]}}","{{ptWords[2]}}","{{ptWords[0]}}"],"correctAnswer":"{{ptWords[4]}}"}"""));

        // Q9: listeningWordSelection
        questions.Add(MakeQuestion(QuestionType.listeningWordSelection, "ESCUTA", "Toque no que escutar", order++,
            $$"""{"audioWord":"{{enWords[0]}}","options":["{{enWords[1]}}","{{enWords[0]}}","{{enWords[3]}}","{{enWords[4]}}"],"correctAnswer":"{{enWords[0]}}"}"""));

        // Q10: multipleChoice
        questions.Add(MakeQuestion(QuestionType.multipleChoice, "REVISÃO", $"Qual é '{enWords[2]}' em português?", order++,
            $$"""{"audioText":"{{enWords[2]}}","choices":["{{ptWords[2]}}","{{ptWords[4]}}","{{ptWords[0]}}"],"correctAnswer":"{{ptWords[2]}}"}"""));

        return questions;
    }

    private static Question MakeQuestion(QuestionType type, string label, string prompt, int order, string dataJson)
    {
        return new Question
        {
            Type           = type,
            Prompt         = prompt,
            CorrectAnswers = ["_"],  // Not used in new format, kept for compatibility
            Options        = dataJson,
            MediaUrl       = label,
            OrderIndex     = order
        };
    }
}
