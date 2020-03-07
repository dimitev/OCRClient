using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NFRV
{
    public class OCRInterface
    {
        HttpClient client = new HttpClient();
        public System.Drawing.Bitmap image;

        public string Recognize()
        {
            HttpWebRequest webRequest = null;
            string response = "";
            byte[] bytes = Encoding.UTF8.GetBytes(BitmapToBase64(image));

            try
            {
                webRequest = (HttpWebRequest)WebRequest.Create("http://192.168.0.102:5000/ocr?lang=bul");
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = "POST";
                webRequest.Timeout = 5000;

                webRequest.ContentLength = bytes.Length;
                using (Stream requeststream = webRequest.GetRequestStream())
                {
                    requeststream.Write(bytes, 0, bytes.Length);
                    requeststream.Close();
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                    {
                        response = sr.ReadToEnd().Trim();
                        sr.Close();
                    }
                    webResponse.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return response;
        }
        public string BitmapToBase64(System.Drawing.Bitmap bi)
        {

            System.IO.MemoryStream ms = new MemoryStream();
            bi.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            return Convert.ToBase64String(byteImage);
        }
    }
}
