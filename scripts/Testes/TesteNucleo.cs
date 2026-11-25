using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RefugioDoCerrado.Nucleo;

namespace RefugioDoCerrado.Testes;

/// <summary>
/// Prova de balanceamento das três fases. A fase 1 e a fase 2 são enumeradas;
/// a fase 3 usa uma heurística gulosa + busca aleatória com semente fixa, porque
/// a enumeração completa cresce demais. Também verifica as sequências canônicas
/// usadas pelo teste de interface.
/// </summary>
public partial class TesteNucleo : Node
{
	int _falhas;

	public override void _Ready()
	{
		var p1 = Carregar(1);
		var p2 = Carregar(2);
		var p3 = Carregar(3);

		GD.Print("== Recursos de conteúdo");
		Confere(p1.Animal?.Nome == "Lobo-guará", "fase 1 aponta para o lobo-guará");
		Confere(p2.Animal?.Nome == "Tamanduá-bandeira", "fase 2 aponta para o tamanduá-bandeira");
		Confere(p3.Animal?.Nome == "Tatu-canastra", "fase 3 aponta para o tatu-canastra");
		Confere(p1.Arvore1?.Nome == "Ipê-amarelo-do-cerrado" && p1.Arvore2?.Nome == "Pequi", "árvores nativas carregadas");
		Confere(p1.Arvore1?.Custo == 120 && p1.Arvore1.BonusBemEstar == 2 && p1.Arvore1.BonusVisitantes == 3, "efeitos das árvores carregados");
		Confere(!string.IsNullOrWhiteSpace(p1.Animal?.Ficha) && p1.Animal.Fontes.Contains("http"), "ficha e fontes do animal preenchidas");
		Confere(Simulacao.TextoAnimais(1) == "1 animal" && Simulacao.TextoAnimais(2) == "2 animais", "concordância de animal/animais");
		Confere(p1.Animal?.CarregarSprite() != null && p2.Animal?.CarregarSprite() != null && p3.Animal?.CarregarSprite() != null, "sprites dos animais carregam");
		Confere(p1.Arvore1?.CarregarSprite() != null && p1.Arvore2?.CarregarSprite() != null, "sprites das árvores carregam");

		GD.Print("\n== FASE 1 — enumeração completa");
		var melhor1 = EnumerarFase1(p1, out var can1);
		Confere(melhor1 != null, "fase 1 tem sequência vencedora");
		Confere(can1 != null, "sequência canônica da fase 1 vence");
		if (can1 != null) Imprimir("canônica F1", can1);

		GD.Print("\n== FASE 2 — enumeração completa a partir da fase 1 canônica");
		var base2 = can1 == null ? null : CriarBaseFase2(p2, can1);
		Confere(base2 != null, "estado inicial da fase 2 pode ser criado");
		Simulacao can2 = null;
		var melhor2 = base2 == null ? null : EnumerarFase2(p2, base2, out can2);
		Confere(melhor2 != null, "fase 2 tem sequência vencedora");
		Confere(can2 != null, "sequência canônica da fase 2 vence");
		if (can2 != null) Imprimir("canônica F2", can2);

		GD.Print("\n== FASE 3 — heurística gulosa + busca aleatória");
		var vitoria2 = melhor2 ?? can2;
		var base3 = vitoria2 == null ? null : CriarBaseFase3(p3, vitoria2);
		Confere(base3 != null, "estado inicial da fase 3 pode ser criado");
		if (base3 != null)
		{
			var canonica3 = ExecutarFase3Canonica(p3, base3);
			Confere(canonica3 != null, "sequência canônica da fase 3 vence");
			if (canonica3 != null) Imprimir("canônica F3", canonica3);

			var base3UI = can2 == null ? null : CriarBaseFase3(p3, can2);
			var canonica3UI = base3UI == null ? null : ExecutarFase3Canonica(p3, base3UI);
			Confere(canonica3UI != null, "sequência canônica da fase 3 vence a partir da fase 2 canônica");
			if (canonica3UI != null) Imprimir("canônica F3 (fluxo F2)", canonica3UI);

			var gulosa = ExecutarFase3Gulosa(p3, base3);
			Confere(gulosa != null, "fase 3 tem sequência vencedora pela heurística");
			if (gulosa != null) Imprimir("gulosa F3", gulosa);

			var ruim = ExecutarFase3SemCuidado(p3, base3);
			Confere(ruim != null && ruim.Estado == EstadoFase.Falha, "fase 3 tem sequência perdedora (sem cuidado)");

			var (vitorias, derrotas, tentativas) = BuscaAleatoriaFase3(p3, base3, 3000, 20261124);
			GD.Print($"== Fase 3 (busca aleatória): {vitorias} vitórias, {derrotas} derrotas em {tentativas} tentativas.");
			Confere(vitorias > 0, "busca aleatória encontra vitória na fase 3");
			Confere(derrotas > 0, "busca aleatória encontra derrota na fase 3 (escolhas importam)");
		}

		GD.Print(_falhas == 0 ? "\nRESULTADO: OK" : $"\nRESULTADO: {_falhas} FALHA(S)");
		GetTree().Quit(_falhas == 0 ? 0 : 1);
	}

