namespace CyberRiskManager.Models;

public enum TipoActivo
{
    Hardware,
    Software,
    Datos,
    Servicios,
    Personas
}

public class Activo
{
    public int Id { get; set; } // identificador automático
    public string Nombre { get; set; } = "";
    public TipoActivo Tipo { get; set; }
    public string? Descripcion { get; set; }
    public int Confidencialidad { get; set; }
    public int Integridad { get; set; }
    public int Disponibilidad { get; set; }

    // Calculo automático: criticidad promedio
    public int Criticidad => (Confidencialidad + Integridad + Disponibilidad) / 3;
}
