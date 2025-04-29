using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Helpers
{
    public static class ButtonAnimations
    {
        public static async Task ShrinkOnPress(VisualElement element)
        {
            if (element != null)
            {
                await element.ScaleTo(0.9, 50); // 90% scale in 50ms
            }
        }

        public static async Task ExpandOnRelease(VisualElement element)
        {
            if (element != null)
            {
                await element.ScaleTo(1.0, 50); // Back to normal size in 50ms
            }
        }
    }
}