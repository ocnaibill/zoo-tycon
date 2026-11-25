# Arquitetura técnica

Godot **4.7.2 .NET** (Mono), C# (`net8.0`), namespace `RefugioDoCerrado`, 2D com visão superior, renderizador **GL Compatibility**, janela base 1280×720 com esticamento `canvas_items` e aspecto `expand`. Os autoloads são `AudioManager` e `SessaoPartida`.

## Pastas

```
scenes/                 menu_inicial.tscn, fase_1.tscn (jogo), modal.tscn, main.tscn (resíduo)
scenes/testes/          teste_nucleo, teste_fase_1, teste_jogo_completo, teste_captura
scripts/                controladores de cena e apresentação
scripts/Nucleo/         regras e dados, sem dependência de nós
scripts/Testes/         testes automáticos
data/                   parâmetros das fases, animais e árvores (.tres)
assets/art/             PNGs da pixel art + SVGs de interface/cenário
assets/audio/           WAVs originais
assets/fonts/           Atkinson Hyperlegible + OFL.txt
tools/                  gerar_audio.ps1, gerar_musicas.ps1, preparar_sprites.ps1
tools/sprites_fonte/    PNGs-fonte da pixel art (ignorados pelo Godot)
addons/godot_mcp/       addon opcional de desenvolvimento
docs/                   esta documentação
```

> O arquivo `scenes/fase_1.tscn` conserva o nome histórico, mas é a cena única das três fases. Quem controla é `FaseJogo`; não existe mais `Fase1.cs`.

## Separação regras × apresentação

### Núcleo (`scripts/Nucleo/`)

- **`Simulacao`** — estado e regras de uma fase: saldo, rodada, recintos, árvores, necessidades, cuidados planejados, previsão, encerramento e avaliação do fim. Não usa nós, cenas nem `GD.*`.
- **`ParametrosFase`** (`Resource`) — tabela central de parâmetros: número da fase, animal, número de recintos, animais/árvores exigidos, apoio de transição, economia, cuidados, metas e reserva. Cada fase tem seu `.tres`.
- **`Recinto`** — id, espécie, construído, três necessidades (alimentação, água, manutenção) e conjunto de cuidados planejados. `Clonar()` evita vazamento de estado entre snapshots.
- **`ArvorePlantada`** — ponto, recinto associado e espécie.
- **`EspecieAnimal`** (`Resource`) — nome, nome científico, ficha em BBCode, fontes, caminho do sprite e cor.
- **`EspecieArvore`** (`Resource`) — nome, nome científico, ficha, fontes, sprite, custo, bônus de bem-estar e de visitantes.

A compatibilidade com a fase 1 foi mantida por atalhos (`RecintoConstruido`, `Alternar(Cuidado)`, `BemEstar`, etc.) que operam sobre o primeiro recinto e são usados pelos testes antigos.

### Sessão (`SessaoPartida`, autoload)

Mantém a partida em memória (o GDD não pede save em disco):

- `IniciarNovaPartida()` zera saldo, recintos e árvores e chama `PrepararFase()`.
- `PrepararFase()` aplica o **apoio de transição**, eleva as necessidades dos recintos já construídos a pelo menos `NecessidadeMinimaTransicao` (60) e cria os recintos da fase. Também grava o **snapshot** de saldo, recintos e árvores para o reinício.
- `CriarSimulacao()` entrega uma simulação nova a partir do estado da sessão.
- `AplicarResultado()` copia saldo, recintos e árvores de volta para a sessão.
- `ConcluirFase()` grava o resultado, avança a fase e prepara a próxima; na fase 3 encerra a partida.
- `ReiniciarFase()` restaura o snapshot do início da fase, sem perder as fases anteriores.
- `EncerrarPartida()` é usado ao voltar ao menu.

### Interface (`scripts/`)

