using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPokerClub.Models
{
    //
    // Shamelessly nicked from https://github.com/jessechunn/StandardPokerHandEvaluator to get us moving quickly
    //
    // Should be able to use this for different ranking tables (e.g. for 2, 3 and 4-card partial hands as well as full 5-card hands)
    //
    public class EvalHand
    {
        private int key;
        private int rank;
        private string name;
        private int presentation_order;
        private long hand_value;
        public int Key { get => key; }
        public int Rank { get => rank; }
        public string Name { get => name; }
        public long HandValue { get => hand_value; }
        public List<int> PresentationOrder() {
            //Source https://www.physicsforums.com/threads/converting-integer-into-array-of-single-digits-in-c.558588/
            string po_string = this.presentation_order.ToString();
            char[] po_chars = po_string.ToCharArray();
            int[] po_ints = po_chars.Select(x => (int)Char.GetNumericValue(x)).ToArray();
            List<int> result = new List<int>(po_ints);
            return result;
        }

        public EvalHand(int key, int rank, string name)
        {
            this.key = key;
            this.rank = rank;
            this.name = name;
            this.presentation_order = 0; // This constructor is for the 2, 3 and 4-card hands which we will not need to reorder
            this.hand_value = 0; // This constructor is for the 2, 3 and 4-card hands which we will not need to show values for
        }
        public EvalHand(int key, int rank, string name, int presentation_order, long hand_value)
        {
            this.key = key;
            this.rank = rank;
            this.name = name;
            this.presentation_order = presentation_order;
            this.hand_value = hand_value;
        }
    }

}
