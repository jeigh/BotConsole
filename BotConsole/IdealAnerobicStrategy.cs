//using System;



//namespace AntPlayground
//{
//    public class IdealAnerobicStrategy : IAnerobicStrategy
//    {
//        public IdealAnerobicStrategy(Func<int> getCurrentDraft, Func<float> getCurrentGrade, int maxAdditionalPower30)
//        {
//            GetCurrentDraft = getCurrentDraft;
//            GetCurrentGrade = getCurrentGrade;

//            _maxAdditionalPower30 = maxAdditionalPower30;
//        }

//        private readonly Func<int> GetCurrentDraft;
//        private readonly Func<float> GetCurrentGrade;
//        private int _maxAdditionalPower30;

//        private int _draftValue;
//        private float _gradeValue;


//        private int GetDraftAddend(float maxThreshold50)
//        {
//            float weight = _maxAdditionalPower30 / maxThreshold50;

//            float currentValue = GetCurrentDraft();
//            if (currentValue > maxThreshold50) currentValue = maxThreshold50;
//            if (currentValue > 0) return (int)(currentValue * weight);

//            else return 0;
//        }

//        private int GetGradeAddend(float maxThreshhold5)
//        {
//            float weight = _maxAdditionalPower30 / maxThreshhold5;

//            float currentValue = GetCurrentGrade();
//            if (currentValue > maxThreshhold5) currentValue = maxThreshhold5;
//            if (currentValue > 0) return (int)(currentValue * weight);

//            else return 0;
//        }

//        public int ApplyAdditionalAnerobicPower(int power) 
//        {
//            int draftAddend = GetDraftAddend(50f);
//            int gradeAddend = GetGradeAddend(5f);
//            int limitedContribution = Min(draftAddend + gradeAddend, _maxAdditionalPower30);
            
//            return power + limitedContribution;
//        }

//        private int Min(int a, int b) => a < b ? a : b;
//    }
//}