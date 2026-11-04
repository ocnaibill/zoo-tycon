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
	[Export] public string Nome { get; set; } = "Primeiro acolhimento";
	[Export] public string Animal { get; set; } = "Lobo-guará";
	[Export] public int Rodadas { get; set; } = 3;

	[ExportGroup("Economia")]
	[Export] public int SaldoInicial { get; set; } = 550;
	[Export] public int CustoRecinto { get; set; } = 450;
	[Export] public int OperacaoPorRecinto { get; set; } = 20;
	[Export] public int ReceitaBase { get; set; } = 40;
	[Export] public float ReceitaPorPontoBemEstar { get; set; } = 0.8f;

	[ExportGroup("Cuidados (custo por rodada)")]
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
}
