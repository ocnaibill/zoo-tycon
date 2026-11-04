using Godot;

namespace RefugioDoCerrado;

public enum EstadoPonto { Livre, Bloqueado, Construido }

/// <summary>Ponto predefinido do mapa onde um recinto pode ser construído (graybox: um botão colorido).</summary>
public partial class PontoRecinto : Button
{
	ColorRect _animal;

	public void Aplicar(string texto, EstadoPonto estado, bool selecionado, bool mostrarAnimal)
	{
		Text = texto;
		var cor = estado switch
		{
			EstadoPonto.Livre => new Color(0.85f, 0.80f, 0.55f),
			EstadoPonto.Construido => new Color(0.55f, 0.75f, 0.50f),
			_ => new Color(0.45f, 0.45f, 0.42f),
		};
		var estilo = new StyleBoxFlat
		{
			BgColor = cor,
			BorderColor = selecionado ? new Color(1f, 1f, 1f) : new Color(0.15f, 0.15f, 0.12f),
		};
		var borda = selecionado ? 5 : 2;
		estilo.SetBorderWidthAll(borda);
		estilo.SetCornerRadiusAll(6);
		foreach (var nome in new[] { "normal", "hover", "pressed", "focus", "disabled" })
			AddThemeStyleboxOverride(nome, estilo);
		AddThemeColorOverride("font_color", new Color(0.1f, 0.1f, 0.08f));
		AddThemeColorOverride("font_hover_color", new Color(0.1f, 0.1f, 0.08f));
		AddThemeColorOverride("font_pressed_color", new Color(0.1f, 0.1f, 0.08f));
		AddThemeColorOverride("font_focus_color", new Color(0.1f, 0.1f, 0.08f));
		AddThemeFontSizeOverride("font_size", 18);

		if (_animal == null)
		{
			_animal = new ColorRect
			{
				Color = new Color(0.85f, 0.45f, 0.1f),
				Size = new Vector2(34, 34),
				MouseFilter = MouseFilterEnum.Ignore,
				Name = "Animal",
			};
			AddChild(_animal);
		}
		_animal.Position = new Vector2(Size.X / 2 - 17, Size.Y - 48);
		_animal.Visible = mostrarAnimal;
	}
}
