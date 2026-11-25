using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado;

/// <summary>Ponto de plantio de árvore nativa. Vazio mostra uma muda; plantado, a copa da espécie.</summary>
public partial class PontoArvore : Button
{
	public int Ponto { get; set; }

	TextureRect _icone;
	Label _rotulo;
	bool _estavaPlantada;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(92, 92);
		ClipContents = false;
		MouseDefaultCursorShape = CursorShape.PointingHand;
		FocusMode = FocusModeEnum.All;

		_icone = new TextureRect
		{
			Name = "Icone",
			MouseFilter = MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
			AnchorRight = 1,
			AnchorBottom = 1,
			OffsetLeft = 6,
			OffsetTop = 4,
			OffsetRight = -6,
			OffsetBottom = -20,
		};
		AddChild(_icone);

		_rotulo = new Label
		{
			Name = "Rotulo",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			AnchorTop = 1,
			AnchorRight = 1,
			AnchorBottom = 1,
			OffsetLeft = 4,
			OffsetTop = -20,
			OffsetRight = -4,
		};
		_rotulo.AddThemeFontSizeOverride("font_size", 11);
		_rotulo.AddThemeConstantOverride("outline_size", 3);
		_rotulo.AddThemeColorOverride("font_outline_color", new Color(0.06f, 0.09f, 0.04f, 0.9f));
		AddChild(_rotulo);

		MouseEntered += () => AudioManager.Instancia?.TocarSfx("sfx_clique");
	}

	static string Rotulo(EspecieArvore especie)
	{
		var nome = especie?.Nome ?? "Árvore";
		if (nome.StartsWith("Ipê", System.StringComparison.OrdinalIgnoreCase)) return "Ipê";
		if (nome.StartsWith("Pequi", System.StringComparison.OrdinalIgnoreCase)) return "Pequi";
		return nome.Length > 10 ? nome[..10] + "…" : nome;
	}

	public void Aplicar(ArvorePlantada arvore, bool selecionado)
	{
		Text = "";
		var plantada = arvore != null;
		_rotulo.Text = plantada ? Rotulo(arvore.Especie) : "Plantar";
		TooltipText = plantada
			? $"{arvore.Especie?.Nome}: +{arvore.Especie?.BonusBemEstar ?? 0} bem-estar, +{arvore.Especie?.BonusVisitantes ?? 0} visitantes. Clique para ver a ficha."
			: "Ponto de plantio. Selecione para plantar uma espécie nativa.";
		_icone.Texture = plantada
			? arvore.Especie?.CarregarSprite()
			: GD.Load<Texture2D>("res://assets/art/muda.png");

		var cor = plantada ? new Color(0.22f, 0.38f, 0.20f) : new Color(0.48f, 0.54f, 0.33f);
		var estilo = new StyleBoxFlat
		{
			BgColor = cor,
			BorderColor = selecionado ? TemaUI.AmbarClaro : new Color(0.18f, 0.24f, 0.13f),
			ContentMarginLeft = 0,
			ContentMarginTop = 0,
			ContentMarginRight = 0,
			ContentMarginBottom = 0,
		};
		estilo.SetBorderWidthAll(selecionado ? 4 : 2);
		estilo.SetCornerRadiusAll(46);
		estilo.ShadowColor = new Color(0, 0, 0, 0.3f);
		estilo.ShadowSize = selecionado ? 8 : 4;
		estilo.ShadowOffset = new Vector2(0, 3);
		foreach (var nome in new[] { "normal", "hover", "pressed", "focus", "disabled" })
			AddThemeStyleboxOverride(nome, estilo);
		_rotulo.AddThemeColorOverride("font_color", plantada ? TemaUI.AmbarClaro : TemaUI.Texto);

		if (plantada && !_estavaPlantada)
		{
			_estavaPlantada = true;
			_icone.Scale = new Vector2(0.3f, 0.3f);
			_icone.PivotOffset = new Vector2(46, 46);
			var t = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			t.TweenProperty(_icone, "scale", Vector2.One, 0.45f);
		}
		else if (!plantada)
		{
			_estavaPlantada = false;
		}
	}
}
