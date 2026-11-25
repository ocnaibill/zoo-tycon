using Godot;

namespace RefugioDoCerrado.Nucleo;

/// <summary>
/// Tabela central de parâmetros de uma fase. Os valores moram em data/*.tres
/// (editáveis no Inspetor) e os padrões abaixo são os da fase 1.
/// </summary>
[GlobalClass]
public partial class ParametrosFase : Resource
{
	[ExportGroup("Fase")]
	[Export] public int Numero { get; set; } = 1;
	[Export] public string Nome { get; set; } = "Primeiro acolhimento";
	[Export] public EspecieAnimal Animal { get; set; }
	[Export] public int Rodadas { get; set; } = 3;
	[Export] public int NumeroRecintos { get; set; } = 1;
	[Export] public int AnimaisMinimos { get; set; } = 1;
	[Export] public int ArvoresMinimas { get; set; } = 0;
	[Export] public bool PermiteArvores { get; set; } = false;
	[Export] public int ApoioTransicao { get; set; } = 0;
	[Export] public int NecessidadeMinimaTransicao { get; set; } = 60;

	[ExportGroup("Arborização")]
	[Export] public EspecieArvore Arvore1 { get; set; }
	[Export] public EspecieArvore Arvore2 { get; set; }

	[ExportGroup("Economia")]
	[Export] public int SaldoInicial { get; set; } = 550;
	[Export] public int CustoRecinto { get; set; } = 450;
	[Export] public int OperacaoPorRecinto { get; set; } = 20;
	[Export] public int PrecoIngresso { get; set; } = 2;
	[Export] public int VisitantesBase { get; set; } = 20;
	[Export] public float VisitantesPorPontoBemEstar { get; set; } = 0.4f;

	[ExportGroup("Cuidados (custo por animal, por rodada)")]
	[Export] public int CustoAlimentacao { get; set; } = 40;
	[Export] public int CustoAgua { get; set; } = 15;
	[Export] public int CustoManutencao { get; set; } = 30;

	[ExportGroup("Necessidades (0 a 100)")]
	[Export] public int NecessidadeInicial { get; set; } = 70;
	[Export] public int EfeitoCuidado { get; set; } = 50;
	[Export] public int QuedaAlimentacao { get; set; } = 25;
	[Export] public int QuedaAgua { get; set; } = 25;
	[Export] public int QuedaManutencao { get; set; } = 15;

	[ExportGroup("Objetivo")]
	[Export] public int MetaBemEstar { get; set; } = 70;
	[Export] public int NecessidadeMinima { get; set; } = 40;
	[Export] public int ReservaMinima { get; set; } = 0;

	public string NomeAnimal => Animal?.Nome ?? "animal";
	public string NomeAnimalMinusculo => NomeAnimal.ToLower();
}
