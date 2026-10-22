# Refúgio do Cerrado

> Título provisório.

Simulação educativa de gestão de um zoológico de acolhimento de animais resgatados do Cerrado, no Distrito Federal. Em três fases curtas, o jogador equilibra receitas e despesas, cuida dos animais e melhora o espaço com vegetação regional.

Projeto da disciplina **Projeto Aplicado VII — Jogos Digitais** (UnDF, 2026.2), da professora Cintia Costa.
Equipe: Bianco da Costa Oliveira, Jhon Wilker e Gabriel Domingos.

- **Engine:** Godot 4.7.2 (versão **.NET / Mono**)
- **Linguagem:** C#
- **Estilo:** 2D com visão superior (top-down)
- **Plataforma-alvo:** PC (Windows)

Os documentos de planejamento (One-Page Concept, GDD do Marco 1 e Plano de Projeto) estão em [`docs/`](docs/).

---

## Passo a passo para começar

### 1. Instalar as ferramentas

| Ferramenta | Versão | Observação |
|---|---|---|
| [Git](https://git-scm.com/downloads) | qualquer recente | |
| [Git LFS](https://git-lfs.com/) | qualquer recente | **Obrigatório**: imagens, áudio e fontes são versionados com LFS |
| [Godot](https://godotengine.org/download) | **4.7.2, versão .NET** | Baixe a build **".NET"**. A versão padrão (só GDScript) **não abre** este projeto |
| [.NET SDK](https://dotnet.microsoft.com/download) | 8 ou superior | O projeto usa `net8.0` |

Um editor de código é opcional, mas ajuda: [Rider](https://www.jetbrains.com/rider/), [VS Code](https://code.visualstudio.com/) com a extensão C# ou Visual Studio.

No Arch/CachyOS: `sudo pacman -S dotnet-sdk git-lfs` e o Godot .NET pelo AUR (`godot-mono-bin`).

### 2. Configurar o Git (uma vez por máquina)

```bash
git lfs install
git config --global user.name "Seu Nome"
git config --global user.email "seu-email@exemplo.com"
```

### 3. Clonar o repositório

```bash
git clone git@github.com:ocnaibill/zoo-tycon.git
cd zoo-tycon
```

Se os arquivos de imagem aparecerem como textos pequenos de poucas linhas (ponteiros do LFS), rode `git lfs pull`.

### 4. Abrir no Godot

1. Abra o Godot (.NET) e, no Project Manager, clique em **Importar** e selecione o arquivo `project.godot` da pasta.
2. Na primeira abertura o Godot importa os recursos e compila o C#. Isso pode levar alguns minutos.
3. Se aparecer erro de compilação ou o plugin não carregar, vá em **Build** (martelo no topo do editor) e compile de novo, ou rode `dotnet build` na pasta do projeto.

Para jogar, aperte **F5**. A cena principal é `scenes/main.tscn`.

### 5. Primeiro teste

Ao rodar, o painel **Output** deve mostrar:

```
Refúgio do Cerrado iniciado.
```

Se aparecer, o ambiente está funcionando.

---

## Estrutura do projeto

```
scenes/    cenas (.tscn): menu, fases, recintos, interface
scripts/   código C# (namespace RefugioDoCerrado)
data/      Resources (.tres) com parâmetros e tabelas de balanceamento
assets/    arte, áudio e fontes (versionados com Git LFS)
addons/    plugins do Godot (godot_mcp)
docs/      documentos de planejamento (PDF)
CREDITS.md registro de autoria, origem e licença de todo recurso externo
```

### Convenções de C# no Godot

- A classe do script precisa ser `partial` e ter **o mesmo nome do arquivo** (`Main.cs` → `public partial class Main`). Sem isso o Godot não associa o script ao nó.
- Namespace: `RefugioDoCerrado`.
- Prefira cenas pequenas (uma por recinto, painel ou fase). Cada pessoa trabalhando em cenas diferentes evita conflito de merge nos `.tscn`.

---

## Trabalhando em equipe com Git

- A branch `main` deve estar sempre abrindo e rodando.
- Crie **branches curtas por tarefa**, por exemplo `feat/rodada-economia` ou `fix/saldo-negativo`, e abra um Pull Request. Um cartão do Trello só vai para *Concluído* depois de revisado por outra pessoa.
- Commits pequenos, com mensagem objetiva (`feat:`, `fix:`, `docs:`, `chore:`).
- **Assets externos:** todo recurso baixado (arte, áudio, fonte) precisa ter autor, origem, licença e atribuição registrados em [`CREDITS.md`](CREDITS.md) **antes** do commit. Use só material com licença compatível.
- Confira com `git lfs ls-files` se os binários de arte e áudio estão indo pelo LFS. Se um `.png` novo não aparecer na lista, o `git lfs install` não foi feito na sua máquina.
- Não commite as pastas `.godot/`, `bin/`, `obj/` e `.ai-game-dev/`. Elas já estão no `.gitignore`.

### Exportar o jogo (Windows)

1. No Godot: **Project → Export → Add… → Windows Desktop**.
2. Na primeira vez, baixe os *export templates* quando o editor pedir.
3. Exporte para uma pasta fora do repositório, ou dentro de `build/` (ignorada pelo Git).

Teste o executável em outro computador antes de cada marco de entrega.

---

## Opcional: Godot-MCP (agente de IA controlando o editor)

O projeto inclui o addon [Godot-MCP](https://github.com/IvanMurzak/Godot-MCP), que permite a um agente de IA (por exemplo o Claude Code) criar nós, editar cenas e scripts e capturar telas do editor. **É totalmente opcional.** Ele roda em modo local: o servidor MCP é um binário baixado na sua máquina, sem conta e sem enviar o projeto para nuvem.

O arquivo [`.mcp.json`](.mcp.json) já está no repositório e aponta para `http://localhost:8080/mcp`. Ele não contém chave nem token.

Para usar:

1. Abra o projeto no Godot (a compilação do C# carrega o addon).
2. No painel do addon (**AI Game Developer**), escolha o modo **Custom** com a URL `http://localhost:8080` e inicie o servidor local (**Start Server**). Na primeira vez ele baixa o binário do servidor para `.ai-game-dev/`, que o Git ignora.
3. Abra o agente de IA na pasta do projeto. O Claude Code lê o `.mcp.json` e pede para aprovar o servidor `ai-game-developer`.

Para outros agentes (Cursor, VS Code, etc.), instale o CLI e gere a configuração:

```bash
npm install -g godot-cli
godot-cli setup-mcp --list
godot-cli setup-mcp <agente> . --url http://localhost:8080 --no-pin
```

**Cuidados:**

- **Não use** `godot-cli login` nem o modo *Cloud*. Ele grava uma chave pessoal (`agd_pk_…`) na configuração, e essa chave **nunca** pode ir para o repositório.
- O addon inclui a ferramenta `script-execute`, que executa código C# dentro do editor. Use apenas com agentes e servidores de sua confiança.
- Revise o que o agente alterou (`git diff`) antes de commitar.

---

## Escopo (resumo)

1 mapa, 3 fases, 3 animais, até 3 recintos em pontos predefinidos, 2 tipos de árvore, economia por rodadas, cuidados, fichas educativas, menu e resumo final. Fora do escopo: 3D, multiplayer, construção livre, mobile e web. Detalhes em [`docs/`](docs/).

> **Nota:** os documentos do Marco 1 citam GDScript. A decisão atual do grupo é usar **C#**, e os documentos serão atualizados.
