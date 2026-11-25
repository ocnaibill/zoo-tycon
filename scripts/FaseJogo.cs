using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado;

/// <summary>
/// Controlador único das três fases: traduz cliques em chamadas à Simulacao e
/// estado em textos, sprites e animações. Nenhuma regra econômica mora aqui.
/// </summary>
public partial class FaseJogo : Control
{
	const string CenaMenu = "res://scenes/menu_inicial.tscn";

	SessaoPartida _sessao;
	ParametrosFase P;
	Simulacao _sim;
	Modal _modal;

	int _selecionado;
	int _arvoreSelecionada = -1;
	bool _fim;
	bool _pausado;
	bool _pronto;

	/// <summary>Usado pelos testes de interface: avança a sessão sem trocar a cena.</summary>
	public bool NaoTrocarCena { get; set; }

	PontoRecinto[] _pontos;
	Control _mapa;
	FluxoVisitantes _fluxo;
	Label _tituloFase, _rodada, _saldo, _previsao, _mensagem, _bemEstarTexto, _tituloCuidados;
	RichTextLabel _objetivo, _infoPonto;
	Button _botaoConstruir, _botaoFicha, _botaoEncerrar, _plantarIpe, _plantarPequi;
	VBoxContainer _acoesArvore, _listaCuidados;
	HSeparator _separadorCuidados;
	ProgressBar _barraBemEstar;
	readonly Dictionary<Cuidado, (Label rotulo, ProgressBar barra, Button botao)> _linhas = new();
	readonly List<PontoArvore> _pontosArvore = new();

