using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using Android.App;
using Newtonsoft.Json;
using Plugin.Media;
using Xamarin.Forms;

namespace IdentificadorDePersona
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {


        const string SuscripcionKey = "0d72b077f997464b945b3476d14cf6b0";
        string urlObtenerFaceId = "https://centralus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true";
        const string urlDetect = "https://centralus.api.cognitive.microsoft.com/face/v1.0/detect";
        const string urlVerify = "https://centralus.api.cognitive.microsoft.com/face/v1.0/verify";


        public MainPage()
        {
            InitializeComponent();

        }

        private async void VerificarIdentidad(object sender, EventArgs e)
        {
            try
            {
                String Indentificación = "";


                Indentificación = IdentificacionEnlaUI.Text;

                if (IdentificacionEnlaUI.Text.Length > 8)
                    {

                        BotonVerficarIdentidad.IsEnabled = false;
                        Upload(Indentificación);

                    }
                    else
                    {

                        await DisplayAlert("Aviso", "La identidad debe tener 9 dígitos!", "Entiendo");
                    }
               

            }
            catch (Exception)
            {
               
                await DisplayAlert("Error", "Hubo un error!\nVerifique la identificación!", "Entiendo");
               
            }
        }

        private async void Upload(string identificacion)
        {

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
                CompressionQuality = 40
            });

            ImagenEnLaUI.Source = file.Path;

            await DisplayAlert("Información", "Espere un momento para cargar la foto", "Entiendo");

            byte[] datosDeLaFoto = System.IO.File.ReadAllBytes(file.Path);

                Persona persona = await ConexionConLaBD.GetPersona(identificacion);

                if (persona == null)
                {

                    BotonVerficarIdentidad.IsEnabled = true;
                    await DisplayAlert("Error", "Identificación no encontrada" , "Ok");
            }
            else
                {
                   string urlDeLaFotoEnBD = persona.Foto;

                    WebClient wc = new WebClient();
                    byte[] datosDeFotosEnBD = wc.DownloadData(urlDeLaFotoEnBD);
                    Comparar(datosDeFotosEnBD, datosDeLaFoto , persona);
            }
            
        }

        private async void Comparar(byte[] fotoDeLaBase, byte[] fotoIngresada, Persona persona)
        {
            FaceDetectRespuesta deteccionDeRostro1;
            FaceDetectRespuesta deteccionDeRostro2;

            using (var envioAzure = new WebClient())
            {

                envioAzure.Headers.Add(HttpRequestHeader.ContentType, "application/octet-stream");
                envioAzure.Headers.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);


                WebClient wc = new WebClient();

                byte[] resultadoFoto1 = envioAzure.UploadData(urlObtenerFaceId, fotoDeLaBase);


                var respuesta = Encoding.UTF8.GetString(resultadoFoto1);
                deteccionDeRostro1 = JsonConvert.DeserializeObject<FaceDetectRespuesta[]>(respuesta)[0];
            }

            using (var envioAzure = new WebClient())
            {
                envioAzure.Headers.Add(HttpRequestHeader.ContentType, "application/octet-stream");
                envioAzure.Headers.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);
                byte[] resultadoFoto2 = envioAzure.UploadData(urlObtenerFaceId, fotoIngresada);
                var respuesta = Encoding.UTF8.GetString(resultadoFoto2);
                deteccionDeRostro2 = JsonConvert.DeserializeObject<FaceDetectRespuesta[]>(respuesta)[0];
            }

            using (var envioAzure = new WebClient())
            {
                envioAzure.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                envioAzure.Headers.Add("Ocp-Apim-Subscription-Key", SuscripcionKey);

                string json = "{\"faceId1\":\"" + deteccionDeRostro1.FaceId + "\", \"faceId2\":\"" + deteccionDeRostro2.FaceId + "\" }";
                string mensajeRespuesta = envioAzure.UploadString(urlVerify, json);
                FaceVerifyRespuesta respuesta = JsonConvert.DeserializeObject<FaceVerifyRespuesta>(mensajeRespuesta);

                if (respuesta.IsIdentical == true)

                    await DisplayAlert("Respuesta", "La foto coincide con la persona " + persona.Nombre + ", identificacíon " + persona.Identificacion, "Entiendo");

                else if (respuesta.IsIdentical == false)
                    await DisplayAlert("Respuesta", "La foto NO coincide con la persona " + persona.Nombre + ", identificacíon " + persona.Identificacion, "Entiendo");

            }

        }


    }
}
