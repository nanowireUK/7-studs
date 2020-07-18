using System;
using System.Collections.Generic;
using System.Text;

namespace SevenStuds.Models
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
        public int Key { get => key; }
        public int Rank { get => rank; }
        public string Name { get => name; }
        public EvalHand(int key, int rank, string name)
        {
            this.key = key;
            this.rank = rank;
            this.name = name;
        }
    }

}
