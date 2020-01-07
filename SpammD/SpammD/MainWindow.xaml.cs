using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpammD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum spamMethod
        {
            oneMessage,
            twoMessage,
            imageToASCII,
            randomPhrase
        }

        //Enum for type of spam method
        private spamMethod spamFlag = spamMethod.oneMessage;
        //This flag is used for the loops to exit endless mode / x number of loops
        private bool stopFlag = false;
        //Deligated background worker to allow sleep to function
        private readonly BackgroundWorker worker = new BackgroundWorker();
        //As different thread owns it
        bool endlessSpamBool = false;
        int numberOfSpam = 0;
        int delayTime = 1000;
        int asciiWidth = 64;
        private static readonly char[] asciiChars = { '█', '░', '@', '&', '$', '%', '!', '(', ')', '=', '+', '^', '*', ';', ':', '_', '-', '"', '/', ',', '.', ' ' };
        string messageOne;
        string messageTwo;
        //Links for additional spam functions
        //REMOVE BLANK LATER
        string asciiImage = "blank";
        string randomPhrases = "blank";

        List<string> asciiStringArray;
        List<string> randomPhraseArray;

        public MainWindow()
        {
            InitializeComponent();


            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += BackgroundWorkerDoingSpam;
            worker.ProgressChanged += BackgroundWorkerDoingSpam_ProgressChanged;
            worker.RunWorkerCompleted += BackgroundWorkerDoingSpam_RunWorkerCompleted;
        }

        private void enableMessageBox2_Checked(object sender, RoutedEventArgs e)
        {
            if (enableMessageBox2.IsChecked == true)
            {
                enableImageToASCII.IsChecked = false;
                enableRandomPhrase.IsChecked = false;
                messageBox2.IsReadOnly = false;
                spamFlag = spamMethod.twoMessage;
            }
            //This function is not working
            else if (enableMessageBox2.IsChecked == false)
            {
                messageBox2.IsReadOnly = true;
                if (enableImageToASCII.IsChecked == false && enableRandomPhrase.IsChecked == false)
                {
                    spamFlag = spamMethod.oneMessage;
                }
            }
        }

        private void enableImageToASCII_Checked(object sender, RoutedEventArgs e)
        {
            if (enableImageToASCII.IsChecked == true)
            {
                enableMessageBox2.IsChecked = false;
                enableRandomPhrase.IsChecked = false;
                spamFlag = spamMethod.imageToASCII;
            }
        }

        private void enableRandomPhrase_Checked(object sender, RoutedEventArgs e)
        {
            if (enableRandomPhrase.IsChecked == true)
            {
                enableMessageBox2.IsChecked = false;
                enableImageToASCII.IsChecked = false;
                spamFlag = spamMethod.randomPhrase;
            }
        }

        private void delaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //delayLabel.Content = delaySlider.Value.ToString() + " ms";
        }

        private void startSpam_Click(object sender, RoutedEventArgs e)
        {
            if (stopFlag == false)
            {
                stopFlag = true;
                startSpam.Content = "Stop";
                //Need check for this thing to ensure its always an int
                endlessSpamBool = Convert.ToBoolean(endlessSpamMode.IsChecked);
                delayTime = Convert.ToInt32(delaySlider.Value);
                numberOfSpam = Convert.ToInt32(timesToSpam.Text);
                messageOne = messageBox1.Text;
                messageTwo = messageBox2.Text;

                if (spamFlag == spamMethod.oneMessage && messageBox1.Text != null)
                {
                    progressBar.Value = 0;
                    worker.RunWorkerAsync();
                }
                else if (spamFlag == spamMethod.twoMessage && messageBox1.Text != null && messageBox2.Text != null)
                {
                    progressBar.Value = 0;
                    worker.RunWorkerAsync();
                }
                else if (spamFlag == spamMethod.imageToASCII && asciiImage != null)
                {
                    progressBar.Value = 0;
                    worker.RunWorkerAsync();
                }
                else if (spamFlag == spamMethod.randomPhrase && randomPhrases != null)
                {
                    progressBar.Value = 0;
                    worker.RunWorkerAsync();
                }
            }
            else if (stopFlag == true)
            {
                if (worker.IsBusy)
                {
                    worker.CancelAsync();
                }
                startSpam.Content = "Start";
            }
        }

        private void BackgroundWorkerDoingSpam(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            try
            {
                if (endlessSpamBool == true)
                {
                    while (true)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }

                        doSpam();
                        Thread.Sleep(delayTime);
                    }
                }
                else if (endlessSpamBool == false)
                {
                    int count = 0;
                    while (count < numberOfSpam)
                    {
                        if (worker.CancellationPending == true)
                        {
                            e.Cancel = true;
                            break;
                        }

                        doSpam();
                        Thread.Sleep(delayTime);
                        count++;

                        //Returns progress bar %
                        int returnProgressPercent = (int)Math.Ceiling((decimal)(count) / numberOfSpam * 100); // NO IDEA WHY IT DOESNT WORK
                        worker.ReportProgress(returnProgressPercent);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An Error Has Occured! Error: " + ex);
            }
        }

        private void BackgroundWorkerDoingSpam_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                System.Windows.MessageBox.Show("Operation Cancelled");
                stopFlag = false;
                startSpam.Content = "Start";
                progressBar.Value = 100;
            }
            else if (e.Error != null)
            {
                System.Windows.MessageBox.Show("Error in Process :" + e.Error);
            }
            else
            {
                System.Windows.MessageBox.Show("Success! The task has finished successfully.");
                stopFlag = false;
                startSpam.Content = "Start";
                progressBar.Value = 100;
            }
        }

        private void BackgroundWorkerDoingSpam_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void doSpam()
        {
            if (spamFlag == spamMethod.oneMessage)
            {
                MessageBox.Show(messageOne);
            }
            else if (spamFlag == spamMethod.twoMessage)
            {
                MessageBox.Show(messageOne);
                Thread.Sleep(delayTime);
                MessageBox.Show(messageTwo);
            }
            else if (spamFlag == spamMethod.imageToASCII)
            {
                for (int i = 0; i < asciiStringArray.Count; i++)
                {
                    MessageBox.Show(asciiStringArray[i]);
                    Thread.Sleep(delayTime);
                }
            }
            else if (spamFlag == spamMethod.randomPhrase)
            {
                Random rand = new Random();
                //Make function to paste message
                MessageBox.Show(randomPhraseArray[rand.Next(0, (randomPhraseArray.Count))]);
            }
        }

        private void selectImage_Click(object sender, RoutedEventArgs e)
        {
            openImage();
        }

        public void openImage()
        {
            try
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JPG|*.jpg|PNG|*.png";
                if (openFileDialog.ShowDialog() == true)
                {
                    asciiImage = openFileDialog.FileName;

                    System.Drawing.Image inputImage = System.Drawing.Image.FromFile(asciiImage);
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
                    MessageBox.Show(asciiImage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: An Error Occured When Loading The File \nERROR:" + ex);
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

        private void selectTXT_Click(object sender, RoutedEventArgs e)
        {
            openTXT();
        }

        private void openTXT()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TXT|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                randomPhrases = openFileDialog.FileName;
                randomPhraseArray = new List<string>();
                using (StreamReader sr = new StreamReader(randomPhrases))
                {
                    string data;
                    while((data = sr.ReadLine()) != null)
                    {
                        randomPhraseArray.Add(data);
                    }
                }
            }
        }

    }
}