- **`FaseJogo`** — controlador único das três fases. Liga a `Simulacao` à cena: atualiza textos, barras e sprites, traduz cliques em `Construir`, `Alternar`, `Plantar` e `Encerrar`, monta fichas e resumos, toca sons e reage ao fim da fase. **Nenhuma regra econômica mora nele.**
- **`PontoRecinto`** — ponto do mapa para construir/acolher; mostra sprite do animal, estado (livre, bloqueado, construído), seleção e rótulos na base.
- **`PontoArvore`** — ponto de plantio; mostra muda, árvore plantada, seleção e rótulo curto (“Ipê”, “Pequi”).
- **`FluxoVisitantes`** — visitantes decorativos que entram pela esquerda e se distribuem entre os recintos construídos. É só visual, sem pathfinding.
- **`Modal`** — janela reutilizável (instruções, ficha, resumo, pausa, resultado, conclusão). O texto usa BBCode; links `http(s)` abrem no navegador via `MetaClicked`.
- **`MenuInicial`** — jogar, sair, sliders de música/SFX e mute. As configurações são persistidas por `AudioManager`.
- **`TemaUI`** — tema programático único: fonte Atkinson Hyperlegible, paleta do Cerrado, botões com foco visível, painéis, barras e sliders.
- **`AudioManager`** — autoload com buses Master/Music/SFX, seis players de efeito com variação leve de pitch, o tema **Noite Estrelada** em loop, volumes e mute persistidos em `user://config.cfg`.

### Fluxo de uma fase

```
Menu → SessaoPartida.IniciarNovaPartida()
     → FaseJogo._Ready() → CriarSimulacao() → instruções
     → construir / cuidar / plantar
     → Encerrar rodada → ResumoRodada → modal
     → 3ª rodada → Vitória ou Falha
         ├─ Vitória e fase < 3 → ConcluirFase() → apoio + veterinária → recarrega a cena
         ├─ Vitória e fase 3   → ConcluirFase() → resumo final da partida
         └─ Falha              → ReiniciarFase() → snapshot do início da fase
```

## Áudio

Todos os WAVs são **originais**, sintetizados no projeto, sem samples externos:

- [`tools/gerar_audio.ps1`](../tools/gerar_audio.ps1) gera os oito efeitos: clique, confirmar, erro, construir, plantar, rodada, vitória e falha.
- [`tools/gerar_musicas.ps1`](../tools/gerar_musicas.ps1) gera o tema **Noite Estrelada** (drone grave, sinos e eco): 32 s, loop contínuo, 22.050 Hz, 16-bit mono, com pico normalizado e sem clipping.

`AudioManager` garante os buses, aplica volumes lineares convertidos para dB e carrega o tema em loop. O volume de música, o volume de efeitos e o mute são persistidos em `user://config.cfg`. O tema pode ser ouvido fora do jogo em [`audicao-musicas.html`](audicao-musicas.html).

## Arte e fonte

- **Pixel art PNG** para os três animais, as duas árvores e a muda. O sprite do lobo-guará é a versão fornecida pela equipe; os demais foram produzidos para o projeto a partir de uma referência visual. Os originais ficam em [`tools/sprites_fonte/`](../tools/sprites_fonte/) e [`tools/preparar_sprites.ps1`](../tools/preparar_sprites.ps1) remove o fundo por preenchimento a partir das bordas, preserva áreas internas, recorta o conteúdo, centraliza em tela quadrada e salva os PNGs finais.
- Os `TextureRect` dos sprites usam **filtro Nearest** para manter os pixels nítidos. Os `.tres` de animais e árvores apontam para os PNGs em `assets/art/`.
- **SVG de interface e cenário:** fundo do Cerrado, visitante e ícones de alimentação/água/manutenção. O Godot importa e escala sem perda.
- Fonte **Atkinson Hyperlegible** (Regular e Bold), SIL OFL 1.1; o texto da licença está em `assets/fonts/OFL.txt`.
- Autoria e licenças em [`CREDITS.md`](../CREDITS.md). Nenhum pack externo de sprite, imagem ou áudio foi usado.

## Testes automáticos

Rodam sem abrir janela, a partir da raiz do projeto, depois de `dotnet build`:

