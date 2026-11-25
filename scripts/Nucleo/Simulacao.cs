using System;
using System.Collections.Generic;
using System.Linq;

namespace RefugioDoCerrado.Nucleo;

public enum Cuidado { Alimentacao = 0, Agua = 1, Manutencao = 2 }

public enum EstadoFase { EmAndamento, Vitoria, Falha }

public sealed class ResumoRodada
{
	public int Rodada;
	public int SaldoAnterior;
	public int Receita;
	public int Visitantes;
	public int DespesasRecorrentes;
	public int DespesasCuidados;
	public int Investimentos;
	public int SaldoNovo;
	public int BemEstarAnterior;
	public int BemEstarNovo;
	public int Animais;
	public int Arvores;
	public List<string> Mensagens = new();
	public EstadoFase Estado;
	public List<string> MotivosFalha = new();
}

/// <summary>
/// Regras de uma fase, sem nenhuma dependência de cena ou interface.
/// Fórmula do GDD: saldo seguinte = saldo atual + receitas − despesas recorrentes − investimentos.
/// Cuidados e operação são cobrados no fim da rodada e têm prioridade na previsão;
/// o investimento (recinto ou árvore) sai na hora.
/// </summary>
public sealed class Simulacao
{
	public static readonly Cuidado[] Todos = { Cuidado.Alimentacao, Cuidado.Agua, Cuidado.Manutencao };

	public ParametrosFase P { get; }
	public int Saldo { get; private set; }
	public int Rodada { get; private set; } = 1;
	public EstadoFase Estado { get; private set; } = EstadoFase.EmAndamento;

	public List<Recinto> Recintos { get; } = new();
	public List<ArvorePlantada> Arvores { get; } = new();

	int _saldoInicioRodada;
	int _investimentosRodada;

	public Simulacao(ParametrosFase p) : this(p, p.SaldoInicial, null, null) { }

	public Simulacao(ParametrosFase p, int saldoInicial, IEnumerable<Recinto> recintos, IEnumerable<ArvorePlantada> arvores)
	{
		P = p;
		Saldo = saldoInicial;
		_saldoInicioRodada = saldoInicial;

		if (recintos != null)
			foreach (var r in recintos.OrderBy(r => r.Id)) Recintos.Add(r.Clonar());

		if (arvores != null)
			foreach (var a in arvores) Arvores.Add(a.Clonar());

		var modelo = p.Animal ?? new EspecieAnimal { Nome = "Animal" };
		for (var id = 1; id <= Math.Max(1, p.NumeroRecintos); id++)
		{
			if (Recintos.Any(r => r.Id == id)) continue;
			Recintos.Add(new Recinto(id, modelo));
		}
	}

	// ---------- compatibilidade com a fase 1 / testes antigos ----------

	public bool RecintoConstruido => Recintos.Count > 0 && Recintos[0].Construido;
	public bool AnimalAcolhido => AnimaisAcolhidos > 0;
	public int Necessidade(Cuidado c) => Necessidade(1, c);
	public bool Planejado(Cuidado c) => Planejado(1, c);
	public bool Construir() => Construir(1);
	public string MotivoSemConstruir() => MotivoSemConstruir(1);
	public bool PodeAlternar(Cuidado c) => PodeAlternar(1, c);
	public bool Alternar(Cuidado c) => Alternar(1, c);
	public string MotivoSemCuidar(Cuidado c) => MotivoSemCuidar(1, c);
	public int NecessidadePrevista(Cuidado c) => NecessidadePrevista(1, c);
	public int BemEstar => BemEstarGlobal;
	public int BemEstarPrevisto => BemEstarGlobalPrevisto;

	// ---------- recintos ----------

	public Recinto Recinto(int id) => Recintos.FirstOrDefault(r => r.Id == id);
	public Recinto RecintoPorIndice(int indice) =>
		indice >= 0 && indice < Recintos.Count ? Recintos[indice] : null;

	public int Necessidade(int recintoId, Cuidado c)
	{
		var r = Recinto(recintoId);
		return r == null ? 0 : r.Necessidades[(int)c];
	}

	public bool Planejado(int recintoId, Cuidado c)
	{
		var r = Recinto(recintoId);
		return r != null && r.Construido && r.Planejado(c);
	}

