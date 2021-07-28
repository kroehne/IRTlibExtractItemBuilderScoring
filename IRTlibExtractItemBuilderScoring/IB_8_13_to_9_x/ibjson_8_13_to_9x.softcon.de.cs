using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractItemBuilderScoring.IB_8_13_to_9_x
{
    public class Stimulus
    {
       
        public string runtimeCompatibilityVersion { get; set; }
     
        public string itemName { get; set; }
      
      
        public int itemWidth { get; set; }
      
        public int itemHeight { get; set; }
         
        public List<string> tasks { get; set; }
    }
}
