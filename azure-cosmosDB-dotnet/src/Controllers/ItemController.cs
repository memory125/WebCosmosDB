namespace todo.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Models;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.Collections;
    using Newtonsoft.Json.Serialization;
    using Microsoft.Azure.Graphs.Elements;

    public class ItemController : Controller
    {
        private static readonly string CollectionLabel = "Todo";

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {          
            var items = await DocumentDBGraph<Item>.GetVertexItemsAsync(CollectionLabel);          
            return View(items);
        }

#pragma warning disable 1998
        [ActionName("Create")]
        public async Task<ActionResult> CreateAsync()
        {
            return View();
        }
#pragma warning restore 1998

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind(Include = "Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    long vCount = await DocumentDBGraph<Item>.GetVertexCountAsync();
                    item.Id = (vCount + 1).ToString();
                }

                //Dictionary<string, string> dictionary = new Dictionary<string, string>()
                //{
                //    { "Id", item.Id},
                //    { "Name", item.Name},
                //    { "Description", item.Description},
                //    { "Completed", item.Completed.ToString() },
                //};

                var itemProperty = JsonConvert.SerializeObject(item);
                //await DocumentDBGraph<Item>.InsertVertexAsync(CollectionLabel, dictionary);
                await DocumentDBGraph<Item>.InsertVertexAsync(CollectionLabel, itemProperty);
                //await DocumentDBRepository<Item>.CreateItemAsync(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind(Include = "Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                var itemProperty = JsonConvert.SerializeObject(item);
                await DocumentDBGraph<Item>.UpdateVertexAsync(CollectionLabel, item.Id, itemProperty);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = await DocumentDBGraph<Item>.GetItemAsync(id, CollectionLabel);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = await DocumentDBGraph<Item>.GetItemAsync(id, CollectionLabel);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind(Include = "Id")] string id)
        {
            await DocumentDBGraph<Item>.DeleteVertexAsync(id, CollectionLabel);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            var item = await DocumentDBGraph<Item>.GetItemAsync(id, CollectionLabel);
            return View(item);
        }
    }
}