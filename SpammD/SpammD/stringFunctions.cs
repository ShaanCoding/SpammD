using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpammD
{
    class stringFunctions
    {
        private static readonly char[] asciiChars = { '█', '░', '@', '&', '$', '%', '!', '(', ')', '=', '+', '^', '*', ';', ':', '_', '-', '"', '/', ',', '.', ' ' };

        public string openFileDialogFile(string userFilter)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = userFilter;
                if(openFileDialog.ShowDialog() == true)
                {
                    return openFileDialog.FileName;
                }
                else
                {
                    return "";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("ERROR: An Error Occured When Loading The File \nERROR:" + ex);
                return "";
            }
        }

        public List<string> openImage(int asciiWidth, string asciiImageDirectory)
        {
            List<string> asciiStringArray = new List<string>();
            try
            {
                System.Drawing.Image inputImage = System.Drawing.Image.FromFile(asciiImageDirectory);
                Bitmap inputImageResize = resizeImage(inputImage, asciiWidth);

                asciiStringArray = new List<string>();
                for (int y = 0; y < inputImageResize.Height; y++)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int x = 0; x < inputImageResize.Width; x++)
                    {
                        System.Drawing.Color colour = inputImageResize.GetPixel(x, y);
                        int brightness = findBrightness(colour);
                        string pxlToChar = brightnessToChar(brightness);
                        stringBuilder.Append(pxlToChar);
                        stringBuilder.Append(pxlToChar);
                    }
                    asciiStringArray.Add(stringBuilder.ToString());
                }
                return asciiStringArray;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: An Error Occured When Loading The File \nERROR:" + ex);
                asciiStringArray = new List<string>();
                return asciiStringArray;
            }
        }

        public static Bitmap resizeImage(System.Drawing.Image inputImage, int asciiWidth)
        {
            //Gains proper ratio height in characters
            int asciiHeight = (int)Math.Ceiling((double)inputImage.Height * asciiWidth / inputImage.Width);

            //Defines new rescaled bitmap
            Bitmap outputImage = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)outputImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputImage, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return outputImage;
        }

        public static int findBrightness(System.Drawing.Color inputColour)
        {
            int brightness = (inputColour.R + inputColour.G + inputColour.B) / 3;
            return brightness;
        }

        public static string brightnessToChar(int brightness)
        {
            int index = brightness * 10 / 255;
            return asciiChars[index].ToString();
        }

        public List<string> openTXT()
        {
            List<string> randomPhraseArray = new List<string>();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TXT|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                string randomPhrasesDirectory = openFileDialog.FileName;
                using (StreamReader sr = new StreamReader(randomPhrasesDirectory))
                {
                    string data;
                    while ((data = sr.ReadLine()) != null)
                    {
                        randomPhraseArray.Add(data);
                    }
                }
            }
            return randomPhraseArray;
        }
    }
}
