using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WarOfRightsUnpack.Common
{
    public class DDSImage
    {
        private readonly Pfim.IImage _image;

        public DDSImage(string file)
        {
            _image = Pfim.Pfimage.FromFile(file);
            Process();
        }

        public void Save(string file)
        {
            if (_image.Format == Pfim.ImageFormat.Rgba32)
                Save<Bgra32>(file);
            else if (_image.Format == Pfim.ImageFormat.Rgb24)
                Save<Bgr24>(file);
            else
                throw new Exception("Unsupported pixel format (" + _image.Format + ")");
        }

        private void Process()
        {
            if (_image == null)
                throw new Exception("DDSImage image creation failed");

            if (_image.Compressed)
                _image.Decompress();
        }

        private void Save<T>(string file) where T : unmanaged, IPixel<T>
        {
            Image<T> image = Image.LoadPixelData<T>(
                _image.Data, _image.Width, _image.Height);
            image.Save(file);
        }

    }
}
