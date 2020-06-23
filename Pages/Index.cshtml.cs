using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GroupDocs.Editor;
using GroupDocs.Editor.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GoogleDocsLite.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public IFormFile UploadedDocument { get; set; }

        public string DocumentContent { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            var projectRootPath = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedDocuments");
            var filePath = Path.Combine(projectRootPath, UploadedDocument.FileName);
            UploadedDocument.CopyTo(new FileStream(filePath, FileMode.Create));

            ShowDocumentContentInTextEditor(filePath);
        }

        private void ShowDocumentContentInTextEditor(string filePath)
        {
            WordProcessingLoadOptions loadOptions = new WordProcessingLoadOptions();
            Editor editor = new Editor(filePath, delegate { return loadOptions; }); //passing path and load options (via delegate) to the constructor
            EditableDocument document = editor.Edit(new WordProcessingEditOptions()); //opening document for editing with format-specific edit options

            DocumentContent = document.GetBodyContent(); //document.GetContent();
            Console.WriteLine("HTMLContent: " + DocumentContent);

            //string embeddedHtmlContent = document.GetEmbeddedHtml();
            //Console.WriteLine("EmbeddedHTMLContent: " + embeddedHtmlContent);
        }
    }
}
