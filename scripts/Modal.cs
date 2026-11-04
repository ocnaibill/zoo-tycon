using System;
using Godot;

namespace RefugioDoCerrado;

/// <summary>Janela modal reutilizável: instruções, resumos, fichas, pausa e resultado.</summary>
public partial class Modal : Control
{
	Label _titulo;
	RichTextLabel _corpo;
	Button _botaoA, _botaoB;
	Action _acaoA, _acaoB;

	public override void _Ready()
	{
		_titulo = GetNode<Label>("%Titulo");
		_corpo = GetNode<RichTextLabel>("%Corpo");
		_botaoA = GetNode<Button>("%BotaoA");
		_botaoB = GetNode<Button>("%BotaoB");
		_botaoA.Pressed += () => Fechar(_acaoA);
		_botaoB.Pressed += () => Fechar(_acaoB);
	}

	public void Mostrar(string titulo, string corpoBbcode, string textoA, Action acaoA = null, string textoB = null, Action acaoB = null)
	{
		_titulo.Text = titulo;
		_corpo.Text = corpoBbcode;
		_botaoA.Text = textoA;
		_acaoA = acaoA;
		_botaoB.Visible = textoB != null;
		_botaoB.Text = textoB ?? "";
		_acaoB = acaoB;
		Visible = true;
		_botaoA.GrabFocus();
	}

	void Fechar(Action acao)
	{
		Visible = false;
		acao?.Invoke();
	}
}
