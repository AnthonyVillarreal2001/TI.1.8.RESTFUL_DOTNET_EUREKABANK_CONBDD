using System;

namespace ec.edu.monster.models;

public class Movimiento
{
    public string Cuenta { get; set; }
    public int NroMov { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; }
    public string Accion { get; set; }
    public decimal Importe { get; set; }
}
