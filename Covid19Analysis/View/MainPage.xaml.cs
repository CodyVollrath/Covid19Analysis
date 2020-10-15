﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Covid19Analysis.Model;
using Covid19Analysis.OutputFormatter;
using Covid19Analysis.Resources;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Covid19Analysis.View
{
    /// <Summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </Summary>
    public sealed partial class MainPage
    {
        #region Properties

        /// <Summary>
        ///     The application height
        /// </Summary>
        public const int ApplicationHeight = 500;

        /// <Summary>
        ///     The application width
        /// </Summary>
        public const int ApplicationWidth = 620;

        #endregion

        #region Private Members

        private readonly CovidDataAssembler covidDataAssembler;

        private readonly ContentDialog mergeOrReplaceDialog;

        private string currentTextContent;

        #endregion

        #region Constructors

        /// <Summary>
        ///     Initializes a new instance of the <see cref="MainPage" /> class.
        /// </Summary>
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size { Width = ApplicationWidth, Height = ApplicationHeight };
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(ApplicationWidth, ApplicationHeight));
            this.covidDataAssembler = new CovidDataAssembler();
            this.mergeOrReplaceDialog = new ContentDialog
            {
                Title = Assets.MergeFilesTitle,
                Content = Assets.MergeFilesContent,
                PrimaryButtonText = Assets.MergeFilesPrimaryButtonText,
                SecondaryButtonText = Assets.MergeFilesSecondaryButtonText
            };

        }

        #endregion

        #region Action Events
        private async void loadFile_ClickAsync(object sender, RoutedEventArgs e)
        {
            string fileContent;

            var openPicker = new FileOpenPicker { ViewMode = PickerViewMode.Thumbnail, SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            openPicker.FileTypeFilter.Add(".csv");
            openPicker.FileTypeFilter.Add(".txt");
            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            using (var fileReader = new StreamReader(stream.AsStream()))
            {
                fileContent = await fileReader.ReadToEndAsync();
            }

            this.currentTextContent = fileContent;
            this.displayCovidData(fileContent);
        }

        private void errorLog_Click(object sender, RoutedEventArgs e)
        {
            this.summaryTextBox.Text = this.covidDataAssembler.GetCovidDataErrors();
        }

        private void clearData_Click(object sender, RoutedEventArgs e)
        {
            this.covidDataAssembler.Reset();
            this.summaryTextBox.Text = string.Empty;
        }

        private void upperPositiveCaseTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void lowerPositiveCaseTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void binSizeTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void upperPositiveCaseTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var isUpperPositiveCaseTextBoxEmpty = this.upperPositiveCaseTextBox.Text.Equals(string.Empty);
            if (isUpperPositiveCaseTextBoxEmpty)
            {
                this.upperPositiveCaseTextBox.Text = Assets.DefaultGreaterThanThreshHold.ToString();
            }
            this.updateCovidData();
        }

        private void lowerPositiveCaseTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var isLowerPositiveCaseTextBoxEmpty = this.lowerPositiveCaseTextBox.Text.Equals(string.Empty);
            if (isLowerPositiveCaseTextBoxEmpty)
            {
                this.lowerPositiveCaseTextBox.Text = Assets.DefaultLessThanThreshold.ToString();
            }
            this.updateCovidData();
        }

        private void binSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var isBinSizeEmpty = this.binSizeTextBox.Text.Equals(string.Empty);
            var binSize = Format.FormatStringToInteger(this.binSizeTextBox.Text);
            var isBinLessThanOrEqualZero = binSize <= 0;
            if (isBinSizeEmpty || isBinLessThanOrEqualZero)
            {
                this.binSizeTextBox.Text = Assets.RangeWidth.ToString();
            }

            this.updateCovidData();
        }
        #endregion

        #region Private Helpers

        private void displayCovidData(string textContent)
        {
            try
            {
                this.applyThresholds();
                this.applyBinSize();
                if (this.covidDataAssembler.IsCovidDataLoaded)
                {
                    this.promptMergeOrReplaceDialog(textContent);
                }
                else
                {
                    this.loadCovidData(textContent);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                this.summaryTextBox.Text = Assets.NoCovidDataText;
            }
        }

        private async void promptMergeOrReplaceDialog(string textContent)
        {
            var answer = await this.mergeOrReplaceDialog.ShowAsync();
            if (answer == ContentDialogResult.Primary)
            {
                this.mergeAndLoadCovidData(textContent);
            }
            else
            {
                this.loadCovidData(textContent);
            }
        }

        private void mergeAndLoadCovidData(string textContent)
        {
            this.covidDataAssembler.MergeAndLoadCovidData(textContent);
            var duplicates = this.covidDataAssembler.GetDuplicatesFromMergedData();
            this.keepOrReplaceDialog(duplicates);
        }

        private async void keepOrReplaceDialog(IEnumerable<CovidRecord> duplicates)
        {
            var duplicateDialogBox = new DuplicateDialogBox();
            foreach (var record in duplicates)
            {
                duplicateDialogBox.SetDuplicateRecord(record);
                if (duplicateDialogBox.DoAll)
                {
                    if (duplicateDialogBox.Replace)
                    {
                        this.covidDataAssembler.ReplaceRecord(record);
                    }
                }
                else
                {
                    var results = await duplicateDialogBox.ShowAsync();
                    if (results == ContentDialogResult.Primary)
                    {
                        this.covidDataAssembler.ReplaceRecord(record);
                    }
                }
            }
            this.summaryTextBox.Text = this.covidDataAssembler.Summary;

        }

        private void loadCovidData(string textContent)
        {
            this.covidDataAssembler.LoadCovidData(textContent);
            this.summaryTextBox.Text = this.covidDataAssembler.Summary;
        }

        private void updateCovidData()
        {
            if (!this.covidDataAssembler.IsCovidDataLoaded)
            {
                return;
            }
            this.applyThresholds();
            this.applyBinSize();
            this.loadCovidData(this.currentTextContent);
        }

        private void applyThresholds()
        {
            this.covidDataAssembler.UpperPositiveThreshold = this.upperPositiveCaseTextBox.Text;
            this.covidDataAssembler.LowerPositiveThreshold = this.lowerPositiveCaseTextBox.Text;
        }

        private void applyBinSize()
        {
            this.covidDataAssembler.BinSize = this.binSizeTextBox.Text;
        }

        #endregion

    }
}
