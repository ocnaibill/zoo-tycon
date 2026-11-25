# Plano de ação — B08 a B12

> Análise feita em **24/11/2026** sobre o estado do repositório naquele momento. A seção 0 registra o que foi implementado depois; as seções seguintes continuam como **registro do plano original** e das decisões que o orientaram. As regras e os parâmetros vigentes estão em [`prototipo-fase-1.md`](prototipo-fase-1.md) e [`economia-e-balanceamento.md`](economia-e-balanceamento.md).

## 0. Status de implementação (24/11/2026)

| Item | Estado | Evidência |
|---|---|---|
| **B08 — Fichas e teste do protótipo** | **Parcial** | Fichas dos 3 animais e 2 árvores com nome científico, texto e fontes em `data/animais` e `data/arvores`; fontes clicáveis e consolidadas em [`fontes-conteudo.md`](fontes-conteudo.md); roteiro em [`roteiro-teste-externo.md`](roteiro-teste-externo.md). **Teste com jogadores ainda não executado** |
| **B09 — 3 fases e reinício** | **Feito** | `FaseJogo` + `SessaoPartida`; transições com apoio e avaliação veterinária; vitória, falha, reinício por snapshot, resumo final e menu |
| **B10 — Recintos e arborização** | **Feito** | Até 3 recintos, 3 animais, 4 pontos de plantio, ipê e pequi, fichas e efeitos |
| **B11 — Conteúdo e balanceamento** | **Feito por simulação** | Fase 1 e 2 por enumeração completa; fase 3 por canônica, gulosa e busca aleatória; `TesteNucleo` termina com `RESULTADO: OK`. Calibração com jogadores pendente |
| **B12 — Arte, áudio e interface** | **Feito** | Pixel art PNG, SVGs de interface, oito efeitos e o tema Noite Estrelada sintetizados, tema e fonte OFL, animações, volumes e mute persistidos, layout em 1280×720 e 1600×900, capturas revisadas |

### Verificação executada

| Verificação | Resultado |
|---|---|
| `dotnet build` | Compila sem erros |
| `teste_nucleo` | `RESULTADO: OK` — fase 1: 60 vitórias em 502 válidas; fase 2: 4.153 em 336.535; fase 3: canônica, gulosa e busca aleatória |
| `teste_fase_1` | `RESULTADO: OK` — vitória, derrota, reinício, bloqueios, ficha e pausa |
| `teste_jogo_completo` | `RESULTADO: OK` — partida 1 → 2 → 3 pela interface e resumo final |
| `teste_captura` | 12 PNGs gerados e revisados (menu, fases 1–3, modais, árvore vazia, resumo, visitantes e 1600×900) |
| Builds exportadas | Ainda não testadas em máquinas reais |

### O que continua pendente

- Executar o teste externo e recalibrar se necessário (B08/B13).
- Testar as builds Windows e macOS em máquinas reais.
- Atualizar os PDFs do Marco 1 e definir a licença de distribuição do projeto.

## 1. Premissas

- **Escopo do GDD:** 1 mapa, 3 fases, **3 animais no total** (um por espécie), até 3 recintos e **2 espécies de árvore**. A fase 1 já tem o lobo-guará; portanto, B10 acrescenta **2 animais novos** (tamanduá-bandeira e tatu) e **2 espécies de árvore**.
- **Ambiguidade resolvida:** “adicionar 3 animais” foi interpretado como **3 no total**, confirmado com a equipe. Não há um quarto animal.
- **Datas do Plano de Projeto:** Alpha em **25/11/2026** e entrega final em **16/12/2026**. O plano mostra um corte mínimo para o Alpha e o acabamento para a final.
- **Engine:** Godot 4.7.2 .NET, C# (`net8.0`), 2D top-down, 1280×720.
- **Verificação:** no início da análise, esta máquina não tinha Godot nem o SDK .NET configurados. Depois, Godot 4.7.2 .NET e o SDK .NET 8 local (`../dotnet8`) foram adicionados, e build, testes e capturas passaram a ser executados aqui. O registro abaixo é histórico.

## 2. Diagnóstico do que já existe

### 2.1 Visão geral

