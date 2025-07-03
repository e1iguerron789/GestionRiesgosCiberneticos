// Riesgo.cs
namespace CyberRiskManager.Models;

public class Riesgo
{
    public int Id { get; set; }

    // Relación al activo
    public int ActivoId { get; set; }
    public Activo? Activo { get; set; }   // se rellena a mano en el controlador

    // Datos que ingresa el usuario
    public string Amenaza { get; set; } = "";
    public string Vulnerabilidad { get; set; } = "";
    public string ControlesExistentes { get; set; } = "";

    // Valoración P‑I (1‑3)
    public int Probabilidad { get; set; }
    public int Impacto { get; set; }

    // Cálculo automático
    public int Nivel => Probabilidad * Impacto;       // 1‑9
    public string Categoria => Nivel switch
    {
        >= 9 => "Crítico",
        >= 6 => "Alto",
        >= 3 => "Medio",
        _ => "Bajo"
    };
}