```bash
godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn
godot-mono --headless --path . res://scenes/testes/teste_fase_1.tscn
godot-mono --headless --path . res://scenes/testes/teste_jogo_completo.tscn
```

| Teste | O que verifica |
|---|---|
| `TesteNucleo` | Recursos de conteúdo; fase 1 e fase 2 por enumeração completa; fase 3 por canônica, gulosa e busca aleatória com semente fixa; exige vitória **e** derrota em cada fase |
| `TesteFase1` | Vitória pela interface, derrota com motivos, reinício por snapshot, bloqueios sem animal, ficha e pausa |
| `TesteJogoCompleto` | Partida 1 → 2 → 3 pela interface, saldos de transição, conclusão final, `EmPartida == false` e resumo |

Todos terminam com `RESULTADO: OK` (código de saída 0) ou indicam a falha. Os testes da interface usam a propriedade `FaseJogo.NaoTrocarCena`: ela impede o `ReloadCurrentScene` e permite que o teste descarte e reinstancie a cena sem quebrar o harness. A troca real de fase continua sendo `ReloadCurrentScene`.

Os números atuais estão em [`economia-e-balanceamento.md`](economia-e-balanceamento.md). O teste pode imprimir avisos de objetos/recursos ainda em uso ao encerrar o processo; são avisos de finalização do Godot e não alteram o `RESULTADO: OK`.

### Capturas de tela

Precisam de renderização (não use `--headless`):

```bash
godot-mono --path . --rendering-driver opengl3 --resolution 1280x720 res://scenes/testes/teste_captura.tscn
```

No Windows, as imagens vão para `%APPDATA%\Godot\app_userdata\Refúgio do Cerrado\capturas`:

| Arquivo | Conteúdo |
|---|---|
| `00_menu.png` | Menu com controles de áudio |
| `01_fase1_instrucoes.png` | Instruções da fase 1 |
| `02_fase1_cuidados.png` | Recinto construído e cuidados |
| `03_fase1_resumo.png` | Resumo de rodada |
| `04_fase1_visitantes.png` | Visitantes no mapa |
| `05_fase1_1600x900.png` | Layout em 1600×900 |
| `06_fase2_jogo.png` | Fase 2 com dois recintos |
| `07_fase3_jogo.png` | Fase 3 com três recintos e árvores |
| `08_fase2_instrucoes.png` | Modal compacto da fase 2, sem tutorial repetido |
| `09_fase3_arvore.png` | Árvore selecionada, sem bloco de cuidados e sem deslocar o painel |
| `10_fase3_instrucoes.png` | Modal compacto da fase 3 |
| `11_fase3_arvore_vazia.png` | Ponto de árvore vazio selecionado, antes do plantio |

## Exportação

Os presets `Windows Desktop` e `macOS` estão em `export_presets.cfg`. Instale os *export templates* da **mesma versão .NET** e exporte pelo editor ou por:

```bash
godot-mono --headless --path . --export-release "Windows Desktop"
godot-mono --headless --path . --export-release "macOS"
```

A pasta `build/` não é versionada. As builds não são assinadas; o Windows pode exibir SmartScreen e o macOS exige abrir com botão direito → *Abrir*. O passo a passo completo está no [`README.md`](../README.md) da raiz.

## Dependências e cuidados

- O addon **Godot-MCP** é opcional e fica no repositório. Os pacotes `com.IvanMurzak.*` do `.csproj` precisam ser compatíveis entre si; algumas extensões usam `Version="*"`, o que pode quebrar o build quando sair uma versão incompatível.
- Se `dotnet build` acusar `NU1605`, alinhe `ReflectorNet`, `McpPlugin` e `Rql` ou fixe as versões das extensões.
- `.NET 8` é obrigatório. Nesta máquina, o SDK local fica em `../dotnet8`; um SDK .NET 10 pode falhar com a mensagem “Your Windows doesn't fully support CET”.
- Arquivos `.uid` gerados pelo Godot são versionados. Binários `.wav` e `.ttf` passam pelo Git LFS; `.svg` fica no Git comum.
