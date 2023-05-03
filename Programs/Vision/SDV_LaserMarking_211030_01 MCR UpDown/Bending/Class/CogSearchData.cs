using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;

namespace Bending
{
    public class CogSearchData
    {
        //pcy210107 DL OK용 데이터
        bool bcogSearch;
        double pointX;
        double pointY;
        double imgWidth;
        double imgHeight;
        //ICogImage imageMemory;
        public ICogImage ImageMemory;

        public bool BcogSearch { get => bcogSearch; set => bcogSearch = value; }
        public double PointX { get => pointX; set => pointX = value; }
        public double PointY { get => pointY; set => pointY = value; }
        public double ImgWidth { get => imgWidth; set => imgWidth = value; }
        public double ImgHeight { get => imgHeight; set => imgHeight = value; }
        //public ICogImage ImageMemory { get => imageMemory; set => imageMemory = value; }
    }
}
