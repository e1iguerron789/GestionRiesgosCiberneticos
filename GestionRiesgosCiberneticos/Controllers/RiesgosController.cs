using Microsoft.AspNetCore.Mvc;
using CyberRiskManager.Models;
using CyberRiskManager.ViewModels;

namespace CyberRiskManager.Controllers;

public class RiesgosController : Controller
{
    // listas en memoria (igual que ActivosController)
    private static List<Riesgo> _riesgos = new();
    private static int _nextId = 1;

    // *** Necesitamos también la lista de activos ya creada ***
    private static List<Activo> Activos => ActivosController.GetAll(); // método helper

    // GET  /Riesgos
    public IActionResult Index()
    {
        // une cada riesgo con su activo para mostrar nombre, tipo, etc.
        var lista = _riesgos.Select(r =>
        {
            r.Activo = Activos.FirstOrDefault(a => a.Id == r.ActivoId);
            return r;
        }).ToList();

        return View(lista);
    }

    // GET  /Riesgos/Create
    public IActionResult Create()
    {
        var vm = new RiesgoCreateVM(new Riesgo(), Activos);
        return View(vm);
    }

    // POST /Riesgos/Create
    [HttpPost]
    public IActionResult Create(RiesgoCreateVM vm)
    {
        if (!ModelState.IsValid || vm.Riesgo is null)
            return View(vm);   // redisplay con errores

        vm.Riesgo.Id = _nextId++;
        _riesgos.Add(vm.Riesgo);
        return RedirectToAction(nameof(Index));
    }
}

