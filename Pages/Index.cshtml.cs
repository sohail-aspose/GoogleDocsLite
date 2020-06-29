using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GroupDocs.Editor;
using GroupDocs.Editor.Formats;
using GroupDocs.Editor.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace GoogleDocsLite.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public IFormFile UploadedDocument { get; set; }

        [BindProperty]
        public string DocumentContent { get; set; }

        static volatile string UploadedDocumentPath;

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

            // Retain the path of uploaded document between sessions.
            UploadedDocumentPath = filePath;

            ShowDocumentContentInTextEditor();
        }

        private void ShowDocumentContentInTextEditor()
        {
            WordProcessingLoadOptions loadOptions = new WordProcessingLoadOptions();
            Editor editor = new Editor(UploadedDocumentPath, delegate { return loadOptions; }); //passing path and load options (via delegate) to the constructor
            EditableDocument document = editor.Edit(new WordProcessingEditOptions()); //opening document for editing with format-specific edit options

            DocumentContent = document.GetBodyContent(); //document.GetContent();
            Console.WriteLine("HTMLContent: " + DocumentContent);

            //string embeddedHtmlContent = document.GetEmbeddedHtml();
            //Console.WriteLine("EmbeddedHTMLContent: " + embeddedHtmlContent);
        }

        public FileResult OnPostDownloadDocument()
        {
            // Editor object is referencing to the document initially uploaded for editing.
            WordProcessingLoadOptions loadOptions = new WordProcessingLoadOptions();
            Editor editor = new Editor(UploadedDocumentPath, delegate { return loadOptions; });

            // <html>, <head> and <body> tags are missing in the HTML string stored in DocumentContent, so we are manually adding it.
            string completeHTML = "<!DOCTYPE html><html><head><title></title></head><body>" + DocumentContent + "</body></html>";
            EditableDocument document = EditableDocument.FromMarkup(completeHTML, null);

            // Path to the output document        
            var projectRootPath = Path.Combine(_hostingEnvironment.ContentRootPath, "DownloadedDocuments");
            var outputPath = Path.Combine(projectRootPath, Path.GetFileName(UploadedDocumentPath));

            // Save the Word Document at the outputPath 
            WordProcessingSaveOptions saveOptions = new WordProcessingSaveOptions(WordProcessingFormats.Docx);
            editor.Save(document, outputPath, saveOptions);

            // Return the Word Document as response to the User
            var bytes = System.IO.File.ReadAllBytes(outputPath);
            return new FileContentResult(bytes, new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
            {
                FileDownloadName = Path.GetFileName(UploadedDocumentPath)
            };
        }
    }
}
