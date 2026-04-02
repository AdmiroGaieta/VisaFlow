namespace VisaFlow.DTOs
{
    // ─── CLIENTE ───────────────────────────────────────────────────────────────

    public class ClienteCreateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
        public string NumeroBI { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ClienteUpdateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
        public string NumeroBI { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class ClienteResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
        public string NumeroBI { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime DataCadastro { get; set; }
        public int TotalProcessos { get; set; }
    }

    // ─── PROCESSO ──────────────────────────────────────────────────────────────

    public class ProcessoCreateDto
    {
        public int ClienteId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string PaisDestino { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public DateTime? DataAgendamento { get; set; }
    }

    public class ProcessoUpdateDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string PaisDestino { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public DateTime? DataAgendamento { get; set; }
    }

    public class ProcessoResponseDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string PaisDestino { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAgendamento { get; set; }
        public DateTime? DataConclusao { get; set; }

        // Financeiro calculado
        public decimal TotalPago { get; set; }
        public decimal Saldo { get; set; }
        public decimal PercentagemPaga { get; set; }
        public string EstadoPagamento { get; set; } = string.Empty;   // Pago, Parcial, Pendente

        // Documentos resumo
        public int TotalDocumentos { get; set; }
        public int DocumentosEntregues { get; set; }
        public int DocumentosPendentes { get; set; }
        public int DocumentosInvalidos { get; set; }
    }

    // ─── PAGAMENTO ─────────────────────────────────────────────────────────────

    public class PagamentoCreateDto
    {
        public int ProcessoId { get; set; }
        public decimal Valor { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
    }

    public class PagamentoResponseDto
    {
        public int Id { get; set; }
        public int ProcessoId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public DateTime DataPagamento { get; set; }
        public string RegistadoPor { get; set; } = string.Empty;
    }

    public class ResumoFinanceiroDto
    {
        public int ProcessoId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string TipoProcesso { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal TotalPago { get; set; }
        public decimal Saldo { get; set; }
        public decimal PercentagemPaga { get; set; }
        public string EstadoPagamento { get; set; } = string.Empty;
        public List<PagamentoResponseDto> HistoricoPagamentos { get; set; } = new();
    }

    // ─── DOCUMENTO ─────────────────────────────────────────────────────────────

    public class DocumentoResponseDto
    {
        public int Id { get; set; }
        public int ProcessoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string NomeFicheiro { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Observacao { get; set; } = string.Empty;
        public long TamanhoBytes { get; set; }
        public DateTime DataUpload { get; set; }
    }

    public class DocumentoUpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;      // Entregue, Invalido, Pendente
        public string Observacao { get; set; } = string.Empty;
    }

    // ─── AUTH ──────────────────────────────────────────────────────────────────

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expiracao { get; set; }
    }

    // ─── DASHBOARD ─────────────────────────────────────────────────────────────

    public class DashboardDto
    {
        public int TotalClientes { get; set; }
        public int ClientesActivos { get; set; }
        public int ProcessosAbertos { get; set; }
        public int ProcessosConcluidos { get; set; }
        public int ProcessosPendentes { get; set; }
        public int ProcessosRejeitados { get; set; }
        public decimal ReceitaTotal { get; set; }
        public decimal ReceitaPendente { get; set; }
        public decimal ReceitaMesActual { get; set; }
        public double TaxaAprovacao { get; set; }
        public List<ProcessoResponseDto> UltimosProcessos { get; set; } = new();
        public List<ClienteComDividaDto> ClientesComDivida { get; set; } = new();
    }

    public class ClienteComDividaDto
    {
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public decimal TotalDivida { get; set; }
        public int NumeroProcessos { get; set; }
    }
}
