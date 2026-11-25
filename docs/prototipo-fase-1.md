# Protótipo jogável — três fases (Alpha)

> Estado em **24/11/2026**. O arquivo mantém o nome histórico `prototipo-fase-1.md`, mas descreve o loop completo das três fases.
> Critério do Plano de Projeto (4.12): **construir, acolher, cuidar, arborizar e avançar rodada com feedback verificável**.

## O que o jogo entrega

| Critério | Como aparece no jogo |
|---|---|
| Construir | Selecionar um ponto livre no mapa e clicar em *Construir recinto*. Custa $450 na hora; fica bloqueado se o saldo não cobre o recinto mais a operação da rodada |
| Acolher | Ao construir, um evento narrativo anuncia a chegada do animal da fase, com sprite e ficha educativa. Cada fase acrescenta um animal: lobo-guará, tamanduá-bandeira e tatu-canastra |
| Cuidar | Três cuidados por recinto e por rodada (alimentação, água, manutenção), com custo por animal. Botões mostram o custo, a necessidade atual e a prevista, e bloqueiam quando o saldo não cobre |
| Arborizar | A partir da fase 2, quatro pontos de plantio recebem ipê-amarelo ou pequi por $120. Cada árvore dá +2 de bem-estar e +3 visitantes, com ficha própria. Na fase 3, duas árvores são obrigatórias |
| Avançar rodada | *Encerrar rodada* aplica cuidados, cobra operação e despesas, calcula visitantes e receita e mostra um resumo com a causa de cada mudança |
| Progressão | Ao vencer, o saldo, os recintos e as árvores continuam. A fase seguinte soma um apoio de transição e uma “avaliação veterinária” eleva as necessidades a pelo menos 60 |
| Feedback verificável | A previsão mostra gastos, receita e saldo antes da decisão; o resumo confirma o resultado. Barras mudam de cor, textos avisam “baixo!”, mensagens animam e cada ação tem som |

## Fluxo completo

Menu → **Jogar** → instruções da fase 1 → 3 rodadas → vitória → transição → fase 2 → 3 rodadas → transição → fase 3 → 3 rodadas → **conclusão da partida**.

- **Vitória em fase intermediária:** modal com saldo, bem-estar, apoio da próxima fase e explicação da avaliação veterinária; botões *Próxima fase* e *Menu*.
- **Vitória na fase 3:** resumo final com animais acolhidos, árvores plantadas, saldo e bem-estar; botões *Jogar de novo* e *Menu*.
- **Falha:** lista os motivos e oferece *Reiniciar fase* (snapshot do início daquela fase, sem perder as anteriores) ou *Menu*.
- **Pausa:** **Esc** abre e fecha; *Continuar* ou *Voltar ao menu*. A pausa informa o estado do áudio.
- **Áudio:** sliders de música e efeitos, além de mute, no menu inicial; a configuração é salva em `user://config.cfg`.
- **Teclado:** **Tab** navega pelos botões, **Enter/Espaço** acionam, **Esc** pausa.

## Regras

- **3 rodadas por fase.** Uma rodada só avança por comando do jogador (*Encerrar rodada*).
- O objetivo é verificado **ao fim da rodada 3**. Se algum critério não for cumprido, a fase falha com explicação e pode ser reiniciada do estado inicial dela.
- O **bem-estar global** é a média das três necessidades de todos os recintos construídos, mais o bônus das árvores (máx. +6), limitado a 100. Cada recinto também tem seu bem-estar local.
- Em cada rodada, cada cuidado marcado soma +50 à sua necessidade (máximo 100). No fim do dia as necessidades caem: alimentação −25, água −25, manutenção −15.
- **Cuidados e operação** ($20 por recinto) são cobrados no fim da rodada e têm prioridade: nenhuma ação é permitida se o saldo previsto ficar negativo.
- **Visitantes:** `(20 + bem-estar × 0,4) × animais + bônus das árvores`; cada um paga **$2**. Sem animal acolhido, ninguém visita.
- **Investimentos** (recinto e árvore) saem do saldo na hora.

### Objetivos por fase

| Fase | Animal novo | Recintos | Árvores | Bem-estar | Cada necessidade | Reserva |
|---|---|---|---|---|---|---|
| 1 — Primeiro acolhimento | Lobo-guará | 1 | não permitidas | ≥ 70 | ≥ 40 | ≥ $0 |
| 2 — Parceria com o tamanduá | Tamanduá-bandeira | 2 | opcionais | ≥ 70 | ≥ 40 | ≥ $80 |
| 3 — Floresta de volta | Tatu-canastra | 3 | **2 exigidas** | ≥ 80 | ≥ 40 | ≥ $120 |

Os números completos, as soluções canônicas e as provas de balanceamento estão em [`economia-e-balanceamento.md`](economia-e-balanceamento.md).

## Mapa e visitantes

O mapa tem três pontos de recinto e quatro pontos de plantio. Os pontos de recinto não usados na fase aparecem como “Disponível na fase N”. Os visitantes são quadradinhos animados que entram pela esquerda, caminham até os recintos construídos, ficam um instante e saem. O ritmo acompanha a visitação prevista. É **só visual**: não há pathfinding nem simulação individual, conforme o GDD (visitação calculada). Há teto de visitantes na tela.

