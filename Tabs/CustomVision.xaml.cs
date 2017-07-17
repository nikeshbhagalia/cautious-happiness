using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using System.Text;
using System.Collections.Generic;

namespace Tabs
{
    public partial class CustomVision : ContentPage
    {
        public CustomVision()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });
            TagLabel.Text = "";
            ReadHandwrittenText(file);
            file.Dispose();
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async void ReadHandwrittenText(MediaFile file)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "bcf289ccb2734314ae4d616154216468");

            // Request parameter. Set "handwriting" to false for printed text.
            string requestParameters = "handwriting=true";

            // Assemble the URI for the REST API Call.
            string uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/recognizeText?" + requestParameters;

            HttpResponseMessage response = null;

            // This operation requrires two REST API calls. One to submit the image for processing,
            // the other to retrieve the text found in the image. This value stores the REST API
            // location to call to retrieve the text.
            string operationLocation = null;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(file);
            ByteArrayContent content = new ByteArrayContent(byteData);

            // This example uses content type "application/octet-stream".
            // You can also use "application/json" and specify an image URL.
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // The first REST call starts the async process to analyze the written text in the image.
            response = await client.PostAsync(uri, content);

            // The response contains the URI to retrieve the result of the process.
            if (response.IsSuccessStatusCode)
                operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
            else
            {
                // Display the JSON error data.
                string ejson = JsonPrettyPrint(await response.Content.ReadAsStringAsync());
                string[] elines = ejson.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                string error = "";
                foreach (string line in elines)
                {
                    if (line.Trim().IndexOf("m") == 1)
                    {
                        error = line.Remove(line.Length - 1).Trim().Substring(12) + " Please try again.";
                    }
                }
                TagLabel.Text = "\nError:\n" + error;
                return;
            }

            // The second REST call retrieves the text written in the image.
            //
            // Note: The response may not be immediately available. Handwriting recognition is an
            // async operation that can take a variable amount of time depending on the length
            // of the handwritten text. You may need to wait or retry this operation.
            //
            // This example checks once per second for ten seconds.
            string contentString;
            int i = 0;
            do
            {
                //System.Threading.Thread.Sleep(1000);
                response = await client.GetAsync(operationLocation);
                contentString = await response.Content.ReadAsStringAsync();
                ++i;
            }
            while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);
            if (i == 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
            {
                TagLabel.Text = "\nTimeout error.\n";
                return;
            }

            // Display the JSON response.
            string json = JsonPrettyPrint(contentString);
            string[] lines = json.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            List<string> textlines = new List<string>();
            foreach (string line in lines)
            {
                if (line.Trim().IndexOf("t") == 1)
                {
                    if (line[line.Length-1].ToString() == ",")
                    {
                        textlines.Add(line.Remove(line.Length - 2).Trim().Substring(9));
                    }
                    else
                    {
                        textlines.Add(line.Remove(line.Length - 1).Trim().Substring(9));
                    }
                }
            }
            string result = "";
            int j = 0;
            while(j<textlines.ToArray().Length)
            {
                result += textlines[j] + "\n";
                j += textlines[j].Split(' ').Length+1;
            }
            TagLabel.Text = "\nResponse:\n" + result;
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }
            //Debug.WriteLine(sb.ToString().Trim());
            return sb.ToString().Trim();
        }
    }
}