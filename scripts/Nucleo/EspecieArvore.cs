using Godot;

namespace RefugioDoCerrado.Nucleo;

/// <summary>
/// Dados fixos de uma espécie de árvore do Cerrado: ficha, custo de plantio
/// e efeito sobre o bem-estar e a visitação.
/// </summary>
[GlobalClass]
public partial class EspecieArvore : Resource
{
	[Export] public string Nome { get; set; } = "";
	[Export] public string NomeCientifico { get; set; } = "";
	[Export(PropertyHint.MultilineText)] public string Ficha { get; set; } = "";
	[Export(PropertyHint.MultilineText)] public string Fontes { get; set; } = "";
	[Export] public string SpritePath { get; set; } = "";
	[Export] public int Custo { get; set; } = 120;
	[Export] public int BonusBemEstar { get; set; } = 2;
	[Export] public int BonusVisitantes { get; set; } = 3;

	public Texture2D CarregarSprite()
	{
		if (string.IsNullOrWhiteSpace(SpritePath) || !ResourceLoader.Exists(SpritePath)) return null;
		return GD.Load<Texture2D>(SpritePath);
	}
}
