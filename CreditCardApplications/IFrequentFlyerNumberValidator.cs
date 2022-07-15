using System;

namespace CreditCardApplications
{
    public interface ILicenceService
    {
        public ILicenceData LicenceData { get; }
    }
    public interface ILicenceData
    {
        public string LicenceKey { get; }
    }
    public interface IFrequentFlyerNumberValidator
    {
        bool IsValid(string frequentFlyerNumber);
        void IsValid(string frequentFlyerNumber, out bool isValid);
        //string LicenceKey { get; }

        public ILicenceService LicenceService { get; }
        public ValidatorType ValidatorType { get; set; }

        public event EventHandler ValidatorLookupPerformance;
    }

    public enum ValidatorType
    {
        Quick,
        Detailed

    }
}