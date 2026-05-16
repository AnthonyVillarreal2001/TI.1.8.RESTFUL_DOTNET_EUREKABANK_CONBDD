namespace WS_EB_DOTNET_REST_Servidor.Models
{
    // Mapeo de columnas reales de la tabla dbo.Movimiento
    // chr_cuencodigo, int_movinumero, dtt_movifecha, chr_emplcodigo, chr_tipocodigo,
    // dec_moviimporte, chr_cuenreferencia
    public class Movimiento
    {
        public string CuentaCodigo { get; set; } = string.Empty; // chr_cuencodigo
        public int Numero { get; set; }                          // int_movinumero
        public DateTime Fecha { get; set; }                      // dtt_movifecha
        public string EmpleadoCodigo { get; set; } = string.Empty; // chr_emplcodigo
        public string TipoCodigo { get; set; } = string.Empty;   // chr_tipocodigo (003,004,008,009)
        public decimal Importe { get; set; }                     // dec_moviimporte
        public string? ReferenciaCuenta { get; set; }            // chr_cuenreferencia
    }
}
