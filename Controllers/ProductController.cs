using BestStore.Models;
using BestStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment? environment;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(x=>x.Id).ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(clsProductDTO productDTO)
        {
            if(productDTO.ImageFile == null) 
            {
                ModelState.AddModelError("ImageFile", "ImageFile is required");
            }
            if(!ModelState.IsValid) 
            {
                return View(productDTO);
            }

            string newImageFile = DateTime.Now.ToString("yyyyyMMddHHmmssfff");
            newImageFile += Path.GetExtension(productDTO.ImageFile!.FileName);

            string imageFullPath = environment!.WebRootPath + "/Products/" + newImageFile;
            using(var stream = System.IO.File.Create(imageFullPath)) 
            {
                productDTO.ImageFile.CopyTo(stream);
            }

            clsProduct prd = new clsProduct()
            {
                Name = productDTO.Name,
                Brand = productDTO.Brand,
                Catagory = productDTO.Catagory,
                Price = productDTO.Price,
                Description = productDTO.Description,
                ImageFileName = newImageFile,
                CreatedAt = DateTime.Now,
            };
            context.Products.Add(prd);
            context.SaveChanges();
            return RedirectToAction("Index", "Product");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if(product == null) 
            {
                return RedirectToAction("Index", "Product");
            }

            var productDTO = new clsProductDTO()
            {
                Name= product.Name,
                Brand = product.Brand,
                Catagory = product.Catagory,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductID"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyyy");

            return View(productDTO);
        }

        [HttpPost]
        public IActionResult Edit(int id, clsProductDTO productDTO)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductID"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");
                return View(productDTO);
            }

            // Default to the current image file if no new image is uploaded
            string newImageFile = product.ImageFileName;

            // Update the image only if a new one is uploaded
            if (productDTO.ImageFile != null)
            {
                newImageFile = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDTO.ImageFile.FileName);

                string imageFullPath = Path.Combine(environment!.WebRootPath, "Products", newImageFile);
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDTO.ImageFile.CopyTo(stream);
                }

                // Delete the old image only if it's different from the new one
                string oldImageFullPath = Path.Combine(environment.WebRootPath, "Products", product.ImageFileName);
                if (!string.IsNullOrEmpty(product.ImageFileName) && System.IO.File.Exists(oldImageFullPath))
                {
                    System.IO.File.Delete(oldImageFullPath);
                }
            }
             
            // Update product fields
            product.Name = productDTO.Name;
            product.Brand = productDTO.Brand;
            product.Catagory = productDTO.Catagory;
            product.Price = productDTO.Price;
            product.Description = productDTO.Description;
            product.ImageFileName = newImageFile;  // Update the image filename (even if it's the same)

            context.SaveChanges();
            return RedirectToAction("Index", "Product");
        }


        public ActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            string oldImageFullPath = environment!.WebRootPath + "/Products/" + product.ImageFileName;
            System.IO.File.Delete(oldImageFullPath);

            context.Products.Remove(product);
            context.SaveChanges(true);
            return RedirectToAction("Index", "Product");
        }
    }
}
