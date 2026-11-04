using System.Threading.Tasks;
using Godot;

namespace RefugioDoCerrado.Testes;

/// <summary>Gera capturas de tela da fase 1 em user://capturas (precisa de renderização, não de --headless).</summary>
public partial class TesteCaptura : Node
{
	public override async void _Ready()
	{
		var dir = ProjectSettings.GlobalizePath("user://capturas");
		DirAccess.MakeDirRecursiveAbsolute(dir);
		var fase = GD.Load<PackedScene>("res://scenes/fase_1.tscn").Instantiate<Control>();
		AddChild(fase);
		await Frames(3);
		await Salvar(dir, "1_instrucoes");
		var modal = fase.GetNode<Modal>("%Modal");
		void ClicaModal() => modal.GetNode<BaseButton>("%BotaoA").EmitSignal(BaseButton.SignalName.Pressed);
		ClicaModal();
		fase.GetNode<BaseButton>("%Ponto1").EmitSignal(BaseButton.SignalName.Pressed);
		await Frames(3);
		await Salvar(dir, "2_ponto_selecionado");
		fase.GetNode<BaseButton>("%BotaoConstruir").EmitSignal(BaseButton.SignalName.Pressed);
		await Frames(3);
		await Salvar(dir, "3_acolhimento");
		ClicaModal();
		var lista = fase.GetNode<VBoxContainer>("%ListaCuidados");
		lista.GetChild(0).GetChild(0).GetChild<Button>(1).EmitSignal(BaseButton.SignalName.Pressed);
		lista.GetChild(1).GetChild(0).GetChild<Button>(1).EmitSignal(BaseButton.SignalName.Pressed);
		await Frames(3);
		await Salvar(dir, "4_cuidados");
		fase.GetNode<BaseButton>("%BotaoEncerrar").EmitSignal(BaseButton.SignalName.Pressed);
		await Frames(3);
		await Salvar(dir, "5_resumo");
		GetTree().Quit();
	}

	async Task Frames(int n)
	{
		for (var i = 0; i < n; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	async Task Salvar(string dir, string nome)
	{
		await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
		var img = GetViewport().GetTexture().GetImage();
		img.SavePng($"{dir}/{nome}.png");
		GD.Print("captura: " + nome + " " + img.GetSize());
	}
}