## Arte, áudio e interface

- **Pixel art original em PNG:** lobo-guará, tamanduá-bandeira, tatu-canastra, ipê-amarelo, pequi e muda, em linguagem chunky de 16-bit, com contorno escuro e paleta do Cerrado. A arte segue uma referência visual fornecida pela equipe, sem reutilizar pixels ou arquivos dela.
- **Interface em SVG:** fundo do Cerrado, visitante e ícones de alimentação/água/manutenção. Estilo flat, legível em 1280×720 e escalável.
- **Animações leves:** o animal “respira” e as árvores balançam; mensagens surgem com tween; botões têm estados de hover, foco e pressionado. Os sprites usam filtro Nearest para preservar os pixels.
- **Áudio original:** oito efeitos sintetizados por `tools/gerar_audio.ps1` e o tema **Noite Estrelada** por `tools/gerar_musicas.ps1`, sem samples de terceiros. A antiga ambiência de vento foi removida. O tema pode ser ouvido em [`audicao-musicas.html`](audicao-musicas.html).
- **Tema único:** fonte Atkinson Hyperlegible (SIL OFL), paleta do Cerrado, painéis, barras, sliders e foco visível.
- **Layout responsivo:** esticamento `canvas_items` com aspecto `expand`; testado em 1280×720 e 1600×900. O painel lateral tem largura fixa e o texto do objetivo usa quebra automática; selecionar recinto ou árvore não desloca a tela. As fases 2 e 3 usam um modal compacto, sem repetir o tutorial.
- **Acessibilidade:** alvos grandes, contraste alto, informação também em texto (não só cor), atalho de pausa e foco de teclado.

## Conteúdo educativo

As fichas dos três animais e das duas árvores estão nos `.tres` de `data/animais` e `data/arvores`, com nome científico, identificação, ambiente, alimentação/importância, conservação, curiosidade e **fontes com link**. O modal transforma as URLs em links clicáveis. As referências estão consolidadas em [`fontes-conteudo.md`](fontes-conteudo.md).

O texto é **educativo e simplificado**. Não substitui material técnico e deve ser revisado antes de uma versão final.

## Dentro e fora do escopo

**Dentro:** loop completo das três fases, três animais, três recintos, duas árvores, economia por rodadas, previsão, resumos, progressão com apoio e avaliação veterinária, falha e reinício por fase, fichas com fontes, menu com controles de áudio, pausa, tema, pixel art, áudio, feedback visual/sonoro, testes automáticos e capturas.

**Fora (ainda não feito):**
- Teste com jogadores externos e recalibração a partir das observações (B08/B13).
- Teste das builds em máquinas reais: o Windows só foi validado sob Wine e o macOS não foi executado.
- Música original elaborada, retratos nas fichas, tutorial interativo e animações mais complexas.
- Save em disco: a partida existe só em memória, como o GDD previa.
- Localização para outros idiomas.

## Builds

| Plataforma | Arquivo | Observação |
|---|---|---|
| Windows x86_64 | `RefugioDoCerrado-windows-x64.zip` (exe + pasta `data_...`) | Não assinado: o SmartScreen avisa. Testado só sob Wine |
| macOS (universal) | `RefugioDoCerrado-macos-universal.zip` | Não assinado nem notarizado: abrir com botão direito → Abrir. Não testado em um Mac |

Como gerar: seção *Exportar o jogo* do [`README.md`](../README.md) da raiz. Os executáveis não vão para o Git.

## Como rodar e verificar

1. Abra o projeto no Godot 4.7.2 .NET e aperte **F5**.
2. Testes automáticos (sem janela):
   ```bash
   dotnet build
   godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn
   godot-mono --headless --path . res://scenes/testes/teste_fase_1.tscn
   godot-mono --headless --path . res://scenes/testes/teste_jogo_completo.tscn
   ```
3. Capturas: veja [`arquitetura-tecnica.md`](arquitetura-tecnica.md#capturas-de-tela).

## Status dos itens do backlog (Anexo A do GDD)

| ID | Item | Situação |
|---|---|---|
| B04 | Projeto Godot, Git e build mínimo | Feito; builds Windows e macOS configuradas, teste em máquina real pendente |
| B05 | Mapa e pontos selecionáveis | Feito: 3 recintos e 4 pontos de plantio |
| B06 | Rodada, saldo e previsão | Feito nas três fases; parâmetros em `data/parametros_fase_*.tres` |
| B07 | Cuidados e feedback | Feito: barras, previsão, avisos, sons e animações |
| B08 | Fichas e teste do protótipo | Fichas dos 3 animais e 2 árvores com fontes feitas; roteiro de teste externo pronto, execução pendente |
| B09 | 3 fases e reinício | Feito: transições, apoio, avaliação veterinária, resumo final e reinício por snapshot |
| B10 | Recintos e arborização | Feito: até 3 recintos, 3 animais, 2 árvores e fichas |
| B11 | Conteúdo e balanceamento | Feito por simulação; calibração com jogadores pendente |
| B12 | Arte, áudio e interface | Feito: pixel art PNG, SVGs de interface, oito efeitos, tema Noite Estrelada, fonte, feedback e layout testado em duas resoluções |

O plano detalhado e o estado de cada tarefa estão em [`plano-b08-b12.md`](plano-b08-b12.md).