	void Confere(bool cond, string msg)
	{
		GD.Print((cond ? "  ok   " : "  FALHA ") + msg);
		if (!cond) _falhas++;
	}

	static ParametrosFase Carregar(int fase) =>
		GD.Load<ParametrosFase>($"res://data/parametros_fase_{fase}.tres")
		?? new ParametrosFase { Numero = fase };

	static string Nome(int mascara) =>
		mascara == 0 ? "nada" : string.Join("+", Simulacao.Todos.Where(c => Bit(mascara, c)).Select(c => c.ToString()[..3]));

	static bool Bit(int mascara, Cuidado c) => (mascara & (1 << (int)c)) != 0;

	static void Imprimir(string rotulo, Simulacao sim) =>
		GD.Print($"  {rotulo}: saldo ${sim.Saldo}, bem-estar {sim.BemEstar}, rodada {sim.Rodada}, estado {sim.Estado}; " +
			string.Join(" | ", sim.Recintos.Where(r => r.Construido)
				.Select(r => $"R{r.Id} nec {string.Join("/", r.Necessidades)}")));

	// ---------- Fase 1 ----------

	static Simulacao ExecutarFase1(ParametrosFase p, int construir, int[] plano, out ResumoRodada ultimo)
	{
		var sim = new Simulacao(p);
		ultimo = null;
		for (var r = 1; r <= p.Rodadas; r++)
		{
			if (r == construir && !sim.Construir()) return null;
			for (var c = 0; c < 3; c++)
				if (Bit(plano[r - 1], (Cuidado)c) && !sim.Alternar((Cuidado)c)) return null;
			ultimo = sim.Encerrar();
		}
		return sim;
	}

	Simulacao EnumerarFase1(ParametrosFase p, out Simulacao canonica)	{
		canonica = ExecutarFase1(p, 1, new[] { 3, 7, 3 }, out var fimCanonico);
		if (canonica?.Estado != EstadoFase.Vitoria) canonica = null;

		var vitorias = new List<Simulacao>();
		var total = 0;
		var bloqueadas = 0;
		var derrotas = 0;
		Simulacao melhor = null;

		for (var construir = 1; construir <= p.Rodadas + 1; construir++)
		{
			foreach (var plano in Planos(p.Rodadas, 8))
			{
				var sim = ExecutarFase1(p, construir, plano, out var ultimo);
				if (sim == null) { bloqueadas++; continue; }
				total++;
				if (ultimo.Estado == EstadoFase.Vitoria)
				{
					vitorias.Add(sim);
					if (melhor == null || sim.Saldo > melhor.Saldo) melhor = sim;
				}
				else derrotas++;
			}
		}

		GD.Print($"== Estratégias válidas: {total}; bloqueadas: {bloqueadas}; vitórias: {vitorias.Count}; derrotas: {derrotas}.");
		Confere(vitorias.Count > 0 && derrotas > 0, "fase 1 tem vitória e derrota");
		if (canonica != null) Imprimir("F1 canônica", canonica);
		return melhor;
	}

	// ---------- Fase 2 ----------

