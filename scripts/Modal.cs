using System;
using Godot;

namespace RefugioDoCerrado;

/// <summary>Janela modal reutilizável: instruções, resumos, fichas, pausa e resultado.</summary>
public partial class Modal : Control
{
	Label _titulo;
	RichTextLabel _corpo;
	ScrollContainer _rolagem;
	Button _botaoA, _botaoB;
	Action _acaoA, _acaoB;

	public override void _Ready()
	{
		_titulo = GetNode<Label>("%Titulo");
		_corpo = GetNode<RichTextLabel>("%Corpo");
		_rolagem = GetNode<ScrollContainer>("Centro/Painel/Margem/Caixa/Rolagem");
		_botaoA = GetNode<Button>("%BotaoA");
		_botaoB = GetNode<Button>("%BotaoB");

		var estilo = new StyleBoxFlat
		{
			BgColor = new Color(0.10f, 0.15f, 0.095f, 0.99f),
			BorderColor = TemaUI.Ambar,
			ContentMarginLeft = 0,
			ContentMarginTop = 0,
			ContentMarginRight = 0,
			ContentMarginBottom = 0,
		};
		estilo.SetBorderWidthAll(2);
		estilo.SetCornerRadiusAll(12);
		estilo.ShadowColor = new Color(0, 0, 0, 0.55f);
		estilo.ShadowSize = 18;
		GetNode<PanelContainer>("Centro/Painel").AddThemeStyleboxOverride("panel", estilo);

		_botaoA.Pressed += () => Fechar(_acaoA);
		_botaoB.Pressed += () => Fechar(_acaoB);
		_corpo.MetaClicked += AbrirLink;
		_corpo.MetaUnderlined = true;
	}

	public void Mostrar(string titulo, string corpoBbcode, string textoA, Action acaoA = null, string textoB = null, Action acaoB = null, bool compacto = false)
	{
		_titulo.Text = titulo;
		_corpo.Text = corpoBbcode;
		if (compacto)
		{
			_rolagem.CustomMinimumSize = new Vector2(0, 120);
			CallDeferred(nameof(AjustarAlturaCompacta));
		}
		else
		{
			_rolagem.CustomMinimumSize = new Vector2(0, 330);
		}
		GetNode<PanelContainer>("Centro/Painel").CustomMinimumSize = new Vector2(compacto ? 580 : 700, 0);
		_botaoA.Text = textoA;
		_acaoA = acaoA;
		_botaoB.Visible = textoB != null;
		_botaoB.Text = textoB ?? "";
		_acaoB = acaoB;
		Visible = true;
		_rolagem.ScrollVertical = 0;
		_botaoA.GrabFocus();
	}

	void AjustarAlturaCompacta()
	{
		var altura = Mathf.Clamp(_corpo.GetContentHeight() + 12, 100, 330);
		_rolagem.CustomMinimumSize = new Vector2(0, altura);
	}

	void Fechar(Action acao)
	{
		Visible = false;
		acao?.Invoke();
	}

	static void AbrirLink(Variant meta)
	{
		var url = meta.AsString();
		if (url.StartsWith("http://") || url.StartsWith("https://"))
			OS.ShellOpen(url);
	}
}
