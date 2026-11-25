# Registro de decisões e mudanças em relação ao GDD

Controle de mudanças (item 4.10 do Plano de Projeto). Cada linha registra o que mudou, por quê e onde está documentado.

## Decisões gerais

| Data | Decisão | Motivo |
|---|---|---|
| 2026-10-22 | **2D com visão superior** (sem 2.5D/isometria) | Menor custo de arte e de código para três iniciantes; combina com mapa de pontos predefinidos e interface de gestão |
| 2026-10-22 | **Godot 4.7.2 .NET com C#** (os documentos do Marco 1 citam GDScript) | Bianco já tem base em C#/Unity; tipagem forte ajuda a manter as regras testáveis |
| 2026-10-22 | Git LFS para arte, áudio e fontes; `CREDITS.md` obrigatório para todo recurso externo | Evitar repositório pesado e problemas de licença |
| 2026-11-03 | Protótipo inicial = **fatia vertical da fase 1 em graybox** | Prazo de 04/11; fases 2 e 3 e árvores ficaram para o Alpha |
| 2026-11-03 | Animal da fase 1: **lobo-guará** | Citado primeiro no GDD |
| 2026-11-03 | macOS exportado como **universal**, não só arm64 | O template oficial só traz o binário universal e o Godot não extrai o arm64 no Linux; roda nativo em Apple Silicon |
| 2026-11-24 | **3 animais no total**, um por fase: lobo-guará, tamanduá-bandeira e tatu-canastra | Fecha a ambiguidade de “adicionar 3 animais”; mantém o escopo do GDD |
| 2026-11-24 | Espécie do tatu: **tatu-canastra** (*Priodontes maximus*) | Maior tatu do mundo, visualmente distinto e com forte apelo de conservação |
| 2026-11-24 | Segunda árvore: **pequi** (*Caryocar brasiliense*); a primeira é o **ipê-amarelo** | Espécie-símbolo do Cerrado, valor cultural e ficha com fontes institucionais |
| 2026-11-24 | **Pixel art original em PNG** para animais, árvores e muda; SVG apenas para interface/cenário; **áudio sintetizado original**; sem packs externos | Não há pack CC0 fiel das três espécies; arte original elimina dúvida de licenciamento e atende ao guia visual pixel art |
| 2026-11-24 | Licenças aceitas para o projeto: **CC0, CC BY 4.0, SIL OFL e arte autoral**; evitar NC | Permite uso, alteração e distribuição acadêmica sem restrição comercial desnecessária. Nenhum recurso externo de arte/áudio foi necessário |
| 2026-11-24 | **Cena única** `fase_1.tscn` com o controlador `FaseJogo` para as três fases | Evita duplicar interface e regras; o nome do arquivo é histórico e os testes já o usam |
| 2026-11-24 | **Noite Estrelada é o único tema musical**; a ambiência de vento e o seletor de faixas foram removidos | A equipe escolheu uma identidade sonora única; o volume continua ajustável e persistido |
| 2026-11-24 | Fases 2 e 3 usam modal compacto e não repetem o tutorial; o painel lateral tem largura fixa e o bloco de cuidados some ao selecionar árvore | Corrige texto repetido, concordância (“animais”) e o deslocamento visual entre recinto e árvore |
| 2026-11-24 | Progressão por **apoio de transição + avaliação veterinária** (necessidades ≥ 60) | Uma vitória apertada não pode tornar a fase seguinte impossível |
| 2026-11-24 | Árvores: $120, **+2 bem-estar** e **+3 visitantes** cada; bônus de bem-estar **limitado a +6** | Torna a arborização relevante sem trivializar as metas |
| 2026-11-24 | Fase 3 exige **duas árvores** e reserva de **$120** | Fecha o arco narrativo de “floresta de volta” e dá uso obrigatório à mecânica |
| 2026-11-24 | **Quatro pontos de plantio** no mapa, em vez de seis | A quantidade concentra a decisão de arborização, mantém o custo relevante e evita pontos sobre recintos e caminho no layout atual; como a fase 3 exige duas árvores, quatro opções continuam suficientes |
| 2026-11-24 | Interface **responsiva** com `canvas_items` + `expand`, tema único e foco visível | Evita texto cortado em 1600×900 e melhora acessibilidade |
| 2026-11-24 | Conteúdo educativo com **fontes institucionais e links clicáveis** | Atende B08 e permite conferência por terceiros |
| 2026-11-24 | O reinício restaura um **snapshot do início da fase** | Preserva as fases anteriores e evita reiniciar a partida inteira |

## Diferenças em relação ao GDD do Marco 1

