import React from 'react';

import { Box, Text } from 'grommet';
import { selectHandCompleted, selectPots, selectLastHandResult } from '../redux/slices/game';
import { useSelector } from 'react-redux';

export default function Pot () {
    const pots = useSelector(selectPots);
    const handCompleted = useSelector(selectHandCompleted);
    const lastHandResult = useSelector(selectLastHandResult);

    if (handCompleted) {
        return (
            <Box fill justify="center" align="center" alignContent="center" round={true} background="brand" direction="column">
                {lastHandResult.map((pot, index) => (
                    <Text key={index} size="xlarge" textAlign="center">{pot}</Text>
                ))}
            </Box>
        )
    }

    return (
        <Box fill justify="center" align="center" alignContent="center" round={true} background="brand" direction="column">
            {pots.map((pot, index) => (
                <Text key={index} size="xlarge" textAlign="center">{pot.reduce((a, b) => a + b, 0)}</Text>
            ))}
        </Box>
    )
}