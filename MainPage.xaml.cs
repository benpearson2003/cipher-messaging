using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace CipherMessaging
{
    public partial class MainPage : PhoneApplicationPage
    {
        Accelerometer accelerometer = null;
        //represents the time of the first significant movement
        DateTimeOffset movementMoment = new DateTimeOffset();
        double firstShakeStep = 0; //represents the value of the first significant movement

        EmailAddressChooserTask emailAddressChooserTask;
        PhoneNumberChooserTask phoneNumberChooserTask;
        public MainPage()
        {
            
            InitializeComponent();
            InitAccelerometer();
            
            emailAddressChooserTask = new EmailAddressChooserTask();
            emailAddressChooserTask.Completed += new EventHandler<EmailResult>(emailAddressChooserTask_Completed);
            
            phoneNumberChooserTask = new PhoneNumberChooserTask();
            phoneNumberChooserTask.Completed += new EventHandler<PhoneNumberResult>(phoneNumberChooserTask_Completed);
        }

        /* Encrypts a message using a key, the key
         * Needs to be given to the recipient of the
         * message.*/
        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            string input = encryptBox.Text;
            encryptBox.Text = "";
            int encryptionKey;
            int.TryParse(encryptKey.Text, out encryptionKey);

            foreach (char c in input)
            {
                char letter = c;
                if (char.IsLetter(c))
                {
                    int num = (int)c;
                    letter = (char)(c + encryptionKey);
                    if (char.IsUpper(c))
                    {
                        if (letter > 'Z')
                        {
                            letter = (char)(letter - 26);
                        }
                        else
                        {
                            if (letter < 'A')
                            {
                                letter = (char)(letter + 26);
                            }
                        }
                    }
                    else
                    {
                        if (char.IsLower(c))
                        {
                            if (letter > 'z')
                            {
                                letter = (char)(letter - 26);
                            }
                            else
                            {
                                if (letter < 'a')
                                {
                                    letter = (char)(letter + 26);
                                }
                            }
                        }
                    }
                    encryptBox.Text += letter;
                }
                else
                {
                    encryptBox.Text += c;
                }
            }
        }

        /*Decrypts a message using a key*/
        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            string input = decryptBox.Text;
            decryptBox.Text = "";
            int encryptionKey;
            int.TryParse(decryptKey.Text, out encryptionKey);

            foreach (char c in input)
            {
                char letter = c;
                if (char.IsLetter(c))
                {
                    int num = (int)c;
                    letter = (char)(c - encryptionKey);
                    if (char.IsUpper(c))
                    {
                        if (letter > 'Z')
                        {
                            letter = (char)(letter - 26);
                        }
                        else
                        {
                            if (letter < 'A')
                            {
                                letter = (char)(letter + 26);
                            }
                        }
                    }
                    else
                    {
                        if (char.IsLower(c))
                        {
                            if (letter > 'z')
                            {
                                letter = (char)(letter - 26);
                            }
                            else
                            {
                                if (letter < 'a')
                                {
                                    letter = (char)(letter + 26);
                                }
                            }
                        }
                    }
                    decryptBox.Text += letter;
                }
                else
                {
                    decryptBox.Text += c;
                }
            }
        }

        /*Opens the contact chooser*/
        private void smsEncrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                phoneNumberChooserTask.Show();
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("An error occurred.");
            }
        }
        
        /*Opens the email chooser*/
        private void emailEncrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                emailAddressChooserTask.Show();
            }
            catch
            {
                MessageBox.Show("An error occurred.");
            }
        }

        /*Sends an encrypted message through SMS*/
        void phoneNumberChooserTask_Completed(object sender, PhoneNumberResult e)
        {
            new SmsComposeTask()
            {
                Body = encryptBox.Text,
                To = e.PhoneNumber
            }.Show();
        }

        /*Sends an encrypted message to an email contact.*/
        private void emailAddressChooserTask_Completed(object sender, EmailResult e)
        {
            new EmailComposeTask()
            {
                Body = encryptBox.Text,
                To = e.Email
            }.Show();
        }
        
        private void InitAccelerometer()
        {
            accelerometer = new Accelerometer();
            accelerometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(Accelerometer_ReadingChanged);
            accelerometer.Start();
        }

        /* Supposing that a shaking indicates the 
         * phone is no longer secure,
         * the decrypted message will be erased.*/
        void Accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
             //if the movement takes less than 500 milliseconds
            if (e.Timestamp.Subtract(movementMoment).Duration().TotalMilliseconds <= 500)
            {
                if ((e.X <= -1 || e.X >= 1) && (firstShakeStep <= Math.Abs(e.X)))
                {
                    firstShakeStep = e.X;
                }

                if (firstShakeStep != 0)
                {
                    firstShakeStep = 0;
                    Deployment.Current.Dispatcher.BeginInvoke(() => ResetTextBox());
                }
            }
            movementMoment = e.Timestamp;
        }

        private void ResetTextBox()
        {
            this.decryptBox.Text = "";
        }
    }
}