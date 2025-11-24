using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CelebrateMeProto.Data;
using CelebrateMeProto.Models;
using CelebrateMeProto.Repositories;
using CelebrateMeProto.ViewModels;

namespace CelebrateMeProto.Controllers;

public class AdminController : Controller
{
    private readonly IProductRepository _repo;
    private readonly IOrderRepository _orders;

    public AdminController(IProductRepository repo, IOrderRepository orders) 
    {
        _repo = repo;
        _orders = orders;
    }

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        // changed: return dashboard view model with products + recent orders
        var products = await _repo.GetAllAsync();
        var orders = (await _orders.GetAllAsync()).Take(20).ToList(); // most recent 20

        var adminDashboardViewModel = new AdminDashboardViewModel
        {
            Products = products,
            RecentOrders = orders
        };

        return View(adminDashboardViewModel);
    }

    // GET: /Admin/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var product = await _repo.GetByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View(product);
    }

    // GET: /Admin/Create
    public IActionResult Create()
    {
        return View(new Product());
    }

    // POST: /Admin/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid) return View(product);
        await _repo.AddAsync(product);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var product = await _repo.GetByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View(product);
    }

    // POST: /Admin/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        if (!ModelState.IsValid) return View(product);

        try
        {
            await _repo.UpdateAsync(product);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _repo.ExistsAsync(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var product = await _repo.GetByIdAsync(id.Value);
        if (product == null) return NotFound();
        return View(product);
    }

    // POST: /Admin/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}