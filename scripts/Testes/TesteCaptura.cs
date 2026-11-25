using System.Threading.Tasks;
using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado.Testes;

/// <summary>
/// Gera capturas de tela do menu e das três fases em user://capturas.
/// Precisa de renderização (não use --headless).
/// </summary>
public partial class TesteCaptura : Node
{
	string _dir;

	public override async void _Ready()
	{
		_dir = ProjectSettings.GlobalizePath("user://capturas");
		DirAccess.MakeDirRecursiveAbsolute(_dir);
		SessaoPartida.Instancia.IniciarNovaPartida();

		GD.Print("== Menu");
		var menu = GD.Load<PackedScene>("res://scenes/menu_inicial.tscn").Instantiate<Control>();
		AddChild(menu);
		await Frames(6);
		await Salvar("00_menu");

		menu.QueueFree();
		await Frames(1);

		GD.Print("== Fase 1");
		var p1 = SessaoPartida.Instancia.ParametrosAtuais;
		var f = await AbrirFase();
		await Salvar("01_fase1_instrucoes");
		FechaModal(f);
		Clica(f, "Ponto1");
		Clica(f, "BotaoConstruir");
		FechaModal(f);
		Cuidado(f, 0); Cuidado(f, 1);
		await Frames(4);
		await Salvar("02_fase1_cuidados");
		Clica(f, "BotaoEncerrar");
		await Frames(4);
		await Salvar("03_fase1_resumo");
		FechaModal(f);
		await ToSignal(GetTree().CreateTimer(1.1), Godot.Timer.SignalName.Timeout);
		await Salvar("04_fase1_visitantes");

		GetWindow().Size = new Vector2I(1600, 900);
		await Frames(8);
		await Salvar("05_fase1_1600x900");
		GetWindow().Size = new Vector2I(1280, 720);
		await Frames(8);

		f.QueueFree();
		await Frames(1);

		// Avança a sessão usando a sequência canônica, sem depender da interface.
		var sim1 = new Simulacao(p1);
		for (var r = 1; r <= p1.Rodadas; r++)
		{
			if (r == 1) sim1.Construir();
			foreach (var c in Simulacao.Todos)
				if (Mascara(r == 1 ? 3 : r == 2 ? 7 : 3, c)) sim1.Alternar(c);
			sim1.Encerrar();
		}
		SessaoPartida.Instancia.ConcluirFase(sim1);

		GD.Print("== Fase 2");
		var p2 = SessaoPartida.Instancia.ParametrosAtuais;
		f = await AbrirFase();
		await Frames(4);
		await Salvar("08_fase2_instrucoes");
		FechaModal(f);
		Clica(f, "Ponto2"); Clica(f, "BotaoConstruir"); FechaModal(f);
		Clica(f, "Ponto1");
		await Frames(4);
		await Salvar("06_fase2_jogo");
		f.QueueFree();
		await Frames(1);

		// Avança a fase 2 com o plano canônico.
		var sim2 = SessaoPartida.Instancia.CriarSimulacao();
		sim2.Construir(2);
		var plano2 = new[] { (2, 4), (5, 3), (7, 3) };
		for (var r = 1; r <= p2.Rodadas; r++)
		{
			Aplicar(sim2, 1, plano2[r - 1].Item1);
			Aplicar(sim2, 2, plano2[r - 1].Item2);
			sim2.Encerrar();
		}
		SessaoPartida.Instancia.ConcluirFase(sim2);

		GD.Print("== Fase 3");
		f = await AbrirFase();
		await Frames(4);
		await Salvar("10_fase3_instrucoes");
		FechaModal(f);
		Clica(f, "Ponto3"); Clica(f, "BotaoConstruir"); FechaModal(f);
		ClicaArvore(f, 0);
		await Frames(4);
		await Salvar("11_fase3_arvore_vazia");
		Clica(f, "PlantarIpe"); FechaModal(f);
		ClicaArvore(f, 1); Clica(f, "PlantarPequi"); FechaModal(f);
		ClicaArvore(f, 0);
		await Frames(4);
		await Salvar("09_fase3_arvore");
		Clica(f, "Ponto3");
		Cuidado(f, 0); Cuidado(f, 1);
		await Frames(4);
		await Salvar("07_fase3_jogo");
		f.QueueFree();
		await Frames(1);

		GD.Print("capturas em " + _dir);
		GetTree().Quit();
	}

	async Task<FaseJogo> AbrirFase()
	{
		var fase = GD.Load<PackedScene>("res://scenes/fase_1.tscn").Instantiate<FaseJogo>();
		fase.NaoTrocarCena = true;
		AddChild(fase);
		await Frames(6);
		return fase;
	}

	async Task Frames(int n)
	{
		for (var i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	async Task Salvar(string nome)
	{
		await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
		var img = GetViewport().GetTexture().GetImage();
		img.SavePng($"{_dir}/{nome}.png");
		GD.Print("captura: " + nome + " " + img.GetSize());
	}

	static bool Mascara(int mascara, Cuidado c) => (mascara & (1 << (int)c)) != 0;

	static void Aplicar(Simulacao sim, int recintoId, int mascara)
	{
		foreach (var c in Simulacao.Todos)
			if (Mascara(mascara, c)) sim.Alternar(recintoId, c);
	}

	static void Clica(Node n, string nome) =>
		n.GetNode<BaseButton>("%" + nome).EmitSignal(BaseButton.SignalName.Pressed);

	static void ClicaArvore(Node n, int i) =>
		n.GetNode<Control>("%PontosArvore").GetChild(i).EmitSignal(BaseButton.SignalName.Pressed);

	static void FechaModal(Node n) =>
		n.GetNode<Modal>("%Modal").GetNode<BaseButton>("%BotaoA").EmitSignal(BaseButton.SignalName.Pressed);

	static void Cuidado(Node f, int i) =>
		f.GetNode<VBoxContainer>("%ListaCuidados").GetChild(i).GetChild(0).GetChild<Button>(1)
			.EmitSignal(BaseButton.SignalName.Pressed);
}