	public int NecessidadePrevista(int recintoId, Cuidado c)
	{
		var r = Recinto(recintoId);
		if (r == null || !r.Construido) return 0;
		var atual = r.Necessidades[(int)c];
		var ganho = r.Planejado(c) ? P.EfeitoCuidado : 0;
		return Math.Clamp(Math.Clamp(atual + ganho, 0, 100) - Queda(c), 0, 100);
	}

	public int BemEstarRecinto(int recintoId)
	{
		var r = Recinto(recintoId);
		if (r == null || !r.Construido) return 0;
		return (int)Math.Round(r.Necessidades.Average());
	}

	public int BemEstarRecintoPrevisto(int recintoId)
	{
		var r = Recinto(recintoId);
		if (r == null || !r.Construido) return 0;
		return (int)Math.Round(Todos.Average(c => (double)NecessidadePrevista(recintoId, c)));
	}

	public int AnimaisAcolhidos => Recintos.Count(r => r.Construido);

	/// <summary>Bem-estar global: média de todas as necessidades + bônus das árvores, limitado a 100.</summary>
	public int BemEstarGlobal => CalcularBemEstarGlobal(false);

	/// <summary>Bem-estar se a rodada terminasse agora, com os cuidados planejados.</summary>
	public int BemEstarGlobalPrevisto => CalcularBemEstarGlobal(true);

	int CalcularBemEstarGlobal(bool previsto)
	{
		var construidos = Recintos.Where(r => r.Construido).ToList();
		if (construidos.Count == 0) return 0;
		var valores = construidos.SelectMany(r => Todos.Select(c =>
			previsto ? NecessidadePrevista(r.Id, c) : r.Necessidades[(int)c]));
		var media = valores.Average();
		return Math.Clamp((int)Math.Round(media) + BonusArvoresBemEstar, 0, 100);
	}

	// ---------- custos e receita ----------

	public int Custo(Cuidado c) => c switch
	{
		Cuidado.Alimentacao => P.CustoAlimentacao,
		Cuidado.Agua => P.CustoAgua,
		_ => P.CustoManutencao,
	};

	int Queda(Cuidado c) => c switch
	{
		Cuidado.Alimentacao => P.QuedaAlimentacao,
		Cuidado.Agua => P.QuedaAgua,
		_ => P.QuedaManutencao,
	};

	public static string Nome(Cuidado c) => c switch
	{
		Cuidado.Alimentacao => "Alimentação",
		Cuidado.Agua => "Água",
		_ => "Manutenção",
	};

	/// <summary>Quantidade de animais com concordância: "1 animal" ou "2 animais".</summary>
	public static string TextoAnimais(int quantidade) =>
		quantidade == 1 ? "1 animal" : $"{quantidade} animais";

	public int DespesaRecorrente => AnimaisAcolhidos * P.OperacaoPorRecinto;
	public int DespesaCuidados => Recintos.Where(r => r.Construido).SelectMany(r => r.Planejados).Sum(Custo);
	public int DespesaPrevista => DespesaRecorrente + DespesaCuidados;
	public int SaldoDisponivel => Saldo - DespesaPrevista;

	public int BonusArvoresBemEstar => Math.Min(6, Arvores.Sum(a => a.Especie?.BonusBemEstar ?? 0));
	public int BonusArvoresVisitantes => Arvores.Sum(a => a.Especie?.BonusVisitantes ?? 0);
	public int ArvoresPlantadas => Arvores.Count;

	/// <summary>Visitantes: (base + bem-estar × fator) por animal + bônus das árvores.</summary>
	public int VisitantesPara(int bemEstar)
	{
		if (AnimaisAcolhidos == 0) return 0;
		var porAnimal = P.VisitantesBase + bemEstar * P.VisitantesPorPontoBemEstar;
		return (int)Math.Round(porAnimal * AnimaisAcolhidos) + BonusArvoresVisitantes;
	}

	public int ReceitaPara(int bemEstar) => VisitantesPara(bemEstar) * P.PrecoIngresso;
	public int ReceitaPrevista => ReceitaPara(BemEstarGlobalPrevisto);
	public int VisitantesPrevistos => VisitantesPara(BemEstarGlobalPrevisto);
	public int SaldoPrevisto => Saldo - DespesaPrevista + ReceitaPrevista;

