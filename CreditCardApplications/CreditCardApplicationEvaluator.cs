using System;

namespace CreditCardApplications
{
    public class CreditCardApplicationEvaluator
    {
        private const int AutoReferralMaxAge = 20;
        private const int HighIncomeThreshold = 100_000;
        private const int LowIncomeThreshold = 20_000;
        private readonly IFrequentFlyerNumberValidator _validator;
        public int ValidatorLookupCount { get; private set; }
        public CreditCardApplicationEvaluator(IFrequentFlyerNumberValidator validator)
        {
            _validator = validator;
            _validator.ValidatorLookupPerformance += ValidatorLookupPerformed;
        }

        private void ValidatorLookupPerformed(object sender, EventArgs e)
        {
            ValidatorLookupCount++;
        }
        public CreditCardApplicationDecision Evaluate(CreditCardApplication application)
        {
            if (application.GrossAnnualIncome >= HighIncomeThreshold)
            {
                return CreditCardApplicationDecision.AutoAccepted;
            }

            if (_validator.LicenceService.LicenceData.LicenceKey == "EXPIRED")
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }
            _validator.ValidatorType = application.Age > 22 ? ValidatorType.Detailed : ValidatorType.Quick;


            bool isValidFrequencyFlyNumber;
            try
            {
                isValidFrequencyFlyNumber = _validator.IsValid(application.FrequentFlyerNumber);
            }
            catch (Exception)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }
            //var isValidFrequencyFlyNumber = _validator.IsValid(application.FrequentFlyerNumber);
            if (!isValidFrequencyFlyNumber)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }
            if (application.Age <= AutoReferralMaxAge)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.GrossAnnualIncome < LowIncomeThreshold)
            {
                return CreditCardApplicationDecision.AutoDeclined;
            }

            return CreditCardApplicationDecision.ReferredToHuman;
        }



        public CreditCardApplicationDecision EvaluateWithOut(CreditCardApplication application)
        {
            if (application.GrossAnnualIncome >= HighIncomeThreshold)
            {
                return CreditCardApplicationDecision.AutoAccepted;
            }

            if (_validator.LicenceService.LicenceData.LicenceKey == "EXPIRED")
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            _validator.ValidatorType = application.Age > 22 ? ValidatorType.Detailed : ValidatorType.Quick;
            _validator.IsValid(application.FrequentFlyerNumber, out var isValid);

            if (!isValid)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }
            if (application.Age <= AutoReferralMaxAge)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.GrossAnnualIncome < LowIncomeThreshold)
            {
                return CreditCardApplicationDecision.AutoDeclined;
            }

            return CreditCardApplicationDecision.ReferredToHuman;
        }
    }
}