	static Simulacao CriarBaseFase2(ParametrosFase p2, Simulacao vitoria1)
	{
		var recs = vitoria1.ClonarRecintos();
		foreach (var r in recs.Where(r => r.Construido))
			for (var i = 0; i < r.Necessidades.Length; i++)
				r.Necessidades[i] = Math.Max(r.Necessidades[i], p2.NecessidadeMinimaTransicao);
		return new Simulacao(p2, vitoria1.Saldo + p2.ApoioTransicao, recs, null);
	}

	static Simulacao ExecutarFase2(ParametrosFase p2, Simulacao baseSim, int construirRec2,
		(int m1, int m2)[] plano, out ResumoRodada ultimo)
	{
		var sim = new Simulacao(p2, baseSim.Saldo, baseSim.Recintos, baseSim.Arvores);
		ultimo = null;
		for (var r = 1; r <= p2.Rodadas; r++)
		{
			if (r == construirRec2 && !sim.Construir(2)) return null;
			if (!AplicarMascara(sim, 1, plano[r - 1].m1)) return null;
			if (sim.Recinto(2).Construido && !AplicarMascara(sim, 2, plano[r - 1].m2)) return null;
			ultimo = sim.Encerrar();
		}
		return sim;
	}

	Simulacao EnumerarFase2(ParametrosFase p2, Simulacao baseSim, out Simulacao canonica)
	{
		var planoCanonico = new[] { (2, 4), (5, 3), (7, 3) };
		canonica = ExecutarFase2(p2, baseSim, 1, planoCanonico, out var fimCanonico);
		if (canonica?.Estado != EstadoFase.Vitoria) canonica = null;

		Simulacao melhor = null;
		var melhorBuild = 0;
		(int m1, int m2)[] melhorPlano = null;
		var total = 0;
		var vitorias = 0;
		var derrotas = 0;

		for (var construir = 1; construir <= 3; construir++)
		{
			for (var a = 0; a < 64; a++)
			for (var b = 0; b < 64; b++)
			for (var c = 0; c < 64; c++)
			{
				var plano = new[] { (a & 7, (a >> 3) & 7), (b & 7, (b >> 3) & 7), (c & 7, (c >> 3) & 7) };
				var sim = ExecutarFase2(p2, baseSim, construir, plano, out var ultimo);
				if (sim == null) continue;
				total++;
				if (ultimo.Estado == EstadoFase.Vitoria)
				{
					vitorias++;
					if (melhor == null || sim.Saldo > melhor.Saldo)
					{
						melhor = sim;
						melhorBuild = construir;
						melhorPlano = plano;
					}
				}
				else derrotas++;
			}
		}

		GD.Print($"== Estratégias válidas: {total}; vitórias: {vitorias}; derrotas: {derrotas}.");
		Confere(vitorias > 0 && derrotas > 0, "fase 2 tem vitória e derrota");
		if (melhor != null && melhorPlano != null)
		{
			GD.Print($"  melhor F2: construir recinto 2 na rodada {melhorBuild}; plano " +
				string.Join(" | ", melhorPlano.Select((m, i) => $"R{i + 1}: {Nome(m.m1)} / {Nome(m.m2)}")));
			Imprimir("melhor F2", melhor);
		}
		if (canonica != null) Imprimir("F2 canônica", canonica);
		return melhor;
	}

	// ---------- Fase 3 ----------

	static Simulacao CriarBaseFase3(ParametrosFase p3, Simulacao vitoria2)
	{
		var recs = vitoria2.ClonarRecintos();
		foreach (var r in recs.Where(r => r.Construido))
			for (var i = 0; i < r.Necessidades.Length; i++)
				r.Necessidades[i] = Math.Max(r.Necessidades[i], p3.NecessidadeMinimaTransicao);
		return new Simulacao(p3, vitoria2.Saldo + p3.ApoioTransicao, recs, null);
	}

	static Simulacao NovoSimFase3(ParametrosFase p3, Simulacao baseSim) =>
		new(p3, baseSim.Saldo, baseSim.Recintos, baseSim.Arvores);

	static bool PlantarDuas(Simulacao sim, ParametrosFase p3) =>
		sim.Plantar(1, 1, p3.Arvore1) && sim.Plantar(2, 1, p3.Arvore2);