	// ---------- construir ----------

	public bool PodeConstruir(int recintoId)
	{
		var r = Recinto(recintoId);
		return Estado == EstadoFase.EmAndamento && r != null && !r.Construido
			&& SaldoDisponivel - P.CustoRecinto - P.OperacaoPorRecinto >= 0;
	}

	public string MotivoSemConstruir(int recintoId)
	{
		var r = Recinto(recintoId);
		if (r == null) return "Ponto inválido.";
		if (r.Construido) return "O recinto já foi construído.";
		if (PodeConstruir(recintoId)) return "";
		return $"Saldo insuficiente: o recinto custa ${P.CustoRecinto} e a operação da rodada, ${P.OperacaoPorRecinto}.";
	}

	public bool Construir(int recintoId)
	{
		if (!PodeConstruir(recintoId)) return false;
		var r = Recinto(recintoId);
		Saldo -= P.CustoRecinto;
		_investimentosRodada += P.CustoRecinto;
		r.Construido = true;
		for (var i = 0; i < r.Necessidades.Length; i++) r.Necessidades[i] = P.NecessidadeInicial;
		return true;
	}

	// ---------- cuidados ----------

	public bool PodeAlternar(int recintoId, Cuidado c)
	{
		var r = Recinto(recintoId);
		if (Estado != EstadoFase.EmAndamento || r == null || !r.Construido) return false;
		if (r.Planejado(c)) return true;
		return SaldoDisponivel - Custo(c) >= 0;
	}

	public string MotivoSemCuidar(int recintoId, Cuidado c)
	{
		var r = Recinto(recintoId);
		if (r == null || !r.Construido) return "Construa o recinto para acolher o animal.";
		if (PodeAlternar(recintoId, c)) return "";
		return $"Saldo insuficiente depois das despesas previstas (${DespesaPrevista}).";
	}

	public bool Alternar(int recintoId, Cuidado c)
	{
		if (!PodeAlternar(recintoId, c)) return false;
		var r = Recinto(recintoId);
		if (!r.Planejados.Remove(c)) r.Planejados.Add(c);
		return true;
	}

	// ---------- árvores ----------

	public ArvorePlantada ArvoreNoPonto(int ponto) => Arvores.FirstOrDefault(a => a.Ponto == ponto);

	public bool PodePlantar(int ponto, EspecieArvore arvore)
	{
		if (Estado != EstadoFase.EmAndamento || !P.PermiteArvores || arvore == null) return false;
		if (ArvoreNoPonto(ponto) != null) return false;
		return SaldoDisponivel - arvore.Custo >= 0;
	}

	public string MotivoSemPlantar(int ponto, EspecieArvore arvore)
	{
		if (!P.PermiteArvores) return "A arborização começa na fase 2.";
		if (arvore == null) return "Árvore inválida.";
		if (ArvoreNoPonto(ponto) != null) return "Este ponto já tem uma árvore.";
		if (PodePlantar(ponto, arvore)) return "";
		return $"Saldo insuficiente: a árvore custa ${arvore.Custo}.";
	}

	public bool Plantar(int ponto, int recintoId, EspecieArvore arvore)
	{
		if (!PodePlantar(ponto, arvore)) return false;
		Saldo -= arvore.Custo;
		_investimentosRodada += arvore.Custo;
		Arvores.Add(new ArvorePlantada(ponto, recintoId, arvore));
		return true;
	}

	// ---------- encerrar rodada ----------

