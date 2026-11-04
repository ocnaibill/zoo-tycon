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
	public List<string> Mensagens = new();
	public EstadoFase Estado;
	public List<string> MotivosFalha = new();
}

/// <summary>
/// Regras da fase, sem nenhuma dependência de cena ou interface.
/// Fórmula do GDD: saldo seguinte = saldo atual + receitas − despesas recorrentes − investimentos.
/// Cuidados e operação são cobrados no fim da rodada e têm prioridade na previsão;
/// o investimento (construção) sai na hora.
/// </summary>
public sealed class Simulacao
{
	public static readonly Cuidado[] Todos = { Cuidado.Alimentacao, Cuidado.Agua, Cuidado.Manutencao };

	public ParametrosFase P { get; }
	public int Saldo { get; private set; }
	public int Rodada { get; private set; } = 1;
	public bool RecintoConstruido { get; private set; }
	public bool AnimalAcolhido { get; private set; }
	public EstadoFase Estado { get; private set; } = EstadoFase.EmAndamento;

	readonly int[] _necessidades = new int[3];
	readonly HashSet<Cuidado> _planejados = new();
	int _saldoInicioRodada;
	int _investimentosRodada;

	public Simulacao(ParametrosFase p)
	{
		P = p;
		Saldo = p.SaldoInicial;
		_saldoInicioRodada = Saldo;
	}

	public int Necessidade(Cuidado c) => _necessidades[(int)c];
	public bool Planejado(Cuidado c) => _planejados.Contains(c);

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

	public int BemEstar => AnimalAcolhido ? (int)Math.Round(_necessidades.Average()) : 0;

	public int DespesaRecorrente => RecintoConstruido ? P.OperacaoPorRecinto : 0;
	public int DespesaCuidados => _planejados.Sum(Custo);
	public int DespesaPrevista => DespesaRecorrente + DespesaCuidados;
	public int ReceitaPrevista => ReceitaPara(BemEstarPrevisto);
	public int VisitantesPrevistos => VisitantesPara(BemEstarPrevisto);

	/// <summary>Valor da necessidade ao fim da rodada com os cuidados planejados hoje.</summary>
	public int NecessidadePrevista(Cuidado c) => !AnimalAcolhido
		? 0
		: Math.Clamp(Math.Clamp(_necessidades[(int)c] + (Planejado(c) ? P.EfeitoCuidado : 0), 0, 100) - Queda(c), 0, 100);

	public int BemEstarPrevisto => AnimalAcolhido ? (int)Math.Round(Todos.Average(c => (double)NecessidadePrevista(c))) : 0;
	public int SaldoDisponivel => Saldo - DespesaPrevista;

	/// <summary>Saldo ao fim da rodada se nada mais mudar. Bate com o resultado de Encerrar().</summary>
	public int SaldoPrevisto => Saldo - DespesaPrevista + ReceitaPrevista;

	/// <summary>Visitantes da rodada: crescem com o bem-estar do animal (sem animal, ninguém visita).</summary>
	public int VisitantesPara(int bemEstar) =>
		AnimalAcolhido ? P.VisitantesBase + (int)Math.Round(bemEstar * P.VisitantesPorPontoBemEstar) : 0;

	int ReceitaPara(int bemEstar) => VisitantesPara(bemEstar) * P.PrecoIngresso;

	public bool PodeConstruir =>
		Estado == EstadoFase.EmAndamento && !RecintoConstruido
		&& SaldoDisponivel - P.CustoRecinto - P.OperacaoPorRecinto >= 0;

	public string MotivoSemConstruir()
	{
		if (RecintoConstruido) return "O recinto já foi construído.";
		if (PodeConstruir) return "";
		return $"Saldo insuficiente: o recinto custa ${P.CustoRecinto} e a operação da rodada, ${P.OperacaoPorRecinto}.";
	}

	public bool PodeAlternar(Cuidado c)
	{
		if (Estado != EstadoFase.EmAndamento || !AnimalAcolhido) return false;
		if (Planejado(c)) return true;
		return SaldoDisponivel - Custo(c) >= 0;
	}