	static Simulacao ExecutarFase3Canonica(ParametrosFase p3, Simulacao baseSim)
	{
		var sim = NovoSimFase3(p3, baseSim);
		if (!PlantarDuas(sim, p3) || !sim.Construir(3)) return null;

		var plano = new[]
		{
			new[] { 3, 0, 0 },
			new[] { 4, 7, 3 },
			new[] { 7, 7, 7 },
		};
		ResumoRodada ultimo = null;
		for (var r = 1; r <= p3.Rodadas; r++)
		{
			for (var id = 1; id <= 3; id++)
				if (sim.Recinto(id)?.Construido == true && !AplicarMascara(sim, id, plano[r - 1][id - 1]))
					return null;
			ultimo = sim.Encerrar();
		}
		return ultimo?.Estado == EstadoFase.Vitoria ? sim : null;
	}

	static Simulacao ExecutarFase3Gulosa(ParametrosFase p3, Simulacao baseSim)
	{
		var sim = NovoSimFase3(p3, baseSim);
		if (!PlantarDuas(sim, p3)) return null;

		ResumoRodada ultimo = null;
		for (var r = 1; r <= p3.Rodadas; r++)
		{
			if (r == 1 && !sim.Construir(3)) return null;
			for (var id = 1; id <= 3; id++)
			{
				if (sim.Recinto(id)?.Construido != true) continue;
				foreach (var c in Simulacao.Todos)
					if (sim.PodeAlternar(id, c)) sim.Alternar(id, c);
			}
			ultimo = sim.Encerrar();
		}
		return ultimo?.Estado == EstadoFase.Vitoria ? sim : null;
	}

	static Simulacao ExecutarFase3SemCuidado(ParametrosFase p3, Simulacao baseSim)
	{
		var sim = NovoSimFase3(p3, baseSim);
		if (!PlantarDuas(sim, p3) || !sim.Construir(3)) return null;
		ResumoRodada ultimo = null;
		for (var r = 1; r <= p3.Rodadas; r++) ultimo = sim.Encerrar();
		return ultimo?.Estado == EstadoFase.Falha ? sim : null;
	}

	static (int vitorias, int derrotas, int tentativas) BuscaAleatoriaFase3(ParametrosFase p3, Simulacao baseSim, int n, int semente)
	{
		var rng = new Random(semente);
		var vitorias = 0;
		var derrotas = 0;
		var tentativas = 0;

		for (var i = 0; i < n; i++)
		{
			var sim = NovoSimFase3(p3, baseSim);
			if (!PlantarDuas(sim, p3)) continue;
			var construir = rng.Next(1, 3);
			var valido = true;
			ResumoRodada ultimo = null;

			for (var r = 1; r <= p3.Rodadas && valido; r++)
			{
				if (r == construir && !sim.Construir(3)) { valido = false; break; }
				for (var id = 1; id <= 3; id++)
				{
					if (sim.Recinto(id)?.Construido != true) continue;
					var mascara = rng.Next(0, 8);
					if (!AplicarMascara(sim, id, mascara)) { valido = false; break; }
				}
				if (!valido) break;
				ultimo = sim.Encerrar();
			}

			if (!valido) continue;
			tentativas++;
			if (ultimo?.Estado == EstadoFase.Vitoria) vitorias++;
			else derrotas++;
		}

		return (vitorias, derrotas, tentativas);
	}

	// ---------- Utilitários ----------

	static bool AplicarMascara(Simulacao sim, int recintoId, int mascara)
	{
		foreach (var c in Simulacao.Todos)
			if (Bit(mascara, c) && !sim.Alternar(recintoId, c)) return false;
		return true;
	}

	static IEnumerable<int[]> Planos(int rodadas, int opcoes)
	{
		var n = 1;
		for (var i = 0; i < rodadas; i++) n *= opcoes;
		for (var k = 0; k < n; k++)
		{
			var plano = new int[rodadas];
			var x = k;
			for (var i = 0; i < rodadas; i++) { plano[i] = x % opcoes; x /= opcoes; }
			yield return plano;
		}
	}
}
