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
using WindowsInput;

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
        int numberOfSpam;
        int delayTime;
        int asciiWidth;
        string messageOne;
        string messageTwo;
        //Links for additional spam functions
        string asciiStringLocation;
        List<string> asciiStringArray;
        List<string> randomPhraseArray;

        private static stringFunctions stringFunctions = new stringFunctions();
        private readonly InputSimulator simulator = new InputSimulator();

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
                spamFlag = spamMethod.twoMessage;
            }
            //This function is not working
            else if (enableMessageBox2.IsChecked == false)
            {
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
            if (delaySlider.IsLoaded)
            {
                delayLabel.Content = Convert.ToInt32(delaySlider.Value).ToString() + " ms";
            }
        }

        private void startSpam_Click(object sender, RoutedEventArgs e)
        {
            if (stopFlag == false)
            {
                stopFlag = true;
                startSpam.Content = "Stop";
                endlessSpamBool = Convert.ToBoolean(endlessSpamMode.IsChecked);
                delayTime = Convert.ToInt32(delaySlider.Value);
                messageOne = messageBox1.Text;
                messageTwo = messageBox2.Text;
                //Need check for this thing to ensure its always an int
                asciiWidth = Convert.ToInt32(asciiWidthBox.Text);
                numberOfSpam = Convert.ToInt32(timesToSpam.Text);

                Thread.Sleep(2000);

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
                else if (spamFlag == spamMethod.imageToASCII && asciiStringLocation != null)
                {
                    progressBar.Value = 0;
                    asciiStringArray = stringFunctions.openImage(asciiWidth, asciiStringLocation);
                    worker.RunWorkerAsync();
                }
                else if (spamFlag == spamMethod.randomPhrase && randomPhraseArray != null)
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
                        int returnProgressPercent = (int)Math.Ceiling((decimal)(count) / numberOfSpam * 100);
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
                copyPasteClipboard(messageOne);
            }
            else if (spamFlag == spamMethod.twoMessage)
            {
                copyPasteClipboard(messageOne);
                Thread.Sleep(delayTime);
                copyPasteClipboard(messageTwo);
            }
            else if (spamFlag == spamMethod.imageToASCII)
            {
                for (int i = 0; i < asciiStringArray.Count; i++)
                {
                    copyPasteClipboard(asciiStringArray[i]);
                    Thread.Sleep(delayTime);
                }
            }
            else if (spamFlag == spamMethod.randomPhrase)
            {
                Random rand = new Random();
                copyPasteClipboard(randomPhraseArray[rand.Next(0, (randomPhraseArray.Count))]);
            }
        }

        private void copyPasteClipboard(string spammingString)
        {
            simulator.Keyboard.TextEntry(spammingString);
            Thread.Sleep(10);
            simulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
        }

        private void selectImage_Click(object sender, RoutedEventArgs e)
        {
            asciiStringLocation = stringFunctions.openFileDialogFile("JPG|*.jpg|PNG|*.png");
        }

        private void selectTXT_Click(object sender, RoutedEventArgs e)
        {
            randomPhraseArray = stringFunctions.openTXT();
        }

    }
}
