import React, { memo } from 'react';
import { Box, Heading, Text } from 'grommet';
import PokerCard from '../PokerCard';

const handRankingsData = [
    { handName: 'Royal Flush', probability: '0.003', exampleCards: ['TS', 'JS', 'QS', 'KS', 'AS'] },
    { handName: 'Straight Flush', probability: '0.03', exampleCards: ['4H', '5H', '6H', '7H', '8H'] },
    { handName: 'Four of a Kind', probability: '0.17', exampleCards: ['AH', 'AD', 'AC', 'AS', '4D'] },
    { handName: 'Full House', probability: '2.60', exampleCards: ['8H', '8D', '8C', 'KH', 'KS'] },
    { handName: 'Flush', probability: '3.03', exampleCards: ['TC', '4C', 'QC', '7C', '2C'] },
    { handName: 'Straight', probability: '4.62', exampleCards: ['7C', '8H', '9D', 'TH', 'JS'] },
    { handName: 'Three of a Kind', probability: '4.83', exampleCards: ['QH', 'QC', 'QD', '5S', 'AD'] },
    { handName: 'Two Pair', probability: '23.5', exampleCards: ['3H', '3D', '6C', '6H', 'KS'] },
    { handName: 'Pair', probability: '43.8', exampleCards: ['5H', '5S', '2C', 'JC', 'AD'] },
    { handName: 'High Card', probability: '17.4', exampleCards: ['2D', '5S', '6S', 'JH', 'AC'] },
];

function Ranking({ handName, probability, exampleCards }) {
    return (
        <Box direction="column" gap="small">
            <Box direction="row" justify="between">
                <Text>{handName}</Text>
                <Text>{probability}%</Text>
            </Box>
            <Box height="35px" gap="xxsmall" direction="row">
                {exampleCards.map(([face, suit], index) => <PokerCard key={index} face={face} suit={suit} />)}
            </Box>
        </Box>
    );
}

function HandRankings() {
    return (
        <Box gap="small">
            <Heading margin="none" level={2}>Hand Rankings</Heading>
            {handRankingsData.map(({ handName, probability, exampleCards }) => (
                <Ranking key={handName} handName={handName} probability={probability} exampleCards={exampleCards} />
            ))}
        </Box>
    );
}

export default memo(HandRankings);
