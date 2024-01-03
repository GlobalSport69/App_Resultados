using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Models;
using LotteryResult.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LotteryResult.Controllers
{
    public class ProductController : Controller
    {
        private ProviderProductMapper _providerProductMapper;
        private IUnitOfWork _unitOfWork;

        public ProductController(ProviderProductMapper providerProductMapper, IUnitOfWork unitOfWork)
        {
            _providerProductMapper = providerProductMapper;
            _unitOfWork = unitOfWork;
        }

        // GET: ProductController
        public async Task<ActionResult> Index()
        {
            var products = await _unitOfWork.ProductRepository.GetAllByAsync();
            ViewBag.products = products.OrderBy(x => x.Id).ToList();

            if (TempData["Message"] != null)
                ViewBag.Message = TempData["Message"];

            return View();
        }

        public async Task<ActionResult> SwicthProductStatus(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByAsync(x => x.Id == id, new string[] {
                nameof(Product.ProviderProducts),
                $"{nameof(Product.ProviderProducts)}.{nameof(ProviderProduct.Provider)}"
            });

            try
            {
                if (product != null)
                {
                    if (!product.Enable)
                    {
                        if (!product.ProviderProducts.Any())
                        {
                            TempData["Message"] = "Provider no configurado";
                            return RedirectToAction(nameof(Index));
                        }

                        product.Enable = true;
                        _unitOfWork.ProductRepository.Update(product);
                        var cronExpression = product.ProviderProducts.First().CronExpression;
                        var jobId = product.ProviderProducts.First().ProviderId.ToString();
                        _providerProductMapper.AddJob(product.Id, jobId, cronExpression);
                        await _unitOfWork.SaveChangeAsync();
                    }
                    else
                    {
                        if (!product.ProviderProducts.Any())
                        {
                            TempData["Message"] = "Provider no configurado";
                            return RedirectToAction(nameof(Index));
                        }

                        product.Enable = false;
                        _unitOfWork.ProductRepository.Update(product);
                        var jobId = product.ProviderProducts.First().ProviderId.ToString();
                        _providerProductMapper.DeleteJob(jobId);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    TempData["Message"] = "Exito";
                }
                else {
                    TempData["Message"] = "Producto no encontrado";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ProductController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return RedirectToAction(nameof(Index));
        //}

        // GET: ProductController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: ProductController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: ProductController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: ProductController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: ProductController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: ProductController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
