namespace VisaFlow.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
        public string NumeroBI { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public List<Processo> Processos { get; set; } = new();
    }
}