O projeto tem uma **fase 1 jogável em graybox**, com a economia separada da interface, testes automáticos e documentação consistente. A base é boa: `Simulacao` não depende de cena, `ParametrosFase` centraliza os números e o `Modal` é reutilizável. O que falta é justamente a parte de conteúdo, progressão e apresentação.

| Item | Estado atual | Lacuna para B08–B12 |
|---|---|---|
| B08 — Fichas e teste do protótipo | Só a ficha provisória do lobo-guará, sem fontes; teste interno da fase 1 | Fichas dos 3 animais e das 2 árvores, com nome científico, fontes e revisão; roteiro de teste externo |
| B09 — 3 fases e reinício | Só a fase 1; pontos 2 e 3 bloqueados; reinício recarrega a cena | Fases 2 e 3, transições, estado persistente entre fases, resumo final e reinício por fase |
| B10 — Recintos e arborização | 1 recinto; nenhuma árvore | Vários recintos, 3 animais, pontos de plantio, custo/efeito das árvores e ficha da árvore |
| B11 — Conteúdo e balanceamento | Prova de balanceamento só da fase 1 (2.048 estratégias, 60 vitórias) | Parâmetros das fases 2 e 3, prova de que são vencíveis, revisão dos textos e documentação |
| B12 — Arte, áudio e interface | `assets/` vazio; tema padrão da Godot; layout com offsets fixos; feedback só textual | Sprites, cenário, tema, sons, música/ambiente, controles de volume, animações e feedback responsivo |

### 2.2 Código

- `scripts/Nucleo/Simulacao.cs` guarda **um** recinto (`RecintoConstruido`, `AnimalAcolhido`) e um conjunto de três necessidades. Para as fases 2 e 3 precisa virar uma lista de recintos, cada um com seu animal, suas necessidades e seus cuidados planejados.
- `scripts/Fase1.cs` carrega `res://data/parametros_fase_1.tres` fixo no código e monta a interface para um único animal.
- `scripts/Nucleo/ParametrosFase.cs` tem os parâmetros da fase 1, mas não tem número de recintos, árvores, reserva mínima nem bônus de arborização.
- `scripts/PontoRecinto.cs` é um botão cinza/colorido com um `ColorRect` no lugar do animal.
- `scripts/FluxoVisitantes.cs` leva todos os visitantes a um único destino (o Ponto 1).
- `scripts/Modal.cs` já serve para instruções, resumo, ficha, pausa e resultado; pode ser reaproveitado.
- `scripts/Testes/TesteNucleo.cs` prova a fase 1 por enumeração completa. `TesteFase1.cs` joga a fase pela interface. Não há teste de progressão entre fases, de árvores nem de áudio.
- `CREDITS.md` está vazio e `assets/art`, `assets/audio` e `assets/fonts` só têm `.gitkeep`. O Git LFS já está configurado para PNG, OGG, WAV, MP3, TTF e OTF.

### 2.3 Documentos

- O GDD (Marco 1) define 3 fases, 3 animais, 2 árvores, fichas com fontes e "arte 2D estilizada, vista superior, poucos sprites".
- O Plano de Projeto prevê Alpha em 25/11 com "3 fases, espécies, árvores e conteúdo" e final em 16/12.
- `docs/prototipo-fase-1.md` já registra as pendências: fases 2 e 3, árvores, segundo/terceiro animais, arte/áudio e tutorial.
- `docs/registro-de-decisoes.md` ainda lista como pendentes a espécie do tatu, a segunda árvore e as fontes das fichas.

## 3. Decisões que precisam ser fechadas antes de codar

> **Fechadas em 24/11/2026:** 3 animais no total; tatu-canastra; pequi; pixel art original (PNG) para bichos/árvores e SVG para interface/cenário; licenças CC0/CC BY/OFL/autoral; avaliação veterinária em 60; cena `fase_1.tscn` mantida com o controlador `FaseJogo`. O registro completo está em [`registro-de-decisoes.md`](registro-de-decisoes.md).

