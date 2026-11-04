using System.Threading.Tasks;
using Godot;

namespace RefugioDoCerrado.Testes;

/// <summary>
/// Joga a fase 1 pela interface (cliques simulados) e confere os números e os textos.
/// Rode: godot-mono --headless --path . res://scenes/testes/teste_fase_1.tscn
/// </summary>
public partial class TesteFase1 : Node
{
	int _falhas;

	public override async void _Ready()
	{
		await Vitoria();
		await Derrota();
		await Bloqueio();
		await Pausa();
		GD.Print(_falhas == 0 ? "RESULTADO: OK" : $"RESULTADO: {_falhas} FALHA(S)");
		GetTree().Quit(_falhas == 0 ? 0 : 1);
	}

	void Confere(bool cond, string msg)
	{
		GD.Print((cond ? "  ok   " : "  FALHA ") + msg);
		if (!cond) _falhas++;
	}

	async Task<Control> Nova()
	{
		var fase = GD.Load<PackedScene>("res://scenes/fase_1.tscn").Instantiate<Control>();
		AddChild(fase);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		return fase;
	}

	static void Clica(Node n, string nome) => n.GetNode<BaseButton>("%" + nome).EmitSignal(BaseButton.SignalName.Pressed);
	static string Texto(Node n, string nome) => n.GetNode<Label>("%" + nome).Text;
	static Modal M(Node f) => f.GetNode<Modal>("%Modal");
	static string TituloModal(Node f) => M(f).GetNode<Label>("%Titulo").Text;
	static void ClicaModal(Node f, string botao = "BotaoA") => M(f).GetNode<BaseButton>("%" + botao).EmitSignal(BaseButton.SignalName.Pressed);
	static void Cuidado(Node f, int i)
	{
		BotaoCuidado(f, i).EmitSignal(BaseButton.SignalName.Pressed);
	}
	static Button BotaoCuidado(Node f, int i) =>
		f.GetNode<VBoxContainer>("%ListaCuidados").GetChild(i).GetChild(0).GetChild<Button>(1);

	async Task Vitoria()
	{
		GD.Print("== Cenário 1: vitória pela interface");
		var f = await Nova();
		Confere(M(f).Visible && TituloModal(f).Contains("Fase 1"), "instruções aparecem ao abrir a fase");
		ClicaModal(f);
		Confere(!M(f).Visible, "botão Começar fecha a janela");
		Confere(Texto(f, "Saldo") == "Saldo: $550", $"saldo inicial 550 ({Texto(f, "Saldo")})");
		Confere(f.GetNode<Button>("%BotaoConstruir").Disabled, "construir bloqueado sem ponto selecionado");

		Clica(f, "Ponto2");
		Confere(f.GetNode<Button>("%BotaoConstruir").Disabled, "ponto 2 não permite construir na fase 1");
		Clica(f, "Ponto1");
		Confere(!f.GetNode<Button>("%BotaoConstruir").Disabled, "ponto 1 livre permite construir");
		Clica(f, "BotaoConstruir");
		Confere(Texto(f, "Saldo") == "Saldo: $100", $"construir cobra 450 na hora ({Texto(f, "Saldo")})");
		Confere(f.GetNode<FluxoVisitantes>("%Visitantes").EmCena == 0, "sem animal ninguém visita antes de construir");
		Confere(M(f).Visible && TituloModal(f).Contains("hóspede"), "animal acolhido por evento narrativo");
		ClicaModal(f);

		// Rodada 1: sobram 80 depois da operação; alimentação+água (55) cabem, os três (85) não.
		Cuidado(f, 0); Cuidado(f, 1);
		Confere(BotaoCuidado(f, 2).Disabled, "manutenção bloqueada: não cabe no saldo");
		var previsto = Texto(f, "Previsao");
		GD.Print("  previsão: " + previsto);
		Confere(previsto.Contains("visitantes"), "previsão mostra visitantes × ingresso");
		await ToSignal(GetTree().CreateTimer(1.6), Godot.Timer.SignalName.Timeout);
		Confere(f.GetNode<FluxoVisitantes>("%Visitantes").EmCena > 0, $"visitantes aparecem no mapa ({f.GetNode<FluxoVisitantes>("%Visitantes").EmCena})");
		Clica(f, "BotaoEncerrar");
		Confere(M(f).Visible && TituloModal(f).Contains("Rodada 1"), "resumo da rodada 1 aparece");
		var corpo = M(f).GetNode<RichTextLabel>("%Corpo").Text;
		Confere(corpo.Contains("Saldo anterior") && corpo.Contains("Receita") && corpo.Contains("Cuidados"), "resumo mostra saldo, receita e cuidados");
		Confere(corpo.Contains("visitantes"), "resumo mostra a conta de visitantes × ingresso");
		Confere(corpo.Contains("Alimentação") && corpo.Contains("Manutenção"), "resumo explica a causa de cada necessidade");
		ClicaModal(f);
		Confere(Texto(f, "Rodada") == "Rodada 2/3", "avançou para a rodada 2");

		Cuidado(f, 0); Cuidado(f, 1); Cuidado(f, 2);
		Clica(f, "BotaoEncerrar"); ClicaModal(f);
		Cuidado(f, 0); Cuidado(f, 1);
		Clica(f, "BotaoEncerrar");
		Confere(M(f).GetNode<Button>("%BotaoA").Text == "Ver resultado", "última rodada oferece o resultado");
		ClicaModal(f);
		Confere(TituloModal(f).Contains("concluída"), $"vitória exibida ({TituloModal(f)})");
		Confere(f.GetNode<Button>("%BotaoEncerrar").Disabled, "não dá para jogar depois do fim");
		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	async Task Derrota()
	{
		GD.Print("== Cenário 2: derrota explica o motivo e permite reiniciar");
		var f = await Nova();
		ClicaModal(f);
		Clica(f, "Ponto1"); Clica(f, "BotaoConstruir"); ClicaModal(f);
		for (var i = 0; i < 3; i++) { Clica(f, "BotaoEncerrar"); ClicaModal(f); }
		Confere(TituloModal(f).Contains("não foi concluída"), $"falha exibida ({TituloModal(f)})");
		var corpo = M(f).GetNode<RichTextLabel>("%Corpo").Text;
		Confere(corpo.Contains("Bem-estar") && corpo.Contains("abaixo"), "falha lista os motivos");
		Confere(M(f).GetNode<Button>("%BotaoA").Text == "Reiniciar fase", "botão de reiniciar presente");
		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	async Task Bloqueio()
	{
		GD.Print("== Cenário 3: sem construir não há animal, ficha nem cuidados");
		var f = await Nova();
		ClicaModal(f);
		Confere(f.GetNode<Button>("%BotaoFicha").Disabled, "ficha só depois do acolhimento");
		Confere(BotaoCuidado(f, 0).Disabled, "cuidados bloqueados sem animal");
		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	async Task Pausa()
	{
		GD.Print("== Cenário 4: Esc abre e fecha a pausa");
		var f = await Nova();
		ClicaModal(f);
		Input.ParseInputEvent(new InputEventAction { Action = "ui_cancel", Pressed = true });
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Confere(M(f).Visible && TituloModal(f) == "Pausa", "Esc abre a pausa");
		Input.ParseInputEvent(new InputEventAction { Action = "ui_cancel", Pressed = false });
		Input.ParseInputEvent(new InputEventAction { Action = "ui_cancel", Pressed = true });
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Confere(!M(f).Visible, "Esc de novo fecha a pausa");
		f.QueueFree();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
}
