using Godot;

namespace RefugioDoCerrado;

public enum EstadoPonto { Livre, Bloqueado, Construido }

/// <summary>
/// Ponto do mapa onde um recinto pode ser construído. Mostra o animal em sprite
/// quando o recinto existe e destaca o ponto selecionado com borda e foco.
/// </summary>
public partial class PontoRecinto : Button
{
	TextureRect _sprite;
	Label _titulo;
	Label _detalhe;
	Label _selo;
	bool _animalVisivel;

	public override void _Ready()
	{
		ClipContents = true;
		MouseDefaultCursorShape = CursorShape.PointingHand;
		FocusMode = FocusModeEnum.All;

		_sprite = new TextureRect
		{
			Name = "Sprite",
			MouseFilter = MouseFilterEnum.Ignore,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
			AnchorLeft = 0.5f,
			AnchorTop = 0.10f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.62f,
			OffsetLeft = -72,
			OffsetRight = 72,
		};
		AddChild(_sprite);

		_titulo = new Label
		{
			Name = "Titulo",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Bottom,
			AnchorLeft = 0,
			AnchorTop = 1,
			AnchorRight = 1,
			AnchorBottom = 1,
			OffsetLeft = 6,
			OffsetRight = -6,
			OffsetTop = -48,
			OffsetBottom = -24,
		};
		_titulo.AddThemeFontSizeOverride("font_size", 17);
		AddChild(_titulo);

		_detalhe = new Label
		{
			Name = "Detalhe",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			AnchorLeft = 0,
			AnchorTop = 1,
			AnchorRight = 1,
			AnchorBottom = 1,
			OffsetLeft = 6,
			OffsetRight = -6,
			OffsetTop = -24,
			OffsetBottom = -2,
		};
		_detalhe.AddThemeFontSizeOverride("font_size", 12);
		AddChild(_detalhe);

		_selo = new Label
		{
			Name = "Selo",
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Center,
			AnchorLeft = 0,
			AnchorTop = 0,
			AnchorRight = 1,
			AnchorBottom = 0,
			OffsetLeft = 6,
			OffsetRight = -6,
			OffsetTop = 8,
			OffsetBottom = 28,
		};
		_selo.AddThemeFontSizeOverride("font_size", 12);
		AddChild(_selo);

		MouseEntered += () => AudioManager.Instancia?.TocarSfx("sfx_clique");
	}

	public void Aplicar(string titulo, string detalhe, EstadoPonto estado, bool selecionado, Texture2D sprite, Color corAnimal)
	{
		Text = "";
		TooltipText = detalhe;
		_titulo.Text = titulo;
		_detalhe.Text = detalhe;
		_selo.Text = estado switch
		{
			EstadoPonto.Livre => "LIVRE",
			EstadoPonto.Construido => "ACOLHENDO",
			_ => "BLOQUEADO",
		};

		var cor = estado switch
		{
			EstadoPonto.Livre => new Color(0.74f, 0.67f, 0.44f),
			EstadoPonto.Construido => new Color(0.32f, 0.46f, 0.28f),
			_ => new Color(0.35f, 0.36f, 0.33f),
		};
		var borda = selecionado ? TemaUI.AmbarClaro : new Color(0.16f, 0.18f, 0.13f);
		var largura = selecionado ? 5 : 2;
		var estilo = new StyleBoxFlat
		{
			BgColor = cor,
			BorderColor = borda,
			ContentMarginLeft = 0,
			ContentMarginTop = 0,
			ContentMarginRight = 0,
			ContentMarginBottom = 0,
		};
		estilo.SetBorderWidthAll(largura);
		estilo.SetCornerRadiusAll(12);
		estilo.ShadowColor = new Color(0, 0, 0, 0.28f);
		estilo.ShadowSize = selecionado ? 10 : 5;
		estilo.ShadowOffset = new Vector2(0, 4);
		foreach (var nome in new[] { "normal", "hover", "pressed", "focus", "disabled" })
			AddThemeStyleboxOverride(nome, estilo);

		var corTexto = estado == EstadoPonto.Bloqueado ? new Color(0.82f, 0.82f, 0.76f) : new Color(0.10f, 0.12f, 0.08f);
		foreach (var item in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color", "font_disabled_color" })
			AddThemeColorOverride(item, corTexto);
		_titulo.AddThemeColorOverride("font_color", corTexto);
		_detalhe.AddThemeColorOverride("font_color", new Color(corTexto, 0.85f));
		_selo.AddThemeColorOverride("font_color", selecionado ? Colors.White : new Color(0.15f, 0.17f, 0.12f));

		_sprite.Texture = sprite;
		_sprite.Visible = estado == EstadoPonto.Construido && sprite != null;
		_sprite.Modulate = Colors.White;
		if (estado == EstadoPonto.Construido && sprite != null && !_animalVisivel)
		{
			_animalVisivel = true;
			_sprite.Scale = new Vector2(0.6f, 0.6f);
			_sprite.PivotOffset = _sprite.Size / 2;
			var t = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			t.TweenProperty(_sprite, "scale", Vector2.One, 0.42f);
		}
		else if (estado != EstadoPonto.Construido)
		{
			_animalVisivel = false;
		}

		Disabled = false;
		MouseDefaultCursorShape = estado == EstadoPonto.Bloqueado ? CursorShape.Arrow : CursorShape.PointingHand;
		Modulate = estado == EstadoPonto.Bloqueado ? new Color(0.78f, 0.78f, 0.72f) : Colors.White;
	}
}
