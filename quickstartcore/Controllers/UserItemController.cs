namespace todo.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using System.Collections.Generic;
    using Microsoft.Azure.Documents.Linq;

    public class UserItemController : Controller
    {
        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {
            var items = await DocumentDBRepository<UserItem>.GetItemsAsync(d => !d.IsAdmin);
            return View(items);
        }
        

#pragma warning disable 1998
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync()
        {
            return View();
        }
#pragma warning restore 1998

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,Name,Description,IsAdmin,CreatedBy")] UserItem item)
        {
            if (ModelState.IsValid)
            {
                //if (!string.IsNullOrEmpty(item.CreatedBy))
                //{
                //    string edgeLabel = "Created By ";
                //    string queryCmd = string.Format("g.V({0}).addE({1}).to(g.V({2}))", item.Name, edgeLabel, item.CreatedBy);

                //    IDocumentQuery<dynamic> query = client.CreateGremlinQuery<dynamic>(graph, gremlinQuery.Value);
                //}

                await DocumentDBRepository<UserItem>.CreateItemAsync(item);           
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,Name,Description,IsAdmin,CreatedBy")] UserItem item)
        {
            if (ModelState.IsValid)
            {
                await DocumentDBRepository<UserItem>.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            UserItem item = await DocumentDBRepository<UserItem>.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            UserItem item = await DocumentDBRepository<UserItem>.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await DocumentDBRepository<UserItem>.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            UserItem item = await DocumentDBRepository<UserItem>.GetItemAsync(id);
            return View(item);
        }
    }
}