| Decisão | Opções | Recomendação | Decisão final |
|---|---|---|
| Quantidade de animais | 3 no total (1 + 2 novos) ou 3 novos (4 no total) | **3 no total**, como no GDD | 3 no total |
| Espécie do tatu | Tatu-canastra (*Priodontes maximus*), tatu-bola (*Tolypeutes tricinctus*) ou tatu-galinha (*Dasypus novemcinctus*) | **Tatu-canastra** para um animal grande e visualmente distinto; **tatu-bola** se a prioridade for conservação/endemismo. Confirmar fontes antes de escrever a ficha | **Tatu-canastra** |
| Segunda árvore | Baru (*Dipteryx alata*), pequi (*Caryocar brasiliense*) ou ipê-roxo (*Handroanthus heptaphyllus*) | **Baru** (espécie típica do Cerrado, fruto e conservação) ou **pequi** (forte valor cultural). A primeira árvore fica sendo o ipê-amarelo | **Pequi** |
| Estilo de arte | Vetor flat original (SVG), pixel art 32×32 ou pack externo | **Pixel art original para animais/árvores + SVG para UI/cenário**. Dá identidade visual ao jogo, é legível em 1280×720 e não depende de achar sprites exatos das espécies | **Pixel art PNG + SVG de interface, sem packs** |
| Licença dos recursos | CC0, CC BY, CC BY-NC ou "grátis para uso pessoal" | **CC0 quando possível; CC BY quando necessário; evitar NC** (ver seção 7) | **CC0, CC BY 4.0, SIL OFL e autoral; sem NC** |
| Recuperação entre fases | Necessidades continuam como terminaram ou passam por uma "avaliação veterinária" que as eleva a um mínimo | **Elevar cada necessidade a pelo menos 60 entre fases**, para evitar que uma vitória apertada torne a fase seguinte impossível. Registrar como decisão de design | **Mínimo 60 + apoio de transição** |
| Nome da cena de jogo | Manter `fase_1.tscn` e torná-la genérica, ou renomear para `fase_jogo.tscn` | Manter `fase_1.tscn` durante o Alpha para não quebrar testes e links; renomear depois, se valer a pena | **Manter `fase_1.tscn` com `FaseJogo`** |

## 4. Arquitetura-alvo

### 4.1 Separação de responsabilidades

```
SessaoPartida (autoload)     estado da partida: fase atual, saldo, recintos, árvores, snapshot
        │
        ├── ParametrosFase  (data/parametros_fase_N.tres)
        ├── EspecieAnimal   (data/animais/*.tres)
        ├── EspecieArvore   (data/arvores/*.tres)
        │
        └── Simulacao       regras da fase atual (sem cena)
                │
                └── FaseJogo (Control)  liga a simulação à interface
                        ├── MapaRefugio (pontos, sprites, visitantes)
                        ├── PainelRecinto (necessidades, cuidados, fichas)
                        ├── BarraObjetivos / Topo
                        ├── Modal (instruções, resumo, ficha, pausa, resultado)
                        └── AudioManager / TemaUI
```

- **`Simulacao`** continua sem depender de cena e continua testável. Ela passa a trabalhar com uma lista de `Recinto`.
- **`SessaoPartida`** é o único lugar que sabe em qual fase o jogador está e o que veio da fase anterior. Ela guarda um **snapshot do início da fase** para o reinício sem punição.
- **`FaseJogo`** só traduz cliques em chamadas e estado em textos/imagens. Nenhuma regra econômica mora nela.
- **`ParametrosFase` e os recursos de espécies** são a fonte única dos números e dos textos.

### 4.2 Modelo de domínio

| Classe | Responsabilidade | Campos principais |
|---|---|---|
| `EspecieAnimal` (`Resource`) | Dados fixos da espécie | `Nome`, `NomeCientifico`, `FichaBbcode`, `Fontes`, `Sprite`, `Cor`, `Fase` |
| `EspecieArvore` (`Resource`) | Dados fixos da árvore | `Nome`, `NomeCientifico`, `FichaBbcode`, `Fontes`, `Sprite`, `Custo`, `BonusBemEstar`, `BonusVisitantes` |
| `Recinto` | Estado de um recinto | `Id`, `Especie`, `Construido`, `Necessidades[3]`, `Planejados`, `Arvores` |
| `ArvorePlantada` | Uma árvore no mapa | `Especie`, `Ponto`, `Rodada` |
| `Simulacao` | Regras da fase | `Saldo`, `Rodada`, `Recintos`, `Arvores`, `Estado` |
| `ResumoRodada` | Resultado de uma rodada | Saldo, receita, visitantes, despesas, bem-estar, mensagens, motivos de falha |
| `ResultadoFase` | Fim da fase | Vitória/falha, motivos, estatísticas, próximo passo |
| `SessaoPartida` (`Node`) | Progressão entre fases | `FaseAtual`, `Saldo`, `Recintos`, `Arvores`, `Snapshot`, `IniciarNovaPartida()`, `ConcluirFase()`, `ReiniciarFase()` |

