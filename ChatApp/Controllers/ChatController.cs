using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.OpenAI;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;

namespace ChatApp.Controllers
{
    public class ChatController : Controller
    {

        string endpoint = "";
        string key = "";
        string model = "";


        public ChatController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetResponse(string userMessage)
        {
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            var chatCompletionOPtions = new ChatCompletionsOptions() { 
                Messages = {
                    new ChatRequestSystemMessage("You are a helpful assistant"),
                    //new ChatRequestUserMessage("Does Azure OpenAI support GPT-4"),
                    //new ChatRequestAssistantMessage("Yes, it does"),
                    new ChatRequestUserMessage(userMessage),
                    },
                MaxTokens = 400,  //This is little faster than usual
                DeploymentName = model,
            };

            var response = await client.GetChatCompletionsAsync(chatCompletionOPtions);
            var choices = response.Value.Choices.First().Message.Content;
            return Json(new {Response = choices});
        }

        [HttpPost]
        public async Task<IActionResult> GetResponseFromPDF(string userMessage)
        {
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            var pdfPath = @"C:\Users\Hansamali Gamage\Desktop\data\original.pdf";
            var pdfText = GetText(pdfPath);
            var chatCompletionOPtions = new ChatCompletionsOptions()
            {
                Messages = {
                    new ChatRequestSystemMessage("You are a helpful assistant"),
                    //new ChatRequestUserMessage("Does Azure OpenAI support GPT-4"),
                    //new ChatRequestAssistantMessage("Yes, it does"),
                    new ChatRequestUserMessage($"The following information is from the PDF text: {pdfText}"),
                    new ChatRequestUserMessage(userMessage),
                    },
                MaxTokens = 1000,
                DeploymentName = model,
                Temperature = 0 //More fractual not more creative
            };

            var response = await client.GetChatCompletionsAsync(chatCompletionOPtions);
            var choices = response.Value.Choices.First().Message.Content;
            return Json(new { Response = choices });
        }

        private static string GetText(string filePath)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath));
            StringBuilder text = new StringBuilder();

            for (var page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                PdfPage pdfPage = pdfDoc.GetPage(page);
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfPage, strategy);
                text.Append(currentText);
            }
            pdfDoc.Close();
            return text.ToString();
        }

    }
}
