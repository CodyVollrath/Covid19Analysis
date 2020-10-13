using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Covid19Analysis.Resources;

namespace Covid19Analysis.Model
{
    public class CovidDataStatistics
    {
        #region Properties

        public CovidDataCollection CovidRecords { get; }

        #endregion

        #region Constructors
        public CovidDataStatistics(CovidDataCollection covidRecords)
        {
            this.CovidRecords = covidRecords ?? throw new ArgumentNullException(nameof(covidRecords));
        }
        #endregion

        #region Public Methods

        public DateTime FindDayOfFirstPositiveTest()
        {
            var dayOfFirstPositive = this.CovidRecords.OrderBy(record => record.Date).First(record => record.PositiveTests > 0).Date;
            return dayOfFirstPositive;
        }

        public int FindHighestPositiveCases()
        {
            var highestPositiveTestCase = this.CovidRecords.OrderByDescending(record => record.PositiveTests).First().PositiveTests;
            return highestPositiveTestCase;
        }

        public int FindLowestPositiveTests()
        {
            var lowestPositiveTestCase = this.CovidRecords.OrderBy(record => record.PositiveTests).First().PositiveTests;
            return lowestPositiveTestCase;
        }

        public int FindHighestNegativeTests()
        {
            var highestNegativeTests = this.CovidRecords.OrderByDescending(record => record.NegativeTests).First().NegativeTests;
            return highestNegativeTests;
        }

        public int FindLowestNegativeTests()
        {
            var lowestNegativeTests = this.CovidRecords.OrderBy(record => record.NegativeTests).First().NegativeTests;
            return lowestNegativeTests;
        }

        public int FindHighestTotalTests()
        {
            var highestTotalTests = this.CovidRecords.OrderByDescending(record => record.TotalTests).First().TotalTests;
            return highestTotalTests;
        }

        public int FindLowestTotalTests()
        {
            var lowestTotalTests = this.CovidRecords.OrderBy(record => record.TotalTests).First().TotalTests;
            return lowestTotalTests;
        }

        public int FindHighestDeaths()
        {
            var highestDeaths = this.CovidRecords.OrderByDescending(record => record.Deaths).First().Deaths;
            return highestDeaths;
        }

        public int FindLowestDeaths()
        {
            var lowestDeaths = this.CovidRecords.OrderBy(record => record.Deaths).First().Deaths;
            return lowestDeaths;
        }

        public int FindHighestHospitalizations()
        {
            var highestHospitalizations =
                this.CovidRecords.OrderByDescending(record => record.Hospitalizations).First().Deaths;
            return highestHospitalizations;
        }

        public int FindLowestHospitalizations()
        {
            var lowestHospitalizations =
                this.CovidRecords.OrderBy(record => record.Hospitalizations).First().Deaths;
            return lowestHospitalizations;
        }

        public double FindHighestPercentage()
        {
            var recordWithHighestPercentage = this.CovidRecords.OrderByDescending(findPercentageForRecord).First(record => record.TotalTests != 0);
            var highestPercentageValue = findPercentageForRecord(recordWithHighestPercentage);
            return highestPercentageValue;
        }

        public double FindAveragePositiveTestsSinceFirstPositiveTest()
        {
            var firstDateWithPositive = this.FindDayOfFirstPositiveTest();
            var averagePositiveTests = this.CovidRecords
                              .Where(record => record.Date.Date >= firstDateWithPositive)
                              .Select(record => record.PositiveTests).Average();
            return averagePositiveTests;
        }

        public double FindAverageTotalTestsSinceFirstPositiveTest()
        {
            var firstDateWithPositive = this.FindDayOfFirstPositiveTest();
            var averageTotalTests = this.CovidRecords
                              .Where(record => record.Date.Date >= firstDateWithPositive)
                              .Select(record => record.TotalTests).Average();
            return averageTotalTests;
        }

        public double FindOverallPositivityRateSinceFirstPositiveTest()
        {
            var firstDateWithPositive = this.FindDayOfFirstPositiveTest();
            var averagePositiveTests = this.FindAveragePositiveTestsSinceFirstPositiveTest();

            var averageOfTotalTests = this.CovidRecords
                                      .Where(record => record.Date.Date >= firstDateWithPositive)
                                      .Select(record => record.TotalTests).Average();

            var positivityRate = averagePositiveTests / averageOfTotalTests;
            return positivityRate;
        }

        public double FindNumberOfDaysSinceFirstPositiveTestGreaterThanThreshold(int threshold)
        {
            var firstDateWithPositive = this.FindDayOfFirstPositiveTest();
            var daysGreaterThanThreshold = this.CovidRecords.Count(record => record.Date.Date >= firstDateWithPositive && record.PositiveTests > threshold);
            return daysGreaterThanThreshold;
        }

        public double FindNumberOfDaysSinceFirstPositiveTestLessThanThreshold(int threshold)
        {
            var firstDateWithPositive = this.FindDayOfFirstPositiveTest();
            var daysGreaterThanThreshold = this.CovidRecords.Count(record => record.Date.Date >= firstDateWithPositive && record.PositiveTests < threshold);
            return daysGreaterThanThreshold;
        }

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
        #endregion
    }
}
