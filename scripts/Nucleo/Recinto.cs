using System.Collections.Generic;

namespace RefugioDoCerrado.Nucleo;

/// <summary>Estado de um recinto durante a fase atual.</summary>
public sealed class Recinto
{
	public int Id { get; set; }
	public EspecieAnimal Especie { get; set; }
	public bool Construido { get; set; }
	public int[] Necessidades { get; set; } = new int[3];
	public HashSet<Cuidado> Planejados { get; set; } = new();

	public Recinto(int id, EspecieAnimal especie)
	{
		Id = id;
		Especie = especie;
	}

	public bool Planejado(Cuidado cuidado) => Planejados.Contains(cuidado);

	public Recinto Clonar() => new(Id, Especie)
	{
		Construido = Construido,
		Necessidades = (int[])Necessidades.Clone(),
		Planejados = new HashSet<Cuidado>(Planejados),
	};
}

/// <summary>Árvore já plantada em um ponto do mapa.</summary>
public sealed class ArvorePlantada
{
	public int Ponto { get; set; }
	public int RecintoId { get; set; }
	public EspecieArvore Especie { get; set; }

	public ArvorePlantada(int ponto, int recintoId, EspecieArvore especie)
	{
		Ponto = ponto;
		RecintoId = recintoId;
		Especie = especie;
	}

	public ArvorePlantada Clonar() => new(Ponto, RecintoId, Especie);
}
