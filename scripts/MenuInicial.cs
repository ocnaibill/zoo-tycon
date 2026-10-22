using Godot;

namespace RefugioDoCerrado;

public partial class MenuInicial : Control
{
	const string CenaFase1 = "res://scenes/fase_1.tscn";

	public override void _Ready()
	{
		GetNode<Button>("Centro/Caixa/BotaoJogar").Pressed += AoJogar;
		GetNode<Button>("Centro/Caixa/BotaoSair").Pressed += AoSair;
	}

	void AoJogar()
	{
		if (!ResourceLoader.Exists(CenaFase1))
		{
			GD.Print("Fase 1 ainda não existe.");
			return;
		}

		GetTree().ChangeSceneToFile(CenaFase1);
	}

	void AoSair() => GetTree().Quit();
}
