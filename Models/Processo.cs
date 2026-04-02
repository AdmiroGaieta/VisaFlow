namespace VisaFlow.Models
{
    public class Processo
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Tipo { get; set; } = string.Empty;         // VistoTrabalho, VistoEstudo, Passaporte, Legalizacao, ReagrupamentoFamiliar
        public string PaisDestino { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Estado { get; set; } = "Pendente";         // Pendente, EmAnalise, Agendado, Aprovado, Rejeitado, Concluido
        public string Observacoes { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAgendamento { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string CriadoPor { get; set; } = string.Empty;

        public Cliente Cliente { get; set; } = null!;
        public List<Pagamento> Pagamentos { get; set; } = new();
        public List<Documento> Documentos { get; set; } = new();
    }
}