| Tema | GDD (Marco 1) | Versão atual | Observação |
|---|---|---|---|
| Linguagem | GDScript | C# | Documentos do Marco 1 devem ser atualizados |
| Rodadas por fase | “cerca de três” | 3 em todas as fases | Dentro do previsto |
| Objetivo | Bem-estar, saldo e cuidados | Bem-estar ≥ meta, cada necessidade ≥ 40, reserva mínima, animais e árvores exigidos | A reserva e as árvores entram nas fases 2 e 3 |
| Bem-estar | Escala 0–100 | Média das necessidades de todos os recintos + bônus das árvores (máx. +6) | Detalha o indicador e dá efeito à arborização |
| Receita | “receita simplificada de visitação” | `(20 + bem-estar × 0,4) × animais + bônus de árvores`, ingresso $2 | Visitação explícita e multiplicada por animal |
| Visitantes | Calculados, sem simulação individual | Calculados + visitantes decorativos no mapa | Sem pathfinding; respeita o GDD |
| Cobrança | Despesas recorrentes e investimentos | Investimento na hora; operação e cuidados no fim da rodada, com previsão | Previsão exata do resultado |
| Despesa recorrente | Mencionada | Operação: $20 por recinto construído por rodada | Valor novo |
| Compatibilidade do recinto | Predefinida | Cada ponto recebe o animal da fase | A compatibilidade narrativa substitui uma tabela de espécies |
| Progressão | Fases consecutivas | Saldo, recintos e árvores persistem; apoio de $400/$600 e necessidades ≥ 60 | Evita fases impossíveis por vitória apertada |
| Árvores | Mecânica de atratividade | Ipê e pequi, $120, +2 bem-estar e +3 visitantes | Implementa M4 |
| Arte | “2D estilizada, vista superior, poucos sprites” | Pixel art original em PNG + interface SVG + animações leves | Cumpre o estilo previsto |
| Áudio | Previsto | Oito efeitos e o tema Noite Estrelada sintetizados, buses, volumes e mute salvos | Sem material de terceiros |
| Tutorial | “Breve” | Instruções por fase + dicas contextuais | Tutorial interativo continua fora do escopo |
| Save | Não exigido | Partida em memória | Igual ao GDD |

## Valores propostos (a validar em teste com jogadores)

Todos os números econômicos foram **propostos pelo desenvolvedor** e provados viáveis por simulação (ver [`economia-e-balanceamento.md`](economia-e-balanceamento.md)). Nenhum foi calibrado com jogadores reais; o GDD já previa essa calibração em tabela e em testes com o público. As soluções canônicas e as contagens de vitórias/derrotas estão documentadas para permitir recalibrar sem perder a referência.

## Histórico

| Data | Mudança |
|---|---|
| 2026-11-03 | Núcleo da economia (`Simulacao`), tabela de parâmetros e prova de balanceamento |
| 2026-11-03 | Fase 1 jogável em graybox: mapa, construir, acolher, cuidados, rodadas, resultado, pausa, ficha provisória |
| 2026-11-03 | Visitantes: receita = visitantes × ingresso e visitantes decorativos no mapa |
| 2026-11-03 | Documentação em Markdown em `docs/` |
| 2026-11-03 | Export para Windows (x86_64) e macOS (universal, sem assinatura) configurado em `export_presets.cfg`; `import_etc2_astc` habilitado |
| 2026-11-24 | Domínio generalizado para vários recintos, espécies e árvores (`Recinto`, `EspecieAnimal`, `EspecieArvore`, `ArvorePlantada`) |
| 2026-11-24 | Autoload `SessaoPartida`: persistência em memória, apoio de transição, avaliação veterinária, snapshot e reinício por fase |
| 2026-11-24 | Três fases completas (`FaseJogo`), com parâmetros próprios e loop até o resumo final |
| 2026-11-24 | Três animais e duas árvores com fichas, nomes científicos, fontes e sprites |
| 2026-11-24 | Arte original, áudio sintetizado, tema, fonte Atkinson Hyperlegible e controles de volume |
| 2026-11-24 | Revisão visual: pixel art PNG para animais/árvores/muda, filtro Nearest, ícones reposicionados e painel lateral de largura fixa |
| 2026-11-24 | Revisão de áudio: vento removido, cinco músicas originais, seletor de faixa no menu e página de audição |
| 2026-11-24 | Trilha reduzida ao tema **Noite Estrelada**; as outras quatro faixas e o seletor do menu foram removidos |
| 2026-11-24 | Sprite do lobo-guará substituído pela versão fornecida pela equipe, mantendo o pipeline de recorte e transparência |
| 2026-11-24 | Textos corrigidos (“2 animais”), tutorial repetido removido das fases 2/3, modal compacto e bloco de cuidados oculto para árvores |
| 2026-11-24 | Feedback visual/sonoro, layout responsivo, `autowrap` no objetivo e rótulos de árvore legíveis |
| 2026-11-24 | `TesteNucleo` ampliado para as três fases; `TesteFase1` reescrito; `TesteJogoCompleto` e `TesteCaptura` adicionados |
| 2026-11-24 | `CREDITS.md`, fontes de conteúdo e roteiro de teste externo documentados |

## Pendências

- [ ] Executar o teste externo com jogadores e registrar observações ([`roteiro-teste-externo.md`](roteiro-teste-externo.md)).
- [ ] Recalibrar os números a partir do teste externo, se necessário.
- [ ] Testar as builds em máquinas reais: o Windows só foi testado sob Wine e o macOS não foi executado nem assinado.
- [ ] Atualizar os PDFs do Marco 1 (GDD e Plano) para C# e para o estado atual.
- [ ] Definir e registrar a licença de distribuição do código e da arte original.
- [ ] Revisar os links das fontes antes da entrega final.
- [ ] Decidir o destino de `scenes/main.tscn` e `scripts/Main.cs` (restos do esqueleto inicial, sem uso).
