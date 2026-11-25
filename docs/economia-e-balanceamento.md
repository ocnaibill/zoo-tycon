# Economia e balanceamento — três fases

Fonte única dos valores: [`data/parametros_fase_1.tres`](../data/parametros_fase_1.tres), [`data/parametros_fase_2.tres`](../data/parametros_fase_2.tres) e [`data/parametros_fase_3.tres`](../data/parametros_fase_3.tres), editáveis no Inspetor do Godot. As regras estão em [`scripts/Nucleo/Simulacao.cs`](../scripts/Nucleo/Simulacao.cs), sem dependência de cena.

## Fórmula

```
saldo seguinte = saldo atual + receitas − operação − cuidados − investimentos da rodada
```

- **Investimento** (construir recinto ou plantar árvore) sai do saldo na hora.
- **Operação** ($20 por recinto construído, por rodada) e **cuidados** são cobrados ao encerrar a rodada, mas entram na previsão desde o momento em que são marcados.
- **Receita** = visitantes × preço do ingresso, calculada com o bem-estar **ao fim** da rodada.
- Bloqueio: uma ação só é permitida se `saldo − despesas previstas − custo da ação ≥ 0`. Operação e cuidados planejados têm prioridade sobre novos gastos.

### Visitantes

```
visitantes = (20 + bem-estar global × 0,4) × animais acolhidos + bônus das árvores
receita    = visitantes × $2
```

O bem-estar global é a média de todas as necessidades de todos os recintos construídos, mais o bônus das árvores (limitado a +6), limitado a 100. Sem animal acolhido não há visitação. Cada árvore soma **+3 visitantes** fixos (não multiplicados por animal).

## Parâmetros por fase

Valores comuns às três fases: 3 rodadas, recinto $450, operação $20 por recinto/rodada, alimentação $40, água $15, manutenção $30, necessidade inicial 70, efeito de um cuidado +50 (máx. 100), quedas de −25 / −25 / −15, ingresso $2, 20 visitantes base e 0,4 visitante por ponto de bem-estar.

| Parâmetro | Fase 1 | Fase 2 | Fase 3 |
|---|---|---|---|
| Nome | Primeiro acolhimento | Parceria com o tamanduá | Floresta de volta |
| Animal novo | Lobo-guará | Tamanduá-bandeira | Tatu-canastra |
| Recintos na fase | 1 | 2 | 3 |
| Animais exigidos | 1 | 2 | 3 |
| Árvores | Não permitidas | Opcionais | **2 exigidas** |
| Saldo ao iniciar | $550 | saldo da fase 1 + $400 | saldo da fase 2 + $600 |
| Apoio de transição | — | $400 | $600 |
| Necessidades na transição | — | sobem para ≥ 60 | sobem para ≥ 60 |
| Meta de bem-estar | ≥ 70 | ≥ 70 | ≥ 80 |
| Necessidade mínima | ≥ 40 | ≥ 40 | ≥ 40 |
| Reserva mínima | ≥ $0 | ≥ $80 | ≥ $120 |

O campo `SaldoInicial` dos arquivos das fases 2 e 3 é 0 porque a partida continua do saldo anterior; o valor realmente usado vem de `SessaoPartida`. O apoio de transição é somado uma vez, ao preparar a fase.

### Árvores

| Espécie | Custo | Bem-estar | Visitantes | Ficha |
|---|---|---|---|---|
| Ipê-amarelo-do-cerrado (*Handroanthus ochraceus*) | $120 | +2 | +3 | `data/arvores/ipe_amarelo.tres` |
| Pequi (*Caryocar brasiliense*) | $120 | +2 | +3 | `data/arvores/pequi.tres` |

São quatro pontos de plantio no mapa. Plantar é investimento imediato e o efeito entra na previsão da mesma rodada. O bônus de bem-estar das árvores é **global e limitado a +6**; o bônus de visitantes é somado depois da multiplicação por animal. Na fase 3, as duas árvores exigidas podem ser da mesma espécie ou de espécies diferentes.

## Por que o jogador precisa escolher

**Fase 1.** Construir deixa $100. Descontada a operação ($20), restam $80 na rodada 1: cabem alimentação + água ($55), mas não os três cuidados ($85). A receita só entra ao fim da rodada, então a primeira rodada exige escolha e ensina a ler a previsão.

**Fase 2.** O jogador chega com o saldo da fase 1 mais $400, mas precisa construir o segundo recinto ($450) e manter dois animais. Depois da construção, a operação dobra e cada cuidado é cobrado por animal. Cuidar dos dois recintos ao mesmo tempo consome rapidamente o caixa; a fase cobra priorização e uma reserva de $80.

**Fase 3.** O saldo da fase 2 mais $600 precisa cobrir o terceiro recinto ($450) e duas árvores ($240) antes de alimentar três animais. A meta de bem-estar sobe para 80 e a reserva para $120. É a fase mais apertada: a sequência canônica só vence com cuidados alternados entre os recintos.

## Soluções canônicas verificadas

As máscaras abaixo usam bits na ordem **alimentação = 1, água = 2, manutenção = 4**. “3” = alimentação + água; “7” = os três cuidados.

### Fase 1

Construir o Ponto 1 na rodada 1.

