using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        // we can use this, So we don't have to write the same code in each method.
        //private readonly CreditCardApplicationEvaluator sut;
        //private readonly Mock<IFrequentFlyerNumberValidator> moq;
        //public CreditCardApplicationEvaluatorShould()
        //{
        //    moq = new Mock<IFrequentFlyerNumberValidator>();
        //    sut = new CreditCardApplicationEvaluator(moq.Object);
        //}
        [Fact]
        public void ReferYoungApplications()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();
            // here we don't return the default value for refrence (null), we return the Mock (for the case of using ILicenceService)
            moq.DefaultValue = DefaultValue.Mock;
            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication();

            var descion = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, descion);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();
            moq.DefaultValue = DefaultValue.Mock;
            #region setup the moq
            // will return true, only when the value is X
            //moq.Setup(m => m.IsValid("X")).Returns(true);

            // to return true for any value, not just X
            //moq.Setup(m => m.IsValid(It.IsAny<string>())).Returns(true);

            // to return true, when a specific condition happens
            // will return true, if the FrequentFlyerNumber starts with y only.
            //moq.Setup(m => m.IsValid(
            //                        It.Is<string>(s => s.StartsWith("y")))
            //                        )
            //    .Returns(true); 

            // to return true, when the value in a specific range
            // will return true, if the value from a to w
            //moq.Setup(m => m.IsValid
            //                (It.IsInRange("a", "w", Moq.Range.Inclusive))
            //         ).Returns(true);


            // to return true, when the value in a specific set
            // will return true, if the value exists in the given values
            //moq.Setup(m => m.IsValid
            //                (It.IsIn("a", "b", "c"))
            //         ).Returns(true);


            // to return true, when the value in a Regex
            // will return true, if the value matches the Regex
            moq.Setup(m => m.IsValid
                            (It.IsRegex("[a-z]"))
                     ).Returns(true);
            #endregion

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication()
            {
                GrossAnnualIncome = 19_999,
                FrequentFlyerNumber = "b",
                Age = 42
            };

            var descion = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, descion);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerapplications()
        {
            // when set the Mock Behavior = strict, So we should explicitly setup any function, that we make use (ex:- isValid)
            // when set the Mock Behavior = Loose, So we don't have to explicitly setup any function, that we make use (ex:- isValid)
            var moq = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Default);
            moq.DefaultValue = DefaultValue.Mock;
            moq.Setup(m => m.IsValid(
                                    It.IsAny<string>()))
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication();

            var descion = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, descion);
        }

        [Fact]
        public void DeclineLowIncomeApplication()
        {
            // configure the out parameter
            var moq = new Mock<IFrequentFlyerNumberValidator>();
            // to create a moq<ILicenceService> when creating the moq.
            //moq.DefaultValue = DefaultValue.Mock;

            // instead of that we can return another value than (ExPIRED)
            moq.Setup(m => m.LicenceService.LicenceData.LicenceKey).Returns("OK");

            bool isValid = true;
            moq.Setup(m => m.IsValid(It.IsAny<string>(), out isValid));

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication();

            var descion = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, descion);
        }


        [Fact]
        public void ReferToHumnaWhenLicenceExpired()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(m => m.IsValid(It.IsAny<string>())).Returns(true);
            //moq.Setup(m => m.LicenceKey).Returns("EXPIRED");
            #region nested properties setup
            moq.Setup(m => m.LicenceService.LicenceData.LicenceKey).Returns("EXPIRED");
            #endregion
            var sut = new CreditCardApplicationEvaluator(moq.Object);
            var application = new CreditCardApplication
            {
                Age = 42
            };

            var descion = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, descion);
        }

        [Fact]
        public void ShouldReturnDetailed()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            // to remember changes happens.
            moq.SetupProperty(m => m.ValidatorType);

            // to remember all the properties, should come first before any property setup (to return specific value)
            moq.SetupAllProperties();

            moq.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");



            var sut = new CreditCardApplicationEvaluator(moq.Object);
            var application = new CreditCardApplication
            {
                Age = 42
            };

            sut.Evaluate(application);
            Assert.Equal(ValidatorType.Detailed, moq.Object.ValidatorType);
        }


        #region Behaviour testing
        [Fact]
        public void ShouldHitTheIsValidFunctionWithNullValue()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication();

            sut.Evaluate(application);

            moq.Verify(x => x.IsValid(null));
        }

        [Fact]
        public void ShouldHitTheIsValidFunctionWithAValue()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication() { FrequentFlyerNumber = "X" };

            sut.Evaluate(application);

            moq.Verify(x => x.IsValid("X"));
        }


        [Fact]
        public void ShouldHitTheIsValidFunctionWithWhatEverValue()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication() { FrequentFlyerNumber = "X" };

            sut.Evaluate(application);

            // just make sure the IsValid function is being called, regaldless of the value being passed
            moq.Verify(x => x.IsValid(It.IsAny<string>()), "The IsValid should be called");
        }

        [Fact]
        public void ShouldNotHitTheIsValidFunctionWithWhatEverValue()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication() { GrossAnnualIncome = 100_100 };

            sut.Evaluate(application);

            // make sure the IsValid function will never be called
            moq.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public void ShouldHitTheLicenceKeyProperty()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication() { GrossAnnualIncome = 99_000 };

            sut.Evaluate(application);

            // make sure the LicenceKey property will be called
            moq.VerifyGet(m => m.LicenceService.LicenceData.LicenceKey);
        }


        #endregion

        [Fact]
        public void ShouldthrowAnException()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");
            moq.Setup(m => m.IsValid(It.IsAny<string>())).Throws<Exception>();

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication() { Age = 42 };

            var descion = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, descion);
        }

        [Fact]
        public void ShouldRaiseAnEventAndIncreseTheNumber()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            moq.Setup(m => m.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformance += null, EventArgs.Empty);

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication()
            {
                FrequentFlyerNumber = "X",
                Age = 42
            };

            sut.Evaluate(application);

            Assert.Equal(1, sut.ValidatorLookupCount);
        }


        [Fact]
        public void ReturnDifferentResultWhenCalledTwice()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            moq.SetupSequence(m => m.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application = new CreditCardApplication()
            {
                Age = 25
            };

            var firstDescion = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDescion);


            var secondDescion = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDescion);
        }

        [Fact]
        public void MakeSureTheFunctionIsCalledWithSpecificNumberOfTimes()
        {
            var moq = new Mock<IFrequentFlyerNumberValidator>();

            moq.Setup(x => x.LicenceService.LicenceData.LicenceKey).Returns("OK");

            var frequentNumbersAccepted = new List<string>();
            moq.Setup(m => m.IsValid(Capture.In(frequentNumbersAccepted)));

            var sut = new CreditCardApplicationEvaluator(moq.Object);

            var application01 = new CreditCardApplication() { Age = 25, FrequentFlyerNumber = "aa" };
            var application02 = new CreditCardApplication() { Age = 25, FrequentFlyerNumber = "bb" };
            var application03 = new CreditCardApplication() { Age = 25, FrequentFlyerNumber = "cc" };

            sut.Evaluate(application01);
            sut.Evaluate(application02);
            sut.Evaluate(application03);

            Assert.Equal(new List<string> { "aa", "bb", "cc" }, frequentNumbersAccepted);
        }
    }
}
