using System.Collections.Generic;
using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado;

/// <summary>
/// Estado da partida entre as fases. Fica em memória (o GDD não pede save em disco):
/// saldo, recintos, árvores, fase atual e um snapshot do início da fase para reiniciar.
/// </summary>
public partial class SessaoPartida : Node
{
	public const int UltimaFase = 3;

	public static SessaoPartida Instancia { get; private set; }

	public bool EmPartida { get; private set; }
	public int FaseAtual { get; private set; } = 1;
	public int Saldo { get; private set; }
	public List<Recinto> Recintos { get; } = new();
	public List<ArvorePlantada> Arvores { get; } = new();

	public int SaldoInicioFase { get; private set; }
	public List<Recinto> SnapshotRecintos { get; private set; } = new();
	public List<ArvorePlantada> SnapshotArvores { get; private set; } = new();
	public int SnapshotSaldo { get; private set; }

	public override void _Ready()
	{
		Instancia = this;
	}

	public ParametrosFase CarregarParametros(int fase)
	{
		var caminho = $"res://data/parametros_fase_{fase}.tres";
		return ResourceLoader.Exists(caminho)
			? GD.Load<ParametrosFase>(caminho)
			: new ParametrosFase { Numero = fase };
	}

	public ParametrosFase ParametrosAtuais => CarregarParametros(FaseAtual);

	public void IniciarNovaPartida()
	{
		EmPartida = true;
		FaseAtual = 1;
		Saldo = CarregarParametros(1).SaldoInicial;
		Recintos.Clear();
		Arvores.Clear();
		PrepararFase();
	}

	/// <summary>
	/// Prepara o estado de início da fase atual. Nas fases 2 e 3 aplica o apoio
	/// de transição e faz uma "avaliação veterinária" que eleva as necessidades
	/// ao mínimo, para uma vitória apertada não tornar a fase seguinte impossível.
	/// </summary>
	public void PrepararFase()
	{
		var p = CarregarParametros(FaseAtual);

		if (FaseAtual > 1)
		{
			Saldo += p.ApoioTransicao;
			foreach (var r in Recintos)
			{
				if (!r.Construido) continue;
				for (var i = 0; i < r.Necessidades.Length; i++)
					r.Necessidades[i] = System.Math.Max(r.Necessidades[i], p.NecessidadeMinimaTransicao);
			}
		}

		var modelo = p.Animal ?? new EspecieAnimal { Nome = "Animal" };
		for (var id = 1; id <= System.Math.Max(1, p.NumeroRecintos); id++)
		{
			if (Recintos.Exists(r => r.Id == id)) continue;
			Recintos.Add(new Recinto(id, modelo));
		}

		SaldoInicioFase = Saldo;
		SnapshotSaldo = Saldo;
		SnapshotRecintos = Clonar(Recintos);
		SnapshotArvores = Clonar(Arvores);
	}

	public Simulacao CriarSimulacao()
	{
		var p = ParametrosAtuais;
		return new Simulacao(p, Saldo, Recintos, Arvores);
	}

	public void AplicarResultado(Simulacao sim)
	{
		Saldo = sim.Saldo;
		Recintos.Clear();
		Recintos.AddRange(sim.ClonarRecintos());
		Arvores.Clear();
		Arvores.AddRange(sim.ClonarArvores());
	}

	/// <summary>Grava o resultado e prepara a fase seguinte. Devolve false quando a fase era a última.</summary>
	public bool ConcluirFase(Simulacao sim)
	{
		AplicarResultado(sim);
		if (FaseAtual >= UltimaFase)
		{
			EmPartida = false;
			return false;
		}

		FaseAtual++;
		PrepararFase();
		return true;
	}

	public void ReiniciarFase()
	{
		Saldo = SnapshotSaldo;
		Recintos.Clear();
		Recintos.AddRange(Clonar(SnapshotRecintos));
		Arvores.Clear();
		Arvores.AddRange(Clonar(SnapshotArvores));
	}

	public void EncerrarPartida() => EmPartida = false;

	static List<Recinto> Clonar(List<Recinto> origem) => origem.ConvertAll(r => r.Clonar());
	static List<ArvorePlantada> Clonar(List<ArvorePlantada> origem) => origem.ConvertAll(a => a.Clonar());
}
