using System;
using System.Linq;
using Covid19Analysis.Model;
using Covid19Analysis.Resources;

namespace Covid19Analysis.OutputFormatter
{
    /// <summary>This class provides functions for all Covid data Summaries</summary>
    public class CovidDataSummary
    {

        #region Properties
        /// <Summary>Gets the covid records.</Summary>
        /// <value>The covid records.</value>
        public CovidDataCollection CovidRecords { get; private set; }

        /// <summary>Gets the state filter of the covid data collection.</summary>
        /// <value>The state filter.</value>
        public string StateFilter { get; }
        #endregion

        #region Private Members
        private CovidDataStatistics covidStatistics;
        #endregion
        #region Constructors

        /// <Summary>
        /// Initializes a new instance of the <a onclick="return false;" href="CovidDataStateSummary" originaltag="see">CovidDataStateSummary</a> class.
        /// <para>If the stateFilter is null, then the collection will not be filtered</para>
        /// <code>Precondition: collection != null</code>
        /// <code>Postcondition: CovidRecords == collection AND StateFilter == stateFilter </code>
        /// </Summary>
        /// <param name="collection">The collection.</param>
        /// <param name="stateFilter">The state filter for the collection</param>
        /// <exception cref="ArgumentNullException">collection</exception>
        public CovidDataSummary(CovidDataCollection collection, string stateFilter)
        {
            
            this.CovidRecords = collection ?? throw new ArgumentNullException(nameof(collection));
            this.StateFilter = stateFilter;
            this.applyFilterToCovidCollection();
            this.covidStatistics = new CovidDataStatistics(this.CovidRecords);
        }

        #endregion

        #region Public Methods

        /// <Summary>Gets the first day of positive test.</Summary>
        /// <returns>The formatted string of the first day of the positive test</returns>
        public string GetFirstDayOfPositiveTest()
        {
            var firstDayOfPositiveTest = this.covidStatistics.FindDayOfFirstPositiveTest();

            var dayOfFirstPositiveTest = $"{Assets.FirstDayOfPositiveTestLabel} " +
                                         $"{firstDayOfPositiveTest.ToString(Assets.DateStringFormatted)}" +
                                         $"{Environment.NewLine}";
            return dayOfFirstPositiveTest;
        }

        #region Highest Metrics
        /// <Summary>Gets the highest positive with date.</Summary>
        /// <returns>A formatted string with the highest positive test and date</returns>
        public string GetHighestPositiveWithDate()
        {
            var highestPositiveRecord = this.covidStatistics.FindRecordWithHighestPositiveCases();
            var highestPositive = Format.FormatIntegerAsFormattedString(highestPositiveRecord.PositiveTests);
            var date = highestPositiveRecord.Date.ToString(Assets.DateStringFormatted);

            return CovidDataLines.GetCovidLineForValueAndDate(Assets.HighestPositiveTestsLabel, highestPositive, date);
        }

        /// <Summary>Gets the highest negative with date.</Summary>
        /// <returns>A formatted string with the highest negative test and date</returns>
        public string GetHighestNegativeWithDate()
        {
            var highestNegativeRecord = this.covidStatistics.FindRecordWithHighestNegativeTests();
            var highestNegative = Format.FormatIntegerAsFormattedString(highestNegativeRecord.NegativeTests);
            var date = highestNegativeRecord.Date.ToString(Assets.DateStringFormatted);

            return CovidDataLines.GetCovidLineForValueAndDate(Assets.HighestNegativeTestsLabel, highestNegative, date);
        }

        /// <Summary>Gets the highest total tests with date.</Summary>
        /// <returns>A formatted string with the highest total tests and date</returns>
        public string GetHighestTotalTestsWithDate()
        {
            var highestTotalTests = this.covidStatistics.FindRecordWithHighestTotalTests();
            var highestTotalTestsFormatted = Format.FormatIntegerAsFormattedString(highestTotalTests.TotalTests);
            var date = highestTotalTests.Date.ToString(Assets.DateStringFormatted);

            return CovidDataLines.GetCovidLineForValueAndDate(Assets.HighestTotalTestsLabel, highestTotalTestsFormatted, date);
        }

        /// <Summary>Gets the highest deaths with date.</Summary>
        /// <returns>A formatted string with the highest deaths test and date</returns>
        public string GetHighestDeathsWithDate()
        {
            var highestDeathsRecord = this.covidStatistics.FindRecordWithHighestDeaths();
            var highestDeaths = Format.FormatIntegerAsFormattedString(highestDeathsRecord.Deaths);
            var date = highestDeathsRecord.Date.ToString(Assets.DateStringFormatted);
            return CovidDataLines.GetCovidLineForValueAndDate(Assets.HighestDeathsLabel, highestDeaths, date);
        }

        /// <Summary>Gets the highest hospitalizations with date.</Summary>
        /// <returns>A formatted string with the highest hospitalizations test and date</returns>
        public string GetHighestHospitalizationsWithDate()
        {
            var highestHospitalizationsRecord = this.covidStatistics.FindRecordWithHighestHospitalizations();
            var highestHospitalizations = Format.FormatIntegerAsFormattedString(highestHospitalizationsRecord.Hospitalizations);
            var date = highestHospitalizationsRecord.Date.ToString(Assets.DateStringFormatted);

            return CovidDataLines.GetCovidLineForValueAndDate(Assets.HighestHospitalizationsLabel, highestHospitalizations, date);
        }

