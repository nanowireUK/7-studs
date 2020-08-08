using System;
using System.ComponentModel.DataAnnotations;

namespace SevenStuds.Models
{
    public class Card
    {

        private CardEnum cardValue;
        private SuitEnum cardSuit;

        public Card(CardEnum CV, SuitEnum CS)
        {
            cardValue = CV;
            cardSuit = CS;
        }

        public Card()
        {
            // Constructor without parameters is required to deserialise
        }
        public CardEnum CardValue { get => cardValue; set => cardValue = value; }
        public SuitEnum CardSuit { get => cardSuit; set => cardSuit = value;}

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