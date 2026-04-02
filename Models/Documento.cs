namespace VisaFlow.Models
{
    public class Documento
    {
        public int Id { get; set; }
        public int ProcessoId { get; set; }
        public string Tipo { get; set; } = string.Empty;         // Passaporte, FotoTipoPasse, ComprovantivoResidencia, etc.
        public string NomeFicheiro { get; set; } = string.Empty;
        public string CaminhoFicheiro { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendente";         // Pendente, Entregue, Invalido
        public string Observacao { get; set; } = string.Empty;
        public long TamanhoBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime DataUpload { get; set; } = DateTime.Now;
        public string CarregadoPor { get; set; } = string.Empty;

        public Processo Processo { get; set; } = null!;
    }
}
