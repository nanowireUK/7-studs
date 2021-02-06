import React from 'react';

import { useSelector } from 'react-redux';
import { Box, Text, Tip } from 'grommet';
import { CircleInformation } from 'grommet-icons';

import { selectRejoinCode } from '../redux/slices/hub';

export default function RejoinCode () {
    const rejoinCode = useSelector(selectRejoinCode);

    return <Box direction="row" gap="xsmall" justify="center">
        <Text alignSelf="center" size="xlarge">{rejoinCode}</Text>
        <Tip content="Use this code to join from another device"><Box><CircleInformation size="small" /></Box></Tip>
    </Box>;
}