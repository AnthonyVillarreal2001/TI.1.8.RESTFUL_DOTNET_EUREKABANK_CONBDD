using System;

namespace _02.CLIMOV.Modelo
{
    public class Movimiento
    {
        public string Cuenta { get; set; }
        public int NroMov { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Accion { get; set; }
        public double Importe { get; set; }
        
        // Propiedad calculada para determinar SALIDA o INGRESO
        public string TipoMovimiento
        {
            get
            {
                // Mapeo de códigos de tipo a INGRESO/SALIDA
                return Tipo switch
                {
                    "001" => "INGRESO",  // Apertura
                    "003" => "INGRESO",  // Depósito
                    "004" => "SALIDA",   // Retiro
                    "008" => "SALIDA",   // Transferencia (egreso)
                    "009" => "INGRESO",  // Transferencia (ingreso)
                    _ => "N/A"
                };
            }
        }
    }
}
