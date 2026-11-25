using System.Collections.Generic;
using Godot;

namespace RefugioDoCerrado;

/// <summary>
/// Áudio centralizado: buses Master/Music/SFX, volumes persistidos em user://config.cfg
/// e sons originais sintetizados em assets/audio.
/// </summary>
public partial class AudioManager : Node
{
	public static AudioManager Instancia { get; private set; }

	const string CaminhoConfig = "user://config.cfg";
	const string CaminhoAudio = "res://assets/audio/";

	const string FaixaTema = "musica_noite";

	readonly List<AudioStreamPlayer> _sfx = new();
	AudioStreamPlayer _musica;
	readonly RandomNumberGenerator _rng = new();
	int _proximo;
	bool _pronto;

	public float VolumeSfx { get; private set; } = 0.8f;
	public float VolumeMusica { get; private set; } = 0.45f;
	public bool Mudo { get; private set; }

	public override void _Ready()
	{
		Instancia = this;
		ProcessMode = ProcessModeEnum.Always;
		ConfigurarBuses();
		CarregarConfig();

		for (var i = 0; i < 6; i++)
		{
			var p = new AudioStreamPlayer { Bus = "SFX", ProcessMode = ProcessModeEnum.Always };
			AddChild(p);
			_sfx.Add(p);
		}

		_musica = new AudioStreamPlayer { Bus = "Music", ProcessMode = ProcessModeEnum.Always };
		AddChild(_musica);
		_musica.Finished += () => _musica.Play();
		AplicarVolumes();
		TocarTema();
		_pronto = true;
	}

	void TocarTema()
	{
		if (_musica == null) return;
		var faixa = GD.Load<AudioStream>($"{CaminhoAudio}{FaixaTema}.wav");
		if (faixa is AudioStreamWav wav)
		{
			wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
			wav.LoopBegin = 0;
			wav.LoopEnd = (int)(wav.GetLength() * wav.MixRate);
		}
		_musica.Stream = faixa;
		_musica.Play();
	}

	public void TocarSfx(string nome)
	{
		if (!_pronto) return;
		var caminho = CaminhoAudio + nome + ".wav";
		if (!ResourceLoader.Exists(caminho)) return;
		var stream = GD.Load<AudioStream>(caminho);
		if (stream == null || _sfx.Count == 0) return;

		var player = _sfx[_proximo];
		_proximo = (_proximo + 1) % _sfx.Count;
		player.Stream = stream;
		player.PitchScale = _rng.RandfRange(0.97f, 1.03f);
		player.Play();
	}

	public void DefinirVolumeSfx(float valor)
	{
		VolumeSfx = Mathf.Clamp(valor, 0f, 1f);
		AplicarVolumes();
		SalvarConfig();
	}

	public void DefinirVolumeMusica(float valor)
	{
		VolumeMusica = Mathf.Clamp(valor, 0f, 1f);
		AplicarVolumes();
		SalvarConfig();
	}

	public void DefinirMudo(bool mudo)
	{
		Mudo = mudo;
		AplicarVolumes();
		SalvarConfig();
	}

	void AplicarVolumes()
	{
		var master = AudioServer.GetBusIndex("Master");
		if (master >= 0) AudioServer.SetBusMute(master, Mudo);
		var sfx = AudioServer.GetBusIndex("SFX");
		if (sfx >= 0) AudioServer.SetBusVolumeDb(sfx, VolumeSfx <= 0.001f ? -80f : Mathf.LinearToDb(VolumeSfx));
		var musica = AudioServer.GetBusIndex("Music");
		if (musica >= 0) AudioServer.SetBusVolumeDb(musica, VolumeMusica <= 0.001f ? -80f : Mathf.LinearToDb(VolumeMusica));
	}

	void ConfigurarBuses()
	{
		GarantirBus("Music");
		GarantirBus("SFX");
	}

	static void GarantirBus(string nome)
	{
		if (AudioServer.GetBusIndex(nome) >= 0) return;
		var idx = AudioServer.BusCount;
		AudioServer.AddBus(idx);
		AudioServer.SetBusName(idx, nome);
		AudioServer.SetBusSend(idx, "Master");
	}

	void CarregarConfig()
	{
		var cfg = new ConfigFile();
		if (cfg.Load(CaminhoConfig) != Error.Ok) return;
		VolumeSfx = (float)cfg.GetValue("audio", "sfx", VolumeSfx);
		VolumeMusica = (float)cfg.GetValue("audio", "musica", VolumeMusica);
		Mudo = (bool)cfg.GetValue("audio", "mudo", false);
	}

	void SalvarConfig()
	{
		var cfg = new ConfigFile();
		cfg.SetValue("audio", "sfx", VolumeSfx);
		cfg.SetValue("audio", "musica", VolumeMusica);
		cfg.SetValue("audio", "mudo", Mudo);
		cfg.Save(CaminhoConfig);
	}
}
