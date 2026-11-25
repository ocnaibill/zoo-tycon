using System.Collections.Generic;
using Godot;

namespace RefugioDoCerrado;

/// <summary>
/// Visitantes decorativos: entram pela esquerda, caminham pela trilha até um recinto
/// construído, observam por um instante e vão embora. O ritmo acompanha os visitantes
/// calculados pela simulação; não há pathfinding nem simulação individual.
/// </summary>
public partial class FluxoVisitantes : Control
{
	const int MaximoEmCena = 18;
	const float Velocidade = 145f;
	const float VisitantesPorSegundoBase = 30f;

	readonly RandomNumberGenerator _rng = new();
	readonly List<Vector2> _destinos = new();
	Texture2D _sprite;
	float _yTrilha = 300f;
	float _intervalo;
	float _acumulado;

	public int EmCena => GetChildCount();
	public float IntervaloEntreChegadas => _intervalo;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		_sprite = GD.Load<Texture2D>("res://assets/art/visitante.svg");
	}

	public void Configurar(IReadOnlyList<Vector2> destinos, float yTrilha)
	{
		_destinos.Clear();
		if (destinos != null) _destinos.AddRange(destinos);
		_yTrilha = yTrilha;
	}

	/// <summary>Define o ritmo a partir da quantidade de visitantes da rodada (0 = ninguém entra).</summary>
	public void DefinirVisitantes(int visitantes)
	{
		_intervalo = visitantes <= 0 || _destinos.Count == 0 ? 0f : VisitantesPorSegundoBase / visitantes;
	}

	public override void _Process(double delta)
	{
		if (_intervalo <= 0f || _destinos.Count == 0) return;
		_acumulado += (float)delta;
		if (_acumulado < _intervalo) return;
		_acumulado = 0f;
		if (GetChildCount() < MaximoEmCena) Criar();
	}

	void Criar()
	{
		if (_destinos.Count == 0) return;
		var destino = _destinos[_rng.RandiRange(0, _destinos.Count - 1)];
		var y = _yTrilha + _rng.RandfRange(-22f, 22f);
		var xDestino = destino.X + _rng.RandfRange(-58f, 58f);
		var cor = Color.FromHsv(_rng.RandfRange(0.02f, 0.15f), 0.35f, 0.95f);

		var visitante = new TextureRect
		{
			Texture = _sprite,
			Size = new Vector2(26, 26),
			Position = new Vector2(-30f, y),
			MouseFilter = MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Modulate = cor,
		};
		AddChild(visitante);

		var chegada = new Vector2(xDestino, y);
		var portao = new Vector2(xDestino, destino.Y + 20f);
		var t = visitante.CreateTween();
		t.TweenProperty(visitante, "position", chegada, (chegada.X + 30f) / Velocidade);
		t.TweenProperty(visitante, "position", portao, Mathf.Abs(y - portao.Y) / Velocidade);
		t.TweenInterval(_rng.RandfRange(0.8f, 2.2f));
		t.TweenProperty(visitante, "position", chegada, Mathf.Abs(y - portao.Y) / Velocidade);
		t.TweenProperty(visitante, "position", new Vector2(-30f, y), (chegada.X + 30f) / Velocidade);
		t.TweenCallback(Callable.From(visitante.QueueFree));
	}
}
