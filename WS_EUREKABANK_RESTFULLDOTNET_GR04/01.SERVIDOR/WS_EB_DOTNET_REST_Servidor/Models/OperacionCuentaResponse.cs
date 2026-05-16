namespace WS_EB_DOTNET_REST_Servidor.Models
{
    // Igual que SOAP: solo Estado y Saldo
    public class OperacionCuentaResponse
    {
        public int Estado { get; set; }     // 1 = OK, -1 = Error
        public decimal Saldo { get; set; }  // saldo actualizado
    }
}
