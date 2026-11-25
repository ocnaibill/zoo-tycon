using Godot;

namespace RefugioDoCerrado.Nucleo;

/// <summary>
/// Dados fixos de uma espécie de animal: identificação, ficha educativa,
/// fontes e o sprite usado no mapa.
/// </summary>
[GlobalClass]
public partial class EspecieAnimal : Resource
{
	[Export] public string Nome { get; set; } = "";
	[Export] public string NomeCientifico { get; set; } = "";
	[Export(PropertyHint.MultilineText)] public string Ficha { get; set; } = "";
	[Export(PropertyHint.MultilineText)] public string Fontes { get; set; } = "";
	[Export] public string SpritePath { get; set; } = "";
	[Export] public Color Cor { get; set; } = new(0.85f, 0.45f, 0.1f);

	public Texture2D CarregarSprite()
	{
		if (string.IsNullOrWhiteSpace(SpritePath) || !ResourceLoader.Exists(SpritePath)) return null;
		return GD.Load<Texture2D>(SpritePath);
	}
}
