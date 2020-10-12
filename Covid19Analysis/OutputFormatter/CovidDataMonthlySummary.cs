using System;
using System.Collections.Generic;
using System.Linq;
using Covid19Analysis.Model;
using Covid19Analysis.Resources;

namespace Covid19Analysis.OutputFormatter
{

    /// <Summary>This class generates the Summary for the monthly values</Summary>
    public class CovidDataMonthlySummary
    {
        
        #region Properties

        /// <Summary>Gets the covid collection.</Summary>
        /// <value>The covid collection.</value>
        public CovidDataCollection CovidRecords { get; }
        #endregion

        #region Constructors
        /// <Summary>
        /// Initializes a new instance of the <a onclick="return false;" href="CovidDataStateSummary" originaltag="see">CovidDataMonthlySummary</a> class.
        /// <code>Precondition: collection != null</code>
        /// <code>Postcondition: CovidRecords1 == collection</code>
        /// </Summary>
        /// <param name="collection">The collection of CovidRecords1.</param>s
        /// <exception cref="ArgumentNullException">collection</exception>
        public CovidDataMonthlySummary(CovidDataCollection collection)
        {
            collection = collection ?? throw new ArgumentNullException(nameof(collection));
            this.CovidRecords = collection;
        }
        #endregion

        #region Public Methods
        /// <summary>Generates the monthly summary.</summary>
        /// <returns>The monthly summary of covid data</returns>
        public string GenerateMonthlySummary()
        {
            var covidRecords = this.getSortedListByMonth().ToList();
            var monthlySummary = string.Empty;

            monthlySummary += getTotalMonthData(covidRecords);

            return monthlySummary;
        }

        #endregion

        #region Private Helpers

        private static string getTotalMonthData(ICollection<CovidRecord> covidRecords)
        {
            var monthsInTheData = covidRecords
                                  .Select(record => new DateTime(year: record.Date.Year, month: record.Date.Month, day:1))
                                  .Distinct().ToList();

            var groupByMonths = covidRecords
                                .GroupBy(record => new DateTime(year: record.Date.Year, month:record.Date.Month, day:1))
                                .ToList();

            var monthYearsWithinCovidDataRange = getMonthYearsWithinCovidData(monthsInTheData);

            var summary = string.Empty;
            foreach (var monthYear in monthYearsWithinCovidDataRange)
            {
                if (!monthsInTheData.Contains(monthYear))
                {
                    var monthAndYear = Format.GetMonthAndYearFromDateTime(monthYear);
                    summary += $"{Environment.NewLine}{monthAndYear} (0 {Assets.DaysOfDataLabel}):{Environment.NewLine}";
                    continue;
                }
                var monthData = groupByMonths.FirstOrDefault(group => group.Key.Equals(monthYear));
                summary += getDataForTheMonthGroup(monthData);
            }

            return summary;
        }

        private static IEnumerable<DateTime> getMonthYearsWithinCovidData(ICollection<DateTime> monthYearsInTheCovidDataData)
        {
            var firstMonthAndYear = monthYearsInTheCovidDataData.First();
            var lastMonthAndYear = monthYearsInTheCovidDataData.Last();
            var monthYearsWithinRange = new List<DateTime>();
            var numberOfYearsInData = lastMonthAndYear.Year - firstMonthAndYear.Year;
            var currentYear = firstMonthAndYear.Year;
            var monthsToCover = lastMonthAndYear.Month + Assets.NumberOfMonthsInOneYear * numberOfYearsInData;

            for (var monthWithinRangeCount = firstMonthAndYear.Month; monthWithinRangeCount <= monthsToCover; monthWithinRangeCount++)
            {
                var monthCount = monthWithinRangeCount % Assets.NumberOfMonthsInOneYear;
                if (monthCount == 0)
                {
                    monthYearsWithinRange.Add(new DateTime(year:currentYear, month:Assets.NumberOfMonthsInOneYear, day:1));
                    currentYear++;
                }
                else
                {
                    monthYearsWithinRange.Add(new DateTime(currentYear, monthCount, day:1));
                }
            }

            monthYearsWithinRange.Sort();

            return monthYearsWithinRange;
        }

        private static string getDataForTheMonthGroup(IGrouping<DateTime, CovidRecord> monthGroup)
        {
            var reportOfTheMonth = string.Empty;
            var monthGroupKey = Format.GetMonthAndYearFromDateTime(monthGroup.Key);
            var monthHeading =
                $"{Environment.NewLine}{monthGroupKey} ({monthGroup.Count()} {Assets.DaysOfDataLabel}):{Environment.NewLine}";

            reportOfTheMonth += monthHeading;
            reportOfTheMonth += getHighestPositiveTestWithDays(monthGroup);
            reportOfTheMonth += getLowestPositiveTestWithDays(monthGroup);
            reportOfTheMonth += getHighestCombinedTestWithDays(monthGroup);
            reportOfTheMonth += getLowestTotalTestsWithDays(monthGroup);
            reportOfTheMonth += getAveragePositiveTests(monthGroup);
            reportOfTheMonth += getAverageTotalTests(monthGroup);
            return $"{reportOfTheMonth}";
        }

