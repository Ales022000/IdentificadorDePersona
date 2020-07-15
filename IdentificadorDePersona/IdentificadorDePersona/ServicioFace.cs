using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace IdentificadorDePersona
{
    class ServicioFace
    {
        const string SuscripcionKey = "0d72b077f997464b945b3476d14cf6b0";
    string urlObtenerFaceId = "https://centralus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true";
    const string urlDetect = "https://centralus.api.cognitive.microsoft.com/face/v1.0/detect";  
    const string urlVerify = "https://centralus.api.cognitive.microsoft.com/face/v1.0/verify";

    public string resultado;

    FaceDetectRespuesta deteccionCara1 = new FaceDetectRespuesta();
    FaceDetectRespuesta deteccionCara2 = new FaceDetectRespuesta();


    byte[] datosImagen1, datosImagen2, respuestaImagen1, respuestaImagen2;






    public async void DetectarCara(string direccionPathImagen)
    {
        HttpClient clienteAzure = new HttpClient();
        clienteAzure.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);

        string ParametrosRequest = "returnFaceId=false&returnFaceLandmarks=false" +
            "&returnFaceAttributes=age,gender,smile,glasses";

        string url = urlDetect + "?" + ParametrosRequest;


        HttpResponseMessage respuestaHttp;

        byte[] imagenEnByte = ObtenerImagenEnBytes(direccionPathImagen);

        using (ByteArrayContent aregloByteImagen = new ByteArrayContent(imagenEnByte))
        {
            aregloByteImagen.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            respuestaHttp = await clienteAzure.PostAsync(url, aregloByteImagen);
            string contenidoRespuesta = await respuestaHttp.Content.ReadAsStringAsync();
            resultado = contenidoRespuesta;
        }

    } // todo igual a azure face

    public string CompararCaras(string rutaDeImagen2)
    {
        string resultadoComparacion = "";

        using (var envioAzure = new WebClient())
        {
            envioAzure.Headers.Add(HttpRequestHeader.ContentType, "application/octet-stream");
            envioAzure.Headers.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);

            WebClient wc = new WebClient();

            byte[] datosImagen1 = wc.DownloadData("https://blobstorageazure.blob.core.windows.net/contenedordearchivos/2cara2.jpg");

            respuestaImagen1 = envioAzure.UploadData(urlObtenerFaceId, datosImagen1);

            var respuesta = Encoding.UTF8.GetString(respuestaImagen1);

            deteccionCara1 = JsonConvert.DeserializeObject<FaceDetectRespuesta[]>(respuesta)[0];


        }

        using (var envioAzure = new WebClient())
        {
            envioAzure.Headers.Add(HttpRequestHeader.ContentType, "application/octet-stream");
            envioAzure.Headers.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);

            datosImagen2 = File.ReadAllBytes(rutaDeImagen2);
            respuestaImagen2 = envioAzure.UploadData(urlObtenerFaceId, datosImagen2);

            var respuesta = Encoding.UTF8.GetString(respuestaImagen2);

            deteccionCara2 = JsonConvert.DeserializeObject<FaceDetectRespuesta[]>(respuesta)[0];
        }


        using (var envioAzure = new WebClient())
        {
            envioAzure.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            envioAzure.Headers.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);

            string json = "{\"faceId1\":\"" + deteccionCara1.FaceId + "\", \"faceId2\":\"" + deteccionCara2.FaceId + "\" }";
            string mensajeRespuesta = envioAzure.UploadString(urlVerify, json);
            FaceVerifyRespuesta respuesta = JsonConvert.DeserializeObject<FaceVerifyRespuesta>(mensajeRespuesta);

            resultadoComparacion = mensajeRespuesta;
        }

        return resultadoComparacion;
    }//Distinto





    public byte[] ObtenerImagenEnBytes(string direccionPathImagen)
    {
        using (FileStream fileStreamImagen = new FileStream(direccionPathImagen, FileMode.Open, FileAccess.Read))
        {
            BinaryReader lectorBinario = new BinaryReader(fileStreamImagen);
            return lectorBinario.ReadBytes((int)fileStreamImagen.Length);
        }
    }

    public string RespuestaImpresionJson(string json)
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

        return sb.ToString().Trim();
    }

}
}
