using GroqApiLibrary;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;

namespace Grok_API_Form
{
    public partial class Form1 : Form
    {
        private List<JObject> myHistoryChat = new List<JObject>();
        private GroqAPI groqApi;

        public Form1()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder().AddUserSecrets<Form1>().Build();
            string groqAiKey = config.GetSection("GROQ_API_KEY").Value;
            groqApi = new GroqAPI(groqAiKey);
        }

        private async void btnenviar_Click(object sender, EventArgs e)
        {
            string userInput = intext.Text.Trim();
            if (string.IsNullOrEmpty(userInput)) return;

            // Guardar el prompt en la base de datos aquí
            SavePromptToDatabase(userInput);

            myHistoryChat.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = userInput
            });

            int maxMessagesSize = 8;
            int messagesToRemoveCount = Math.Max(0, myHistoryChat.Count - maxMessagesSize);
            myHistoryChat.RemoveRange(0, messagesToRemoveCount);

            outtext.AppendText($"Usuario: {userInput}{System.Environment.NewLine}");

            intext.Clear();
            btnenviar.Enabled = false;

            JObject response = await GenerateAIResponce(groqApi);
            string? aiResponse = response?["choices"]?[0]?["message"]?["content"]?.ToString();

            outtext.AppendText($"{aiResponse}{System.Environment.NewLine}");

            myHistoryChat.Add(new JObject
            {
                ["role"] = "assistant",
                ["content"] = aiResponse
            });

            btnenviar.Enabled = true;
            intext.Focus();
        }

        private async Task<JObject> GenerateAIResponce(GroqAPI anApi)
        {
            JArray totalChatJArray = new JArray();
            foreach (var chat in myHistoryChat)
            {
                totalChatJArray.Add(chat);
            }

            JObject request = new JObject
            {
                ["model"] = "llama-3.1-8b-instant",
                ["messages"] = totalChatJArray
            };

            var result = await anApi.CreateChatCompletionAsync(request);
            return result;
        }

        private void btnword_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document|*.docx",
                Title = "Guardar como documento Word"
            };
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string[] lines = Regex.Split(outtext.Text, @"\r?\n")
                .Where(line => !line.TrimStart().StartsWith("Usuario:", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (lines.Length == 0)
            {
                MessageBox.Show("No hay contenido para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new W.Document();
                W.Body body = new W.Body();

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("**") && trimmedLine.EndsWith("**") && trimmedLine.Length > 4)
                    {
                        string titleText = trimmedLine.Substring(2, trimmedLine.Length - 4);

                        W.Paragraph titleParagraph = new W.Paragraph();
                        W.ParagraphProperties titleProps = new W.ParagraphProperties();
                        titleProps.Append(new W.Justification() { Val = W.JustificationValues.Center });
                        titleParagraph.Append(titleProps);

                        W.Run titleRun = new W.Run();
                        W.RunProperties runProps = new W.RunProperties();
                        runProps.Append(new W.Bold());
                        titleRun.Append(runProps);
                        titleRun.Append(new W.Text(titleText) { Space = SpaceProcessingModeValues.Preserve });
                        titleParagraph.Append(titleRun);

                        body.Append(titleParagraph);
                    }
                    else
                    {
                        W.Paragraph para = new W.Paragraph();
                        W.ParagraphProperties props = new W.ParagraphProperties();
                        props.Append(new W.Justification() { Val = W.JustificationValues.Both });
                        para.Append(props);

                        W.Run run = new W.Run();
                        run.Append(new W.Text(trimmedLine) { Space = SpaceProcessingModeValues.Preserve });
                        para.Append(run);

                        body.Append(para);
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
            MessageBox.Show("Documento Word guardado correctamente en:\n" + saveFileDialog.FileName, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnpwpt_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PowerPoint Presentation|*.pptx",
                Title = "Guardar como presentación PowerPoint"
            };
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            string[] lines = Regex.Split(outtext.Text, @"\r?\n")
                .Where(line => !line.TrimStart().StartsWith("Usuario:", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (lines.Length == 0)
            {
                MessageBox.Show("No hay contenido para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (PresentationDocument pptDoc = PresentationDocument.Create(saveFileDialog.FileName, PresentationDocumentType.Presentation))
            {
                PresentationPart presentationPart = pptDoc.AddPresentationPart();
                presentationPart.Presentation = new P.Presentation();

                // Slide Master
                SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
                slideMasterPart.SlideMaster = new P.SlideMaster(
                    new P.CommonSlideData(new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()
                        ),
                        new P.GroupShapeProperties()
                    )),
                    new P.SlideLayoutIdList(),
                    new P.TextStyles()
                );
                slideMasterPart.SlideMaster.Save();

                // Slide Layout
                SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
                slideLayoutPart.SlideLayout = new P.SlideLayout(
                    new P.CommonSlideData(new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()
                        ),
                        new P.GroupShapeProperties()
                    )),
                    new P.ColorMapOverride(new MasterColorMapping())
                );
                slideLayoutPart.SlideLayout.Save();

                // Relacionar layout con master
                slideMasterPart.SlideMaster.SlideLayoutIdList.Append(
                    new P.SlideLayoutId() { Id = 1U, RelationshipId = slideMasterPart.GetIdOfPart(slideLayoutPart) }
                );
                slideMasterPart.SlideMaster.Save();

                // Relacionar master con presentation
                presentationPart.Presentation.SlideMasterIdList = new P.SlideMasterIdList(
                    new P.SlideMasterId() { Id = 1U, RelationshipId = presentationPart.GetIdOfPart(slideMasterPart) }
                );

                P.SlideIdList slideIdList = new P.SlideIdList();
                uint slideId = 256U;

                // Título
                string firstLine = lines[0];
                string? titleText = null;
                int startIndex = 0;
                var trimmedFirstLine = firstLine.Trim();
                if (trimmedFirstLine.StartsWith("**") && trimmedFirstLine.EndsWith("**") && trimmedFirstLine.Length > 4)
                {
                    titleText = trimmedFirstLine.Substring(2, trimmedFirstLine.Length - 4);
                    startIndex = 1;
                }
                else
                {
                    titleText = firstLine;
                    startIndex = 1;
                }

                // Diapositiva de título
                SlidePart titleSlidePart = presentationPart.AddNewPart<SlidePart>();
                titleSlidePart.Slide = CreateSlideWithText(titleText);
                titleSlidePart.AddPart(slideLayoutPart);
                slideIdList.Append(new P.SlideId() { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(titleSlidePart) });

                // Diapositiva de contenido (todas las líneas restantes)
                string contentText = string.Join(System.Environment.NewLine, lines.Skip(startIndex));
                if (!string.IsNullOrWhiteSpace(contentText))
                {
                    SlidePart contentSlidePart = presentationPart.AddNewPart<SlidePart>();
                    contentSlidePart.Slide = CreateSlideWithText(contentText);
                    contentSlidePart.AddPart(slideLayoutPart);
                    slideIdList.Append(new P.SlideId() { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(contentSlidePart) });
                }

                presentationPart.Presentation.Append(slideIdList);
                presentationPart.Presentation.Save();
            }
            MessageBox.Show("Presentación PowerPoint guardada correctamente en:\n" + saveFileDialog.FileName, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Crea una diapositiva con un solo cuadro de texto
        private P.Slide CreateSlideWithText(string text)
        {
            var slide = new P.Slide();
            var commonSlideData = new P.CommonSlideData();
            var shapeTree = new P.ShapeTree();

            shapeTree.Append(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()
            ));
            shapeTree.Append(new P.GroupShapeProperties());

            var shape = new P.Shape();
            shape.Append(new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Texto" },
                new P.NonVisualShapeDrawingProperties(new A.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties()
            ));
            shape.Append(new P.ShapeProperties());

            var textBody = new P.TextBody();
            textBody.Append(new A.BodyProperties());
            textBody.Append(new A.ListStyle());
            var para = new A.Paragraph();
            var run = new A.Run();
            run.Append(new A.Text(text));
            para.Append(run);
            para.Append(new A.EndParagraphRunProperties());
            textBody.Append(para);
            shape.Append(textBody);

            shapeTree.Append(shape);
            commonSlideData.Append(shapeTree);
            slide.Append(commonSlideData);
            slide.Append(new P.ColorMapOverride(new MasterColorMapping()));
            return slide;
        }

        // Método para guardar el prompt en la base de datos con manejo de errores
        private void SavePromptToDatabase(string promptText)
        {
            string connectionString = "Server=ZEPHYRUS-DE-DAN\\SQLEXPRESS;Database=dbproyecto1;Trusted_Connection=True;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO prompts (prompt_text, prompt_time) VALUES (@PromptText, @PromptTime)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PromptText", promptText);
                        cmd.Parameters.AddWithValue("@PromptTime", DateTime.Now);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("No se insertó ningún registro en la base de datos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar en la base de datos:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