| Rodada | Cuidados |
|---|---|
| 1 | 3 (alimentação + água) |
| 2 | 7 (todos) |
| 3 | 3 (alimentação + água) |

Resultado: **$139**, bem-estar **73**, necessidades finais **75 / 75 / 70**. Vitória.

### Fase 2

Construir o recinto 2 na rodada 1 e manter o recinto 1.

| Rodada | Recinto 1 | Recinto 2 |
|---|---|---|
| 1 | 2 (água) | 4 (manutenção) |
| 2 | 5 (alimentação + água) | 3 (alimentação + água) |
| 3 | 7 (todos) | 3 (alimentação + água) |

Resultado: **$221**, bem-estar **73**. Necessidades finais: recinto 1 em **75 / 75 / 85** e recinto 2 em **75 / 75 / 55**. Vitória.

A melhor estratégia encontrada pela enumeração termina com **$275**, mas usa uma combinação diferente e menos intuitiva; a canônica foi escolhida por ser jogável pela interface e reproduzível nos testes.

### Fase 3

Plantar ipê + pequi e construir o recinto 3 (o controlador faz os investimentos antes dos cuidados).

| Rodada | Recinto 1 | Recinto 2 | Recinto 3 |
|---|---|---|---|
| 1 | 3 (alimentação + água) | 0 | 0 |
| 2 | 4 (manutenção) | 7 (todos) | 3 (alimentação + água) |
| 3 | 7 (todos) | 7 (todos) | 7 (todos) |

Partindo do **fluxo canônico** (fase 2 terminando com $221 → $821 na fase 3): **$375**, bem-estar **81**. Partindo da melhor fase 2 ($275 → $875): **$431**, bem-estar **81**. A heurística gulosa, que marca todo cuidado possível, termina com **$332** e também vence. Necessidades finais nos dois casos: **75 / 75 / 85**, **75 / 75 / 85** e **75 / 75 / 75**.

## Prova de que as fases são vencíveis e não triviais

`TesteNucleo` roda a fase 1 e a fase 2 por **enumeração completa** e a fase 3 por heurística + busca aleatória com semente fixa.

### Fase 1 — enumeração completa

Combina a rodada de construção (1, 2, 3 ou nunca) com qualquer subconjunto dos três cuidados em cada uma das 3 rodadas.

| | |
|---|---|
| Estratégias possíveis | 2.048 |
| Bloqueadas pelo saldo | 1.546 |
| Estratégias válidas | 502 |
| Vencem | **60 (11,95% das válidas)** |
| Perdem | 442 |

### Fase 2 — enumeração completa a partir da fase 1 canônica

Combina a rodada de construção do recinto 2 (1, 2 ou 3) com todas as máscaras de cuidado dos dois recintos nas 3 rodadas (64 × 64 × 64 por rodada de construção).

| | |
|---|---|
| Estratégias válidas | 336.535 |
| Vencem | **4.153 (1,23%)** |
| Perdem | 332.382 |

### Fase 3 — heurística e busca aleatória

A fase 3 tem espaço de busca grande demais para enumeração completa no teste.

| Verificação | Resultado |
|---|---|
| Sequência canônica a partir da melhor fase 2 | Vitória — **$431**, bem-estar 81 |
| Sequência canônica a partir do fluxo canônico da fase 2 | Vitória — **$375**, bem-estar 81 |
| Heurística gulosa (marca todo cuidado que cabe) | Vitória — **$332**, bem-estar 81 |
| Sem nenhum cuidado | Derrota, com motivos |
| Busca aleatória com semente 20261124 | **2 vitórias e 2.109 derrotas** em 2.111 tentativas válidas |

A busca aleatória quase não encontra vitória, o que confirma que a fase 3 não é trivial, mas as três abordagens dirigidas vencem, o que confirma que ela é possível. O teste falha se alguma fase não tiver pelo menos uma vitória e uma derrota.

## Como recalibrar

1. Edite o `.tres` correspondente (ou o recurso no Inspetor do Godot).
2. Rode `dotnet build` e depois:
   ```bash
   godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn
   ```
3. Confira a proporção de vitórias, o saldo canônico e os motivos das falhas. O teste imprime a sequência canônica e a melhor estratégia de cada fase.
4. Se a fase 2 mudar, a fase 3 também muda: o teste recalcula a base a partir da fase 1 e da fase 2 canônicas.
5. Registre a mudança em [`registro-de-decisoes.md`](registro-de-decisoes.md) e atualize as tabelas acima.

Alavancas úteis: custo do recinto, apoio de transição, reserva mínima, meta de bem-estar, queda diária, efeito dos cuidados e bônus das árvores.

## Limitações conhecidas

- Os números foram provados viáveis por simulação, mas **ainda não foram calibrados com jogadores**. O roteiro de teste externo está em [`roteiro-teste-externo.md`](roteiro-teste-externo.md).
- A fase 3 não tem prova exaustiva; a conclusão vem de soluções canônicas, heurística, busca aleatória e teste da partida completa pela interface.
- A manutenção atua como necessidade do recinto, não como obra. Se a equipe preferir outro significado, é preciso mudar o texto e a interface, não só o número.
- O bônus de bem-estar das árvores é limitado a +6 para evitar que o plantio torne as metas triviais; o limite está no código (`Simulacao.BonusArvoresBemEstar`), não no `.tres`.
