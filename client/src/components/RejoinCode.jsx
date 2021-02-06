import React from 'react';

import { useSelector } from 'react-redux';
import { Text, Tip } from 'grommet';

import { selectRejoinCode } from '../redux/slices/hub';

export default function RejoinCode () {
    const rejoinCode = useSelector(selectRejoinCode);

    return <Tip content="Use your rejoin code to play from another device!"><Text alignSelf="center" size="large">{rejoinCode}</Text></Tip>;
}