	public override void _Ready()
	{
		TemaUI.Aplicar(this);
		_sessao = SessaoPartida.Instancia;
		if (_sessao == null)
		{
			GD.PushError("SessaoPartida não encontrada. Confira os autoloads do projeto.");
			return;
		}

		if (!_sessao.EmPartida) _sessao.IniciarNovaPartida();
		P = _sessao.ParametrosAtuais;
		_sim = _sessao.CriarSimulacao();

		_modal = GetNode<Modal>("%Modal");
		_tituloFase = GetNode<Label>("%TituloFase");
		_rodada = GetNode<Label>("%Rodada");
		_saldo = GetNode<Label>("%Saldo");
		_previsao = GetNode<Label>("%Previsao");
		_mensagem = GetNode<Label>("%Mensagem");
		_objetivo = GetNode<RichTextLabel>("%Objetivo");
		_infoPonto = GetNode<RichTextLabel>("%InfoPonto");
		_botaoConstruir = GetNode<Button>("%BotaoConstruir");
		_botaoFicha = GetNode<Button>("%BotaoFicha");
		_botaoEncerrar = GetNode<Button>("%BotaoEncerrar");
		_barraBemEstar = GetNode<ProgressBar>("%BarraBemEstar");
		_bemEstarTexto = GetNode<Label>("%BemEstarTexto");
		_tituloCuidados = GetNode<Label>("%TituloCuidados");
		_acoesArvore = GetNode<VBoxContainer>("%AcoesArvore");
		_listaCuidados = GetNode<VBoxContainer>("%ListaCuidados");
		_separadorCuidados = GetNode<HSeparator>("%SeparadorCuidados");
		_plantarIpe = GetNode<Button>("%PlantarIpe");
		_plantarPequi = GetNode<Button>("%PlantarPequi");
		_mapa = GetNode<Control>("%Mapa");
		_fluxo = GetNode<FluxoVisitantes>("%Visitantes");

		_pontos = new[]
		{
			GetNode<PontoRecinto>("%Ponto1"),
			GetNode<PontoRecinto>("%Ponto2"),
			GetNode<PontoRecinto>("%Ponto3"),
		};
		for (var i = 0; i < _pontos.Length; i++)
		{
			var indice = i;
			_pontos[i].Pressed += () => SelecionarRecinto(indice);
		}

		CriarLinhasDeCuidado();
		CriarPontosDeArvore();

		_botaoConstruir.Pressed += Construir;
		_botaoFicha.Pressed += AbrirFichaAtual;
		_botaoEncerrar.Pressed += EncerrarRodada;
		_plantarIpe.Pressed += () => Plantar(P.Arvore1);
		_plantarPequi.Pressed += () => Plantar(P.Arvore2);
		GetNode<Button>("%BotaoMenu").Pressed += AbrirPausa;
		_mapa.Resized += AtualizarVisitantes;

		_selecionado = PrimeiroRecintoSelecionavel();
		Atualizar();
		_pronto = true;
		MostrarInstrucoes();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsActionPressed("ui_cancel") || _fim) return;
		if (_pausado) { FecharPausa(); return; }
		if (_modal.Visible) return;
		AbrirPausa();
	}

	int PrimeiroRecintoSelecionavel()
	{
		for (var i = 0; i < _sim.Recintos.Count; i++)
			if (_sim.Recintos[i].Construido) return i;
		return 0;
	}

	Recinto RecintoSelecionado =>
		_selecionado >= 0 && _selecionado < _sim.Recintos.Count ? _sim.Recintos[_selecionado] : null;

	// ---------- Montagem ----------

	void CriarLinhasDeCuidado()
	{
		var lista = GetNode<VBoxContainer>("%ListaCuidados");
		foreach (var c in Simulacao.Todos)
		{
			var linha = new VBoxContainer();
			linha.AddThemeConstantOverride("separation", 2);
			var topo = new HBoxContainer();
			var rotulo = new Label
			{
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				VerticalAlignment = VerticalAlignment.Center,
			};
			var botao = new Button
			{
				ToggleMode = true,
				CustomMinimumSize = new Vector2(150, 32),
				Alignment = HorizontalAlignment.Left,
			};
			botao.AddThemeConstantOverride("icon_max_width", 24);
			var barra = new ProgressBar
			{
				MinValue = 0,
				MaxValue = 100,
				ShowPercentage = false,
				CustomMinimumSize = new Vector2(0, 11),
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
			};
			var cuidado = c;
			botao.Pressed += () => AlternarCuidado(cuidado);
			topo.AddChild(rotulo);
			topo.AddChild(botao);
			linha.AddChild(topo);
			linha.AddChild(barra);
			lista.AddChild(linha);
			_linhas[c] = (rotulo, barra, botao);
		}

		_linhas[Cuidado.Alimentacao].botao.Icon = GD.Load<Texture2D>("res://assets/art/icone_alimentacao.svg");
		_linhas[Cuidado.Agua].botao.Icon = GD.Load<Texture2D>("res://assets/art/icone_agua.svg");
		_linhas[Cuidado.Manutencao].botao.Icon = GD.Load<Texture2D>("res://assets/art/icone_manutencao.svg");
	}

	void CriarPontosDeArvore()
	{
		var container = GetNode<Control>("%PontosArvore");
		var posicoes = new (float x, float y)[]
		{
			(0.436f, 0.187f),
			(0.812f, 0.585f),
			(0.637f, 0.755f),
			(0.230f, 0.724f),
		};

		for (var i = 0; i < posicoes.Length; i++)
		{
			var ponto = i + 1;
			var p = new PontoArvore
			{
				Name = $"Arvore{ponto}",
				Ponto = ponto,
				AnchorLeft = posicoes[i].x,
				AnchorRight = posicoes[i].x,
				AnchorTop = posicoes[i].y,
				AnchorBottom = posicoes[i].y,
				OffsetRight = 94,
				OffsetBottom = 94,
				MouseFilter = MouseFilterEnum.Stop,
			};
			p.Pressed += () => SelecionarArvore(ponto);
			container.AddChild(p);
			_pontosArvore.Add(p);
		}
	}

	static void PintarBarra(ProgressBar barra, bool alerta)
	{
		var fundo = new StyleBoxFlat { BgColor = new Color(0.07f, 0.10f, 0.06f) };
		var cheio = new StyleBoxFlat { BgColor = alerta ? TemaUI.Perigo : TemaUI.Verde };
		fundo.SetCornerRadiusAll(3);
		cheio.SetCornerRadiusAll(3);
		barra.AddThemeStyleboxOverride("background", fundo);
		barra.AddThemeStyleboxOverride("fill", cheio);
	}

	// ---------- Seleção ----------

	void SelecionarRecinto(int indice)
	{
		_arvoreSelecionada = -1;
		_selecionado = indice;
		Atualizar();
	}

	void SelecionarArvore(int ponto)
	{
		_arvoreSelecionada = ponto;
		_selecionado = -1;
		Atualizar();
	}

	// ---------- Ações do jogador ----------

	void Construir()
	{
		var r = RecintoSelecionado;
		if (r == null) { Dizer("Selecione um ponto do mapa para construir.", true); return; }
		var id = r.Id;
		if (!_sim.Construir(id))
		{
			Dizer(_sim.MotivoSemConstruir(id), true);
			return;
		}

		var especie = _sim.Recinto(id)?.Especie;
		AudioManager.Instancia?.TocarSfx("sfx_construir");
		Dizer($"Recinto {id} construído (−${P.CustoRecinto}). {especie?.Nome ?? "Animal"} acolhido!");
		Atualizar();
		_modal.Mostrar(
			"Um novo hóspede!",
			$"A equipe especializada trouxe um [b]{(especie?.Nome ?? "animal").ToLower()}[/b] resgatado ao refúgio.\n\n" +
			$"Ele chegou com todas as necessidades em {P.NecessidadeInicial}. A cada rodada elas caem; " +
			"escolha os cuidados do recinto (cobrados ao encerrar a rodada) e acompanhe o bem-estar.\n\n" +
			"[i]Dica: o bem-estar global é a média de todos os animais e influencia a visitação.[/i]",
			"Continuar", null,
			"Ver ficha", () => MostrarFichaAnimal(especie));
	}

	void AlternarCuidado(Cuidado c)
	{
		var r = RecintoSelecionado;
		if (r == null || !r.Construido)
		{
			Dizer("Selecione um recinto construído para planejar cuidados.", true);
			return;
		}

		if (!_sim.Alternar(r.Id, c))
		{
			Dizer(_sim.MotivoSemCuidar(r.Id, c), true);
			Atualizar();
			return;
		}

		AudioManager.Instancia?.TocarSfx(_sim.Planejado(r.Id, c) ? "sfx_confirmar" : "sfx_clique");
		Dizer(_sim.Planejado(r.Id, c)
			? $"{Simulacao.Nome(c)} de {r.Especie?.Nome} planejada: −${_sim.Custo(c)} no fim da rodada."
			: $"{Simulacao.Nome(c)} de {r.Especie?.Nome} cancelada.");
		Atualizar();
	}

	void Plantar(EspecieArvore arvore)
	{
		if (_arvoreSelecionada < 0 || arvore == null)
		{
			Dizer("Selecione um ponto de plantio vazio.", true);
			return;
		}

		var recintoId = RecintoSelecionado?.Id ?? 1;
		if (!_sim.Plantar(_arvoreSelecionada, recintoId, arvore))
		{
			Dizer(_sim.MotivoSemPlantar(_arvoreSelecionada, arvore), true);
			return;
		}

		AudioManager.Instancia?.TocarSfx("sfx_plantar");
		Dizer($"{arvore.Nome} plantada (−${arvore.Custo}). +{arvore.BonusBemEstar} bem-estar e +{arvore.BonusVisitantes} visitantes.");
		Atualizar();
		_modal.Mostrar(
			"Árvore nativa plantada",
			$"O [b]{arvore.Nome.ToLower()}[/b] já está no mapa.\n\n" +
			$"Efeito: [b]+{arvore.BonusBemEstar}[/b] de bem-estar global e [b]+{arvore.BonusVisitantes}[/b] visitantes por rodada. " +
			"O efeito entra na previsão imediatamente e é confirmado no fim da rodada.\n\n" +
			"As árvores também rendem a ficha educativa da espécie.",
			"Continuar", null,
			"Ver ficha", () => MostrarFichaArvore(arvore));
	}

	void EncerrarRodada()
	{
		if (_fim || _sim.Estado != EstadoFase.EmAndamento) return;
		var r = _sim.Encerrar();
		AudioManager.Instancia?.TocarSfx("sfx_rodada");
		Atualizar();
		var ultima = r.Estado != EstadoFase.EmAndamento;
		if (ultima)
		{
			AudioManager.Instancia?.TocarSfx(r.Estado == EstadoFase.Vitoria ? "sfx_vitoria" : "sfx_falha");
		}
		_modal.Mostrar(
			$"Rodada {r.Rodada} concluída",
			MontarResumo(r),
			ultima ? "Ver resultado" : "Próxima rodada",
			() =>
			{
				if (ultima) MostrarResultado(r);
				else Dizer($"Rodada {_sim.Rodada} começou. Planeje os cuidados e encerre quando quiser.");
			});
	}

	// ---------- Atualização da interface ----------

	void Atualizar()
	{
		_tituloFase.Text = $"Fase {P.Numero} — {P.Nome}";
		_rodada.Text = $"Rodada {_sim.Rodada}/{P.Rodadas}";
		_saldo.Text = $"Saldo: ${_sim.Saldo}";
		_saldo.AddThemeColorOverride("font_color", _sim.Saldo < 0 ? TemaUI.Perigo : TemaUI.Texto);
		_previsao.Text =
			$"Previsão: gastos ${_sim.DespesaPrevista} (operação ${_sim.DespesaRecorrente} + cuidados ${_sim.DespesaCuidados})" +
			$"  ·  receita ${_sim.ReceitaPrevista} ({_sim.VisitantesPrevistos} visitantes)" +
			$"  ·  saldo ao fim ${_sim.SaldoPrevisto}";
		_objetivo.Text = MontarObjetivo();

		var r = RecintoSelecionado;
		var construido = r?.Construido == true;
		_tituloCuidados.Text = r == null
			? "Cuidados (selecione um recinto)"
			: construido
				? $"Cuidados — {r.Especie?.Nome}"
				: $"Cuidados — recinto {r.Id} (ainda não construído)";

		foreach (var c in Simulacao.Todos)
		{
			var (rotulo, barra, botao) = _linhas[c];
			var atual = construido ? _sim.Necessidade(r.Id, c) : 0;
			var prevista = construido ? _sim.NecessidadePrevista(r.Id, c) : 0;
			var baixo = construido && prevista < P.NecessidadeMinima;
			rotulo.Text = construido
				? $"{Simulacao.Nome(c)} {atual}→{prevista}{(baixo ? " (baixo!)" : "")}"
				: Simulacao.Nome(c);
			barra.Value = atual;
			PintarBarra(barra, baixo);
			botao.SetPressedNoSignal(construido && _sim.Planejado(r.Id, c));
			botao.Text = (construido && _sim.Planejado(r.Id, c) ? "Feito " : "Cuidar ") + $"${_sim.Custo(c)}";
			botao.Disabled = _fim || !construido || !_sim.PodeAlternar(r.Id, c);
			botao.TooltipText = botao.Disabled
				? (construido ? _sim.MotivoSemCuidar(r.Id, c) : "Construa o recinto para acolher o animal.")
				: "";
		}

		_barraBemEstar.Value = _sim.BemEstar;
		PintarBarra(_barraBemEstar, _sim.AnimaisAcolhidos > 0 && _sim.BemEstarPrevisto < P.MetaBemEstar);
		_bemEstarTexto.Text = _sim.AnimaisAcolhidos > 0
			? $"Bem-estar global {_sim.BemEstar}→{_sim.BemEstarPrevisto} (meta {P.MetaBemEstar})"
			: $"Bem-estar global (meta {P.MetaBemEstar})";

		for (var i = 0; i < _pontos.Length; i++)
		{
			var selecionado = i == _selecionado;
			if (i >= P.NumeroRecintos)
			{
				_pontos[i].Aplicar($"Ponto {i + 1}", $"Disponível na fase {i + 1}", EstadoPonto.Bloqueado, selecionado, null, TemaUI.TextoFraco);
				continue;
			}

			var recinto = _sim.Recinto(i + 1);
			if (recinto == null) continue;
			var sprite = recinto.Especie?.CarregarSprite();
			if (recinto.Construido)
				_pontos[i].Aplicar(
					recinto.Especie?.Nome ?? $"Recinto {recinto.Id}",
					$"Recinto {recinto.Id} · bem-estar {_sim.BemEstarRecinto(recinto.Id)}",
					EstadoPonto.Construido, selecionado, sprite, recinto.Especie?.Cor ?? TemaUI.Ambar);
			else
				_pontos[i].Aplicar(
					$"Ponto {recinto.Id}",
					$"Livre para {recinto.Especie?.Nome ?? "o próximo animal"}",
					EstadoPonto.Livre, selecionado, null, recinto.Especie?.Cor ?? TemaUI.Ambar);
		}

		var mostrarArvores = P.PermiteArvores;
		foreach (var ponto in _pontosArvore)
		{
			ponto.Visible = mostrarArvores;
			if (!mostrarArvores) continue;
			ponto.Aplicar(_sim.ArvoreNoPonto(ponto.Ponto), _arvoreSelecionada == ponto.Ponto);
		}
		if (!mostrarArvores) _arvoreSelecionada = -1;

		AtualizarInfoPonto();
		_botaoEncerrar.Disabled = _fim || _sim.Estado != EstadoFase.EmAndamento;
		AtualizarVisitantes();
	}

	void AtualizarInfoPonto()
	{
		var modoArvore = _arvoreSelecionada >= 0;
		_tituloCuidados.Visible = !modoArvore;
		_listaCuidados.Visible = !modoArvore;
		_separadorCuidados.Visible = !modoArvore;

		if (_arvoreSelecionada >= 0)
		{
			_botaoConstruir.Visible = false;
			var arvore = _sim.ArvoreNoPonto(_arvoreSelecionada);
			if (arvore != null)
			{
				_acoesArvore.Visible = false;
				_infoPonto.Text =
					$"[b]{arvore.Especie?.Nome}[/b]\n[i]{arvore.Especie?.NomeCientifico}[/i]\n" +
					$"+{arvore.Especie?.BonusBemEstar ?? 0} bem-estar · +{arvore.Especie?.BonusVisitantes ?? 0} visitantes.\n" +
					"Use o botão de ficha para conhecer a espécie.";
				_botaoFicha.Text = $"Ficha: {arvore.Especie?.Nome}";
				_botaoFicha.Disabled = _fim;
				return;
			}

			var ipe = P.Arvore1;
			var pequi = P.Arvore2;
			_acoesArvore.Visible = true;
			_infoPonto.Text =
				$"[b]Ponto de plantio {_arvoreSelecionada}[/b]\n" +
				"Escolha uma espécie nativa. A árvore custa na hora e passa a valer nesta rodada.\n" +
				$"[b]Efeito por árvore:[/b] +{ipe?.BonusBemEstar ?? 0} bem-estar e +{ipe?.BonusVisitantes ?? 0} visitantes.";
			_plantarIpe.Text = $"Ipê (${ipe?.Custo ?? 0})";
			_plantarPequi.Text = $"Pequi (${pequi?.Custo ?? 0})";
			_plantarIpe.Disabled = _fim || !_sim.PodePlantar(_arvoreSelecionada, ipe);
			_plantarPequi.Disabled = _fim || !_sim.PodePlantar(_arvoreSelecionada, pequi);
			_plantarIpe.TooltipText = _plantarIpe.Disabled ? _sim.MotivoSemPlantar(_arvoreSelecionada, ipe) : "";
			_plantarPequi.TooltipText = _plantarPequi.Disabled ? _sim.MotivoSemPlantar(_arvoreSelecionada, pequi) : "";
			_botaoFicha.Text = "Ficha da árvore";
			_botaoFicha.Disabled = true;
			return;
		}

		_acoesArvore.Visible = false;
		_botaoConstruir.Visible = true;
		var r = RecintoSelecionado;
		if (r == null)
		{
			_infoPonto.Text = "Selecione um ponto do mapa para construir.";
			_botaoConstruir.Disabled = true;
			_botaoFicha.Text = $"Ficha: {P.Animal?.Nome}";
			_botaoFicha.Disabled = _fim;
			return;
		}

		if (r.Construido)
		{
			_infoPonto.Text =
				$"[b]Recinto {r.Id}: {r.Especie?.Nome}[/b]\n" +
				$"[i]{r.Especie?.NomeCientifico}[/i]\n" +
				$"Bem-estar {_sim.BemEstarRecinto(r.Id)} · operação ${P.OperacaoPorRecinto}/rodada.\n" +
				"Os cuidados planejados aparecem no painel abaixo.";
			_botaoConstruir.Disabled = true;
			_botaoConstruir.Text = "Recinto construído";
			_botaoFicha.Text = $"Ficha: {r.Especie?.Nome}";
			_botaoFicha.Disabled = _fim;
			return;
		}

		var pode = _sim.PodeConstruir(r.Id);
		_infoPonto.Text =
			$"[b]Ponto {r.Id} — recinto livre[/b]\n" +
			$"Compatível com o {r.Especie?.Nome?.ToLower()}.\n" +
			$"Custo: ${P.CustoRecinto} + operação ${P.OperacaoPorRecinto} por rodada.";
		if (!pode) _infoPonto.Text += "\n[b]Bloqueado:[/b] saldo insuficiente para construir.";
		_botaoConstruir.Disabled = _fim || !pode;
		_botaoConstruir.Text = $"Construir recinto (${P.CustoRecinto})";
		_botaoFicha.Text = $"Ficha: {r.Especie?.Nome}";
		_botaoFicha.Disabled = _fim;
	}

	string MontarObjetivo()
	{
		var animaisOk = _sim.AnimaisAcolhidos >= P.AnimaisMinimos;
		var bemOk = _sim.AnimaisAcolhidos > 0 && _sim.BemEstar >= P.MetaBemEstar;
		var reservaOk = _sim.Saldo >= P.ReservaMinima;
		var arvoresOk = _sim.ArvoresPlantadas >= P.ArvoresMinimas;
		string Marca(bool ok) => ok ? "[color=#8fbf6f]cumprido[/color]" : "[color=#e8b34a]pendente[/color]";

		var arvores = P.ArvoresMinimas > 0
			? $"Árvores: {_sim.ArvoresPlantadas}/{P.ArvoresMinimas} — {Marca(arvoresOk)}"
			: P.PermiteArvores
				? "Arborização opcional nesta fase"
				: "Sem arborização nesta fase";

		return
			$"[b]Objetivo:[/b] ao fim da rodada {P.Rodadas}, acolher {Simulacao.TextoAnimais(P.AnimaisMinimos)}, " +
			$"bem-estar ≥ {P.MetaBemEstar}, cada necessidade ≥ {P.NecessidadeMinima} e saldo ≥ ${P.ReservaMinima}.\n" +
			$"Animais: {_sim.AnimaisAcolhidos}/{P.AnimaisMinimos} — {Marca(animaisOk)}   ·   " +
			$"Bem-estar: {_sim.BemEstar}/{P.MetaBemEstar} — {Marca(bemOk)}\n" +
			$"Reserva: ${_sim.Saldo}/${P.ReservaMinima} — {Marca(reservaOk)}   ·   {arvores}";
	}

	void AtualizarVisitantes()
	{
		if (_mapa == null) return;
		var destinos = new List<Vector2>();
		for (var i = 0; i < _pontos.Length && i < _sim.Recintos.Count; i++)
		{
			var r = _sim.Recintos[i];
			if (!r.Construido) continue;
			var p = _pontos[i];
			destinos.Add(new Vector2(p.Position.X + p.Size.X / 2f, p.Position.Y + p.Size.Y / 2f));
		}
		_fluxo.Configurar(destinos, _mapa.Size.Y * 0.61f);
		_fluxo.DefinirVisitantes(_sim.VisitantesPrevistos);
	}

	void Dizer(string texto, bool erro = false)
	{
		_mensagem.Text = texto;
		_mensagem.AddThemeColorOverride("font_color", erro ? new Color(1f, 0.72f, 0.62f) : TemaUI.Texto);
		var t = CreateTween();
		t.TweenProperty(_mensagem, "modulate", Colors.White, 0.55f);
	}

	// ---------- Janelas ----------

	void MostrarInstrucoes()
	{
		var extra = P.Numero switch
		{
			1 => "Nesta fase, construa o primeiro recinto e cuide do lobo-guará.",
			2 => "Agora o refúgio recebe um tamanduá-bandeira. Você também pode plantar árvores nativas para melhorar o bem-estar e a visitação.",
			_ => "O tatu-canastra chega ao refúgio. Além de cuidar dos três animais, plante pelo menos duas árvores nativas para fechar o corredor ecológico.",
		};

		var arvores = P.ArvoresMinimas > 0 ? $" e {P.ArvoresMinimas} árvores" : "";
		var objetivo =
			$"{Simulacao.TextoAnimais(P.AnimaisMinimos)}, bem-estar ≥ {P.MetaBemEstar}, " +
			$"cada necessidade ≥ {P.NecessidadeMinima}, saldo ≥ ${P.ReservaMinima}{arvores}.";

		if (P.Numero > 1)
		{
			// As fases 2 e 3 não repetem o tutorial: mostram só o novo desafio e o objetivo.
			_modal.Mostrar(
				$"Fase {P.Numero} — {P.Nome}",
				$"{extra}\n\n[b]Objetivo:[/b] {objetivo}",
				"Continuar",
				compacto: true);
			return;
		}

		_modal.Mostrar(
			"Refúgio do Cerrado — Fase 1",
			$"Você administra um refúgio para animais resgatados do Cerrado. {extra}\n\n" +
			"[b]Como jogar[/b]\n" +
			"•  Clique em um ponto do mapa e construa o recinto para acolher o animal.\n" +
			"•  Selecione um recinto construído e marque os cuidados da rodada. Eles custam dinheiro e são cobrados ao encerrar.\n" +
			"•  Os visitantes pagam ingresso e aumentam com o bem-estar e com árvores nativas.\n" +
			"•  A previsão mostra gastos, receita e saldo antes de você decidir.\n" +
			"•  Use o botão de ficha para consultar a espécie e as fontes.\n\n" +
			$"[b]Objetivo:[/b] {objetivo}" +
			"\n\n[i]Atalhos: Esc pausa; Tab navega pelos botões.[/i]",
			"Começar");
	}

	void AbrirFichaAtual()
	{
		if (_arvoreSelecionada >= 0)
		{
			var arvore = _sim.ArvoreNoPonto(_arvoreSelecionada);
			if (arvore?.Especie == null)
			{
				Dizer("Selecione uma árvore plantada para abrir a ficha.", true);
				return;
			}
			MostrarFichaArvore(arvore.Especie);
			return;
		}

		var r = RecintoSelecionado;
		MostrarFichaAnimal(r?.Especie ?? P.Animal);
	}

	void MostrarFichaAnimal(EspecieAnimal especie)
	{
		if (especie == null)
		{
			Dizer("A ficha desta espécie ainda não está disponível.", true);
			return;
		}

		_modal.Mostrar(
			$"Ficha: {especie.Nome}",
			MontarFicha(especie.Nome, especie.NomeCientifico, especie.Ficha, especie.Fontes),
			"Fechar");
	}

	void MostrarFichaArvore(EspecieArvore arvore)
	{
		if (arvore == null)
		{
			Dizer("A ficha desta árvore ainda não está disponível.", true);
			return;
		}

		_modal.Mostrar(
			$"Ficha: {arvore.Nome}",
			MontarFicha(arvore.Nome, arvore.NomeCientifico, arvore.Ficha, arvore.Fontes),
			"Fechar");
	}

	static string MontarFicha(string nome, string cientifico, string ficha, string fontes)
	{
		var sb = new StringBuilder();
		sb.Append("[b]").Append(nome).Append("[/b]\n");
		if (!string.IsNullOrWhiteSpace(cientifico)) sb.Append("[i]").Append(cientifico).Append("[/i]\n\n");
		if (!string.IsNullOrWhiteSpace(ficha)) sb.Append(ficha).Append("\n\n");
		if (!string.IsNullOrWhiteSpace(fontes))
		{
			sb.Append("[b]Fontes[/b]\n").Append(LinkificarFontes(fontes)).Append("\n\n");
		}
		sb.Append("[i]Conteúdo educativo do protótipo. Consulte as fontes para detalhes e atualizações.[/i]");
		return sb.ToString();
	}

	static string LinkificarFontes(string fontes)
	{
		var sb = new StringBuilder();
		foreach (var linhaOriginal in fontes.Split('\n'))
		{
			var linha = linhaOriginal.Trim();
			if (linha.Length == 0) continue;
			var inicio = linha.IndexOf("http", StringComparison.OrdinalIgnoreCase);
			if (inicio < 0)
			{
				sb.Append("•  ").Append(linha).Append('\n');
				continue;
			}
			var url = linha[inicio..].Trim();
			var prefixo = linha[..inicio].TrimEnd(' ', ':', '-', '—');
			sb.Append("•  ").Append(prefixo).Append(": ");
			sb.Append("[url=").Append(url).Append(']').Append(url).Append("[/url]\n");
		}
		return sb.ToString();
	}

	string MontarResumo(ResumoRodada r)
	{
		var t = new StringBuilder();
		t.Append("[table=2]");
		void Linha(string a, string b) => t.Append($"[cell]{a}[/cell][cell][right]{b}[/right][/cell]");
		Linha("Saldo anterior", $"${r.SaldoAnterior}");
		Linha($"+ Receita ({r.Visitantes} visitantes × ${P.PrecoIngresso})", $"${r.Receita}");
		Linha("− Despesas recorrentes (operação)", $"${r.DespesasRecorrentes}");
		Linha("− Cuidados", $"${r.DespesasCuidados}");
		Linha("− Investimentos", $"${r.Investimentos}");
		Linha("[b]= Saldo novo[/b]", $"[b]${r.SaldoNovo}[/b]");
		Linha("Bem-estar", $"{r.BemEstarAnterior} → {r.BemEstarNovo}");
		Linha("Animais / árvores", $"{r.Animais} / {r.Arvores}");
		t.Append("[/table]\n\n[b]O que aconteceu:[/b]\n");
		foreach (var m in r.Mensagens) t.Append("•  ").Append(m).Append('\n');
		return t.ToString();
	}

	void MostrarResultado(ResumoRodada r)
	{
		_fim = true;
		Atualizar();

		if (r.Estado == EstadoFase.Vitoria)
		{
			var ultima = P.Numero >= SessaoPartida.UltimaFase;
			if (ultima)
			{
				_sessao.ConcluirFase(_sim);
				_modal.Mostrar(
					"Refúgio do Cerrado — conclusão",
					$"Você acolheu os três animais, plantou árvores nativas e fechou a última fase com [b]${_sim.Saldo}[/b] e bem-estar [b]{_sim.BemEstar}[/b].\n\n" +
					"[b]Resumo da partida[/b]\n" +
					$"•  Animais acolhidos: {_sim.AnimaisAcolhidos}\n" +
					$"•  Árvores plantadas: {_sim.ArvoresPlantadas}\n" +
					$"•  Saldo final: ${_sim.Saldo}\n" +
					$"•  Bem-estar global: {_sim.BemEstar}\n\n" +
					"O jogo lembra que conservar o Cerrado é manter habitats, corredores ecológicos e uma comunidade cuidando da fauna e da flora. Obrigado por jogar!",
					"Jogar de novo", () =>
					{
						_sessao.IniciarNovaPartida();
						RecarregarJogo();
					},
					"Menu", IrParaMenu);
				return;
			}

			var proxima = _sessao.CarregarParametros(P.Numero + 1);
			_modal.Mostrar(
				$"Fase {P.Numero} concluída!",
				$"Você cumpriu os objetivos com [b]${_sim.Saldo}[/b] e bem-estar [b]{_sim.BemEstar}[/b].\n\n" +
				$"A próxima fase é [b]Fase {proxima.Numero} — {proxima.Nome}[/b], com o {proxima.NomeAnimalMinusculo}. " +
				$"Você receberá um apoio de transição de [b]${proxima.ApoioTransicao}[/b]; o saldo, os recintos e as árvores continuam com você. " +
				"As necessidades dos animais são avaliadas pela equipe veterinária e sobem para pelo menos " +
				$"{proxima.NecessidadeMinimaTransicao} antes do novo desafio.",
				"Próxima fase", () =>
				{
					_sessao.ConcluirFase(_sim);
					RecarregarJogo();
				},
				"Menu", IrParaMenu);
			return;
		}

		var motivos = new StringBuilder();
		foreach (var m in r.MotivosFalha) motivos.Append("•  ").Append(m).Append('\n');
		_modal.Mostrar(
			"A fase não foi concluída",
			$"Os objetivos não foram cumpridos:\n{motivos}\n" +
			"Tente de novo: cuidados em dia aumentam o bem-estar, que também aumenta a receita de visitação. " +
			"O reinício restaura o estado inicial desta fase, sem perder as fases anteriores.",
			"Reiniciar fase", () =>
			{
				_sessao.ReiniciarFase();
				RecarregarJogo();
			},
			"Menu", IrParaMenu);
	}

	void AbrirPausa()
	{
		if (_fim) return;
		_pausado = true;
		var som = AudioManager.Instancia;
		var estadoSom = som == null
			? "Áudio indisponível."
			: som.Mudo
				? "Som silenciado."
				: $"Volume da música {som.VolumeMusica:0.0} e dos efeitos {som.VolumeSfx:0.0}.";
		_modal.Mostrar(
			"Pausa",
			$"O jogo está pausado.\n\n{estadoSom}\n\nOs volumes podem ser ajustados no menu inicial.",
			"Continuar", () => _pausado = false,
			"Voltar ao menu", IrParaMenu);
	}

	void FecharPausa()
	{
		_pausado = false;
		_modal.Visible = false;
	}

	void RecarregarJogo()
	{
		if (NaoTrocarCena) return;
		GetTree().ReloadCurrentScene();
	}

	void IrParaMenu()
	{
		_sessao?.EncerrarPartida();
		GetTree().ChangeSceneToFile(CenaMenu);
	}
}
