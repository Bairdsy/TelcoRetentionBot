using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using MultiDialogsBot.Dialogs;
using MultiDialogsBot.Database;

namespace MultiDialogsBot.Helper
{
    public class ScoreFuncs 
    {
        HandSets basket;

        public ScoreFuncs(HandSets bag)
        {
            basket = bag;
        }

        public double PictureLover(string model)
        {
            double max, min;
            double secondaryCameraScore, 
                cameraScore,
                screenSizeScore,
                dualCameraScore;
            HandSetFeatures phone = basket.GetModelFeatures(model);

            /**** Camera (most important) ****/
            basket.GetMaxAndMinLimits(x => x.Camera, out min, out max);
            cameraScore = LogisticFunc(phone.Camera, max - min);
            /*** Screen Size (second most important)  ***/
            basket.GetMaxAndMinLimits(x => x.ScreenSize, out min, out max);
            screenSizeScore = LogisticFunc(phone.ScreenSize, max - min);
            /******* Dual Camera (third), let's consider 1 if it has, zero otherwise ****/
            dualCameraScore = phone.DualCamera ? 1 : 0;
            /******* Secondary Camera Score (for selfies) ****/
            basket.GetMaxAndMinLimits(x => x.SecondaryCamera, out min, out max);
            secondaryCameraScore = LogisticFunc(phone.SecondaryCamera, max - min);
            return (secondaryCameraScore + 2 * dualCameraScore + 3 * screenSizeScore + 4 * cameraScore) / 10;
        }

        public double MovieWatcherScore(string model)  
        {
            double ramScore, screenSizeScore,storageScore,batteryLifeScore;    
            double max, min;
            HandSetFeatures phone = basket.GetModelFeatures(model);

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

        private double LogisticFunc(double x,double barLen)
        {
            double innerFuncResult;

            innerFuncResult = (12 / barLen)  * x;  // Scales to length 12
            innerFuncResult -= 6;             // Pulls back 6 positions

            return 1 / (1 + Math.Exp(-innerFuncResult));
        }
    }
}