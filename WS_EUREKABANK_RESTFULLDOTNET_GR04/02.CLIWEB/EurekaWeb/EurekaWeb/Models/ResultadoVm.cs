namespace EurekaWeb.Models
{
    public class ResultadoVm
    {
        public string? Cuenta { get; set; }
        public bool EsExitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public decimal? Saldo { get; set; }   // <-- nullable
    }
}
