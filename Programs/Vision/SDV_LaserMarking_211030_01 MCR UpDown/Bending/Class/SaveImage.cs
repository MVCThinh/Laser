using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bending
{
    /// <summary>
    /// 이미지 저장할 때 사용 
    /// </summary>
    public class SaveImage //JJ , 2017-05-20 : Image Save -- create class
    {
        /// <summary>
        /// 적용할 이미지 Format
        /// </summary>
        public System.Drawing.Imaging.ImageFormat format { get; set; }
        /// <summary>
        /// 이미지 Quality, BMP는 적용 불가
        /// </summary>
        public long Quality { get; set; }

 
        public void Save(Cognex.VisionPro.CogImage8Grey cTempImage, string fileName)
        {
            Save(cTempImage.ToBitmap(), fileName);
        }
        /// <summary>
        /// 이미지 파일로 저장
        /// </summary>
        /// <param name="image">저장 할 이미지</param>
        /// <param name="fileName"></param>
        public void Save(System.Drawing.Bitmap image, string fileName)
        {
            if (image == null) return;
            if (fileName == null || fileName.Length == 0) return;
            if (format == null) return;

            try
            {
                if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) == true) // JPEG 저장
                {
                    // 확장자 명 결정
                    fileName = fileName + ".jpeg";
                    // 이미지 Format
                    System.Drawing.Imaging.ImageCodecInfo imgCodec = null;
                    foreach (System.Drawing.Imaging.ImageCodecInfo info in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
                    {
                        if (info.MimeType.Equals("image/jpeg") == true)
                        {
                            imgCodec = info;
                            break;
                        }
                    }
                    // 이미지 Quality
                    System.Drawing.Imaging.EncoderParameters eps = new System.Drawing.Imaging.EncoderParameters(1);
                    eps.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
                    // 이미지 저장
                    image.Save(fileName, imgCodec, eps);

                }
                else // BMP 저장
                {
                    // 확장자 명 결정
                    fileName = fileName + ".bmp";
                    image.Save(fileName);
                }

                //File Remove 부분 추가


            }
            catch { }
        }
    }
}
