using System;
using System.Collections.Generic;
using Covid19Analysis.DataTier;
using Covid19Analysis.Model;
using Covid19Analysis.Resources;

namespace Covid19Analysis.OutputFormatter
{

    /// <summary>This class assembles COVID data output from all summaries and accumulates them together.</summary>
    public class CovidDataAssembler
    {
        #region Properties

        /// <summary>Gets a value indicating whether this instance is covid data loaded.</summary>
        /// <value>
        ///   <c>true</c> if this instance is covid data loaded; otherwise, <c>false</c>.</value>
        public bool IsCovidDataLoaded { get; private set; }


        /// <summary>Gets the state filter.</summary>
        /// <value>The state filter.</value>
        public string StateFilter { get; }


        /// <summary>Gets or sets the upper positive threshold.</summary>
        /// <value>The upper positive threshold.</value>
        public string UpperPositiveThreshold { get; set; }


        /// <summary>Gets or sets the lower positive threshold.</summary>
        /// <value>The lower positive threshold.</value>
        public string LowerPositiveThreshold { get; set; }


        /// <summary>Gets the covid data summary.</summary>
        /// <value>The summary.</value>
        public string Summary { get; private set; }

        #endregion

        #region Private Members
        private CovidDataErrorLogger covidErrorLogger;

        private CovidDataCollection loadedCovidDataCollection;

        private CovidDataMergeController mergeController;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <a onclick="return false;" href="CovidDataAssembler" originaltag="see">CovidDataAssembler</a> class.
        /// <para>By default the filter is set to GA</para>
        /// <para>If set to null all states represented in the table will be represented</para>
        /// <code>Postcondition: StateFilter == stateFilter AND Summary == null AND IsCovidDataLoaded == false</code>
        /// </summary>
        /// <param name="stateFilter">The state filter.</param>
        public CovidDataAssembler(string stateFilter = Assets.GeorgiaFilterValue)
        {
            this.StateFilter = stateFilter;
            this.Summary = string.Empty;
            this.IsCovidDataLoaded = false;
            this.covidErrorLogger = null;
            this.loadedCovidDataCollection = null;
            this.mergeController = null;
        }

        #endregion

        #region Public Methods

        /// <Summary>Gets the covid data errors.</Summary>
        /// <returns>
        ///   <para>the string that represents the covid data errors</para>
        /// </returns>
        public string GetCovidDataErrors()
        {
            return this.covidErrorLogger == null ? string.Empty : this.covidErrorLogger.ErrorString;
        }

        /// <summary>Loads the covid data.</summary>
        /// <param name="textContent">Content of the text.</param>
        /// <exception cref="ArgumentNullException">textContent</exception>
        public void LoadCovidData(string textContent)
        {
            textContent = textContent ?? throw new ArgumentNullException(nameof(textContent));
            var parser = new CovidCsvParser(textContent);
            this.covidErrorLogger = parser.CovidErrorLogger;
            this.loadedCovidDataCollection = parser.GenerateCovidDataCollection();
            this.IsCovidDataLoaded = true;
            this.buildCovidSummary();
        }


        /// <summary>Merges the and loads the covid data.</summary>
        /// <param name="textContent">Content of the text.</param>
        /// <exception cref="ArgumentNullException">textContent</exception>
        public void MergeAndLoadCovidData(string textContent)
        {
            textContent = textContent ?? throw new ArgumentNullException(nameof(textContent));

            var parser = new CovidCsvParser(textContent);
            var newCovidDataCollection = parser.GenerateCovidDataCollection();

            this.covidErrorLogger = parser.CovidErrorLogger;
            this.mergeController = new CovidDataMergeController(this.loadedCovidDataCollection, newCovidDataCollection);
            this.mergeController.AddAllNonDuplicates();

        }

        /// <summary>Replaces the Covid covidRecord with the covidRecord passed in to it.</summary>
        /// <param name="covidRecord">The covidRecord.</param>
        public void ReplaceRecord(CovidRecord covidRecord)
        {
            this.mergeController.ReplaceDuplicate(covidRecord);
            this.loadedCovidDataCollection = this.mergeController.MergedCovidDataCollection;
            this.IsCovidDataLoaded = true;
            this.buildCovidSummary();
        }


        /// <summary>Gets the duplicates from merged data.</summary>
        /// <returns>The duplicates from the merged data</returns>
        public IEnumerable<CovidRecord> GetDuplicatesFromMergedData()
        {
            var duplicates = this.mergeController.GetDuplicates();
            var isDuplicatesNotEmpty = this.mergeController.GetDuplicates().Count > 0;
            return isDuplicatesNotEmpty ? duplicates : null;
        }

        #endregion

        #region Private Methods

        private void buildCovidSummary()
        {
            const string genericHeader = Assets.StateCovidDataHeadingLabel;
            var stateSummary = new CovidDataSummary(this.loadedCovidDataCollection, this.StateFilter);
            var isStateNotNull = this.StateFilter != null;
            var stateSpecificHeader = $"{this.StateFilter} {Assets.StateCovidDataHeadingLabel}";
            
            this.Summary = string.Empty;
            this.Summary += isStateNotNull ? stateSpecificHeader : genericHeader;

            this.Summary += stateSummary.GetFirstDayOfPositiveTest();
            this.Summary += stateSummary.GetHighestPositiveWithDate();
            this.Summary += stateSummary.GetHighestNegativeWithDate();

            this.Summary += stateSummary.GetHighestTotalTestsWithDate();
            this.Summary += stateSummary.GetHighestDeathsWithDate();
            this.Summary += stateSummary.GetHighestHospitalizationsWithDate();

            this.Summary += stateSummary.GetHighestPercentageOfTestsPerDayWithDate();
            this.Summary += stateSummary.GetAveragePositiveTestsSinceFirstPositiveTest();
            this.Summary += stateSummary.GetOverallPositivityRateSinceFirstPositiveTest();

            this.Summary += this.getPositiveThresholds(stateSummary);
            this.Summary += stateSummary.GetTheFrequencyTableHistogramOfPositiveTests();
            this.Summary += stateSummary.GetMonthlySummary();
        }

        private string getPositiveThresholds(CovidDataSummary stateSummary)
        {
            var upperPositiveCaseThreshold = Assets.DefaultGreaterThanThreshHold;
            var lowerPositiveCaseThreshold = Assets.DefaultLessThanThreshold;
            if (this.UpperPositiveThreshold != null && !this.UpperPositiveThreshold.Equals(string.Empty))
            {
                upperPositiveCaseThreshold = Format.FormatStringToInteger(this.UpperPositiveThreshold);
            }

            if (this.LowerPositiveThreshold != null && !this.LowerPositiveThreshold.Equals(string.Empty))
            {
                lowerPositiveCaseThreshold = Format.FormatStringToInteger(this.LowerPositiveThreshold);
            }

            var summary = string.Empty;
            summary += stateSummary.GetTheDaysFromTheFirstPositiveTestGreaterThanThreshold(upperPositiveCaseThreshold);
            summary += stateSummary.GetTheDaysFromTheFirstPositiveTestLessThanThreshold(lowerPositiveCaseThreshold);
            return summary;
        }

        #endregion

    }
}
