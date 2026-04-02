using Microsoft.EntityFrameworkCore;
using VisaFlow.Data;
using VisaFlow.DTOs;
using VisaFlow.Models;

namespace VisaFlow.Services
{
    public interface IDocumentoService
    {
        Task<List<DocumentoResponseDto>> GetByProcessoAsync(int processoId);
        Task<(bool sucesso, string mensagem, DocumentoResponseDto? doc)> UploadAsync(int processoId, IFormFile ficheiro, string tipo, string carregadoPor);
        Task<bool> AlterarStatusAsync(int documentoId, DocumentoUpdateStatusDto dto);
        Task<bool> EliminarAsync(int documentoId);
        Task<(byte[] bytes, string contentType, string nome)?> DownloadAsync(int documentoId);
        List<string> GetChecklistPorTipo(string tipoProcesso);
    }

    public class DocumentoService : IDocumentoService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Checklist de documentos obrigatórios por tipo de processo
        private static readonly Dictionary<string, List<string>> _checklists = new()
        {
            ["VistoTrabalho"] = new()
            {
                "Passaporte", "FotoTipoPasse", "ComprovantivoResidencia",
                "DeclaracaoEmpregador", "CertificadoAntecedentes", "ComprovantiBancario"
            },
            ["VistoEstudo"] = new()
            {
                "Passaporte", "FotoTipoPasse", "CartaAceitacaoUniversidade",
                "ComprovantivoFinanceiro", "SeguroSaude"
            },
            ["Passaporte"] = new()
            {
                "BilheteIdentidade", "CertidaoNascimento", "FotoTipoPasse", "ComprovantivoResidencia"
            },
            ["Legalizacao"] = new()
            {
                "Passaporte", "BilheteIdentidade", "CertidaoNascimento",
                "ComprovantivoResidencia", "DeclaracaoEmprego"
            },
            ["ReagrupamentoFamiliar"] = new()
            {
                "Passaporte", "CertidaoCasamento", "CertidaoNascimentoFilhos",
                "ComprovantivoFinanceiro", "ComprovantivoResidenciaPatrocinador"
            }
        };

        public DocumentoService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<DocumentoResponseDto>> GetByProcessoAsync(int processoId)
        {
            return await _context.Documentos
                .Where(d => d.ProcessoId == processoId)
                .OrderByDescending(d => d.DataUpload)
                .Select(d => new DocumentoResponseDto
                {
                    Id = d.Id,
                    ProcessoId = d.ProcessoId,
                    Tipo = d.Tipo,
                    NomeFicheiro = d.NomeFicheiro,
                    Status = d.Status,
                    Observacao = d.Observacao,
                    TamanhoBytes = d.TamanhoBytes,
                    DataUpload = d.DataUpload
                })
                .ToListAsync();
        }

        public async Task<(bool sucesso, string mensagem, DocumentoResponseDto? doc)> UploadAsync(
            int processoId, IFormFile ficheiro, string tipo, string carregadoPor)
        {
            var processo = await _context.Processos.FindAsync(processoId);
            if (processo == null)
                return (false, "Processo não encontrado.", null);

            // Extensões permitidas
            var extensoesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx" };
            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            if (!extensoesPermitidas.Contains(extensao))
                return (false, "Formato de ficheiro não permitido. Use PDF, JPG, PNG ou DOCX.", null);

            // Tamanho máximo: 10 MB
            if (ficheiro.Length > 10 * 1024 * 1024)
                return (false, "O ficheiro não pode exceder 10 MB.", null);

            // Criar pasta de upload se não existir
            var uploadFolder = Path.Combine(_env.ContentRootPath, "Uploads", processoId.ToString());
            Directory.CreateDirectory(uploadFolder);

            var nomeUnico = $"{Guid.NewGuid()}{extensao}";
            var caminho = Path.Combine(uploadFolder, nomeUnico);

            using (var stream = new FileStream(caminho, FileMode.Create))
                await ficheiro.CopyToAsync(stream);

            var documento = new Documento
            {
                ProcessoId = processoId,
                Tipo = tipo,
                NomeFicheiro = ficheiro.FileName,
                CaminhoFicheiro = caminho,
                ContentType = ficheiro.ContentType,
                TamanhoBytes = ficheiro.Length,
                Status = "Entregue",
                CarregadoPor = carregadoPor
            };

            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            return (true, "Documento carregado com sucesso.", new DocumentoResponseDto
            {
                Id = documento.Id,
                ProcessoId = documento.ProcessoId,
                Tipo = documento.Tipo,
                NomeFicheiro = documento.NomeFicheiro,
                Status = documento.Status,
                TamanhoBytes = documento.TamanhoBytes,
                DataUpload = documento.DataUpload
            });
        }

        public async Task<bool> AlterarStatusAsync(int documentoId, DocumentoUpdateStatusDto dto)
        {
            var doc = await _context.Documentos.FindAsync(documentoId);
            if (doc == null) return false;

            doc.Status = dto.Status;
            doc.Observacao = dto.Observacao;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int documentoId)
        {
            var doc = await _context.Documentos.FindAsync(documentoId);
            if (doc == null) return false;

            if (File.Exists(doc.CaminhoFicheiro))
                File.Delete(doc.CaminhoFicheiro);

            _context.Documentos.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(byte[] bytes, string contentType, string nome)?> DownloadAsync(int documentoId)
        {
            var doc = await _context.Documentos.FindAsync(documentoId);
            if (doc == null || !File.Exists(doc.CaminhoFicheiro)) return null;

            var bytes = await File.ReadAllBytesAsync(doc.CaminhoFicheiro);
            return (bytes, doc.ContentType, doc.NomeFicheiro);
        }

        public List<string> GetChecklistPorTipo(string tipoProcesso)
        {
            return _checklists.TryGetValue(tipoProcesso, out var lista) ? lista : new List<string>();
        }
    }
}
