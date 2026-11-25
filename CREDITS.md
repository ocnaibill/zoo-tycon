# Créditos e licenças

Registro de autoria, origem, licença e atribuição de todo recurso externo ou de terceiros usado no projeto, conforme a regra do repositório: **o crédito entra junto do recurso**.

## Recursos do jogo

| Recurso | Autor | Origem | Licença | Atribuição |
|---|---|---|---|---|
| Pixel art PNG: lobo-guará, tamanduá-bandeira, tatu-canastra, ipê-amarelo, pequi e muda | Equipe do projeto (Bianco da Costa Oliveira, Jhon Wilker e Gabriel Domingos) | fontes em `tools/sprites_fonte/`, recorte/transparência por [`tools/preparar_sprites.ps1`](tools/preparar_sprites.ps1), saída em `assets/art/`; o lobo-guará usa o sprite fornecido pela equipe e os demais foram produzidos para esta entrega | Autoral do projeto; o sprite do lobo-guará foi fornecido pela equipe; nenhum pixel da imagem de referência inicial foi incorporado | © equipe. A licença de distribuição do repositório ainda não foi definida |
| SVG de interface e cenário: fundo do Cerrado, visitante e ícones de alimentação/água/manutenção | Equipe do projeto (Bianco da Costa Oliveira, Jhon Wilker e Gabriel Domingos) | `assets/art/` (autoral, criado para este projeto) | Autoral do projeto; não contém material de terceiros | © equipe |
| Efeitos sonoros: clique, confirmar, erro, construir, plantar, rodada, vitória, falha | Equipe do projeto | `assets/audio/`, gerados por [`tools/gerar_audio.ps1`](tools/gerar_audio.ps1) | Autoral; síntese digital própria, sem samples de terceiros | © equipe |
| Tema musical: Noite Estrelada | Equipe do projeto | `assets/audio/musica_noite.wav`, gerado por [`tools/gerar_musicas.ps1`](tools/gerar_musicas.ps1) | Autoral; síntese digital própria, sem samples de terceiros | © equipe |
| Fonte Atkinson Hyperlegible — Regular e Bold | Braille Institute of America, Inc. | [Google Fonts — Atkinson Hyperlegible](https://fonts.google.com/specimen/Atkinson+Hyperlegible) | SIL Open Font License 1.1 | O texto completo está em [`assets/fonts/OFL.txt`](assets/fonts/OFL.txt); copyright 2020 Braille Institute of America, Inc. |

**Não foram usados** packs de sprites, bancos de imagens, músicas ou gravações de terceiros. As fichas educativas citam documentos e páginas institucionais apenas como **referência bibliográfica**; elas não são assets e estão listadas em [`docs/fontes-conteudo.md`](docs/fontes-conteudo.md).

## Ferramentas

| Ferramenta | Autor | Origem | Licença | Observação |
|---|---|---|---|---|
| Godot Engine 4.7.2 (.NET) | Juan Linietsky, Ariel Manzur e contribuidores | [godotengine.org](https://godotengine.org/) | MIT | Engine do jogo |
| .NET 8 | Microsoft e contribuidores | [dotnet.microsoft.com](https://dotnet.microsoft.com/) | MIT | Runtime/SDK usado pelo projeto |
| Godot-MCP 0.32.0 | Ivan Murzak | [github.com/IvanMurzak/Godot-MCP](https://github.com/IvanMurzak/Godot-MCP) | MIT, conforme o catálogo do addon | Addon **opcional** de desenvolvimento; não é conteúdo do jogo |

## Verificação

- [x] Toda a arte (PNG e SVG) e todo o áudio (WAV) do jogo são do projeto e estão dentro de `assets/`.
- [x] A pixel art foi produzida para o projeto, com o sprite do lobo-guará fornecido pela equipe; a imagem de referência inicial serviu apenas de guia de estilo e não entrou no jogo.
- [x] A fonte acompanha o texto da licença OFL.
- [x] As referências das fichas estão registradas com URL e data de acesso.
- [x] Áudios, imagens e fonte estão cobertos pelas regras do Git LFS (`.gitattributes`: `*.png`, `*.svg`, `*.wav`, `*.ttf`); confirmar com `git lfs ls-files` depois de adicioná-los ao índice.
- [ ] Definir e registrar a licença de distribuição do código e da arte original do projeto.
- [ ] Revisar os links das fontes antes da entrega final.
