namespace CyberRiskManager.ViewModels;

public record RiesgoCreateVM
(
    CyberRiskManager.Models.Riesgo Riesgo,
    List<CyberRiskManager.Models.Activo> Activos
);
