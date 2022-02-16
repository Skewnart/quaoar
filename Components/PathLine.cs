using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Quaoar.Components
{
    public class PathLine
    {
        public string Name { get; set; }

        private string fileName;
        public BitmapImage Source { get { return new BitmapImage(new Uri($"/Images/{fileName}.png", UriKind.RelativeOrAbsolute)); } }

        public PathLine(string name, string filename)
        {
            this.Name = name;
            this.fileName = filename;
        }
    }
}