### 4.3 Regras das três fases

Os números abaixo são **propostas iniciais**; a seção 5 (B11) define como calibrá-los com teste.

| Regra | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Recintos/animais | 1 | 2 | 3 |
| Rodadas | 3 | 3 | 3 |
| Bem-estar mínimo | 70 | 70 | 80 |
| Necessidade mínima | 40 | 40 | 40 |
| Reserva mínima | $0 | uma rodada de custos essenciais | uma rodada de custos essenciais |
| Árvores exigidas | 0 | 0 | 2 |
| Animal novo | Lobo-guará | Tamanduá-bandeira | Tatu |
| Saldo | $550 | herdado + apoio de transição | herdado + apoio de transição |

- **Cuidados** continuam por animal: alimentação, água e manutenção. Cada recinto tem seus próprios três valores.
- **Operação** é cobrada por recinto e por rodada.
- **Visitantes:** proposta inicial `(20 + bem-estar × 0,4) × número de animais`, com ingresso de $2. Isso mantém a lógica da fase 1 e faz cada animal contribuir para a receita; o equilíbrio fino vem dos custos e da reserva. Se ficar fácil ou impossível, ajustar base, ingresso ou custos — não a fórmula estrutural.
- **Bem-estar global:** média de todas as necessidades de todos os animais, mais o bônus das árvores, limitado a 100.
- **Árvores:** custo inicial proposto de $120 cada; +2 de bem-estar por árvore (máximo +6) e +3 visitantes por árvore. A árvore só pode ser plantada em ponto predefinido e passa a valer no fim da rodada. A fase 3 exige 2 árvores plantadas.
- **Transição:** ao vencer uma fase, o saldo, os recintos, as necessidades e as árvores são gravados na sessão. Na fase seguinte, as necessidades são elevadas a pelo menos 60 (avaliação veterinária) e um apoio narrativo cobre parte do novo recinto. Isso evita progressão impossível.
- **Falha:** o jogador recebe os motivos e pode reiniciar **a fase atual** a partir do snapshot. O saldo e os bens das fases anteriores não são perdidos.
- **Fim de jogo:** depois da fase 3, um resumo final mostra animais acolhidos, árvores, saldo, bem-estar e uma mensagem educativa; oferece "Jogar de novo" (nova partida) e "Menu".

### 4.4 Dados

```
data/
  parametros_fase_1.tres      (adaptar)
  parametros_fase_2.tres      (novo)
  parametros_fase_3.tres      (novo)
  animais/
    lobo_guara.tres
    tamandua_bandeira.tres
    tatu_canastra.tres
  arvores/
    ipe_amarelo.tres
    baru.tres
```

Os textos das fichas saem do código e vão para esses recursos. A interface só monta o BBCode com o conteúdo do recurso e as fontes. Isso permite revisar o conteúdo sem mexer na cena e cumpre B08.

### 4.5 Cena e interface

- Uma cena de jogo reutilizada para as 3 fases, carregando os parâmetros pela `SessaoPartida`. Evita manter três `.tscn` quase iguais.
- O mapa ganha:
  - fundo de cerrado (textura ou desenho vetorial);
  - 3 pontos de recinto, com sprite do animal quando construído;
  - quatro pontos de árvore, com sprite e estado plantado/livre;
  - visitantes usando sprites simples e escolhendo um recinto construído como destino.
- O painel lateral passa a mostrar **o recinto selecionado**: nome do animal, barras de necessidade, botões de cuidado, árvores daquele recinto e botão de ficha. Assim, 3 animais não viram 9 botões empilhados.
- O topo mostra: fase, rodada, saldo, previsão e progresso dos objetivos (animais, árvores, bem-estar, reserva).
- O rodapé continua sendo a mensagem curta de feedback.
- O `Modal` existente é reaproveitado; acrescentar um `Toast` simples para feedback que não merece janela e um `Fade` para transição de fase.

