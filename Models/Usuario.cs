namespace VisaFlow.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Operador";           // Admin, Operador
        public bool Activo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? UltimoLogin { get; set; }
    }
}
