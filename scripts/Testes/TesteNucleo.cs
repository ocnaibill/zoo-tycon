using System.Collections.Generic;
using System.Linq;
using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado.Testes;

/// <summary>
/// Prova de balanceamento da fase 1: testa todas as estratégias possíveis
/// (rodada de construção × cuidados escolhidos a cada rodada) e mostra quais vencem.
/// Rode: godot-mono --headless --path . res://scenes/testes/teste_nucleo.tscn
/// </summary>
public partial class TesteNucleo : Node
{
	const string CaminhoParametros = "res://data/parametros_fase_1.tres";

	public override void _Ready()
	{
		var p = GD.Load<ParametrosFase>(CaminhoParametros) ?? new ParametrosFase();
		GD.Print($"== Parâmetros: saldo {p.SaldoInicial}, recinto {p.CustoRecinto}, operação {p.OperacaoPorRecinto}/rodada, rodadas {p.Rodadas}");

		var vitorias = new List<string>();
		var total = 0;
		var bloqueadas = 0;
		var porMotivo = new Dictionary<string, int>();
		var falhas = new List<string>();

		for (var construir = 1; construir <= p.Rodadas + 1; construir++) // p.Rodadas+1 = nunca constrói
		{
			foreach (var plano in Planos(p.Rodadas))
			{
				var sim = new Simulacao(p);
				var descricao = $"constrói R{construir}";
				ResumoRodada ultimo = null;
				var valido = true;
				for (var r = 1; r <= p.Rodadas && valido; r++)
				{
					if (r == construir && !sim.Construir()) { valido = false; break; }
					foreach (var c in Simulacao.Todos)
					{
						if (!Bit(plano[r - 1], c)) continue;
						if (!sim.Alternar(c)) { valido = false; break; }
					}
					if (!valido) break;
					descricao += $" | R{r}: {Nome(plano[r - 1])}";
					ultimo = sim.Encerrar();
				}
				if (!valido) { bloqueadas++; continue; }
				total++;
				if (ultimo.Estado == EstadoFase.Vitoria) vitorias.Add($"{descricao} → saldo {sim.Saldo}, bem-estar {sim.BemEstar}");
				else
				{
					foreach (var m in ultimo.MotivosFalha) porMotivo[m.Split(' ')[0]] = porMotivo.GetValueOrDefault(m.Split(' ')[0]) + 1;
					if (falhas.Count < 3) falhas.Add(descricao + " → " + string.Join("; ", ultimo.MotivosFalha));
				}
			}
		}

		GD.Print($"== Estratégias válidas (não bloqueadas por saldo): {total}; bloqueadas: {bloqueadas}");
		GD.Print($"== Vencem: {vitorias.Count} ({100.0 * vitorias.Count / total:0.#}%)");
		foreach (var v in vitorias.Take(6)) GD.Print("  OK  " + v);
		var piorMargem = vitorias.Count;
		GD.Print("== Exemplos de falha:");
		foreach (var f in falhas) GD.Print("  X   " + f);
		GD.Print("== Falhas por tipo: " + string.Join(", ", porMotivo.Select(kv => $"{kv.Key}={kv.Value}")));

		// Tem de existir ao menos uma sequência viável e ao menos uma que falha (escolhas relevantes).
		var ok = vitorias.Count > 0 && vitorias.Count < total;
		GD.Print(ok ? "RESULTADO: OK (há vitória e há derrota)" : "RESULTADO: FALHOU (sem escolhas relevantes)");
		GetTree().Quit(ok ? 0 : 1);
	}

	static IEnumerable<int[]> Planos(int rodadas)
	{
		var n = 1;
		for (var i = 0; i < rodadas; i++) n *= 8;
		for (var k = 0; k < n; k++)
		{
			var plano = new int[rodadas];
			var x = k;
			for (var i = 0; i < rodadas; i++) { plano[i] = x % 8; x /= 8; }
			yield return plano;
		}
	}

	static bool Bit(int mascara, Cuidado c) => (mascara & (1 << (int)c)) != 0;

	static string Nome(int mascara) =>
		mascara == 0 ? "nada" : string.Join("+", Simulacao.Todos.Where(c => Bit(mascara, c)).Select(c => c.ToString()[..3]));
}
