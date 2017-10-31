namespace AdsDashboard.Controllers
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

    public class AdsItemController : Controller
    {
        private static readonly string CollectionLabel = "Ads";

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {          
            var items = await DocumentDBGraph<AdsItem>.GetVertexItemsAsync(CollectionLabel);          
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
        public async Task<ActionResult> CreateAsync([Bind(Include = "Id,Name,Url,Duration,FirstImpression,ImpressionInterval")] AdsItem item)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    long vCount = await DocumentDBGraph<AdsItem>.GetVertexCountAsync();
                    item.Id = (vCount + 1).ToString();
                }

             
                var itemProperty = JsonConvert.SerializeObject(item);
                await DocumentDBGraph<AdsItem>.InsertVertexAsync(CollectionLabel, itemProperty);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind(Include = "Id,Name,Url,Duration,FirstImpression,ImpressionInterval")] AdsItem item)
        {
            if (ModelState.IsValid)
            {
                var itemProperty = JsonConvert.SerializeObject(item);
                await DocumentDBGraph<AdsItem>.UpdateVertexAsync(CollectionLabel, item.Id, itemProperty);
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

            var item = await DocumentDBGraph<AdsItem>.GetItemAsync(id, CollectionLabel);
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

            var item = await DocumentDBGraph<AdsItem>.GetItemAsync(id, CollectionLabel);
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
            await DocumentDBGraph<AdsItem>.DeleteVertexAsync(id, CollectionLabel);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            var item = await DocumentDBGraph<AdsItem>.GetItemAsync(id, CollectionLabel);
            return View(item);
        }
    }
}