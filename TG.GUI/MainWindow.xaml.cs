using LinearModelControlLib;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
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

namespace ThinGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            myLinearModelControl.Logger = LogLine;
        }

        private void AddConstraint(object sender, RoutedEventArgs e) { myLinearModelControl.AddConstraint(); }
        private void AddVariable(object sender, RoutedEventArgs e) { myLinearModelControl.AddVariable(); }
        private void RemConstraint(object sender, RoutedEventArgs e) { myLinearModelControl.RemConstraint(); }
        private void RemVariable(object sender, RoutedEventArgs e) { myLinearModelControl.RemVariable(); }

        /// <summary>
        /// Starts the solve process.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event args.</param>
        private void ButtonSolve_Click(object sender, RoutedEventArgs e)
        {
            LogLine(">>> Solving ...");
            DisableButtonsForSolve();
            Action solveAction = () =>
            {
                try
                {
                    // Solve it
                    myLinearModelControl.Solve();
                    // Enable buttons again
                    Dispatcher.Invoke(() => { EnableButtonsAfterSolve(); });
                }
                catch (LinModelException ex)
                {
                    MessageBox.Show("Error while solving:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogLine("Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected Error while solving:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogLine("Error: " + ex.Message);
                }
                finally { if (_buttonsBlockedForSolve) EnableButtonsAfterSolve(); }
            };
            Thread solveThread = new Thread(new ThreadStart(solveAction));
            solveThread.Start();
        }
        /// <summary>
        /// Attempts to stop an ongoing solve process.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event args.</param>
        private void ButtonStopSolve_Click(object sender, RoutedEventArgs e) { LogLine("Attempting to stop solver ..."); myLinearModelControl.StopSolve(); }

        #region About box

        /// <summary>
        /// Shows a small about box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event.</param>
        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            if (Resources.Contains("WindowAbout"))
            {
                Window about = Resources["WindowAbout"] as Window;
                about.Owner = this;
                about.ShowDialog();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Logs a line to the GUI.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        private void LogLine(string msg) { OutputTextBox.Dispatcher.Invoke(() => { OutputTextBox.AppendText(msg + "\n"); OutputTextBox.ScrollToEnd(); }); }

        /// <summary>
        /// Indicates whether the buttons are currently blocked for the solve process to execute.
        /// </summary>
        private bool _buttonsBlockedForSolve = false;
        /// <summary>
        /// Disables the buttons for the solve procedure to operate without any interruptions.
        /// </summary>
        private void DisableButtonsForSolve()
        {
            Dispatcher.Invoke(() =>
            {
                _buttonsBlockedForSolve = true;
                ButtonOpenFile.IsEnabled = false; ImageOpenFile.Visibility = Visibility.Hidden;
                ButtonSaveFile.IsEnabled = false; ImageSaveFile.Visibility = Visibility.Hidden;
                ButtonAddVariable.IsEnabled = false; ImageAddVariable.Visibility = Visibility.Hidden;
                ButtonRemVariable.IsEnabled = false; ImageRemVariable.Visibility = Visibility.Hidden;
                ButtonAddConstraint.IsEnabled = false; ImageAddConstraint.Visibility = Visibility.Hidden;
                ButtonRemConstraint.IsEnabled = false; ImageRemConstraint.Visibility = Visibility.Hidden;
                ButtonSolve.IsEnabled = false; ButtonSolve.Visibility = Visibility.Collapsed; ImageSolve.Visibility = Visibility.Collapsed;
                ButtonStopSolve.IsEnabled = true; ButtonStopSolve.Visibility = Visibility.Visible; ImageStopSolve.Visibility = Visibility.Visible;
            });
        }
        /// <summary>
        /// Enables the buttons again after the solve procedure is done.
        /// </summary>
        private void EnableButtonsAfterSolve()
        {
            Dispatcher.Invoke(() =>
            {
                _buttonsBlockedForSolve = false;
                ButtonOpenFile.IsEnabled = true; ImageOpenFile.Visibility = Visibility.Visible;
                ButtonSaveFile.IsEnabled = true; ImageSaveFile.Visibility = Visibility.Visible;
                ButtonAddVariable.IsEnabled = true; ImageAddVariable.Visibility = Visibility.Visible;
                ButtonRemVariable.IsEnabled = true; ImageRemVariable.Visibility = Visibility.Visible;
                ButtonAddConstraint.IsEnabled = true; ImageAddConstraint.Visibility = Visibility.Visible;
                ButtonRemConstraint.IsEnabled = true; ImageRemConstraint.Visibility = Visibility.Visible;
                ButtonSolve.IsEnabled = true; ButtonSolve.Visibility = Visibility.Visible; ImageSolve.Visibility = Visibility.Visible;
                ButtonStopSolve.IsEnabled = false; ButtonStopSolve.Visibility = Visibility.Collapsed; ImageStopSolve.Visibility = Visibility.Collapsed;
            });
        }
        /// <summary>
        /// Handles a clicked link.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) { System.Diagnostics.Process.Start(e.Uri.ToString()); }

        #endregion

        #region I/O

        /// <summary>
        /// The filter used to look for instances.
        /// </summary>
        private readonly string IO_FILTER = Constants.IO_FILE_ENDING.ToUpper() + " Files (." + Constants.IO_FILE_ENDING + ")|*." + Constants.IO_FILE_ENDING;
        /// <summary>
        /// Handler for opening files.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event.</param>
        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog instanceDialog = new OpenFileDialog();

            // Set filter options and filter index.
            instanceDialog.Filter = IO_FILTER;
            instanceDialog.FilterIndex = 1;
            instanceDialog.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? instClick = instanceDialog.ShowDialog();

            // Process input if the user clicked OK.
            if (instClick == true)
            {
                // Parse the instance
                try
                {
                    myLinearModelControl.Read(instanceDialog.FileName);
                }
                catch (LinModelException ex)
                {
                    MessageBox.Show("Error while reading the file:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogLine("Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error while reading the file:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogLine("Error: " + ex.Message);
                }
            }
        }
        /// <summary>
        /// Handler for saving files.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event.</param>
        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            // Init save dialog
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "myInstance"; // Default file name
            dialog.DefaultExt = "." + Constants.IO_FILE_ENDING; // Default file extension
            dialog.Filter = IO_FILTER; // Filter files by extension

            // Show save file dialog box
            bool? userClickedOK = dialog.ShowDialog();

            // Process save file dialog box results
            if (userClickedOK == true)
            {
                // Save the instance
                string filename = dialog.FileName;
                try
                {
                    myLinearModelControl.Write(filename);
                }
                catch (LinModelException ex)
                {
                    MessageBox.Show("Error while writing the file:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogLine("Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error while writing the file:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LogLine("Error: " + ex.Message);
                }
            }
        }

        #endregion
    }
}
