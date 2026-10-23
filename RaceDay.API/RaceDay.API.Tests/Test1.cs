
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.API.Models;

namespace RaceDay.API.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Result_CanStoreResultDetails()
        {
            // Arrange
            var result = new Result
            {
                EnrolmentId = 3,
                FinishTime = new TimeOnly(1, 45, 30),
                FinishingPosition = 3
            };

            // Assert
            Assert.AreEqual(3, result.EnrolmentId);
            Assert.AreEqual(new TimeOnly(1, 45, 30), result.FinishTime);
            Assert.AreEqual(3, result.FinishingPosition);
        }

        [TestMethod]
        public void Result_InvalidFinishingPosition_IsLessThanOne()
        {
            // Arrange
            var result = new Result
            {
                EnrolmentId = 3,
                FinishTime = new TimeOnly(1, 45, 30),
                FinishingPosition = 0
            };

            // Assert
            Assert.IsTrue(result.FinishingPosition < 1);
        }

    }
}
