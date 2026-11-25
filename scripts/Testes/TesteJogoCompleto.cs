using System.Threading.Tasks;
using Godot;

namespace RefugioDoCerrado.Testes;

/// <summary>
/// Joga a partida inteira pela interface: fase 1, transição, fase 2, transição
/// e fase 3 até a conclusão. Usa as sequências canônicas provadas no TesteNucleo.
/// Rode: godot-mono --headless --path . res://scenes/testes/teste_jogo_completo.tscn
/// </summary>
public partial class TesteJogoCompleto : Node
{
	int _falhas;

	public override async void _Ready()
	{
		var f = await NovaPartida();

		GD.Print("== Fase 1: construir, cuidar e concluir");
		ClicaModal(f);
		Confere(Texto(f, "TituloFase").Contains("Fase 1"), "fase 1 carregada");
		Clica(f, "Ponto1"); Clica(f, "BotaoConstruir"); ClicaModal(f);
		Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		Cuidado(f, 0); Cuidado(f, 1); Cuidado(f, 2);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar");
		ClicaModal(f);
		Confere(TituloModal(f).Contains("concluída"), $"fase 1 concluída ({TituloModal(f)})");
		Confere(Texto(f, "Saldo") == "Saldo: $139", $"saldo final da fase 1 ({Texto(f, "Saldo")})");

		GD.Print("== Transição para a fase 2");
		ClicaModal(f);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Confere(SessaoPartida.Instancia.FaseAtual == 2, "sessão avançou para a fase 2");
		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		GD.Print("== Fase 2: segundo recinto e dois animais");
		f = await Abrir();
		ClicaModal(f);
		Confere(Texto(f, "TituloFase").Contains("Fase 2"), "fase 2 carregada");
		Confere(Texto(f, "Saldo") == "Saldo: $539", $"saldo herdado + apoio ({Texto(f, "Saldo")})");
		Clica(f, "Ponto3");
		Confere(f.GetNode<Button>("%BotaoConstruir").Disabled, "terceiro ponto bloqueado na fase 2");
		Clica(f, "Ponto2"); Clica(f, "BotaoConstruir"); ClicaModal(f);

		// R1: recinto 1 recebe água; recinto 2 recebe manutenção.
		Clica(f, "Ponto1"); Cuidado(f, 1);
		Clica(f, "Ponto2"); Cuidado(f, 2);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		// R2: recinto 1 alimentação + manutenção; recinto 2 alimentação + água.
		Clica(f, "Ponto1"); Cuidado(f, 0); Cuidado(f, 2);
		Clica(f, "Ponto2"); Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		// R3: recinto 1 com tudo; recinto 2 com alimentação + água.
		Clica(f, "Ponto1"); Cuidado(f, 0); Cuidado(f, 1); Cuidado(f, 2);
		Clica(f, "Ponto2"); Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar");
		ClicaModal(f);
		Confere(TituloModal(f).Contains("Fase 2 concluída"), $"fase 2 concluída ({TituloModal(f)})");
		Confere(Texto(f, "Saldo") == "Saldo: $221", $"saldo final da fase 2 ({Texto(f, "Saldo")})");

		GD.Print("== Transição para a fase 3");
		ClicaModal(f);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Confere(SessaoPartida.Instancia.FaseAtual == 3, "sessão avançou para a fase 3");
		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		GD.Print("== Fase 3: terceiro recinto, duas árvores e conclusão");
		f = await Abrir();
		ClicaModal(f);
		Confere(Texto(f, "TituloFase").Contains("Fase 3"), "fase 3 carregada");
		Confere(Texto(f, "Saldo") == "Saldo: $821", $"saldo herdado + apoio ({Texto(f, "Saldo")})");
		Clica(f, "Ponto3"); Clica(f, "BotaoConstruir"); ClicaModal(f);

		ClicaArvore(f, 0); Clica(f, "PlantarIpe"); ClicaModal(f);
		ClicaArvore(f, 1); Clica(f, "PlantarPequi"); ClicaModal(f);
		Confere(f.GetNode<Control>("%PontosArvore").GetChildCount() >= 2, "dois pontos de árvore disponíveis");
		Confere(f.GetNode<RichTextLabel>("%Objetivo").Text.Contains("2"), "objetivo exige duas árvores");

		// R1: apenas o recinto 1 recebe alimentação + água.
		Clica(f, "Ponto1"); Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		// R2: recinto 1 manutenção; recinto 2 tudo; recinto 3 alimentação + água.
		Clica(f, "Ponto1"); Cuidado(f, 2);
		Clica(f, "Ponto2"); Cuidado(f, 0); Cuidado(f, 1); Cuidado(f, 2);
		Clica(f, "Ponto3"); Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		// R3: todos os cuidados em todos os recintos.
		for (var id = 1; id <= 3; id++)
		{
			Clica(f, "Ponto" + id);
			Cuidado(f, 0); Cuidado(f, 1); Cuidado(f, 2);
		}
		Clica(f, "BotaoEncerrar");
		ClicaModal(f);
		Confere(TituloModal(f).Contains("conclusão"), $"conclusão exibida ({TituloModal(f)})");
		Confere(!SessaoPartida.Instancia.EmPartida, "partida encerrada após a última fase");
		Confere(f.GetNode<Button>("%BotaoEncerrar").Disabled, "não dá para jogar depois do fim");
		var corpo = M(f).GetNode<RichTextLabel>("%Corpo").Text;
		Confere(corpo.Contains("Saldo final") && corpo.Contains("Bem-estar global"), "resumo final mostra saldo e bem-estar");

		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		GD.Print(_falhas == 0 ? "RESULTADO: OK" : $"RESULTADO: {_falhas} FALHA(S)");
		GetTree().Quit(_falhas == 0 ? 0 : 1);
	}

	void Confere(bool cond, string msg)
	{
		GD.Print((cond ? "  ok   " : "  FALHA ") + msg);
		if (!cond) _falhas++;
	}

	async Task<FaseJogo> NovaPartida()
	{
		SessaoPartida.Instancia.IniciarNovaPartida();
		return await Abrir();
	}

	async Task<FaseJogo> Abrir()
	{
		var fase = GD.Load<PackedScene>("res://scenes/fase_1.tscn").Instantiate<FaseJogo>();
		fase.NaoTrocarCena = true;
		AddChild(fase);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		return fase;
	}

	static void Clica(Node n, string nome) => n.GetNode<BaseButton>("%" + nome).EmitSignal(BaseButton.SignalName.Pressed);
	static void ClicaArvore(Node n, int i) =>
		n.GetNode<Control>("%PontosArvore").GetChild(i).EmitSignal(BaseButton.SignalName.Pressed);
	static string Texto(Node n, string nome) => n.GetNode<Label>("%" + nome).Text;
	static Modal M(Node f) => f.GetNode<Modal>("%Modal");
	static string TituloModal(Node f) => M(f).GetNode<Label>("%Titulo").Text;
	static void ClicaModal(Node f, string botao = "BotaoA") =>
		M(f).GetNode<BaseButton>("%" + botao).EmitSignal(BaseButton.SignalName.Pressed);
	static void Cuidado(Node f, int i) =>
		f.GetNode<VBoxContainer>("%ListaCuidados").GetChild(i).GetChild(0).GetChild<Button>(1)
			.EmitSignal(BaseButton.SignalName.Pressed);
}