### 4.6 Áudio e interface responsiva

- `AudioManager` como autoload, com buses `Master`, `Music` e `SFX`, volume persistido em `user://config.cfg`.
- Sons: clique, confirmar, erro/bloqueio, construir, acolher, plantar, encerrar rodada, vitória, falha.
- Música/ambiente: uma faixa curta em loop, volume baixo; o jogo deve funcionar bem sem ela.
- Feedback responsivo:
  - hover/pressed/focus visíveis em todos os botões;
  - barra anima do valor atual para o previsto ao marcar cuidado;
  - saldo e visitantes animam ao encerrar a rodada;
  - bloqueio sempre com **texto + ícone + som**, nunca só cor;
  - árvore entra com um tween e libera a ficha;
  - vitória/falha com transição e resumo claro.
- Layout: usar containers e âncoras em vez de offsets fixos; testar em 1280×720, 1600×900 e 1024×600. O modo `canvas_items` já escala a interface; o mapa precisa de um `AspectRatioContainer` ou coordenadas normalizadas.

## 5. Plano por item do backlog

### B08 — Fichas e teste do protótipo (Gabriel)

| # | Tarefa | Evidência de conclusão |
|---|---|---|
| 1 | Escolher as espécies (tatu e segunda árvore) e registrar a decisão | Linha em `registro-de-decisoes.md` |
| 2 | Pesquisar cada espécie em fontes institucionais (ICMBio, Embrapa Cerrados, Jardim Botânico, Flora do Brasil, IUCN) | Tabela de fontes com URL, data de acesso e trecho usado |
| 3 | Escrever as fichas: identificação, ambiente, alimentação, conservação e uma curiosidade | 3 animais + 2 árvores, texto revisado e sem afirmação sem fonte |
| 4 | Colocar as fichas nos recursos e exibir no jogo com link/fonte | Botão "Fichas" abre animal/árvore; fonte visível |
| 5 | Criar o roteiro de teste externo (3 colegas) | Formulário curto: entenderam o objetivo? lembram uma informação? onde travaram? |
| 6 | Revisar o conteúdo com outra pessoa da equipe | Registro de revisão em `docs/` |

**Cuidado:** não copiar texto de sites. Escrever com as próprias palavras e citar a fonte. Status de conservação e nome científico precisam ser conferidos na fonte, não presumidos.

### B09 — Três fases e reinício (Bianco/Gabriel)

| # | Tarefa | Evidência de conclusão |
|---|---|---|
| 1 | Criar `SessaoPartida` (autoload) com estado, snapshot e reset | Teste de nova partida e de reinício |
| 2 | Generalizar `Simulacao` para lista de recintos | Teste da fase 1 continua passando com os mesmos números |
| 3 | Criar `parametros_fase_2.tres` e `parametros_fase_3.tres` | Fases carregam por dados, sem `if` de fase no código |
| 4 | Implementar transição: resumo da fase → próxima fase | Jogo completo 1→2→3 pela interface |
| 5 | Implementar falha e reinício por fase | Snapshot restaura saldo, recintos e árvores |
| 6 | Implementar resumo final e "Jogar de novo" | Tela final com estatísticas e retorno ao menu |
| 7 | Atualizar `TesteFase1` e criar `TesteJogoCompleto` | Testes terminam com `RESULTADO: OK` |

### B10 — Recintos e arborização (Jhon/Gabriel)

| # | Tarefa | Evidência de conclusão |
|---|---|---|
| 1 | Transformar `PontoRecinto` em ponto com sprite e estado | Animal aparece no recinto construído |
| 2 | Adicionar os 3 animais e seus recintos | Fases 1, 2 e 3 com 1, 2 e 3 animais |
| 3 | Criar `PontoArvore` e os pontos de plantio | Plantar, cobrar e exibir a árvore |
| 4 | Implementar efeito da árvore no bem-estar/visitantes | Efeito aparece na previsão e no resumo |
| 5 | Liberar a ficha da árvore ao plantar | Ficha com fonte e nome científico |
| 6 | Ajustar o fluxo de visitantes para vários recintos | Visitantes visitam recintos construídos, sem pathfinding |
| 7 | Testar a fase 3 com 2 árvores | Objetivo de árvores verificado |

