using System;
using System.ComponentModel.DataAnnotations;

namespace SevenStuds.Models
{
    public class Card
    {
        public Card()
        {
            // Constructor without parameters is required to enabled JSON deserialisation
        }

        public Card(CardEnum CV, SuitEnum CS)
        {
            CardValue = CV;
            CardSuit = CS;
        }
        public Card(string cardCode)
        {
            switch ( cardCode.Substring(0,1) )
            {
                case "2" : CardValue = CardEnum.Two; break;
                case "3" : CardValue = CardEnum.Three; break;
                case "4" : CardValue = CardEnum.Four; break;
                case "5" : CardValue = CardEnum.Five; break;
                case "6" : CardValue = CardEnum.Six; break;
                case "7" : CardValue = CardEnum.Seven; break;
                case "8" : CardValue = CardEnum.Eight; break;
                case "9" : CardValue = CardEnum.Nine; break;
                case "T" : CardValue = CardEnum.Ten; break;
                case "J" : CardValue = CardEnum.Jack; break;
                case "Q" : CardValue = CardEnum.Queen; break;
                case "K" : CardValue = CardEnum.King; break;
                case "A" : CardValue = CardEnum.Ace; break;
            }   
            switch ( cardCode.Substring(1,1) )
            {
                case "c" : CardSuit = SuitEnum.Clubs; break;
                case "d" : CardSuit = SuitEnum.Diamonds; break;
                case "h" : CardSuit = SuitEnum.Hearts; break;
                case "s" : CardSuit = SuitEnum.Spades; break;
            }                     
        }
        public CardEnum CardValue { get; set; }
        public SuitEnum CardSuit { get; set;}

        public override string ToString()
        {
            return ToString(CardToStringFormatEnum.LongCardName);
        }

        public string ToString(CardToStringFormatEnum format)
        {
            switch (format)
            {
                case CardToStringFormatEnum.LongCardName:
                    {
                        return CardValue.ToString() + " of " + CardSuit.ToString();
                    }

                case CardToStringFormatEnum.ShortCardName:
                    {
                        switch (CardValue)
                        {
                            case CardEnum.Dummy:
                                {
                                    return "";
                                }

                            case CardEnum.Two:
                                {
                                    return "2" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }                                

                            case CardEnum.Three:
                                {
                                    return "3" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Four:
                                {
                                    return "4" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Five:
                                {
                                    return "5" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Six:
                                {
                                    return "6" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Seven:
                                {
                                    return "7" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Eight:
                                {
                                    return "8" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Nine:
                                {
                                    return "9" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Ten:
                                {
                                    return "T" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Jack:
                                {
                                    return "J" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Queen:
                                {
                                    return "Q" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.King:
                                {
                                    return "K" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }

                            case CardEnum.Ace:
                                {
                                    return "A" + CardSuit.ToString().Substring(0, 1).ToLower();
                                }
                        }

                        break;
                    }
            }

            return "<Card value not set>";
        }
    }
}