using IntelliTect.Coalesce.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Coalesce.Web;
using Coalesce.Domain;
using Coalesce.Domain.External;

namespace Coalesce.Web.Controllers
{
    [Authorize]
    public partial class CaseController 
        : BaseViewController<Coalesce.Domain.Case, AppDbContext> 
    { 
        public CaseController() : base() { }

        [AllowAnonymous]
        public ActionResult Cards(){
            return IndexImplementation(false, @"~/Views/Generated/Case/Cards.cshtml");
        }

        [AllowAnonymous]
        public ActionResult Table(){
            return IndexImplementation(false, @"~/Views/Generated/Case/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit(){
            return IndexImplementation(true, @"~/Views/Generated/Case/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit(){
            return CreateEditImplementation(@"~/Views/Generated/Case/CreateEdit.cshtml");
        }
                      
        [Authorize]
        public ActionResult EditorHtml(bool simple = false)
        {
            return EditorHtmlImplementation(simple);
        }
                              
        [Authorize]
        public ActionResult Docs()
        {
            return DocsImplementation();
        }    
    }
}