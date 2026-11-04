# Economia e balanceamento — Fase 1

Fonte única dos valores: [`data/parametros_fase_1.tres`](../data/parametros_fase_1.tres), editável no Inspetor do Godot. As regras estão em `scripts/Nucleo/Simulacao.cs`, sem dependência de cena.

## Fórmula

```
saldo seguinte = saldo atual + receitas − despesas recorrentes − cuidados − investimentos da rodada
```

- **Investimento** (construir o recinto) sai do saldo na hora.
- **Despesas recorrentes** (operação do recinto) e **cuidados** são cobrados ao encerrar a rodada, mas entram na previsão desde o momento em que são marcados.
- **Receita** = visitantes × preço do ingresso, calculada com o bem-estar **ao fim** da rodada.
- Bloqueio: uma ação só é permitida se `saldo − despesas previstas − custo da ação ≥ 0`. Cuidados e operação têm prioridade sobre novos gastos.

## Parâmetros atuais

| Parâmetro | Valor |
|---|---|
| Rodadas | 3 |
| Saldo inicial | $550 |
| Custo do recinto | $450 |
| Operação por recinto, por rodada | $20 |
| Custo de alimentação / água / manutenção | $40 / $15 / $30 |
| Necessidade inicial | 70 |
| Efeito de um cuidado | +50 (máx. 100) |
| Queda diária: alimentação / água / manutenção | −25 / −25 / −15 |
| Visitantes | 20 base + bem-estar × 0,4 |
| Preço do ingresso | $2 |
| Meta de bem-estar | ≥ 70 |
| Necessidade mínima | ≥ 40 cada |
| Saldo mínimo | ≥ $0 |

## Por que o jogador precisa escolher

Construir deixa $100 no saldo. Descontada a operação ($20), restam $80 na rodada 1: cabem alimentação + água ($55), mas não os três cuidados ($85). A receita só entra ao fim da rodada, então a primeira rodada exige uma escolha e ensina a ler a previsão.

Em regime, com os três cuidados em dia, as necessidades se estabilizam em torno de 75 / 75 / 85 (bem-estar ≈ 78), com receita próxima de $102 contra $105 de gastos: manter o refúgio no limite do equilíbrio é o que o GDD pede para a fase 1.

## Prova de que a fase é vencível

`TesteNucleo` joga **todas** as combinações: rodada em que o recinto é construído (1, 2, 3 ou nunca) × qualquer subconjunto dos três cuidados em cada uma das 3 rodadas. Resultado atual:

| | |
|---|---|
| Estratégias possíveis (combinações) | 2.048 |
| Bloqueadas pelo saldo (ação proibida) | 1.546 |
| Estratégias válidas | 502 |
| Vencem | **60 (12%)** |

Exemplos que vencem (saldo final, bem-estar):

- construir na R1 · R1: água · R2: alimentação + manutenção · R3: alimentação + água → $178, 73
- construir na R1 · R1: alimentação + água · R2: manutenção · R3: alimentação + água → $182, 73
- construir na R1 · R1: água + manutenção · R2: alimentação + manutenção · R3: alimentação + água → $156, 73

Falham, por exemplo: não cuidar de nada; cuidar só na rodada 1; ou construir tarde demais. As falhas mais comuns são bem-estar abaixo da meta e alimentação ou água abaixo do mínimo.

O teste termina com `RESULTADO: OK` quando existe pelo menos uma sequência vencedora **e** pelo menos uma que perde, isto é, quando as escolhas importam.

## Como recalibrar

1. Edite `data/parametros_fase_1.tres` (ou o Inspetor, abrindo o recurso).
2. Rode `dotnet build` e depois `godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn`.
3. Confira a porcentagem de vitórias e os motivos das falhas. Um alvo razoável para a fase 1 é um mínimo de vitórias que não seja trivial, mas alcançável.
4. Registre a mudança em [`registro-de-decisoes.md`](registro-de-decisoes.md).

Alavancas úteis: custo do recinto e saldo inicial (tensão na rodada 1), queda diária (rigor), efeito dos cuidados (folga) e visitantes/ingresso (retorno do bom cuidado).

## Limitações conhecidas

- Os números ainda **não foram testados com jogadores**. O GDD pede testes externos (B13).
- Nas fases 2 e 3 a reserva exigida e o número de recintos vão alterar a pressão; esta tabela é só da fase 1.
- A manutenção atua como necessidade do recinto, não como obra. Se a equipe preferir outro significado, mudar o texto, não só o número.
