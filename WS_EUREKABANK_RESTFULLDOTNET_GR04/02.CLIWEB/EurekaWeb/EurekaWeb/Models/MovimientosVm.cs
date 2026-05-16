using System;
using System.Collections.Generic;
using System.Globalization;
using MovRest = EurekaWeb.Models.Rest.Movimiento;

namespace EurekaWeb.Models
{
    public class MovimientosVm
    {
        public string Cuenta { get; set; } = string.Empty;
        public List<Item> Movs { get; set; } = new();

        public class Item
        {
            // Propiedades ya formateadas para la vista
            public int Numero { get; set; }
            public string Fecha { get; set; } = "";
            public string Tipo { get; set; } = "";
            public string Accion { get; set; } = "";
            public string Importe { get; set; } = "";
            public string RowClass { get; set; } = "";
            public string BadgeClass { get; set; } = "";
            public string ImporteClass { get; set; } = "";

            // ===== Mapeo desde DTO REST =====
            public static Item FromDto(MovRest m)
            {
                // Traducción de códigos a etiquetas
                string tipo = TipoDescripcion(m.TipoCodigo);
                string accion = TipoAccion(m.TipoCodigo);

                // Clases visuales que tu CSS espera
                string row = accion == "INGRESO" ? "tr-ingreso"
                           : accion == "SALIDA" ? "tr-salida"
                           : "";

                string badge = "badge";
                var tl = tipo.ToLowerInvariant();
                if (tl.Contains("depós") || tl.Contains("deposit")) badge += " badge-deposito";
                else if (tl.Contains("retiro")) badge += " badge-retiro";
                else if (tl.Contains("transferencia")) badge += (accion == "INGRESO" ? " badge-deposito" : " badge-retiro");
                else badge += " badge-transferencia";

                string impClass = accion == "INGRESO" ? "importe-ingreso" : "importe-salida";

                return new Item
                {
                    Numero = m.Numero,
                    Fecha = m.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Tipo = tipo,
                    Accion = accion,
                    Importe = m.Importe.ToString("C2", CultureInfo.CreateSpecificCulture("es-EC")), // $40,00
                    RowClass = row,
                    BadgeClass = badge,
                    ImporteClass = impClass
                };
            }

            private static string TipoDescripcion(string? codigo) => codigo switch
            {
                "003" => "DEPOSITO",
                "004" => "RETIRO",
                "008" => "TRANSFERENCIA",
                "009" => "TRANSFERENCIA",
                _ => "OTRO"
            };

            private static string TipoAccion(string? codigo) => codigo switch
            {
                "003" => "INGRESO",
                "004" => "SALIDA",
                "008" => "INGRESO",
                "009" => "SALIDA",
                _ => ""
            };
        }
    }
}
