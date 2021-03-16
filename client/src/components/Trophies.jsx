import React from 'react';

import { Box, Text } from 'grommet';
import { Trophy } from 'grommet-icons';

export default function Trophies ({trophyCount, color}) {
    if (!trophyCount) return null;
    return <Box direction="row" gap="xxsmall">
                <Text size="12px" color={color}>{trophyCount}</Text>
                <Trophy size="small" color={color}/>
            </Box>
}