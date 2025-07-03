using Microsoft.AspNetCore.Mvc;
using CyberRiskManager.Models;

namespace CyberRiskManager.Controllers;

public class ActivosController : Controller
{
    // Lista simulada en memoria
    private static List<Activo> _activos = new();
    private static int _nextId = 1;

    public IActionResult Index()
    {
        return View(_activos); // Lista de activos
    }

    public IActionResult Create()
    {
        return View(new Activo()); // Formulario vacío
    }

    [HttpPost]
    public IActionResult Create(Activo activo)
    {
        activo.Id = _nextId++;
        _activos.Add(activo);
        return RedirectToAction(nameof(Index)); // Volver a lista
    }

    public IActionResult Details(int id)
    {
        var activo = _activos.FirstOrDefault(a => a.Id == id);
        return activo == null ? NotFound() : View(activo);
    }
}