        /// <Summary>Gets the highest percentage of positive tests with date.</Summary>
        /// <returns>A formatted string with the highest percentage of positive  test and date</returns>
        public string GetHighestPercentageOfTestsPerDayWithDate()
        {
            var highestPercentageRecord = this.covidStatistics.FindRecordWithHighestPercentageOfPositiveTests();

            var highestPercentage = Format.FormatNumericValueAsPercentage(CovidDataStatistics.FindPositivePercentageForRecord(highestPercentageRecord));
            var date = highestPercentageRecord.Date.ToString(Assets.DateStringFormatted);
            return CovidDataLines.GetCovidLineForValueAndDate(Assets.HighestPercentageOfPositiveCasesLabel, highestPercentage, date);
        }


        #endregion

        #region Average Metrics

        /// <Summary>Gets the average positive tests.</Summary>
        /// <returns>A formatted string with the average positive  test.</returns>
        public string GetAveragePositiveTestsSinceFirstPositiveTest()
        {
            var averagePositiveTest = this.covidStatistics.FindAveragePositiveTestsSinceFirstPositiveTest();

            var average = Format.FormatAveragesWithTwoDecimalPlaces(averagePositiveTest);

            return CovidDataLines.GetCovidLineForValue(Assets.AveragePositiveTestsLabel, average);
        }

        /// <Summary>Gets the Overall Positivity Rate.</Summary>
        /// <returns>A formatted string with the Overall Positivity Rate.</returns>
        public string GetOverallPositivityRateSinceFirstPositiveTest()
        {
            var overallPositivityRate = this.covidStatistics.FindOverallPositivityRateSinceFirstPositiveTest();
            var positivityRate = Format.FormatNumericValueAsPercentage(overallPositivityRate);
            return CovidDataLines.GetCovidLineForValue(Assets.OverallPositivityRateLabel, positivityRate);
        }

        #endregion

        #region Threshold Metrics
        /// <Summary>Gets the Days greater than a threshold.</Summary>
        /// <returns>A formatted string with the days greater than a threshold.</returns>
        public string GetTheDaysFromTheFirstPositiveTestGreaterThanThreshold(int threshold)
        {
            var daysGreaterThanThreshold =
                this.covidStatistics.FindNumberOfDaysForPositiveTestsGreaterThanThreshold(threshold);

            var days = Format.FormatIntegerAsFormattedString(daysGreaterThanThreshold);
            var thresholdFormatted = Format.FormatIntegerAsFormattedString(threshold);
            return CovidDataLines.GetCovidLineForValueWithThreshold(Assets.DaysGreaterThanValueLabel, days, thresholdFormatted);
        }

        /// <Summary>Gets the Days less than a threshold.</Summary>
        /// <returns>A formatted string with the days less than a threshold.</returns>
        public string GetTheDaysFromTheFirstPositiveTestLessThanThreshold(int threshold)
        {
            var daysLessThanThreshold =
                this.covidStatistics.FindNumberOfDaysForPositiveTestsLessThanThreshold(threshold);

            var days = Format.FormatIntegerAsFormattedString(daysLessThanThreshold);
            var thresholdFormatted = Format.FormatIntegerAsFormattedString(threshold);
            return CovidDataLines.GetCovidLineForValueWithThreshold(Assets.DaysLessThanValueLabel, days, thresholdFormatted);
        }


        #endregion

        #region Complex Metrics

        /// <summary>Gets the frequency table histogram of positive tests.</summary>
        /// <returns>The histogram of the positive tests</returns>
        public string GetTheFrequencyTableHistogramOfPositiveTests()
        {
            var positiveTests = (from record in this.CovidRecords
                                 where record.Date.Date >= this.getDateOfFirstPositiveTest()
                                 orderby record.PositiveTests descending
                                 select record.PositiveTests).ToList();
            var histogramGenerator = new CovidDataHistogramGenerator(positiveTests);
            var histogram = $"{Environment.NewLine}{Assets.HistogramLabel}{Environment.NewLine}";
            histogram += histogramGenerator.GenerateHistogram();
            return histogram;

        }

        /// <summary>Gets the monthly summary.</summary>
        /// <returns>Returns the string of the monthly summary</returns>
        public string GetMonthlySummary()
        {
            var monthlySummaryBuilder = new CovidDataMonthlySummary(this.CovidRecords);
            return monthlySummaryBuilder.GenerateMonthlySummary();
        }
        #endregion

        #endregion

        #region Private Helpers
        private static double findPercentageForRecord(CovidRecord record)
        {
            var totalTests = record.TotalTests;
            if (totalTests != 0)
            {
                return record.PositiveTests / Format.FormatIntegerToDouble(totalTests);
            }

            return 0;
        }

        private DateTime getDateOfFirstPositiveTest()
        {
            var firstDateOfPositiveTestRecord = (from record in this.CovidRecords
                                                 orderby record.Date
                                                 where record.PositiveTests > 0
                                                 select record).First();
            return firstDateOfPositiveTestRecord.Date;
        }

        private void applyFilterToCovidCollection()
        {
            if (!FormatValidator.IsStateLabelValid(this.StateFilter))
            {
                return;
            }

            var filteredCovidCollection = this.CovidRecords.Where(record => record.State.Equals(this.StateFilter, StringComparison.InvariantCultureIgnoreCase)).ToList();
            this.CovidRecords.Clear();
            this.CovidRecords.ReplaceAllWithNewCovidCollection(filteredCovidCollection);
        }

        #endregion

    }
}
