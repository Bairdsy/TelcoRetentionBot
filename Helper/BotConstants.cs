using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiDialogsBot.Helper
{
    public static class BotConstants
    {
        public const int MAX_CAROUSEL_CARDS = 5;
        public const int TOP_INTENTS = 3;

        public const double PERCENT_HIGH_CERTAINTY = 0.75;  
        public const double PERCENT_MEDIUM_CERTAINTY = 0.40;

        public const int MAX_ATTEMPTS = 3;
          
        public const string FLOW_TYPE_KEY = "FlowType";
        public const string EQUIPMENT_FLOW_TYPE = "equipment";
        public const string PLAN_FLOW_TYPE = "plan only";
        public const string BOTH_FLOW_TYPE = "both";

        public const string SELECTED_BRANDS_KEY = "LuisBrand";
        public const string SHOW_ME_ALL = "Show Me All";

        public const string LAST_FEATURE_KEY = "LastFeature";
        public const string LAST_NEED_KEY = "LastNeed";
        

        public const double FEATURE_PHONE_THRESHOLD = 0.7;
    }
}