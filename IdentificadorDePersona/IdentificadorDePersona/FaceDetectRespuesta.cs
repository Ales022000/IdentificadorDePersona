using System;
using System.Collections.Generic;
using System.Text;

namespace IdentificadorDePersona
{
    public class FaceDetectRespuesta
    {
        public string FaceId { get; set; }
        public FaceRectangle FaceRectangle { get; set; }
    }

    public class FaceRectangle
    {
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }

    }
}