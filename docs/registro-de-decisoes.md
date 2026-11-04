# Registro de decisões e mudanças em relação ao GDD

Controle de mudanças (item 4.10 do Plano de Projeto). Cada linha: o que mudou, por quê e onde está registrado.

## Decisões gerais

| Data | Decisão | Motivo |
|---|---|---|
| 2026-10-22 | **2D com visão superior** (sem 2.5D/isometria) | Menor custo de arte e de código para três iniciantes; combina com mapa de pontos predefinidos e interface de gestão |
| 2026-10-22 | **Godot 4.7.2 .NET com C#** (os documentos do Marco 1 citam GDScript) | Bianco já tem base em C#/Unity; tipagem forte ajuda a manter as regras testáveis |
| 2026-10-22 | Git LFS para arte, áudio e fontes; `CREDITS.md` obrigatório para todo recurso externo | Evitar repositório pesado e problemas de licença |
| 2026-11-03 | Protótipo = **fatia vertical da fase 1 em graybox** | Prazo de 04/11; fases 2 e 3 e árvores ficam para depois do protótipo |
| 2026-11-03 | Animal da fase 1: **lobo-guará** | Citado primeiro no GDD; espécie do tatu ainda indefinida |

## Diferenças em relação ao GDD do Marco 1

| Tema | GDD (Marco 1) | Protótipo | Observação |
|---|---|---|---|
| Linguagem | GDScript | C# | Documentos do Marco 1 devem ser atualizados |
| Rodadas por fase | "cerca de três" | 3 na fase 1 | Dentro do previsto |
| Objetivo da fase 1 | bem-estar ≥ 70; saldo ≥ 0; cuidados atendidos | Igual, com "cuidados atendidos" definido como **nenhuma necessidade abaixo de 40** | Definição nova, precisa de validação |
| Bem-estar | Escala 0–100 | Média de três necessidades (alimentação, água, manutenção) | Detalha o indicador |
| Receita | "receita simplificada de visitação" | **Visitantes × ingresso** (20 + bem-estar × 0,4 visitantes, $2 cada) | Torna a visitação explícita |
| Visitantes | Calculados, sem simulação individual | Calculados + visitantes **decorativos** no mapa | Sem pathfinding; respeita o GDD |
| Cobrança | Despesas recorrentes e investimentos | Investimento na hora; operação e cuidados no fim da rodada, com previsão | Previsão exata do resultado |
| Despesa recorrente | Mencionada | Operação do recinto: $20 por rodada | Valor novo |
| Compatibilidade do recinto | Predefinida | Ponto 1 = campo aberto, compatível com o lobo-guará; pontos 2 e 3 bloqueados na fase 1 | Compatibilidade dos demais a definir |
| Tutorial | "Breve" | Tela de instruções antes da fase | Tutorial interativo fica para depois |

## Valores propostos (a validar em teste com jogadores)

Todos os números econômicos desta versão foram **propostos pelo desenvolvedor** e provados viáveis por simulação (ver [`economia-e-balanceamento.md`](economia-e-balanceamento.md)). Nenhum foi calibrado com jogadores reais; o GDD já previa essa calibração em tabela e em testes.

## Histórico

| Data | Mudança |
|---|---|
| 2026-11-03 | Núcleo da economia (`Simulacao`), tabela de parâmetros e prova de balanceamento |
| 2026-11-03 | Fase 1 jogável em graybox: mapa, construir, acolher, cuidados, rodadas, resultado, pausa, ficha provisória |
| 2026-11-03 | Visitantes: receita = visitantes × ingresso e visitantes decorativos no mapa |
| 2026-11-03 | Documentação em Markdown em `docs/` |

## Pendências

- [ ] Atualizar os PDFs do Marco 1 (GDD e Plano) para C# e para o estado do protótipo (Gabriel/Bianco).
- [ ] Definir a espécie do tatu e a segunda árvore.
- [ ] Pesquisar e registrar as fontes das fichas educativas (B08).
- [ ] Mecânica de atratividade (aparência/arborização) para as fases 2 e 3.
- [ ] Testes com jogadores externos e recalibração (B13).
- [ ] Exportar a build para Windows (exige instalar os *export templates* do Godot 4.7.2 .NET).
- [ ] Decidir o destino de `scenes/main.tscn` e `scripts/Main.cs` (restos do esqueleto inicial, sem uso).