### B11 — Conteúdo e balanceamento (Gabriel + equipe)

| # | Tarefa | Evidência de conclusão |
|---|---|---|
| 1 | Revisar todos os textos (ortografia, clareza, tom para 12+) | Revisão registrada |
| 2 | Definir a reserva mínima de cada fase e documentar a conta | Tabela em `economia-e-balanceamento.md` |
| 3 | Estender a prova de balanceamento para as fases 2 e 3 | Relatório com taxa de vitória e exemplos |
| 4 | Garantir pelo menos uma sequência vencedora e uma perdedora por fase | Teste falha se não houver as duas |
| 5 | Recalibrar custos, saldo, visitantes e bônus de árvore | Parâmetros finais nos `.tres` |
| 6 | Atualizar os documentos no mesmo commit das mudanças | `git diff` coerente |

Para as fases 2 e 3, a enumeração completa fica grande (8 escolhas por animal por rodada). Estratégia prática:

- enumerar todas as combinações para a fase 2 (2 animais × 3 rodadas = 262.144 possibilidades, viável);
- para a fase 3, usar busca aleatória com semente fixa + uma heurística gulosa, e guardar as melhores sequências;
- registrar no documento que a prova da fase 3 é por busca, não exaustiva. Isso é honesto e suficiente para o marco.

### B12 — Arte, áudio e interface (Jhon + Gabriel)

| # | Tarefa | Evidência de conclusão |
|---|---|---|
| 1 | Definir guia visual (paleta, traço, tamanho, sombra) | Mini guia em `docs/` ou no `CREDITS.md` |
| 2 | Criar sprites vetoriais dos 3 animais e 2 árvores | Arquivos em `assets/art/` com nomes claros |
| 3 | Criar fundo do mapa, cercas, trilha e ícones | Mapa deixa de ser graybox |
| 4 | Criar tema de UI (fonte, cores, botões, foco) | `Theme` aplicado em menu, fase e modal |
| 5 | Adicionar sons e ambiente + controle de volume | Sliders no menu/pausa e configuração persistida |
| 6 | Adicionar feedback animado (barras, saldo, árvore, transições) | Ações têm resposta visual e sonora |
| 7 | Testar layout em 1280×720 e 1600×900 | Capturas das duas resoluções |
| 8 | Preencher `CREDITS.md` e baixar tudo pelo LFS | `git lfs ls-files` mostra os binários |

## 6. Ordem de execução e cronograma

### 6.1 Fatias verticais (recomendado)

1. **Fundação (4–6 h):** `SessaoPartida`, generalização de `Simulacao`, parâmetros por fase, fase 1 de volta ao verde. Nada visual novo ainda.
2. **Fase 2 (6–8 h):** segundo recinto, tamanduá, transição, objetivo e teste.
3. **Fase 3 + árvores (8–10 h):** terceiro recinto, tatu, plantio, efeito, ficha da árvore, resumo final.
4. **Conteúdo (8–10 h, em paralelo):** pesquisa, fichas, fontes, revisão.
5. **Balanceamento (4–6 h):** reserva, visitantes, árvores, prova das fases 2 e 3.
6. **Arte/áudio/UI (16–20 h):** tema, sprites, cenário, sons, feedback, layout.
7. **QA, docs e build (8–10 h):** testes completos, capturas, créditos, exportação.

Total realista: **55–70 h** de trabalho. Não cabe em um dia.

### 6.2 Corte para o Alpha de 25/11

Se o Alpha for amanhã, a única entrega honesta é um **vertical slice completo com arte provisória**:

- 3 fases jogáveis, 3 animais e 2 árvores **em código**, com sprites vetoriais simples;
- estado persistente e transições;
- fichas com fontes (mesmo que curtas);
- tema básico, 5–6 SFX e música ambiente (a entrega final tem oito SFX e o tema Noite Estrelada);
- testes de progressão passando.

O que pode ficar para a final: animações elaboradas, música original, retratos das fichas, polimento do cenário, tutorial interativo, testes externos e ajustes finos de balanceamento.

