# Arquitetura técnica

Godot 4.7.2 .NET, C# (`net8.0`), namespace `RefugioDoCerrado`, 2D com visão superior, renderizador GL Compatibility, janela 1280×720.

## Pastas

```
scenes/            cenas (.tscn): menu_inicial, fase_1, modal
scenes/testes/     cenas que rodam os testes automáticos
scripts/           controladores de cena (Fase1, MenuInicial, Modal, PontoRecinto, FluxoVisitantes)
scripts/Nucleo/    regras do jogo, sem dependência de cena (Simulacao, ParametrosFase)
scripts/Testes/    testes automáticos
data/              recursos (.tres) com os parâmetros
docs/              documentação
```

## Separação regras × interface

- **`Simulacao`** guarda todo o estado e as regras: saldo, rodada, recinto, necessidades, cuidados planejados, previsão, encerramento da rodada e avaliação do fim. Não usa nós, cenas nem `GD.*`. Pode ser testada isoladamente e reaproveitada nas fases 2 e 3.
- **`ParametrosFase`** (`Resource`) é a tabela central de parâmetros. Cada fase terá o seu `.tres`.
- **`Fase1`** liga a `Simulacao` à interface: atualiza os textos e as barras a partir do estado e traduz os cliques em chamadas (`Construir`, `Alternar`, `Encerrar`). **Nenhuma regra mora nele.**
- **`Modal`** é uma janela reutilizável (instruções, acolhimento, resumo, ficha, pausa, resultado), chamada por `Mostrar(titulo, corpo, botaoA, acaoA, botaoB, acaoB)`.
- **`PontoRecinto`** é o ponto do mapa (botão colorido por estado: livre, bloqueado, construído).
- **`FluxoVisitantes`** gera os visitantes decorativos a partir do número de visitantes previsto.

## Convenções

- Classes de script são `partial` e têm o mesmo nome do arquivo.
- Nós acessados pelo código usam nome único (`%Nome`) na cena.
- Cada pessoa trabalha em cenas diferentes para evitar conflitos de merge nos `.tscn`.
- Os arquivos `.uid` gerados pelo Godot são versionados.

## Testes automáticos

Rodam sem abrir janela, a partir da raiz do projeto, depois de `dotnet build`:

```bash
godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn   # balanceamento: todas as estratégias
godot-mono --headless --path . res://scenes/testes/teste_fase_1.tscn   # joga a fase pela interface
```

| Teste | O que verifica |
|---|---|
| `TesteNucleo` | Testa 2.048 estratégias, mostra quantas vencem e falha se não houver vitória ou se não houver derrota |
| `TesteFase1` | Quatro cenários pela interface: vitória, derrota com motivos, bloqueios sem animal e pausa com Esc. Confere saldo, previsão, resumo, visitantes e janelas |

Ambos terminam com `RESULTADO: OK` (código de saída 0) ou indicam a falha.

**Capturas de tela** (precisa de renderização; em Linux sem monitor, use `xvfb-run`):

```bash
xvfb-run -a -s "-screen 0 1280x720x24" godot-mono --path . --rendering-driver opengl3 --resolution 1280x720 res://scenes/testes/teste_captura.tscn
```

Os PNG vão para a pasta de dados do usuário do Godot (`user://capturas`).

## Como estender para as fases 2 e 3

1. Crie `data/parametros_fase_2.tres` com os valores da fase.
2. Generalize `Simulacao` para vários recintos (hoje há um) e para a lista de animais, mantendo a fórmula do saldo.
3. Desbloqueie o Ponto 2 em `Fase1.Atualizar`/`AtualizarInfoPonto` (ou crie `Fase2.cs`) conforme a fase.
4. Persista o estado entre fases (recintos, árvores e saldo permanecem): um *autoload* de sessão é o caminho natural.
5. Árvores: pontos de plantio no mapa, custo, ficha e efeito sobre a qualidade do espaço (M4).

## Dependências e cuidados

- O addon **Godot-MCP** é opcional e vem no repositório. Os pacotes `com.IvanMurzak.*` do `RefugioDoCerrado.csproj` precisam estar em versões compatíveis entre si; as extensões usam `Version="*"`, o que pode voltar a quebrar o build quando saírem versões novas.
- Se `dotnet build` acusar `NU1605` (downgrade de pacote), alinhe `ReflectorNet`, `McpPlugin` e `Rql` às versões exigidas pelas extensões ou fixe as versões das extensões.