	public string MotivoSemCuidar(Cuidado c)
	{
		if (!AnimalAcolhido) return "Construa o recinto para acolher o animal.";
		if (PodeAlternar(c)) return "";
		return $"Saldo insuficiente depois das despesas previstas (${DespesaPrevista}).";
	}

	/// <summary>Constrói o recinto e acolhe o animal. Devolve false se bloqueado.</summary>
	public bool Construir()
	{
		if (!PodeConstruir) return false;
		Saldo -= P.CustoRecinto;
		_investimentosRodada += P.CustoRecinto;
		RecintoConstruido = true;
		AnimalAcolhido = true;
		for (var i = 0; i < _necessidades.Length; i++) _necessidades[i] = P.NecessidadeInicial;
		return true;
	}

	public bool Alternar(Cuidado c)
	{
		if (!PodeAlternar(c)) return false;
		if (!_planejados.Remove(c)) _planejados.Add(c);
		return true;
	}

	public ResumoRodada Encerrar()
	{
		var r = new ResumoRodada
		{
			Rodada = Rodada,
			SaldoAnterior = _saldoInicioRodada,
			Investimentos = _investimentosRodada,
			DespesasRecorrentes = DespesaRecorrente,
			DespesasCuidados = DespesaCuidados,
			BemEstarAnterior = BemEstar,
		};

		if (_investimentosRodada > 0)
			r.Mensagens.Add($"Investimento: recinto construído (${_investimentosRodada}). O {P.Animal} foi acolhido.");

		if (AnimalAcolhido)
		{
			foreach (var c in Todos)
			{
				var antes = _necessidades[(int)c];
				var ganho = Planejado(c) ? P.EfeitoCuidado : 0;
				var depois = Math.Clamp(antes + ganho, 0, 100);
				var final = Math.Clamp(depois - Queda(c), 0, 100);
				_necessidades[(int)c] = final;
				var causa = Planejado(c)
					? $"{Nome(c)}: cuidado feito (+{depois - antes}), o dia passa (−{depois - final}) → {final}."
					: $"{Nome(c)}: sem cuidado, o dia passa (−{antes - final}) → {final}.";
				r.Mensagens.Add(causa);
			}
		}

		r.BemEstarNovo = BemEstar;
		r.Visitantes = VisitantesPara(r.BemEstarNovo);
		r.Receita = r.Visitantes * P.PrecoIngresso;
		if (AnimalAcolhido)
			r.Mensagens.Add($"Visitação: {P.VisitantesBase} visitantes base + bem-estar {r.BemEstarNovo} × {P.VisitantesPorPontoBemEstar:0.0} = {r.Visitantes} visitantes × ${P.PrecoIngresso} de ingresso = ${r.Receita}.");
		else
			r.Mensagens.Add("Sem animal acolhido, não há visitação nem receita.");

		Saldo = Saldo + r.Receita - r.DespesasRecorrentes - r.DespesasCuidados;
		r.SaldoNovo = Saldo;

		_planejados.Clear();
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
		if (!AnimalAcolhido) r.MotivosFalha.Add("Nenhum animal foi acolhido: era preciso construir o recinto.");
		if (AnimalAcolhido && BemEstar < P.MetaBemEstar)
			r.MotivosFalha.Add($"Bem-estar {BemEstar}, abaixo da meta de {P.MetaBemEstar}.");
		foreach (var c in Todos.Where(c => AnimalAcolhido && Necessidade(c) < P.NecessidadeMinima))
			r.MotivosFalha.Add($"{Nome(c)} ficou em {Necessidade(c)}, abaixo do mínimo de {P.NecessidadeMinima}.");
		if (Saldo < P.ReservaMinima) r.MotivosFalha.Add($"Saldo ${Saldo}, abaixo do mínimo de ${P.ReservaMinima}.");
		Estado = r.MotivosFalha.Count == 0 ? EstadoFase.Vitoria : EstadoFase.Falha;
	}
}
