using System.Collections.Generic;
using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado;

/// <summary>Controlador da fase 1: liga a Simulacao (regras) à interface.</summary>
public partial class Fase1 : Control
{
	const string CaminhoParametros = "res://data/parametros_fase_1.tres";
	const string CenaMenu = "res://scenes/menu_inicial.tscn";

	ParametrosFase P;
	Simulacao _sim;
	Modal _modal;
	bool _pausado;
	int _selecionado = -1;
	bool _fim;

	PontoRecinto[] _pontos;
	Label _tituloFase, _rodada, _saldo, _previsao, _mensagem, _bemEstarTexto;
	RichTextLabel _objetivo, _infoPonto;
	Button _botaoConstruir, _botaoFicha, _botaoEncerrar;
	ProgressBar _barraBemEstar;
	readonly Dictionary<Cuidado, (Label rotulo, ProgressBar barra, Button botao)> _linhas = new();

	public override void _Ready()
	{
		P = GD.Load<ParametrosFase>(CaminhoParametros) ?? new ParametrosFase();
		_sim = new Simulacao(P);

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

		_pontos = new[]
		{
			GetNode<PontoRecinto>("%Ponto1"),
			GetNode<PontoRecinto>("%Ponto2"),
			GetNode<PontoRecinto>("%Ponto3"),
		};
		for (var i = 0; i < _pontos.Length; i++)
		{
			var indice = i;
			_pontos[i].Pressed += () => Selecionar(indice);
		}

		CriarLinhasDeCuidado();

		_botaoConstruir.Pressed += Construir;
		_botaoFicha.Pressed += () => MostrarFicha(null);
		_botaoEncerrar.Pressed += EncerrarRodada;

		_objetivo.Text =
			$"[b]Objetivo:[/b] acolher o {P.Animal.ToLower()} e, ao fim da rodada {P.Rodadas}, manter bem-estar ≥ {P.MetaBemEstar}, " +
			$"cada necessidade ≥ {P.NecessidadeMinima} e saldo ≥ ${P.ReservaMinima}.";

		Atualizar();
		MostrarInstrucoes();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsActionPressed("ui_cancel") || _fim) return;
		if (_pausado) { FecharPausa(); return; }
		if (_modal.Visible) return;
		AbrirPausa();
	}

	// ---------- Montagem ----------

	void CriarLinhasDeCuidado()
	{
		var lista = GetNode<VBoxContainer>("%ListaCuidados");
		foreach (var c in Simulacao.Todos)
		{
			var linha = new VBoxContainer();
			linha.AddThemeConstantOverride("separation", 2);
			var topo = new HBoxContainer();
			var rotulo = new Label { SizeFlagsHorizontal = SizeFlags.ExpandFill, VerticalAlignment = VerticalAlignment.Center };
			var botao = new Button { ToggleMode = true, CustomMinimumSize = new Vector2(150, 34) };
			var barra = new ProgressBar
			{
				MinValue = 0, MaxValue = 100, ShowPercentage = false,
				CustomMinimumSize = new Vector2(0, 14), SizeFlagsHorizontal = SizeFlags.ExpandFill,
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
	}

	static void PintarBarra(ProgressBar barra, bool alerta)
	{
		var fundo = new StyleBoxFlat { BgColor = new Color(0.08f, 0.10f, 0.07f) };
		var cheio = new StyleBoxFlat { BgColor = alerta ? new Color(0.90f, 0.45f, 0.15f) : new Color(0.45f, 0.75f, 0.35f) };
		fundo.SetCornerRadiusAll(3);
		cheio.SetCornerRadiusAll(3);
		barra.AddThemeStyleboxOverride("background", fundo);
		barra.AddThemeStyleboxOverride("fill", cheio);
	}

	// ---------- Ações do jogador ----------

	void Selecionar(int i)
	{
		_selecionado = i;
		Atualizar();
	}

	void Construir()
	{
		if (!_sim.Construir())
		{
			Dizer(_sim.MotivoSemConstruir());
			return;
		}
		Dizer($"Recinto construído (−${P.CustoRecinto}). O {P.Animal.ToLower()} foi acolhido!");
		Atualizar();
		_modal.Mostrar(
			"Um novo hóspede!",
			$"A equipe especializada trouxe um [b]{P.Animal.ToLower()}[/b] resgatado ao refúgio.\n\n" +
			$"Ele chegou com todas as necessidades em {P.NecessidadeInicial}. A cada rodada elas caem; " +
			"escolha os cuidados da rodada (eles são cobrados ao encerrá-la) e acompanhe o bem-estar.",
			"Continuar", null,
			"Ver ficha", () => MostrarFicha(null));
	}

	void AlternarCuidado(Cuidado c)
	{
		if (!_sim.Alternar(c))
		{
			Dizer(_sim.MotivoSemCuidar(c));
			Atualizar();
			return;
		}
		Dizer(_sim.Planejado(c)
			? $"{Simulacao.Nome(c)} planejada: −${_sim.Custo(c)} no fim da rodada."
			: $"{Simulacao.Nome(c)} cancelada.");
		Atualizar();
	}

	void EncerrarRodada()
	{
		var r = _sim.Encerrar();
		Atualizar();
		var ultima = r.Estado != EstadoFase.EmAndamento;
		_modal.Mostrar(
			$"Rodada {r.Rodada} concluída",
			MontarResumo(r),
			ultima ? "Ver resultado" : "Próxima rodada",
			() => { if (ultima) MostrarResultado(r); else Dizer($"Rodada {_sim.Rodada} começou. Planeje os cuidados e encerre quando quiser."); });
	}

	// ---------- Atualização da interface ----------

	void Atualizar()
	{
		_tituloFase.Text = $"Fase 1 — {P.Nome}";
		_rodada.Text = $"Rodada {_sim.Rodada}/{P.Rodadas}";
		_saldo.Text = $"Saldo: ${_sim.Saldo}";
		_previsao.Text =
			$"Previsão desta rodada: gastos ${_sim.DespesaPrevista} (operação ${_sim.DespesaRecorrente} + cuidados ${_sim.DespesaCuidados})  ·  " +
			$"receita ${_sim.ReceitaPrevista}  ·  saldo ao fim ${_sim.SaldoPrevisto}";

		foreach (var c in Simulacao.Todos)
		{
			var (rotulo, barra, botao) = _linhas[c];
			var atual = _sim.AnimalAcolhido ? _sim.Necessidade(c) : 0;
			var prevista = _sim.NecessidadePrevista(c);
			var baixo = _sim.AnimalAcolhido && prevista < P.NecessidadeMinima;
			rotulo.Text = _sim.AnimalAcolhido
				? $"{Simulacao.Nome(c)} {atual}→{prevista}{(baixo ? " (baixo!)" : "")}"
				: Simulacao.Nome(c);
			barra.Value = atual;
			PintarBarra(barra, baixo);
			botao.SetPressedNoSignal(_sim.Planejado(c));
			botao.Text = (_sim.Planejado(c) ? "Planejado " : "Fazer ") + $"${_sim.Custo(c)}";
			botao.Disabled = !_sim.PodeAlternar(c);
			botao.TooltipText = botao.Disabled ? _sim.MotivoSemCuidar(c) : "";
		}

		_barraBemEstar.Value = _sim.BemEstar;
		PintarBarra(_barraBemEstar, _sim.AnimalAcolhido && _sim.BemEstarPrevisto < P.MetaBemEstar);
		_bemEstarTexto.Text = _sim.AnimalAcolhido
			? $"Bem-estar {_sim.BemEstar}→{_sim.BemEstarPrevisto} (meta {P.MetaBemEstar})"
			: $"Bem-estar (meta {P.MetaBemEstar})";

		for (var i = 0; i < _pontos.Length; i++)
		{
			var sel = i == _selecionado;
			if (i == 0 && _sim.RecintoConstruido)
				_pontos[i].Aplicar($"Recinto 1\n{P.Animal}", EstadoPonto.Construido, sel, true);
			else if (i == 0)
				_pontos[i].Aplicar("Ponto 1\nCampo aberto\n(livre)", EstadoPonto.Livre, sel, false);
			else
				_pontos[i].Aplicar($"Ponto {i + 1}\n(fase {i + 1})", EstadoPonto.Bloqueado, sel, false);
		}

		AtualizarInfoPonto();
		_botaoFicha.Disabled = !_sim.AnimalAcolhido;
		_botaoEncerrar.Disabled = _fim;
	}

	void AtualizarInfoPonto()
	{
		string info;
		var podeConstruir = false;
		switch (_selecionado)
		{
			case -1:
				info = "Selecione um ponto do mapa para construir.";
				break;
			case 0 when _sim.RecintoConstruido:
				info = $"[b]Recinto 1:[/b] {P.Animal} acolhido. Operação: ${P.OperacaoPorRecinto} por rodada.";
				break;
			case 0:
				podeConstruir = _sim.PodeConstruir;
				info = $"[b]Ponto 1 — Campo aberto[/b]\nCompatível com o {P.Animal.ToLower()}.\n" +
					$"Custo: ${P.CustoRecinto} + operação ${P.OperacaoPorRecinto} por rodada.";
				if (!podeConstruir) info += $"\n[b]Bloqueado:[/b] {_sim.MotivoSemConstruir()}";
				break;
			default:
				info = $"[b]Ponto {_selecionado + 1}[/b]\nSó fica disponível na fase {_selecionado + 1}.";
				break;
		}
		_infoPonto.Text = info;
		_botaoConstruir.Disabled = !podeConstruir;
		_botaoConstruir.Text = $"Construir recinto (${P.CustoRecinto})";
	}

	void Dizer(string texto) => _mensagem.Text = texto;

	// ---------- Janelas ----------

	void MostrarInstrucoes()
	{
		_modal.Mostrar(
			"Refúgio do Cerrado — Fase 1",
			$"Você administra um refúgio para animais resgatados do Cerrado. Nesta fase: acolher o primeiro animal, o {P.Animal.ToLower()}.\n\n" +
			"[b]Como jogar[/b]\n" +
			"•  Selecione o [b]Ponto 1[/b] no mapa e construa o recinto.\n" +
			"•  Em cada rodada, marque os [b]cuidados[/b] (alimentação, água, manutenção). Eles custam dinheiro e são cobrados no fim da rodada.\n" +
			"•  A [b]previsão[/b] mostra gastos, receita e saldo antes de você decidir.\n" +
			"•  Clique em [b]Encerrar rodada[/b] para ver o que aconteceu e por quê.\n\n" +
			$"[b]Objetivo:[/b] bem-estar ≥ {P.MetaBemEstar} ao fim da rodada {P.Rodadas}, sem deixar nenhuma necessidade abaixo de {P.NecessidadeMinima} e sem saldo negativo.  (Esc: pausa)",
			"Começar");
	}

	void MostrarFicha(System.Action aoFechar)
	{
		_modal.Mostrar(
			"Ficha: Lobo-guará",
			"[b]Chrysocyon brachyurus[/b]\n" +
			"É o maior canídeo da América do Sul, reconhecido pelas pernas longas e pela pelagem avermelhada. " +
			"Vive em ambientes abertos, como os campos e o Cerrado.\n\n" +
			"[b]Alimentação:[/b] é onívoro. Come pequenos animais e frutos, como a lobeira.\n" +
			"[b]Conservação:[/b] a perda de habitat e os atropelamentos ameaçam a espécie, que é considerada vulnerável à extinção.\n\n" +
			"[i]Texto provisório do protótipo: as fontes ainda serão registradas e o conteúdo revisado (backlog B08).[/i]",
			"Fechar", aoFechar);
	}

	string MontarResumo(ResumoRodada r)
	{
		var t = new System.Text.StringBuilder();
		t.Append("[table=2]");
		void Linha(string a, string b) => t.Append($"[cell]{a}[/cell][cell][right]{b}[/right][/cell]");
		Linha("Saldo anterior", $"${r.SaldoAnterior}");
		Linha("+ Receita de visitação", $"${r.Receita}");
		Linha("− Despesas recorrentes (operação)", $"${r.DespesasRecorrentes}");
		Linha("− Cuidados", $"${r.DespesasCuidados}");
		Linha("− Investimentos", $"${r.Investimentos}");
		Linha("[b]= Saldo novo[/b]", $"[b]${r.SaldoNovo}[/b]");
		Linha("Bem-estar", $"{r.BemEstarAnterior} → {r.BemEstarNovo}");
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
			_modal.Mostrar(
				"Fase 1 concluída!",
				$"O {P.Animal.ToLower()} está bem cuidado e o refúgio fechou a fase com ${_sim.Saldo} e bem-estar {_sim.BemEstar}.\n\n" +
				"Saldo positivo e cuidados em dia sustentam o refúgio. Na próxima fase, um segundo animal vai exigir mais equilíbrio.\n\n" +
				"[i]As fases 2 e 3 ainda estão em desenvolvimento.[/i]",
				"Jogar de novo", () => GetTree().ReloadCurrentScene(),
				"Menu", IrParaMenu);
		}
		else
		{
			var motivos = new System.Text.StringBuilder();
			foreach (var m in r.MotivosFalha) motivos.Append("•  ").Append(m).Append('\n');
			_modal.Mostrar(
				"A fase não foi concluída",
				$"Os objetivos não foram cumpridos:\n{motivos}\n" +
				"Tente de novo: cuidados em dia aumentam o bem-estar, que também aumenta a receita de visitação. Não há penalidade, a fase recomeça do estado inicial.",
				"Reiniciar fase", () => GetTree().ReloadCurrentScene(),
				"Menu", IrParaMenu);
		}
	}

	void AbrirPausa()
	{
		_pausado = true;
		_modal.Mostrar("Pausa", "O jogo está pausado.", "Continuar", () => _pausado = false, "Voltar ao menu", IrParaMenu);
	}

	void FecharPausa()
	{
		_pausado = false;
		_modal.Visible = false;
	}

	void IrParaMenu() => GetTree().ChangeSceneToFile(CenaMenu);
}
