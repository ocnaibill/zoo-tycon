using Godot;

namespace RefugioDoCerrado;

public partial class MenuInicial : Control
{
	const string CenaJogo = "res://scenes/fase_1.tscn";

	HSlider _sliderMusica, _sliderSfx;
	Label _valorMusica, _valorSfx;
	CheckBox _mudo;

	public override void _Ready()
	{
		TemaUI.Aplicar(this);
		_sliderMusica = GetNode<HSlider>("%SliderMusica");
		_sliderSfx = GetNode<HSlider>("%SliderSfx");
		_valorMusica = GetNode<Label>("%ValorMusica");
		_valorSfx = GetNode<Label>("%ValorSfx");
		_mudo = GetNode<CheckBox>("%Mudo");

		var audio = AudioManager.Instancia;
		if (audio != null)
		{
			_sliderMusica.Value = audio.VolumeMusica;
			_sliderSfx.Value = audio.VolumeSfx;
			_mudo.ButtonPressed = audio.Mudo;
		}
		AtualizarRotulos();

		_sliderMusica.ValueChanged += valor =>
		{
			AudioManager.Instancia?.DefinirVolumeMusica((float)valor);
			AtualizarRotulos();
		};
		_sliderSfx.ValueChanged += valor =>
		{
			AudioManager.Instancia?.DefinirVolumeSfx((float)valor);
			AtualizarRotulos();
		};
		_mudo.Toggled += ativo => AudioManager.Instancia?.DefinirMudo(ativo);

		GetNode<Button>("%BotaoJogar").Pressed += AoJogar;
		GetNode<Button>("%BotaoSair").Pressed += AoSair;
		GetNode<Button>("%BotaoJogar").GrabFocus();
	}

	void AtualizarRotulos()
	{
		_valorMusica.Text = $"{Mathf.RoundToInt((float)_sliderMusica.Value * 100)}%";
		_valorSfx.Text = $"{Mathf.RoundToInt((float)_sliderSfx.Value * 100)}%";
	}

	void AoJogar()
	{
		AudioManager.Instancia?.TocarSfx("sfx_confirmar");
		SessaoPartida.Instancia?.IniciarNovaPartida();
		if (!ResourceLoader.Exists(CenaJogo))
		{
			GD.Print("Cena de jogo ainda não existe.");
			return;
		}

		GetTree().ChangeSceneToFile(CenaJogo);
	}

	void AoSair()
	{
		AudioManager.Instancia?.TocarSfx("sfx_clique");
		GetTree().Quit();
	}
}
