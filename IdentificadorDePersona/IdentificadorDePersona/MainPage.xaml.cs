using System;
using System.ComponentModel;
using Plugin.Media;
using Xamarin.Forms;

namespace IdentificadorDePersona
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private async void VerificarURL (object sender, EventArgs e)
        {
            try
            {

                string URLDeLaFoto = UrlDeLaFoto.Text;


                bool Resultado = Uri.TryCreate(URLDeLaFoto, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);


                if (Resultado) {

                    await DisplayAlert("Verificando", "Espere un momento para cargar su foto.\nPor favor.", "Entendido");

                    ImagenEnLaUI.Source = URLDeLaFoto;

                    BotonVerificarURL.IsEnabled = true;
                }
                else {
                    await DisplayAlert("Verificación", "Compruebe su URL.\nPor favor.", "Entendido");
                }

            }
            catch (Exception)
            {
                await DisplayAlert("Error","¿Seguro que tu URL corresponde a una foto?","Ok");
            }

        }

        private async void EntrarAGaleria(object sender, EventArgs e)
        {

            if (CrossMedia.Current.IsTakePhotoSupported) {

                var imagen = await CrossMedia.Current.PickPhotoAsync();

                if (imagen != null) {

                    ImagenEnLaUI.Source = ImageSource.FromStream(() =>
                    {
                        var stream = imagen.GetStream();
                        imagen.Dispose();
                        return stream;

                    });

                    UrlDeLaFoto.Text = "Galeria";

                }

            }

        }

        private async void VerificarIdentidad(object sender, EventArgs e)
        {

            try
            {
                int.TryParse(IdentificacionEnlaUI.Text, out int IndentificaciónParseada);

                Image imagenlocal, imagenStorage;

                 
                if (IdentificacionEnlaUI.Text.Length > 8) {

                    await ConexionConLaBD.Initialize();

                    Persona persona = await ConexionConLaBD.GetPersona(IndentificaciónParseada);

                    string nombre = persona.Nombre;

                    ImagenEnLaUI.Source = persona.Foto;

                    UrlDeLaFoto.Text = persona.Foto;

                    if (nombre != null)
                    {   
                        await DisplayAlert("Prueba", "Nombre: " + nombre, "Bien");
                    }
                }
                else {

                    await DisplayAlert("Aviso", "La identidad debe tener 9 dígitos!", "Entiendo");
                }

            }
            catch (NotImplementedException notImp)
            {
               
                await DisplayAlert("Error", notImp.Message, "Ok");
               
            }
        }
    }
}
