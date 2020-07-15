using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentificadorDePersona
{
    public class Persona
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "Identificación")]
        public string Identificacion { get; set; }

        [JsonProperty(PropertyName = "Nombre")]
        public string Nombre { get; set; }

        [JsonProperty(PropertyName = "Primer Apellido")]
        public string PrimerApellido { get; set; }

        [JsonProperty(PropertyName = "Segundo Apellido")]
        public string SegundoApellido { get; set; }

        [JsonProperty(PropertyName = "Foto")]
        public string Foto { get; set; }



    }
}