using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;

namespace Tabs
{
    public partial class CustomVision : ContentPage
    {
        public CustomVision()
        {
            InitializeComponent();
        }

        private async void LoadCamera(object sender, EventArgs e)
        {
            Save.IsVisible = false;
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

            if (file is null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });
            TagLabel.Text = string.Empty;
            ReadHandwrittenText(file);
            file.Dispose();
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            var binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async void ReadHandwrittenText(MediaFile file)
        {
            loading.IsVisible = true;
            loading.IsRunning = true;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "bcf289ccb2734314ae4d616154216468");
            var requestParameters = "handwriting=true";
            var uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/recognizeText?" + requestParameters;
            HttpResponseMessage response = null;
            string operationLocation = null;
            var byteData = GetImageAsByteArray(file);
            var content = new ByteArrayContent(byteData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response = await client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
                operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
            else
            {
                var ejson = JsonPrettyPrint(await response.Content.ReadAsStringAsync());
                var elines = ejson.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var error = string.Empty;
                foreach (var line in elines)
                {
                    if (line.Trim().IndexOf("m") == 1)
                    {
                        error = line.Remove(line.Length - 1).Trim().Substring(12) + " Please try again.";
                    }
                }
                TagLabel.Text = "\nError:\n" + error;
                loading.IsRunning = false;
                loading.IsVisible = false;
                return;
            }
            string contentString;
            var i = 0;
            do
            {
                response = await client.GetAsync(operationLocation);
                contentString = await response.Content.ReadAsStringAsync();
                ++i;
            }
            while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);
            if (i == 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
            {
                TagLabel.Text = "\nTimeout error.\n";
                loading.IsRunning = false;
                loading.IsVisible = false;
                return;
            }
            var json = JsonPrettyPrint(contentString);
            var lines = json.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var textlines = new List<string>();
            foreach (var line in lines)
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
            var result = "";
            var j = 0;
            while(j<textlines.ToArray().Length)
            {
                result += textlines[j] + "\n";
                j += textlines[j].Split(' ').Length+1;
            }
            TagLabel.Text = "\nResponse:\n" + result;
            Save.IsVisible = true;
            loading.IsRunning = false;
            loading.IsVisible = false;
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, string.Empty).Replace("\t", "");

            var sb = new StringBuilder();
            var quote = false;
            var ignore = false;
            var offset = 0;
            var indentLength = 3;

            foreach (var ch in json)
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
            return sb.ToString().Trim();
        }

        async Task PostNoteAsync()
        {
            loading.IsRunning = true;
            loading.IsVisible = true;
            var model = new nbha675()
            {
                Text = TagLabel.Text.Substring(11).Trim()
            };

            await AzureManager.AzureManagerInstance.PostNote(model);
            loading.IsRunning = false;
            loading.IsVisible = false;
            Save.IsVisible = false;
        }
    }
}