**Não cortar:** persistência entre fases, objetivo da fase 3, fontes das fichas, reinício por fase e créditos. São critérios de aceite do projeto, não enfeites.

## 7. Arte, áudio e licenças

### 7.1 "Licença não comercial" não é a melhor opção

É possível usar arte CC BY-NC, mas ela **restringe** o uso: o projeto não pode ser reaproveitado comercialmente, e alguns packs NC também proíbem alterações. Para um trabalho acadêmico público, isso cria dúvida desnecessária. O mais seguro é:

- **CC0 / domínio público:** use, altere e distribua sem atribuição obrigatória (Kenney é CC0).
- **CC BY 4.0:** use e altere, desde que cite autor e fonte.
- **SIL OFL:** para fontes.
- **CC BY-SA:** evite misturar com o projeto se não quiser a obrigação de compartilhar adaptações sob a mesma licença.

Se a equipe fizer questão de NC, use **CC BY-NC 4.0** (permite adaptações) e registre a restrição no `CREDITS.md`. Mas a recomendação é **CC0/CC BY**.

### 7.2 Estratégia de arte recomendada

**Pixel art original para os bichos, árvores e muda + SVGs originais para UI e cenário.**

Por quê:

- não existe pack CC0 com lobo-guará, tamanduá-bandeira e tatu-canastra fiéis e no mesmo estilo;
- usar um lobo genérico ou um urso recolorido compromete o valor educativo;
- A pixel art dá unidade ao mapa e é nítida com filtro Nearest; a interface continua em SVG, que escala sem perder qualidade;
- a arte original resolve licenciamento de vez: a equipe é a autora.

O desenho pode ser simples: corpo em uma cor, contorno, sombra elíptica, olho e um detalhe da espécie (pernas longas do lobo, faixa preta do tamanduá, casco do tatu). As árvores são copas vistas de cima com tronco e sombra. Animações: um "respirar" com tween e um leve balanço; não precisa de sprite sheet.

### 7.3 Fornecedores e licenças verificadas

