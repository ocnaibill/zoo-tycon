# Protótipo — Fase 1 (Marco 2)

> Estado em **03/11/2026**, para a apresentação do protótipo de 04/11. Visual em *graybox* (formas simples, sem arte final).
> Critério do Plano de Projeto (4.12): **construir, acolher, cuidar e avançar rodada com feedback verificável.**

## O que o protótipo entrega

| Critério | Como aparece no jogo |
|---|---|
| Construir | Selecionar o **Ponto 1** (campo aberto) no mapa e clicar em *Construir recinto*. Custa $450 na hora; fica bloqueado se o saldo não cobre o recinto mais a operação da rodada |
| Acolher | Ao construir, um evento narrativo anuncia a chegada do lobo-guará. A ficha educativa passa a estar disponível |
| Cuidar | Três cuidados por rodada (alimentação, água, manutenção), cada um com custo. Bloqueiam se o saldo previsto não cobre |
| Avançar rodada | Botão *Encerrar rodada*: aplica cuidados, cobra despesas, calcula visitantes e receita e mostra o resumo |
| Feedback verificável | Previsão antes de decidir (gastos, receita, saldo ao fim) e resumo depois, com a causa de cada mudança. A previsão bate com o resultado real |

## Fluxo

Menu → **Jogar** → instruções → fase 1 (3 rodadas) → resumo por rodada → resultado.
Vitória: oferece *Jogar de novo* e *Menu*. Falha: lista os motivos e oferece *Reiniciar fase*. **Esc** abre e fecha a pausa (continuar ou voltar ao menu).

## Regras da fase 1

- **3 rodadas.** Uma rodada só avança por comando do jogador (*Encerrar rodada*).
- Objetivo, verificado **ao fim da rodada 3**: bem-estar ≥ 70, nenhuma necessidade abaixo de 40 e saldo ≥ $0. Se algum não for cumprido, a fase falha com explicação e pode ser reiniciada do estado inicial (sem punição).
- O **bem-estar** é a média de três necessidades (0 a 100): alimentação, água e manutenção do recinto. O animal chega com todas em 70.
- Em cada rodada, cada cuidado marcado soma +50 à sua necessidade (máximo 100). No fim do dia as necessidades caem: alimentação −25, água −25, manutenção −15.
- Os **cuidados e a operação do recinto** ($20 por rodada) são cobrados no fim da rodada e têm prioridade: nenhuma ação é permitida se o saldo previsto ficar negativo.
- **Visitantes:** `20 + bem-estar × 0,4`, cada um paga **$2** de ingresso. Sem animal acolhido, ninguém visita. Veja [`economia-e-balanceamento.md`](economia-e-balanceamento.md).

## Visitantes no mapa

Quadradinhos entram pela esquerda, andam pela trilha até o recinto, ficam um instante e saem. O ritmo acompanha o número de visitantes previsto da rodada (mais bem-estar, mais gente). É **só visual**: não há pathfinding nem simulação individual, conforme o GDD (visitação calculada). Teto de 14 visitantes na tela.

## Dentro e fora do escopo deste protótipo

**Dentro:** mapa com 3 pontos selecionáveis (só o 1 ativo), construção, acolhimento, 3 cuidados, indicador de bem-estar com causas, economia por rodada, visitantes, previsão, resumo, vitória/falha/reinício, ficha educativa provisória, instruções, pausa, menu.

**Fora (ainda não feito):**
- Fases 2 e 3 (os pontos 2 e 3 já existem no mapa, bloqueados com o aviso da fase).
- Árvores e arborização (M4) e a ficha da árvore.
- Segundo e terceiro animais (tamanduá-bandeira e tatu, cuja espécie ainda não foi definida).
- Arte, áudio e animações finais. O animal é um quadrado laranja.
- Tutorial além da tela de instruções.

## Conteúdo educativo

A ficha do lobo-guará (*Chrysocyon brachyurus*) é **texto provisório**: identificação, ambiente, alimentação e conservação, escritos sem fonte registrada. Antes de qualquer versão final é preciso pesquisar, citar as fontes e revisar (backlog **B08**, Gabriel).

## Builds

| Plataforma | Arquivo | Observação |
|---|---|---|
| Windows x86_64 | `RefugioDoCerrado-windows-x64.zip` (exe + pasta `data_...`) | Não assinado: o SmartScreen avisa. Testado só sob Wine |
| macOS (universal) | `RefugioDoCerrado-macos-universal.zip` | Não assinado nem notarizado: abrir com botão direito → Abrir. Não testado em um Mac |

Como gerar: seção *Exportar o jogo* do `README.md` da raiz. Os executáveis não vão para o Git.

## Como rodar e verificar

1. Abra o projeto no Godot 4.7.2 .NET e aperte **F5**.
2. Ou use os testes automáticos descritos em [`arquitetura-tecnica.md`](arquitetura-tecnica.md), que jogam a fase pela interface e provam o balanceamento.

## Status dos itens do backlog (Anexo A do GDD)

| ID | Item | Situação |
|---|---|---|
| B04 | Projeto Godot, Git e build mínimo | Feito (builds Windows e macOS geradas; teste em máquina real pendente) |
| B05 | Mapa e pontos selecionáveis | Feito em graybox (construir 1 recinto) |
| B06 | Rodada, saldo e previsão | Feito; tabela em `data/parametros_fase_1.tres` |
| B07 | Cuidados e feedback | Feito em graybox |
| B08 | Fichas e teste do protótipo | Ficha provisória; fontes e testes externos pendentes |
| B09 | 3 fases e reinício | Reinício feito; fases 2 e 3 pendentes |
| B10 | Recintos e arborização | Pendente (1 recinto feito) |
