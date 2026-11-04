using Godot;

namespace RefugioDoCerrado;

/// <summary>
/// Visitantes decorativos (graybox): quadradinhos entram pela esquerda, andam pela trilha até o recinto,
/// ficam um instante e saem. O ritmo acompanha o número de visitantes da rodada; não há pathfinding
/// nem simulação individual (o GDD manda a visitação ser calculada).
/// </summary>
public partial class FluxoVisitantes : Control
{
	const int MaximoEmCena = 14;
	const float Velocidade = 140f; // pixels por segundo
	const float VisitantesPorSegundoBase = 30f; // intervalo entre chegadas = 30 / visitantes

	readonly RandomNumberGenerator _rng = new();
	Vector2 _destino;
	float _yTrilha = 260f;
	float _intervalo;
	float _acumulado;

	public int EmCena => GetChildCount();
	public float IntervaloEntreChegadas => _intervalo;

	public void Configurar(Vector2 destino, float yTrilha)
	{
		_destino = destino;
		_yTrilha = yTrilha;
	}

	/// <summary>Define o ritmo a partir da quantidade de visitantes da rodada (0 = ninguém entra).</summary>
	public void DefinirVisitantes(int visitantes)
	{
		_intervalo = visitantes <= 0 ? 0f : VisitantesPorSegundoBase / visitantes;
	}

	public override void _Process(double delta)
	{
		if (_intervalo <= 0f) return;
		_acumulado += (float)delta;
		if (_acumulado < _intervalo) return;
		_acumulado = 0f;
		if (GetChildCount() < MaximoEmCena) Criar();
	}

	void Criar()
	{
		var y = _yTrilha + _rng.RandfRange(-14f, 14f);
		var xDestino = _destino.X + _rng.RandfRange(-70f, 70f);
		var cor = Color.FromHsv(_rng.RandfRange(0.5f, 0.65f), 0.55f, 0.9f);
		var visitante = new ColorRect
		{
			Color = cor,
			Size = new Vector2(14, 14),
			Position = new Vector2(-16f, y),
			MouseFilter = MouseFilterEnum.Ignore,
		};
		AddChild(visitante);

		var chegada = new Vector2(xDestino, y);
		var portao = new Vector2(xDestino, _destino.Y);
		var t = visitante.CreateTween();
		t.TweenProperty(visitante, "position", chegada, (chegada.X + 16f) / Velocidade);
		t.TweenProperty(visitante, "position", portao, Mathf.Abs(y - portao.Y) / Velocidade);
		t.TweenInterval(_rng.RandfRange(1.0f, 2.5f));
		t.TweenProperty(visitante, "position", chegada, Mathf.Abs(y - portao.Y) / Velocidade);
		t.TweenProperty(visitante, "position", new Vector2(-16f, y), (chegada.X + 16f) / Velocidade);
		t.TweenCallback(Callable.From(visitante.QueueFree));
	}
}