| Recurso | Sugestão | Licença | Observação |
|---|---|---|---|
| UI, ícones, teclas | [Kenney Assets](https://kenney.nl/assets) | CC0 | Atribuição não obrigatória; mesmo assim, registrar |
| SFX de interface | [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) | CC0 | Cliques, confirmação, erro, abrir/fechar |
| SFX de jogo | [Kenney RPG Audio](https://kenney.nl/assets) ou [OpenGameArt](https://opengameart.org/content/cc0-resources) | CC0 (verificar cada pack) | Baixar só depois de conferir a licença na página |
| Música/ambiente | [OpenGameArt](https://opengameart.org/) ou [Freesound](https://freesound.org/) | CC0 ou CC BY | Preferir CC0; se CC BY, citar autor, faixa e URL |
| Fonte | Atkinson Hyperlegible ou Inter | SIL OFL | Incluir o texto da licença em `assets/fonts/` |
| Terreno | Desenho próprio ou pack CC0 | CC0/original | Evitar textura sem licença clara |
| Fotos para fichas | [Wikimedia Commons](https://commons.wikimedia.org/) | CC0/CC BY/CC BY-SA | Opcional; se usar, atribuir e não afirmar que a foto é do jogo |

### 7.4 Pipeline de recurso externo

1. Baixar o pack e guardar o `LICENSE` original.
2. Registrar no `CREDITS.md`: recurso, autor, URL, licença, atribuição exigida.
3. Converter/otimizar (PNG/OGG) e conferir se o LFS está rastreando.
4. Importar no Godot e testar.
5. Commitar o crédito **antes ou junto** do asset.

**Nunca usar:** rip de jogo comercial, sprite do Google Imagens, pack "free for personal use", textura sem autor, música de YouTube. Se a licença não estiver explícita, não entra.

## 8. Testes e verificação

### 8.1 Automáticos

```bash
dotnet build
godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn
godot-mono --headless --path . res://scenes/testes/teste_fase_1.tscn
godot-mono --headless --path . res://scenes/testes/teste_jogo_completo.tscn
```

Implementado:

- `TesteNucleo`: fase 1 e fase 2 exaustivas (regressão), fase 3 por canônica, heurística gulosa e busca aleatória com semente fixa; exige pelo menos uma vitória e uma derrota por fase.
- `TesteFase1`: vitória, derrota, reinício por snapshot, bloqueios, ficha e pausa.
- `TesteJogoCompleto`: joga 1→2→3 pela interface, confere saldos, transições, conclusão e resumo final.
- `TesteCaptura`: capturas em 1280×720 e 1600×900.

### 8.2 Manual

- Partida completa em 10–20 minutos.
- Vitória e derrota em cada fase; reinício restaura o snapshot.
- Plantar árvore, ver efeito e abrir ficha.
- Volume, mute e persistência da configuração.
- Navegação por teclado (Tab/Enter/Esc) e foco visível.
- Alertas com texto + ícone, não só cor.
- Nenhum texto cortado em 1024×600, 1280×720 e 1600×900.
- Créditos completos e links funcionando.

## 9. Riscos e mitigação

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Prazo do Alpha (25/11) curto demais | Alta | Alto | Entregar vertical slice com arte provisória; polir até 16/12 |
| Refatoração de `Simulacao` quebrar a fase 1 | Média | Alto | Fazer a fatia 0 primeiro e manter `TesteFase1` como regressão |
| Fases 2/3 impossíveis ou triviais | Média | Alto | Provar com teste e recalibrar antes de integrar arte |
| Não achar sprite CC0 fiel das espécies | Alta | Médio | Pixel art original; packs só para terreno/UI |
| Licença errada em asset | Média | Alto | Checklist no `CREDITS.md`; baixar só de fonte oficial |
| Conteúdo educativo incorreto | Média | Médio | Fontes institucionais, revisão por outra pessoa e aviso de simplificação |
| Conflito de merge em `.tscn` | Média | Médio | Uma pessoa por cena; branches curtas; cena de jogo única |
| Sem Godot/SDK na máquina de análise | Alta | Médio | Rodar build/testes na máquina de quem desenvolve antes de cada commit |
| Escopo de arte crescer | Alta | Alto | Guia visual, lista fechada de assets e corte do que não é essencial |

## 10. Critérios de conclusão (Definition of Done)

> Situação em 24/11/2026. O único item aberto é o teste das builds em outra máquina; o teste com jogadores é acompanhado em B08/B13.

- [x] Partida completa do menu ao resumo final, com 3 fases distintas.
- [x] 3 animais e 2 espécies de árvore integrados, com fichas e fontes.
- [x] Estado (saldo, recintos, necessidades, árvores) preservado entre fases.
- [x] Falha e reinício por fase funcionando sem perder fases anteriores.
- [x] Parâmetros das 3 fases em `.tres`, com prova de vitória e derrota.
- [x] Arte, áudio e interface integrados; nenhum graybox essencial.
- [x] Feedback visual e sonoro em construir, cuidar, plantar, encerrar rodada e resultado.
- [x] Testes automáticos com `RESULTADO: OK` e capturas revisadas.
- [x] `CREDITS.md` completo, licenças e autores registrados; assets cobertos pelas regras do LFS.
- [x] Documentos (`prototipo-fase-1.md`, `economia-e-balanceamento.md`, `arquitetura-tecnica.md`, `registro-de-decisoes.md`) atualizados no mesmo commit das mudanças.
- [ ] Build Windows (e macOS, se possível) testada em outra máquina.

## 11. Próximos passos imediatos (histórico)

> Passos 1–6 concluídos em 24/11/2026. O próximo trabalho é o teste externo ([`roteiro-teste-externo.md`](roteiro-teste-externo.md)) e o teste das builds em máquinas reais.

1. Confirmar as decisões da seção 3 (animais, tatu, segunda árvore, estilo e corte do Alpha).
2. Criar a branch `feat/b08-b12` e não misturar com mudanças de outras pessoas.
3. Implementar a fatia 0 (sessão + domínio + regressão da fase 1) antes de qualquer arte.
4. Começar a pesquisa das fichas em paralelo, porque ela não bloqueia o código.
5. Fechar o guia visual e produzir os primeiros sprites de animais/árvores como placeholders "de verdade".
6. Só então integrar tema, áudio e feedback, com capturas a cada etapa.