	public ResumoRodada Encerrar()
	{
		var r = new ResumoRodada
		{
			Rodada = Rodada,
			SaldoAnterior = _saldoInicioRodada,
			Investimentos = _investimentosRodada,
			DespesasRecorrentes = DespesaRecorrente,
			DespesasCuidados = DespesaCuidados,
			BemEstarAnterior = BemEstarGlobal,
			Animais = AnimaisAcolhidos,
			Arvores = ArvoresPlantadas,
		};

		if (_investimentosRodada > 0)
			r.Mensagens.Add($"Investimentos da rodada: ${_investimentosRodada} (recintos e árvores).");

		if (AnimaisAcolhidos == 0)
			r.Mensagens.Add("Nenhum animal acolhido: construa um recinto para começar.");

		foreach (var recinto in Recintos.Where(r => r.Construido))
		{
			var nomeAnimal = recinto.Especie?.Nome ?? "Animal";
			foreach (var c in Todos)
			{
				var antes = recinto.Necessidades[(int)c];
				var ganho = recinto.Planejado(c) ? P.EfeitoCuidado : 0;
				var depois = Math.Clamp(antes + ganho, 0, 100);
				var final = Math.Clamp(depois - Queda(c), 0, 100);
				recinto.Necessidades[(int)c] = final;
				var causa = recinto.Planejado(c)
					? $"{nomeAnimal} · {Nome(c)}: cuidado feito (+{depois - antes}), o dia passa (−{depois - final}) → {final}."
					: $"{nomeAnimal} · {Nome(c)}: sem cuidado, o dia passa (−{antes - final}) → {final}.";
				r.Mensagens.Add(causa);
			}
			recinto.Planejados.Clear();
		}

		r.BemEstarNovo = BemEstarGlobal;
		r.Visitantes = VisitantesPara(r.BemEstarNovo);
		r.Receita = r.Visitantes * P.PrecoIngresso;

		if (AnimaisAcolhidos > 0)
		{
			var bonus = BonusArvoresVisitantes > 0 ? $" + {BonusArvoresVisitantes} de bônus das árvores" : "";
			r.Mensagens.Add(
				$"Visitação: ({P.VisitantesBase} base + bem-estar {r.BemEstarNovo} × {P.VisitantesPorPontoBemEstar:0.0}) " +
				$"× {TextoAnimais(AnimaisAcolhidos)}{bonus} = {r.Visitantes} visitantes × ${P.PrecoIngresso} = ${r.Receita}.");
		}
		else
		{
			r.Mensagens.Add("Sem animal acolhido, não há visitação nem receita.");
		}

		Saldo = Saldo + r.Receita - r.DespesasRecorrentes - r.DespesasCuidados;
		r.SaldoNovo = Saldo;

		_investimentosRodada = 0;

		if (Rodada >= P.Rodadas)
		{
			AvaliarFim(r);
		}
		else
		{
			Rodada++;
			_saldoInicioRodada = Saldo;
		}
		r.Estado = Estado;
		return r;
	}

	void AvaliarFim(ResumoRodada r)
	{
		if (AnimaisAcolhidos < P.AnimaisMinimos)
			r.MotivosFalha.Add($"Animais acolhidos: {AnimaisAcolhidos}, abaixo do exigido ({P.AnimaisMinimos}).");
		if (AnimaisAcolhidos > 0 && BemEstarGlobal < P.MetaBemEstar)
			r.MotivosFalha.Add($"Bem-estar {BemEstarGlobal}, abaixo da meta de {P.MetaBemEstar}.");
		foreach (var recinto in Recintos.Where(r => r.Construido))
		{
			foreach (var c in Todos.Where(c => recinto.Necessidades[(int)c] < P.NecessidadeMinima))
				r.MotivosFalha.Add($"{recinto.Especie?.Nome ?? "Animal"} · {Nome(c)} ficou em {recinto.Necessidades[(int)c]}, abaixo do mínimo de {P.NecessidadeMinima}.");
		}
		if (Saldo < P.ReservaMinima)
			r.MotivosFalha.Add($"Saldo ${Saldo}, abaixo da reserva mínima de ${P.ReservaMinima}.");
		if (ArvoresPlantadas < P.ArvoresMinimas)
			r.MotivosFalha.Add($"Árvores plantadas: {ArvoresPlantadas}, abaixo do exigido ({P.ArvoresMinimas}).");

		Estado = r.MotivosFalha.Count == 0 ? EstadoFase.Vitoria : EstadoFase.Falha;
	}

	public List<Recinto> ClonarRecintos() => Recintos.Select(r => r.Clonar()).ToList();
	public List<ArvorePlantada> ClonarArvores() => Arvores.Select(a => a.Clonar()).ToList();
}
