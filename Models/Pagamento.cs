namespace VisaFlow.Models
{
    public class Pagamento
    {
        public int Id { get; set; }
        public int ProcessoId { get; set; }
        public decimal Valor { get; set; }
        public string Metodo { get; set; } = string.Empty;       // Cash, Transferencia, TPA, MobileMoney
        public string Observacao { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;   // número de comprovativo, referência bancária
        public DateTime DataPagamento { get; set; } = DateTime.Now;
        public string RegistadoPor { get; set; } = string.Empty;

        public Processo Processo { get; set; } = null!;
    }
}
