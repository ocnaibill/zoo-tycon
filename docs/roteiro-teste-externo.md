# Roteiro de teste externo (B08)

> **Status: não executado.** Este documento prepara a sessão com jogadores; os resultados devem ser registrados no fim, sem inventar dados.

## Objetivo

Verificar se uma pessoa que nunca jogou entende o loop, consegue chegar ao fim das três fases sem ajuda e percebe as relações entre **cuidado → bem-estar → visitantes → receita → reserva**. O teste também coleta impressões sobre clareza, dificuldade e conteúdo educativo.

## Participantes

- **5 a 8 pessoas** que não participaram do desenvolvimento.
- Perfil desejado: pelo menos 3 com pouca experiência em jogos de gestão; variedade de idade e familiaridade com o Cerrado.
- Sessões individuais de **20 a 30 minutos**, presenciais ou por chamada de vídeo com compartilhamento de tela.
- Consentimento verbal para observação e registro anônimo. Não gravar sem autorização.

## Material

- Build mais recente (Windows ou macOS) ou o projeto aberto no Godot.
- Cronômetro.
- Formulário de observação (seção 5) e termo simples de consentimento.
- Caderno ou planilha para anotar falas e momentos de confusão.
- Opcional: captura de tela durante a sessão, com autorização.

## Condução

### 1. Abertura (2 min)

Leia em voz alta:

> “Você vai testar um jogo educativo sobre um refúgio de animais resgatados do Cerrado. Não existe resposta certa e o objetivo é avaliar o jogo, não você. Pode falar em voz alta o que estiver pensando. Eu não vou ensinar como jogar; se travar, diga o que está tentando fazer.”

### 2. Tarefas (15–20 min)

Peça que a pessoa jogue sem interferência. Intervenha só se ela ficar parada por mais de 2 minutos ou pedir ajuda.

| # | Tarefa | O que observar | Critério de sucesso |
|---|---|---|---|
| 1 | Abrir o jogo, ajustar o volume se quiser e começar | Encontra *Jogar*; entende o menu de áudio | Inicia a fase 1 sem ajuda |
| 2 | Ler as instruções e construir o primeiro recinto | Entende o custo, o ponto livre e o bloqueio por saldo | Constrói o recinto na fase 1 |
| 3 | Cuidar do lobo-guará e encerrar a primeira rodada | Lê a previsão; entende por que não cabem os três cuidados | Explica a escolha antes de encerrar |
| 4 | Vencer a fase 1 | Percebe a meta de bem-estar, necessidades e saldo | Chega à vitória, com ou sem uma tentativa de reinício |
| 5 | Avançar para a fase 2 e construir o segundo recinto | Entende a persistência do saldo e o apoio de transição | Constrói o recinto 2 |
| 6 | Plantar uma árvore e abrir a ficha dela | Encontra os pontos de plantio, entende custo/efeito e lê a ficha | Planta e abre a ficha |
| 7 | Chegar à fase 3 e concluir a partida | Entende a exigência de duas árvores e a reserva de $120 | Conclui as três fases ou explica onde travou |
| 8 | Abrir a ficha de um animal e uma fonte | Reconhece o conteúdo educativo e o link | Abre a ficha e clica em uma fonte |

### 3. Perguntas abertas (5 min)

1. O que você entendeu que precisava fazer em cada fase?
2. O que foi mais difícil: dinheiro, tempo, cuidar de vários animais ou entender os números?
3. A previsão ajudou a decidir? Em que momento você olhou para ela?
4. O que as árvores mudaram no jogo?
5. Alguma informação pareceu cortada, pequena ou confusa?
6. Você abriu alguma ficha? O texto fez sentido? Confiaria nas fontes?
7. O que você faria diferente se jogasse de novo?
8. Nota de dificuldade (1 = muito fácil, 5 = muito difícil) e nota geral (1–5).

## Formulário de observação

| Campo | Registro |
|---|---|
| Código do participante | P01, P02… (sem nome) |
| Data e duração | |
| Perfil (experiência com jogos, idade aproximada) | |
| Fase alcançada | 1 / 2 / 3 / conclusão |
| Tempo até construir o 1º recinto | |
| Tempo total | |
| Vitórias / derrotas por fase | |
| Precisou de ajuda? Em quê? | |
| Momentos de confusão (fala ou ação) | |
| Erros de interpretação da previsão | |
| Achou as árvores? Plantou? | |
| Abriu fichas? Leu as fontes? | |
| Comentários sobre arte, som e layout | |
| Dificuldade (1–5) e nota geral (1–5) | |
| Sugestões | |

## Critérios de sucesso da rodada

- Pelo menos **4 de 6** participantes constroem o primeiro recinto sem ajuda.
- Pelo menos **3 de 6** explicam corretamente por que não conseguem pagar os três cuidados na rodada 1.
- Pelo menos **3 de 6** chegam à fase 2; pelo menos **2 de 6** concluem a partida no tempo previsto.
- Nenhum participante relata texto cortado ou informação essencial invisível em 1280×720.
- A maioria entende que árvores aumentam bem-estar e visitantes.
- Nenhuma ficha é apontada como incoerente com a fonte; erros factuais são corrigidos antes da entrega.

Esses números são um **alvo inicial**, não uma reprovação automática. O valor principal está nas observações qualitativas.

## Registro de resultados

Preencha após as sessões, sem identificar pessoas:

| Participante | Fase máxima | Tempo | Ajuda | Dificuldade | Observações principais |
|---|---|---|---|---|---|
| P01 | | | | | |
| P02 | | | | | |
| P03 | | | | | |
| P04 | | | | | |
| P05 | | | | | |
| P06 | | | | | |

### Síntese

- **O que funcionou:**
- **Problemas recorrentes:**
- **Mudanças de interface/economia propostas:**
- **Mudanças de conteúdo/fontes propostas:**
- **Decisão:** manter / ajustar / repetir teste

## Depois do teste

1. Classifique cada problema: bloqueante, importante ou cosmético.
2. Corrija primeiro o que impede a conclusão ou induz a erro econômico.
3. Se os números mudarem, recalibre os `.tres`, rode `TesteNucleo` e atualize [`economia-e-balanceamento.md`](economia-e-balanceamento.md).
4. Registre as decisões em [`registro-de-decisoes.md`](registro-de-decisoes.md).
5. Repita o teste com pelo menos 3 pessoas depois dos ajustes maiores.

## Limitações

- Uma sessão única não mede aprendizado; mede compreensão inicial e usabilidade.
- O moderador influencia o comportamento. Use o mesmo roteiro com todos.
- O jogo ainda não coleta telemetria; os tempos e erros dependem de observação manual.
