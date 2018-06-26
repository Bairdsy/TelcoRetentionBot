using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using MultiDialogsBot.Dialogs;
using MultiDialogsBot.Database;

namespace MultiDialogsBot.Helper
{
    public class ScoreFuncs : IComparer<HandSetFeatures>
    {
        readonly Dictionary<NodeLuisSubsNeeds.ENeeds, sgn> comparingFunctions;
        readonly Dictionary<NodeLuisSubsNeeds.ENeeds, HandSets.accessor> getters;

        delegate int sgn(Tuple<HandSetFeatures, HandSetFeatures> x);


        HandSets basket;
        NodeLuisSubsNeeds.ENeeds intent;

        public ScoreFuncs(HandSets bag)
        {
            basket = bag;
            comparingFunctions = new Dictionary<NodeLuisSubsNeeds.ENeeds, sgn>()
            {
                {NodeLuisSubsNeeds.ENeeds.MovieWatcher,x => -Math.Sign(MovieWatcherScore(x.Item1) - MovieWatcherScore(x.Item2)) },
                {NodeLuisSubsNeeds.ENeeds.PictureLover,x => -Math.Sign(PictureLoverScore(x.Item1) - PictureLoverScore(x.Item2)) },
                { NodeLuisSubsNeeds.ENeeds.GamesAddict,x => -Math.Sign(GamesAddictScore(x.Item1) - GamesAddictScore(x.Item2)) },
                {NodeLuisSubsNeeds.ENeeds.Camera, x => -Math.Sign(CameraScore(x.Item1) - CameraScore(x.Item2)) }

            };
            getters = new Dictionary<NodeLuisSubsNeeds.ENeeds, HandSets.accessor>()
            {
                {NodeLuisSubsNeeds.ENeeds.MovieWatcher,x => MovieWatcherScore(x) },
                {NodeLuisSubsNeeds.ENeeds.PictureLover,x => PictureLoverScore(x) },
                {NodeLuisSubsNeeds.ENeeds.GamesAddict,x => GamesAddictScore(x) },
                {NodeLuisSubsNeeds.ENeeds.ShowOff, x => 1 },
                {NodeLuisSubsNeeds.ENeeds.Camera, x => CameraScore(x) }
            };
        }


        int IComparer<HandSetFeatures>.Compare(HandSetFeatures x, HandSetFeatures y)
        {
            Tuple<HandSetFeatures, HandSetFeatures> tuple;

            tuple = new Tuple<HandSetFeatures, HandSetFeatures>(x, y);
            return comparingFunctions[intent](tuple);
        }

        public int GetTopFive(NodeLuisSubsNeeds.ENeeds need)
        {
            List<string> returnVal = new List<string>();
            int numberObtained;

            intent = need;
            numberObtained = basket.SortAndGetTop(this, getters[intent]);
            if (numberObtained == 0)
                return 0;
            basket.removeAllButTop(numberObtained);
            return numberObtained;
        }

        private double CameraScore(HandSetFeatures phone)
        {
            double max, min;
            double secondaryCameraScore,
                cameraScore,
                dualCameraScore;

            /**** Camera  ****/

            basket.GetMaxAndMinLimits(x => x.Camera, out min, out max);
            cameraScore = LogisticFunc(phone.Camera, max - min);


            /******* Dual Camera  ****/

            basket.GetMaxAndMinLimits(x => x.DualCamera, out min, out max);
            dualCameraScore = LogisticFunc(phone.DualCamera, max - min);

            /******* Secondary Camera Score  ****/

            basket.GetMaxAndMinLimits(x => x.SecondaryCamera, out min, out max);
            secondaryCameraScore = LogisticFunc(phone.SecondaryCamera, max - min);

            return dualCameraScore * secondaryCameraScore * cameraScore;
        }


        private double PictureLoverScore(HandSetFeatures phone)
        {
            double max, min;
            double secondaryCameraScore, 
                cameraScore,
                screenSizeScore,
                dualCameraScore;
            

            /**** Camera (most important) ****/

            basket.GetMaxAndMinLimits(x => x.Camera, out min, out max);
            cameraScore = LogisticFunc(phone.Camera, max - min);



            /*** Screen Size (second most important)  ***/

            basket.GetMaxAndMinLimits(x => x.ScreenSize, out min, out max);
            screenSizeScore = LogisticFunc(phone.ScreenSize, max - min);


            /******* Dual Camera (third), let's consider 1 if it has, zero otherwise ****/

            basket.GetMaxAndMinLimits(x => x.DualCamera, out min, out max);
            dualCameraScore = LogisticFunc(phone.DualCamera, max - min);

            /******* Secondary Camera Score (for selfies) ****/

            basket.GetMaxAndMinLimits(x => x.SecondaryCamera, out min, out max);
            secondaryCameraScore = LogisticFunc(phone.SecondaryCamera, max - min);

            return (secondaryCameraScore + 2 * dualCameraScore + 3 * screenSizeScore + 4 * cameraScore) / 10;
        }

        private double MovieWatcherScore(HandSetFeatures phone)  
        {
            double ramScore, screenSizeScore,storageScore,batteryLifeScore;    
            double max, min;
            

            /*** RAM ****/
            basket.GetMaxAndMinLimits(x => x.RamSize, out min, out max);
            ramScore = LogisticFunc(phone.RamSize, max - min);

            /***** Screen Size */
            basket.GetMaxAndMinLimits(x => x.ScreenSize, out min, out max);
            screenSizeScore = LogisticFunc(phone.ScreenSize, max - min);

            /**** Storage ****/
            basket.GetMaxAndMinLimits(x => x.MemoryMB, out min, out max);
            storageScore = LogisticFunc(phone.MemoryMB, max - min);

            /*** Battery Life ***/
            basket.GetMaxAndMinLimits(x => x.BatteryLife, out min, out max);
            batteryLifeScore = LogisticFunc(phone.BatteryLife, max - min);

            return (2 * (ramScore + screenSizeScore) + storageScore + 3 * batteryLifeScore) / 8;
        }

        private double GamesAddictScore(HandSetFeatures phone)
        {
            double ramScore,resolutionScore,batteryLifeScore,screenSizeScore;
            double min,max;

            /**** RAM (Most important) **/

            basket.GetMaxAndMinLimits(x => x.RamSize, out min, out max);
            ramScore = LogisticFunc(phone.RamSize, max - min);



            /*** Resolution (Second most important)  ****/

            basket.GetMaxAndMinLimits(x => Miscellany.Product(x.DisplayResolution), out min, out max);
            resolutionScore = LogisticFunc(Miscellany.Product(phone.DisplayResolution), max - min);


            /***** Battery Life (third most important)  ********/

            basket.GetMaxAndMinLimits(x => x.BatteryLife, out min, out max);
            batteryLifeScore = LogisticFunc(phone.BatteryLife, max - min);



            /**********  Screen Size  (The least important)  *************/

            basket.GetMaxAndMinLimits(x => x.ScreenSize, out min, out max);
            screenSizeScore = LogisticFunc(phone.ScreenSize, max - min);

            return (4 * ramScore + 3 * resolutionScore + 2 * batteryLifeScore + screenSizeScore) / 10;
        }


        private double LogisticFunc(double x,double barLen)
        {
            double innerFuncResult;

            innerFuncResult = (12 / barLen)  * x;  // Scales to length 12
            innerFuncResult -= 6;             // Pulls back 6 positions

            return 1 / (1 + Math.Exp(-innerFuncResult));
        }
    }
}