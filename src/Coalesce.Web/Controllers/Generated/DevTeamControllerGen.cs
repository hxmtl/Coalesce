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
    public partial class DevTeamController 
        : BaseViewController<Coalesce.Domain.External.DevTeam, AppDbContext> 
    { 
        public DevTeamController() : base() { }

        [Authorize]
        public ActionResult Cards(){
            return IndexImplementation(false, @"~/Views/Generated/DevTeam/Cards.cshtml");
        }

        [Authorize]
        public ActionResult Table(){
            return IndexImplementation(false, @"~/Views/Generated/DevTeam/Table.cshtml");
        }


        [Authorize]
        public ActionResult TableEdit(){
            return IndexImplementation(true, @"~/Views/Generated/DevTeam/Table.cshtml");
        }

        [Authorize]
        public ActionResult CreateEdit(){
            return CreateEditImplementation(@"~/Views/Generated/DevTeam/CreateEdit.cshtml");
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