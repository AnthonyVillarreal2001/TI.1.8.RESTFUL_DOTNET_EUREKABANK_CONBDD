using System;

namespace _02.CLIMOV.Modelo
{
    /// <summary>
    /// Modelo para la respuesta de operaciones bancarias (Depósito, Retiro, Transferencia)
    /// </summary>
    public class OperacionResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public double SaldoActual { get; set; }
        public string Cuenta { get; set; }
    }
}
