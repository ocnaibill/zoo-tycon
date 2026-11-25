using Godot;

namespace RefugioDoCerrado;

/// <summary>
/// Tema visual único do jogo: fonte Atkinson Hyperlegible (SIL OFL), paleta do Cerrado,
/// botões com foco visível e painéis consistentes. Aplicado na raiz de cada cena.
/// </summary>
public static class TemaUI
{
	public static readonly Color Fundo = new(0.105f, 0.145f, 0.095f);
	public static readonly Color Painel = new(0.145f, 0.205f, 0.135f);
	public static readonly Color PainelClaro = new(0.19f, 0.27f, 0.17f);
	public static readonly Color Texto = new(0.955f, 0.925f, 0.835f);
	public static readonly Color TextoFraco = new(0.78f, 0.82f, 0.68f);
	public static readonly Color Ambar = new(0.86f, 0.62f, 0.22f);
	public static readonly Color AmbarClaro = new(0.98f, 0.78f, 0.38f);
	public static readonly Color Verde = new(0.48f, 0.72f, 0.38f);
	public static readonly Color Perigo = new(0.88f, 0.39f, 0.24f);
	public static readonly Color Borda = new(0.38f, 0.48f, 0.28f);

	public static void Aplicar(Control raiz)
	{
		var regular = GD.Load<FontFile>("res://assets/fonts/AtkinsonHyperlegible-Regular.ttf");
		var negrito = GD.Load<FontFile>("res://assets/fonts/AtkinsonHyperlegible-Bold.ttf");
		if (regular == null) return;

		var tema = new Theme
		{
			DefaultFont = regular,
			DefaultFontSize = 16,
		};

		tema.SetFont("font", "Button", negrito ?? regular);
		tema.SetFontSize("font_size", "Button", 17);
		tema.SetColor("font_color", "Button", Texto);
		tema.SetColor("font_hover_color", "Button", Colors.White);
		tema.SetColor("font_pressed_color", "Button", new Color(0.12f, 0.10f, 0.06f));
		tema.SetColor("font_focus_color", "Button", Colors.White);
		tema.SetColor("font_disabled_color", "Button", new Color(0.55f, 0.52f, 0.46f));
		tema.SetStylebox("normal", "Button", EstiloBotao(PainelClaro, Borda, 2));
		tema.SetStylebox("hover", "Button", EstiloBotao(new Color(0.27f, 0.37f, 0.23f), Ambar, 2));
		tema.SetStylebox("pressed", "Button", EstiloBotao(Ambar, AmbarClaro, 3));
		tema.SetStylebox("focus", "Button", EstiloBotao(new Color(0.27f, 0.37f, 0.23f), AmbarClaro, 3));
		tema.SetStylebox("disabled", "Button", EstiloBotao(new Color(0.18f, 0.20f, 0.17f), new Color(0.28f, 0.30f, 0.25f), 1));

		tema.SetColor("font_color", "Label", Texto);
		tema.SetColor("font_shadow_color", "Label", new Color(0, 0, 0, 0.35f));
		tema.SetConstant("shadow_offset_x", "Label", 1);
		tema.SetConstant("shadow_offset_y", "Label", 1);

		tema.SetColor("default_color", "RichTextLabel", Texto);
		tema.SetFont("normal_font", "RichTextLabel", regular);
		if (negrito != null) tema.SetFont("bold_font", "RichTextLabel", negrito);
		tema.SetFontSize("normal_font_size", "RichTextLabel", 16);
		tema.SetFontSize("bold_font_size", "RichTextLabel", 16);
		tema.SetConstant("line_separation", "RichTextLabel", 2);

		tema.SetStylebox("panel", "PanelContainer", EstiloPainel());
		tema.SetStylebox("panel", "Panel", EstiloPainel());

		tema.SetStylebox("background", "ProgressBar", EstiloBarra(new Color(0.07f, 0.10f, 0.06f), 4));
		tema.SetStylebox("fill", "ProgressBar", EstiloBarra(Verde, 4));
		tema.SetColor("font_color", "ProgressBar", Texto);

		var linha = new StyleBoxLine { Color = Borda, Thickness = 1 };
		tema.SetStylebox("separator", "HSeparator", linha);

		tema.SetStylebox("panel", "PopupPanel", EstiloPainel());
		tema.SetStylebox("panel", "TooltipPanel", EstiloPainel());
		tema.SetColor("font_color", "TooltipLabel", Texto);
		tema.SetStylebox("slider", "HSlider", EstiloBarra(new Color(0.07f, 0.10f, 0.06f), 5));
		tema.SetStylebox("grabber_area", "HSlider", EstiloBarra(Ambar, 5));
		tema.SetStylebox("grabber_area_highlight", "HSlider", EstiloBarra(AmbarClaro, 5));

		raiz.Theme = tema;
	}

	static StyleBoxFlat EstiloPainel()
	{
		var s = new StyleBoxFlat
		{
			BgColor = new Color(Painel, 0.96f),
			BorderColor = Borda,
			ContentMarginLeft = 12,
			ContentMarginTop = 10,
			ContentMarginRight = 12,
			ContentMarginBottom = 10,
		};
		s.SetBorderWidthAll(1);
		s.SetCornerRadiusAll(10);
		s.ShadowColor = new Color(0, 0, 0, 0.35f);
		s.ShadowSize = 6;
		s.ShadowOffset = new Vector2(0, 3);
		return s;
	}

	static StyleBoxFlat EstiloBotao(Color fundo, Color borda, int largura)
	{
		var s = new StyleBoxFlat
		{
			BgColor = fundo,
			BorderColor = borda,
			ContentMarginLeft = 12,
			ContentMarginTop = 6,
			ContentMarginRight = 12,
			ContentMarginBottom = 6,
		};
		s.SetBorderWidthAll(largura);
		s.SetCornerRadiusAll(8);
		return s;
	}

	static StyleBoxFlat EstiloBarra(Color cor, int raio)
	{
		var s = new StyleBoxFlat { BgColor = cor };
		s.SetCornerRadiusAll(raio);
		return s;
	}
}