        private static string getHighestPositiveTestWithDays(IEnumerable<CovidRecord> collection)
        {
            var covidRecords = collection.ToArray();
            var highestPositiveTests = (from record in covidRecords orderby record.PositiveTests descending select record.PositiveTests).First();

            var daysOccurred =
                (from record in covidRecords where record.PositiveTests == highestPositiveTests select record.Date).ToList();

            var daysOutput = Format.GetListOfDaysWithOrdinals(daysOccurred);
            var highestPositivesFormatted = Format.FormatIntegerAsFormattedString(highestPositiveTests);

            return CovidDataLines.GetCovidLineForValueAndDaysOfMonth(Assets.HighestPositiveTestsLabel, highestPositivesFormatted, daysOutput);
        }

        private static string getLowestPositiveTestWithDays(IEnumerable<CovidRecord> collection)
        {
            var covidRecords = collection.ToArray();
            var lowestPositiveTests = (from record in covidRecords orderby record.PositiveTests ascending select record.PositiveTests).First();

            var daysOccurred =
                (from record in covidRecords where record.PositiveTests == lowestPositiveTests select record.Date).ToList();

            var daysOutput = Format.GetListOfDaysWithOrdinals(daysOccurred);
            var lowestPositivesFormatted = Format.FormatIntegerAsFormattedString(lowestPositiveTests);

            return CovidDataLines.GetCovidLineForValueAndDaysOfMonth(Assets.LowestPositiveTestsLabel,
                lowestPositivesFormatted, daysOutput);
        }

        private static string getHighestCombinedTestWithDays(IEnumerable<CovidRecord> collection)
        {
            var covidRecords = collection.ToArray();
            var highestTotalTests = (from record in covidRecords
                                        orderby record.TotalTests descending
                                        select record.TotalTests).First();

            var daysOccurred =
                (from record in covidRecords where record.TotalTests == highestTotalTests select record.Date).ToList();

            var daysOutput = Format.GetListOfDaysWithOrdinals(daysOccurred);
            var highestTotalTestsFormatted = Format.FormatIntegerAsFormattedString(highestTotalTests);

            return CovidDataLines.GetCovidLineForValueAndDaysOfMonth(Assets.HighestTotalTestsLabel,
                highestTotalTestsFormatted, daysOutput);
        }

        private static string getLowestTotalTestsWithDays(IEnumerable<CovidRecord> collection)
        {
            var covidRecords = collection.ToArray();
            var lowestTotalTests = (from record in covidRecords
                                       orderby record.TotalTests ascending
                                       select record.TotalTests)
                .First();

            var daysOccurred =
                (from record in covidRecords where record.TotalTests == lowestTotalTests select record.Date).ToList();

            var daysOutput = Format.GetListOfDaysWithOrdinals(daysOccurred);
            var lowestTotalTestsFormatted = Format.FormatIntegerAsFormattedString(lowestTotalTests);
            return CovidDataLines.GetCovidLineForValueAndDaysOfMonth(Assets.LowestTotalTestsLabel, lowestTotalTestsFormatted, daysOutput);
        }

        private static string getAveragePositiveTests(IEnumerable<CovidRecord> collection)
        {
            var positiveAverage = collection.Average(record => record.PositiveTests);
            var positiveAverageFormatted = Format.FormatAveragesWithTwoDecimalPlaces(positiveAverage);
            return CovidDataLines.GetCovidLineForValue(Assets.AveragePositiveTestsLabel, positiveAverageFormatted);
        }

        private static string getAverageTotalTests(IEnumerable<CovidRecord> collection)
        {
            var totalTestsAverage = collection.Average(record => record.TotalTests);
            var totalTestsAverageFormatted = Format.FormatAveragesWithTwoDecimalPlaces(totalTestsAverage);
            return CovidDataLines.GetCovidLineForValue(Assets.AverageTotalTestsLabel, totalTestsAverageFormatted);
        }

        private IEnumerable<CovidRecord> getSortedListByMonth()
        {
            var sortedListByMonth = (from record in this.CovidRecords where record.Date.Date >= this.getDateOfFirstPositiveTest() orderby record.Date select record).ToList();
            return sortedListByMonth;
        }

        private DateTime getDateOfFirstPositiveTest()
        {
            var firstDateOfPositiveTestRecord = (from record in this.CovidRecords
                                                 orderby record.Date
                                                 where record.PositiveTests > 0
                                                 select record).First();
            return firstDateOfPositiveTestRecord.Date;
        }

        #endregion


    }
}
